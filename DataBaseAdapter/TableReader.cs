using DataBaseAdapter.Classes;
using DataBaseAdapter.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseAdapter
{
    public class TableReader<T> where T : new()
    {

        /// <summary>
        /// when set to true, allows to update and delete without a where-statemen
        /// </summary>
        public bool AllowUnsaveDeleteAndUpdate { get; set; }

        /// <summary>
        /// Stores the propertynames
        /// </summary>
        private List<PropertyInfo> _valueProperties;

        /// <summary>
        /// stores the fieldnames (DB)
        /// </summary>
        private List<string> _valuesFields;

        /// <summary>
        /// Stores the table names
        /// </summary>
        private List<string> _tables;

        private SqlWhereStatement _whereStatement;

        /// <summary>
        /// Reads the properties of the given type and adds alle tables an valuefields that can be comprehended (requieres AttributeTable(tablename) for each property)
        /// </summary>
        private void InitBase()
        {
            AllowUnsaveDeleteAndUpdate = false;
            _valueProperties = new List<PropertyInfo>();
            _valuesFields = new List<string>();
            _tables = new List<string>();

            foreach (PropertyInfo propI in typeof(T).GetProperties())
            {
                CustomAttributeData data = propI.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(AttributeTable));
                if (data != null)
                {
                    string tablename = (string)data.ConstructorArguments.FirstOrDefault().Value;

                    if (data.NamedArguments.Count > 0) //In case maskedfield is set
                    {
                        AddValueField(tablename, (string)data.NamedArguments.FirstOrDefault().TypedValue.Value, propI.Name);
                    }
                    else
                    {
                        AddValueField(tablename, propI.Name);
                    }
                    AddValueProperty(propI);
                }
            }
        }

        private void AddTable(string tableName)
        {
            tableName = tableName.ToLower();
            if (!_tables.Contains(tableName))
            {
                _tables.Add(tableName);
            }
        }

        private void AddValueField(string tablename, string valueFieldName, string propetyName)
        {
            AddValueField(tablename, string.Format("{0} as {1}", valueFieldName, propetyName));
        }

        private void AddValueField(string tableName, string valueFieldName)
        {
            AddTable(tableName);
            valueFieldName = string.Format("{0}.{1}", tableName, valueFieldName).ToLower();

            if (!_valuesFields.Contains(valueFieldName))
            {
                _valuesFields.Add(valueFieldName);
            }
        }

        private void AddValueProperty(PropertyInfo property)
        {
            if (_valueProperties.Contains(property))
            {
                throw new InvalidTypeException("The type has two properties with the same name. If you want to select two valuesfields with the same identifier, mask one");
            }
            else
            {
                _valueProperties.Add(property);
            }
        }

        public TableReader()
        {
            InitBase();
        }

        public TableReader(SqlWhereStatement whereStatement)
        {
            InitBase();
            AddWhereStatement(whereStatement);
        }

        public void AddWhereStatement(SqlWhereStatement whereStatement)
        {
            if (_whereStatement != null)
            {
                throw new InvalidOperationException("There is already a where Statement in this TableReader.");
            }
            else
            {
                _whereStatement = whereStatement;
            }

            foreach (string s in _whereStatement.GetTablesInStatement())
            {
                AddTable(s);
            }

        }

        public string GetSelectSql()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");

            for (int i = 0; i < _valuesFields.Count; i++)
            {
                if (i < _valuesFields.Count - 1)
                {
                    sb.AppendLine(string.Format(" {0}, ", _valuesFields[i]));
                }
                else
                {
                    sb.AppendLine(string.Format(" {0} ", _valuesFields[i]));
                }
            }

            sb.Append(" FROM ");

            for (int i = 0; i < _tables.Count; i++)
            {
                if (i < _tables.Count - 1)
                {
                    sb.AppendFormat("{0}, ", _tables[i]);
                }
                else
                {
                    sb.AppendFormat("{0} ", _tables[i]);
                }
            }

            sb.AppendLine();
            if (_whereStatement != null)
            {
                sb.AppendLine(_whereStatement.ToString());
            }

            return sb.ToString();
        }

        public string GetInsertSql(string table, T obj)
        {
            table = table.ToLower();
            StringBuilder sbFields = new StringBuilder();
            sbFields.AppendLine(string.Format("INSERT INTO {0} (", table));

            StringBuilder sbValues = new StringBuilder();
            sbValues.AppendLine(" VALUES (");

            bool first = true;

            foreach (PropertyInfo propI in _valueProperties)
            {
                CustomAttributeData attr = propI.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(AttributeTable));

                if ((string)attr.ConstructorArguments.FirstOrDefault().Value == table)//Property refrences table
                {
                    if (!first)
                    {
                        sbFields.AppendLine(", ");
                        sbValues.AppendLine(", ");
                    }
                    else
                    {
                        first = false;
                    }
                    if (attr.NamedArguments.Count > 0)//masked field
                    {
                        sbFields.Append(attr.NamedArguments.FirstOrDefault().TypedValue.Value);
                    }
                    else
                    {
                        sbFields.Append(propI.Name);
                    }

                    sbValues.Append(StaticHelper.MaskValueForSql(propI.GetValue(obj)));
                }
            }

            sbFields.AppendLine(")");
            sbValues.AppendLine(")");

            return sbFields.ToString() + sbValues.ToString();

        }

        public List<T> GetItems()
        {
            List<T> result = new List<T>();

            DbConnection con = StaticHelper.GetDbConnection();

            Exception exception = null;

            try
            {
                con.Open();
                DbTransaction trans = con.BeginTransaction();
                DbCommand command = StaticHelper.GetDbCommand(GetSelectSql(), con, trans);
                DbDataReader reader = command.ExecuteReader();

                try
                {
                    while (reader.Read())
                    {
                        T newObject = new T();
                        foreach (PropertyInfo i in _valueProperties)
                        {
                            int ordinal = reader.GetOrdinal(i.Name.ToLower());
                            i.SetValue(newObject, StaticHelper.MaskValueForType(reader.GetValue(ordinal), i.PropertyType));
                        }
                        result.Add(newObject);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                reader.Close();

            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                con.Close();
            }

            if (exception != null)
            {
                throw exception;
            }


            return result;
        }

        public int InsertItems(string tableName, IEnumerable<T> items)
        {
            int numberInserted = 0;

            DbConnection con = StaticHelper.GetDbConnection();
            Exception exception = null;

            try
            {
                con.Open();
                DbTransaction transAction = con.BeginTransaction();

                foreach (T item in items)
                {
                    DbCommand command = StaticHelper.GetDbCommand(GetInsertSql(tableName, item), con, transAction);
                    command.ExecuteNonQuery();
                    numberInserted++;
                }

                transAction.Commit();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                con.Close();
            }

            if(exception != null)
            {
                throw exception;
            }

            return numberInserted;
        }

        public void DeleteItems(string tableName)
        {
            if (AllowUnsaveDeleteAndUpdate || _whereStatement != null)
            {
                string sql = string.Format("delete from {0} {1}", tableName, _whereStatement);

                Exception exception = null;

                DbConnection con = StaticHelper.GetDbConnection();

                try
                {
                    con.Open();
                    DbTransaction trans = con.BeginTransaction();
                    DbCommand command = StaticHelper.GetDbCommand(sql, con, trans);
                    command.ExecuteNonQuery();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    con.Close();
                }

                if (exception != null)
                {
                    throw exception;
                }
            }
            else
            {
                throw new InvalidOperationException("Requested to query a delete statement without a where-statement.\nIf that was your Intention set \"AllowUnsaveDeleteAndUpdate\" to true.");
            }
        }
    }
}

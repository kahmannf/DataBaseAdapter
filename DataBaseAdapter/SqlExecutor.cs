using DataBaseAdapter.Exceptions;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseAdapter
{
    public static class SqlExecutor
    {
        public static int ExcecuteNoQuery(string sql)
        {
            DbConnection con = StaticHelper.GetDbConnection();
            Exception exception = null;
            int returnvalue = 0;
            try
            {
                con.Open();
                DbTransaction transAction = con.BeginTransaction();
                DbCommand command = StaticHelper.GetDbCommand(sql, con, transAction);
                returnvalue = command.ExecuteNonQuery();
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
            if (exception != null)
            {
                throw exception;
            }
            else
            {
                return returnvalue;
            }
        }

        public static DataTable ExcecuteReader(string sql)
        {
            DbConnection con = StaticHelper.GetDbConnection();
            Exception exception = null;
            DataTable returnvalue = null;
            try
            {
                con.Open();
                DbTransaction transAction = con.BeginTransaction();
                DbCommand command = StaticHelper.GetDbCommand(sql, con, transAction);
                DbDataReader reader = command.ExecuteReader();
                returnvalue = GetDataTableFromReader(reader);
                reader.Close();
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
            if (exception != null)
            {
                throw exception;
            }
            else
            {
                return returnvalue;
            }
        }

        

        private static DataTable GetDataTableFromReader(DbDataReader reader)
        {
            DataTable result = new DataTable();
            
            Dictionary<int, string> columns = new Dictionary<int, string>();


            //Read the table schema and create DataTable
            foreach (DataRow row in reader.GetSchemaTable().Rows)
            {
                string name = (string)row["ColumnName"];
                int ordinal = reader.GetOrdinal(name);
                columns.Add(ordinal, name);
                result.Columns.Add(new DataColumn(name, reader.GetFieldType(ordinal)));
            }

            
            while (reader.Read())
            {
                DataRow newRow = result.NewRow();

                foreach (int ordinal in columns.Keys)
                {
                    string name = string.Empty;
                    if (columns.TryGetValue(ordinal, out name))
                    {
                        newRow[name] = reader.GetValue(ordinal);
                    }
                }

                result.Rows.Add(newRow);
            }
            
            return result;
        }
    }
}

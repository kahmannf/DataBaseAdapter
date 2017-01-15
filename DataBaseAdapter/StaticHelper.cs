using DataBaseAdapter.Classes;
using DataBaseAdapter.Exceptions;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseAdapter
{
    public delegate object MaskedTypeSupport(Type t);
    public delegate string MaskedValueForSqlSupport(object value);
    public static class StaticHelper
    {
        public static string MaskValueForSql(object value)
        {
            if (value == null)
            {
                return "null";
            }
            Type t = value.GetType();

            if (t == typeof(System.String))
            {
                return string.Format("'{0}'", value);
            }
            if (t == typeof(System.Int64) || t == typeof(System.Int32) || t == typeof(System.Int16) || t == typeof(System.Byte))
            {
                return string.Format("{0}", value);
            }
            if (t == typeof(System.DateTime))
            {
                return string.Format("STR_TO_DATE('{0}', '%Y.%c.%d.%H.%i.%s')", ((DateTime)value).ToString("yyyy.MM.dd.HH.mm.ss"));
            }
            else if (t == typeof(System.DBNull))
            {
                return "null";
            }
            else
            {
                if (MaskValueForSqlDelegate != null)
                {
                    return MaskValueForSqlDelegate(value);
                }
                throw new NotImplementedException(string.Format("The type {0} is not supported. You can", t.Name));
            }
        }

        public static MaskedValueForSqlSupport MaskValueForSqlDelegate { get; set; }

        public static object MaskValueForType(object value, Type t)
        {
            if (value == null || value.GetType() == typeof(System.DBNull))
            {
                return MaskNullForType(t);
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Makes a value for null (If).
        /// </summary>
        /// <param name="t">The type</param>
        /// <returns></returns>
        public static object MaskNullForType(Type t)
        {
            if (IsNullable(t))
            {
                return null;
            }
            else
            {
                if (t == typeof(System.String))
                {
                    return string.Empty;
                }
                if (t == typeof(System.Int64) || t == typeof(System.Int32) || t == typeof(System.Int16) || t == typeof(System.Byte))
                {
                    return 0;
                }
                if (t == typeof(System.DateTime))
                {
                    return DateTime.MinValue;
                }
                else if (t == typeof(System.DBNull))
                {
                    return DBNull.Value;
                }


                if (MaskNullDelegate != null)
                {
                    return MaskNullDelegate(t);
                }
                throw new InvalidTypeException(string.Format("Type \"{0}\" has no support from masking null.\nYour can mask your own values by providing a MakedTypeSoppurt delegate to \"MaskNullDelegate\" that handles masking of costum types.", t.Name));
            }
        }

        public static MaskedTypeSupport MaskNullDelegate { get; set; }

        public static bool IsNullable(Type type)
        {
            if (!type.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
        }


        public static string GetSqlComparerAsString(SqlConditionComparer comparer)
        {
            switch (comparer)
            {
                case SqlConditionComparer.Equals:
                    return "=";
                case SqlConditionComparer.Larger:
                    return ">";
                case SqlConditionComparer.LargerEquals:
                    return ">=";
                case SqlConditionComparer.Smaller:
                    return "<";
                case SqlConditionComparer.SmallerEquals:
                    return "<=";
                case SqlConditionComparer.Like:
                    return "like";
                case SqlConditionComparer.NotLike:
                    return "not like";
                case SqlConditionComparer.Is:
                    return "is";
                case SqlConditionComparer.IsNot:
                    return "is not";
                default:
                    throw new NotSupportedException();
            }
        }

        public static string GetSqlConnectorAsString(SqlConditionConnector connector)
        {
            switch (connector)
            {
                case SqlConditionConnector.AND:
                    return "AND";
                case SqlConditionConnector.OR:
                    return "OR";
                default:
                    throw new NotSupportedException();
            }
        }

        public static DbConnection GetDbConnection()
        {
            switch (DataBaseConfiguration.DataBaseType)
            {
                case DataBaseConfiguration.SupportedDataBases.MySql:
                    return new MySqlConnection(DataBaseConfiguration.GetConnectionString());
                default:
                    throw new ConfigurationException();
            }
        }

        public static DbCommand GetDbCommand(string sql, DbConnection con, DbTransaction trans)
        {
            switch (DataBaseConfiguration.DataBaseType)
            {
                case DataBaseConfiguration.SupportedDataBases.MySql:
                    return new MySqlCommand(sql, con as MySqlConnection, trans as MySqlTransaction);
                default:
                    throw new ConfigurationException();
            }
        }
    }
}

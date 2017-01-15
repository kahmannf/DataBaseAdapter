using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseAdapter.Classes
{

    #region Enums

    /// <summary>
    /// Represents a the way two sql-conditions are connected
    /// </summary>
    public enum SqlConditionConnector { AND, OR }

    /// <summary>
    /// Represents a way of comparing two values in sql
    /// </summary>
    public enum SqlConditionComparer { Equals, Larger, LargerEquals, Smaller, SmallerEquals, Like, NotLike, Is, IsNot }

    #endregion

    /// <summary>
    /// A sql where condition that can be apended to a sql statment
    /// </summary>
    public class SqlWhereStatement
    {
        private List<SqlConditionGroup> ConditionGroups { get; set; }
        private List<SqlCondition> SimpleAndCondition { get; set; }

        public SqlWhereStatement()
        {
            ConditionGroups = new List<SqlConditionGroup>();
            SimpleAndCondition = new List<SqlCondition>();
        }

        #region AddRegion
        
        /// <summary>
        /// Adds a condition to the statement and connects it with an AND
        /// </summary>
        /// <param name="condition"></param>
        public void AddStatements(SqlCondition condition)
        {
            SimpleAndCondition.Add(condition);
        }

        /// <summary>
        /// Adds mutiple conditions to the statement
        /// </summary>
        /// <param name="conditions"></param>
        public void AddStatements(IEnumerable<SqlCondition> conditions)
        {
            SimpleAndCondition.AddRange(conditions);
        }

        /// <summary>
        /// Adds a conditionGroup to the statement
        /// </summary>
        /// <param name="conditionGroup"></param>
        public void AddStatements(SqlConditionGroup conditionGroup)
        {
            ConditionGroups.Add(conditionGroup);
        }

        /// <summary>
        /// Adds mutliple conditionGroups to the statement
        /// </summary>
        /// <param name="conditionGroups"></param>
        public void AddStatements(IEnumerable<SqlConditionGroup> conditionGroups)
        {
            ConditionGroups.AddRange(conditionGroups);
        }

        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(" WHERE ");

            bool first = true;

            foreach (SqlCondition condition in SimpleAndCondition)
            {
                if (!first)
                {
                    sb.Append(" AND ");
                }
                else
                {
                    first = false;
                }

                sb.AppendLine(condition.ToString());
            }

            foreach (SqlConditionGroup group in ConditionGroups)
            {
                if (!first)
                {
                    sb.Append(" AND ");
                }
                else
                {
                    first = false;
                }

                sb.AppendLine(group.ToString());
            }

            return sb.ToString();
        }

        public List<string> GetTablesInStatement()
        {
            List<string> result = new List<string>();

            foreach (SqlCondition condition in SimpleAndCondition)
            {
                result.AddRange(condition.GetTables());
            }

            foreach (SqlConditionGroup group in ConditionGroups)
            {
                result.AddRange(group.GetTables());
            }

            return result;
        }
    }

    /// <summary>
    /// Represents a Colection of Sql-Condition-Expression all connected with the same SqlConditionCollector
    /// </summary>
    public class SqlConditionGroup
    {
        public List<SqlCondition> ChildConditions { get; set; }

        public SqlConditionConnector ChildConnector { get; set; }

        public SqlConditionGroup(IEnumerable<SqlCondition> conditions, SqlConditionConnector connector)
        {
            ChildConnector = connector;
            ChildConditions = new List<SqlCondition>(conditions);
        }

        public SqlConditionGroup(SqlConditionConnector connector)
        {
            ChildConnector = connector;
            ChildConditions = new List<SqlCondition>();
        }

        public void AddConditions(SqlCondition condition)
        {
            ChildConditions.Add(condition);
        }

        public void AddConditions(IEnumerable<SqlCondition> conditions)
        {
            ChildConditions.AddRange(conditions);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("(");

            for(int i = 0; i < ChildConditions.Count; i++)
            {
                if (i < 0)
                {
                    sb.AppendFormat(" {0} ", StaticHelper.GetSqlConnectorAsString(ChildConnector));
                }
                sb.AppendLine(ChildConditions[i].ToString());
            }

            sb.AppendLine(")");

            return sb.ToString(); 
        }

        public List<string> GetTables()
        {
            List<string> result = new List<string>();

            foreach(SqlCondition condition in ChildConditions)
            {
                result.AddRange(condition.GetTables());
            }

            return result;
        }
    }

    /// <summary>
    /// Represents a Sql-Condition-Expression
    /// </summary>
    public class SqlCondition
    {
        #region Contructors

        public SqlCondition(string table, string field, object value)
        {
            LeftExpression = new SqlTableCondition(table, field);
            RightExpression = new SqlValueCondition(value);
            Comparer = SqlConditionComparer.Equals;
        }

        public SqlCondition(string table, string field, object value, SqlConditionComparer comparer)
        {
            LeftExpression = new SqlTableCondition(table, field);
            RightExpression = new SqlValueCondition(value);
            Comparer = comparer;
        }

        public SqlCondition(string table1, string field_table1, string table2, string field_table2)
        {
            LeftExpression = new SqlTableCondition(table1, field_table1);
            RightExpression = new SqlTableCondition(table2, field_table2);
            Comparer = SqlConditionComparer.Equals;
        }

        public SqlCondition(string table1, string field_table1, string table2, string field_table2, SqlConditionComparer comparer)
        {
            LeftExpression = new SqlTableCondition(table1, field_table1);
            RightExpression = new SqlTableCondition(table2, field_table2);
            Comparer = comparer;
        }


        public SqlCondition(SqlConditionExpression leftExpession, SqlConditionExpression rightExpression)
        {
            LeftExpression = leftExpession;
            RightExpression = rightExpression;
            Comparer = SqlConditionComparer.Equals;
        }

        public SqlCondition(SqlConditionExpression leftExpession, SqlConditionExpression rightExpression, SqlConditionComparer comparer)
        {
            LeftExpression = leftExpession;
            RightExpression = rightExpression;
            Comparer = comparer;
        }

        #endregion

        public SqlConditionExpression LeftExpression { get; set; }
        public SqlConditionExpression RightExpression { get; set; }
        public SqlConditionComparer Comparer { get; set; }

        public string ComparerString
        {
            get { return StaticHelper.GetSqlComparerAsString(Comparer); }
        }

        public string Condition
        {
            get
            {
                return string.Format("{0} {1} {2}", LeftExpression, ComparerString, RightExpression);
            }
        }

        public override string ToString()
        {
            return Condition;
        }

        public List<string> GetTables()
        {
            List<string> result = new List<string>();
            if ((LeftExpression as SqlTableCondition) != null)
            {
                result.Add((LeftExpression as SqlTableCondition).TableName);
            }
            if ((RightExpression as SqlTableCondition) != null)
            {
                result.Add((RightExpression as SqlTableCondition).TableName);
            }

            return result;
        }
    }

    #region ConditionExpression

    public abstract class SqlConditionExpression
    {
        public abstract string Expression { get; }
    }

    public class SqlValueCondition : SqlConditionExpression
    {
        public SqlValueCondition(object value)
        {
            Value = value;
        }

        public object Value { get; set; }
        public override string Expression
        {
            get
            {
                return string.Format("{0}", StaticHelper.MaskValueForSql(Value));
            }
        }

        public override string ToString()
        {
            return Expression;
        }
    }

    public class SqlTableCondition : SqlConditionExpression
    {
        public SqlTableCondition(string table, string field)
        {
            TableName = table;
            FieldName = field;
        }

        public string TableName { get; set; }
        public string FieldName { get; set; }

        public override string Expression
        {
            get
            {
                return string.Format("{0}.{1}",TableName, FieldName);
            }
        }

        public override string ToString()
        {
            return Expression;
        }
    }

    #endregion
}

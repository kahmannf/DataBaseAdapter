using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseAdapter;
using System.Data.Common;
using System.Data;
using DataBaseAdapter.Classes;

namespace TestSuit
{
    class Program
    {
        static void Main(string[] args)
        {
            DataBaseConfiguration.SetDataBase(DataBaseConfiguration.SupportedDataBases.MySql, "vahlkamp.selfhost.eu", 10010, "recipemanager", "recipemanager", "mysql");

            SqlWhereStatement whereStatemnt = new SqlWhereStatement();

            SqlCondition condition = new SqlCondition(new SqlTableCondition("recipes", "guid"), new SqlTableCondition("test", "guid_recipes"), SqlConditionComparer.Equals);

            whereStatemnt.AddStatements(condition);

            //DataTable table = SqlExecutor.ExcecuteReader("select * from recipes, test");

            TableReader<TestObject> test = new TableReader<TestObject>(whereStatemnt);

            List<TestObject> result = test.GetItems();
            
        }
    }
}

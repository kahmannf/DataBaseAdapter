using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseAdapter
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class AttributeTable : Attribute
    {
        public AttributeTable(string tableName)
        {
        }

        /// <summary>
        /// This descrips the field which will be in the select part (only neccessary for mutliply fileds with the same name)
        /// </summary>
        public string MaskedValueField { get; set; }
    }
}

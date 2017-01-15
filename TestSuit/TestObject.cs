using DataBaseAdapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSuit
{
    public class TestObject
    {
        [AttributeTable("recipes")]
        public string Name { get; set; }

        [AttributeTable("test", MaskedValueField = "amount")]
        public int Amount { get; set; }
    }
}

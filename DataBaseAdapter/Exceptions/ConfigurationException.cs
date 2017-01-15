using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseAdapter.Exceptions
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException()
            : base("A Configuration was missing or not correct.")
        {
        }

        public ConfigurationException(string message)
            :base(message)
        {
        }


        protected ConfigurationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

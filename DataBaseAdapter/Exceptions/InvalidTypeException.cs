using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseAdapter.Exceptions
{

    [Serializable]
    public class InvalidTypeException : Exception
    {
        public InvalidTypeException() : base("The given type was Invalid for this Operation.") { }
        public InvalidTypeException(string message) : base(message) { }
        protected InvalidTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

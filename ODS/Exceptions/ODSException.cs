using System;
using System.Collections.Generic;
using System.Text;

namespace ODS.Exceptions
{
    class ODSException : Exception
    {
        public ODSException() { }

        public ODSException(string message) : base(message) { }

        public ODSException(string message, Exception inner) : base(message, inner) { }
    }
}

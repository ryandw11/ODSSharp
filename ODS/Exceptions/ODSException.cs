using System;
namespace ODS.Exceptions
{
    /**
     * <summary>The standard ODS exception.</summary>
     */
    public class ODSException : Exception
    {
        public ODSException() { }

        public ODSException(string message) : base(message) { }

        public ODSException(string message, Exception inner) : base(message, inner) { }
    }
}

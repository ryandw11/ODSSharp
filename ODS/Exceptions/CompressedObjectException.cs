using System;

namespace ODS.Exceptions
{
    /// <summary>
    /// This exception is thrown when you try and traverse a compressed object using get(), find(), set(), etc.
    /// <para>To preven this exception, first obtain the compress object to decompress it, then get the tags inside.</para>
    /// </summary>
    public class CompressedObjectException : Exception
    {
        public CompressedObjectException() { }

        public CompressedObjectException(string message) : base(message) { }
    }
}

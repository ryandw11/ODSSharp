using System;
using System.Collections.Generic;
using System.Text;
using ODS.Stream;
using System.IO;
using ODS.Exceptions;

namespace ODS.Tags
{
    class InvalidTag : Tag<byte[]>
    {
        private string name;
        private byte[] value;

        public InvalidTag(string name, byte[] value)
        {
            this.name = name;
            this.value = value;
        }

        public void SetValue(byte[] s)
        {
            this.value = s;
        }

        public byte[] GetValue()
        {
            return value;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public string GetName()
        {
            return name;
        }

        public void WriteData(BigBinaryWriter dos)
        {
            throw new ODSException("Cannot write an InvalidTag.");
        }

        public Tag<byte[]> CreateFromData(byte[] value)
        {
            this.value = value;
            return this;
        }

        public byte GetID()
        {
            return 0;
        }
    }
}

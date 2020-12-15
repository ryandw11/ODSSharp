using System;
using System.Text;
using ODS.ODSStreams;
using System.IO;

namespace ODS.Tags
{
    public class CharTag : Tag<Char>
    {
        private string name;
        private char value;

        public CharTag(String name, char value)
        {
            this.name = name;
            this.value = value;
        }

        public void SetValue(Char s)
        {
            this.value = s;
        }

        public Char GetValue()
        {
            return value;
        }

        public void SetName(String name)
        {
            this.name = name;
        }

        public String GetName()
        {
            return name;
        }

        public void WriteData(BigBinaryWriter dos)
        {
            dos.Write(GetID());
            MemoryStream memStream = new MemoryStream();
            BigBinaryWriter writer = new BigBinaryWriter(memStream);
            writer.Write((short) Encoding.UTF8.GetByteCount(name));
            writer.Write(Encoding.UTF8.GetBytes(name));
            writer.Write(value);

            dos.Write((int)writer.BaseStream.Length);
            writer.Close();
            dos.Write(memStream.ToArray());
        }

        public Tag<Char> CreateFromData(byte[] value)
        {
            this.value = BitConverter.ToChar(value, 0);
            return this;
        }

        public byte GetID()
        {
            return 7;
        }
    }
}

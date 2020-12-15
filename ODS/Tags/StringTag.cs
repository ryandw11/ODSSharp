using System;
using System.Text;
using ODS.ODSStreams;
using System.IO;

namespace ODS.Tags
{
    public class StringTag : Tag<string>
    {
        private string name;
        private string value;

        public StringTag(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public void SetValue(string s)
        {
            this.value = s;
        }

        public string GetValue()
        {
            return value;
        }

        public void SetName(string name)
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
            writer.Write(Encoding.UTF8.GetBytes(value));

            dos.Write((int)writer.BaseStream.Length);
            writer.Close();
            dos.Write(memStream.ToArray());
        }

        public Tag<string> CreateFromData(byte[] value)
        {
            this.value = Encoding.UTF8.GetString(value);
            return this;
        }

        public byte GetID()
        {
            return 1;
        }
    }
}

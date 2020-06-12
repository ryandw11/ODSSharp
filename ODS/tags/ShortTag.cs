using System;
using System.Text;
using ODS.Stream;
using System.IO;

namespace ODS.tags
{
    public class ShortTag : Tag<short>
    {
        private string name;
        private short value;

        public ShortTag(String name, short value)
        {
            this.name = name;
            this.value = value;
        }

        public void SetValue(short s)
        {
            this.value = s;
        }

        public short GetValue()
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

        public Tag<short> CreateFromData(byte[] value)
        {
            this.value = ByteConverter.ToInt16(value);
            return this;
        }

        public byte GetID()
        {
            return 5;
        }
    }
}

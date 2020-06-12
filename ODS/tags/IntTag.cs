using System;
using System.Text;
using ODS.Stream;
using System.IO;

namespace ODS.tags
{
    public class IntTag : Tag<int>
    {
        private string name;
        private int value;

        public IntTag(String name, int value)
        {
            this.name = name;
            this.value = value;
        }

        public void SetValue(int s)
        {
            this.value = s;
        }

        public int GetValue()
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

        public Tag<int> CreateFromData(byte[] value)
        {
            this.value = ByteConverter.ToInt32(value);
            return this;
        }

        public byte GetID()
        {
            return 2;
        }
    }
}

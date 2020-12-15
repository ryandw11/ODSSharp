using System;
using System.Text;
using ODS.ODSStreams;
using System.IO;

namespace ODS.Tags
{
    public class LongTag : Tag<long>
    {
        private string name;
        private long value;

        public LongTag(String name, long value)
        {
            this.name = name;
            this.value = value;
        }

        public void SetValue(long s)
        {
            this.value = s;
        }

        public long GetValue()
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

        public Tag<long> CreateFromData(byte[] value)
        {
            this.value = ByteConverter.ToInt64(value);
            return this;
        }

        public byte GetID()
        {
            return 6;
        }
    }
}

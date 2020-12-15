using System;
using System.Text;
using ODS.ODSStreams;
using System.IO;

namespace ODS.Tags
{
    public class ByteTag : Tag<byte>
    {
        private string name;
        private byte value;

        public ByteTag(String name, byte value)
        {
            this.name = name;
            this.value = value;
        }

        public void SetValue(byte s)
        {
            this.value = s;
        }

        public byte GetValue()
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
            writer.Write((byte)value);

            dos.Write((int)writer.BaseStream.Length);
            writer.Close();
            dos.Write(memStream.ToArray());
        }

        public Tag<byte> CreateFromData(byte[] value)
        {
            this.value = value[0];
            return this;
        }

        public byte GetID()
        {
            return 8;
        }
    }
}

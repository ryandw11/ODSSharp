using System;
using System.Text;
using ODS.Stream;
using System.IO;

namespace ODS.Tags
{
    public class DoubleTag : Tag<Double>
    {
        private string name;
        private Double value;

        public DoubleTag(String name, Double value)
        {
            this.name = name;
            this.value = value;
        }

        public void SetValue(Double s)
        {
            this.value = s;
        }

        public Double GetValue()
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

        public Tag<Double> CreateFromData(byte[] value)
        {
            this.value = ByteConverter.ToDouble(value);
            return this;
        }

        public byte GetID()
        {
            return 4;
        }
    }
}

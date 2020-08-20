using System;
using System.Text;
using ODS.Stream;
using System.IO;

namespace ODS.Tags
{
    public class FloatTag : Tag<float>
    {
        private string name;
        private float value;

        public FloatTag(String name, float value)
        {
            this.name = name;
            this.value = value;
        }

        public void SetValue(float s)
        {
            this.value = s;
        }

        public float GetValue()
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

        public Tag<float> CreateFromData(byte[] value)
        {
            this.value = ByteConverter.ToFloat(value);
            return this;
        }

        public byte GetID()
        {
            return 3;
        }
    }
}

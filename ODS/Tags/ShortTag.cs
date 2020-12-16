using System;
using System.Text;
using ODS.ODSStreams;
using System.IO;

namespace ODS.Tags
{
    /**
     * <summary>A tag that contains a short.</summary>
     */
    public class ShortTag : Tag<short>
    {
        private string name;
        private short value;

        /**
         * <summary>Construct a short tag.</summary>
         * <param name="name">The name of the tag.</param>
         * <param name="value">The value of the tag.</param>
         */
        public ShortTag(string name, short value)
        {
            this.name = name;
            this.value = value;
        }

        /**
         * <inheritdoc/>
         */
        public void SetValue(short s)
        {
            this.value = s;
        }

        /**
         * <inheritdoc/>
         */
        public short GetValue()
        {
            return value;
        }

        /**
         * <inheritdoc/>
         */
        public void SetName(string name)
        {
            this.name = name;
        }

        /**
         * <inheritdoc/>
         */
        public string GetName()
        {
            return name;
        }

        /**
         * <inheritdoc/>
         */
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

        /**
         * <inheritdoc/>
         */
        public Tag<short> CreateFromData(byte[] value)
        {
            this.value = ByteConverter.ToInt16(value);
            return this;
        }

        /**
         * <inheritdoc/>
         */
        public byte GetID()
        {
            return 5;
        }
    }
}

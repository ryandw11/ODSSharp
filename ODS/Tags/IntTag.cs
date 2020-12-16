using System;
using System.Text;
using ODS.ODSStreams;
using System.IO;

namespace ODS.Tags
{
    /**
     * <summary>A tag that contains an integer.</summary>
     */
    public class IntTag : Tag<int>
    {
        private string name;
        private int value;

        /**
         * <summary>Construct an integer tag.</summary>
         * <param name="name">The name of the tag.</param>
         * <param name="value">The value of the tag.</param>
         */
        public IntTag(string name, int value)
        {
            this.name = name;
            this.value = value;
        }

        /**
         * <inheritdoc/>
         */
        public void SetValue(int s)
        {
            this.value = s;
        }

        /**
         * <inheritdoc/>
         */
        public int GetValue()
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
        public Tag<int> CreateFromData(byte[] value)
        {
            this.value = ByteConverter.ToInt32(value);
            return this;
        }

        /**
         * <inheritdoc/>
         */
        public byte GetID()
        {
            return 2;
        }
    }
}

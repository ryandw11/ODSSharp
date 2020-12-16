using System;
using System.Text;
using ODS.ODSStreams;
using System.IO;

namespace ODS.Tags
{
    /**
     * <summary>A tag that contains a string.</summary>
     */
    public class StringTag : Tag<string>
    {
        private string name;
        private string value;

        /**
         * <summary>Construct a string tag.</summary>
         * <param name="name">The name of the tag.</param>
         * <param name="value">The value of the tag.</param>
         */
        public StringTag(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        /**
         * <inheritdoc/>
         */
        public void SetValue(string s)
        {
            this.value = s;
        }

        /**
         * <inheritdoc/>
         */
        public string GetValue()
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
            writer.Write(Encoding.UTF8.GetBytes(value));

            dos.Write((int)writer.BaseStream.Length);
            writer.Close();
            dos.Write(memStream.ToArray());
        }

        /**
         * <inheritdoc/>
         */
        public Tag<string> CreateFromData(byte[] value)
        {
            this.value = Encoding.UTF8.GetString(value);
            return this;
        }

        /**
         * <inheritdoc/>
         */
        public byte GetID()
        {
            return 1;
        }
    }
}

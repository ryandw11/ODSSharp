using System;
using System.Text;
using ODS.ODSStreams;
using System.IO;

namespace ODS.Tags
{
    /**
     * <summary>This tag stores byte data.</summary>
     * <remarks>See <see cref="Tag{T}"/> for the API comments.</remarks>
     */
    public class ByteTag : Tag<byte>
    {
        private string name;
        private byte value;

        /**
         * <summary>Construct a byte tag.</summary>
         * <param name="name">The name of the tag.</param>
         * <param name="value">The value of the tag.</param>
         */
        public ByteTag(string name, byte value)
        {
            this.name = name;
            this.value = value;
        }

        /**
         * <inheritdoc/>
         */
        public void SetValue(byte s)
        {
            this.value = s;
        }

        /**
         * <inheritdoc/>
         */
        public byte GetValue()
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
            writer.Write((byte)value);

            dos.Write((int)writer.BaseStream.Length);
            writer.Close();
            dos.Write(memStream.ToArray());
        }

        /**
         * <inheritdoc/>
         */
        public Tag<byte> CreateFromData(byte[] value)
        {
            this.value = value[0];
            return this;
        }

        /**
         * <inheritdoc/>
         */
        public byte GetID()
        {
            return 8;
        }
    }
}

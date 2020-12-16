using System;
using System.Collections.Generic;
using System.Text;
using ODS.ODSStreams;
using System.IO;
using ODS.Exceptions;

namespace ODS.Tags
{
    /**
     * <summary>An InvalidTag represents a custom tag that ODS does not recognize.</summary>
     * <remarks>This should only be constructed by ODS internally. This class has no other use.</remarks>
     */
    class InvalidTag : Tag<byte[]>
    {
        private string name;
        private byte[] value;

        /**
         * <summary>Construct an invalid tag.</summary>
         * <remarks>This is for internal use only.</remarks>
         * <param name="name">The name of the tag.</param>
         * <param name="value">The value of the tag.</param>
         */
        public InvalidTag(string name, byte[] value)
        {
            this.name = name;
            this.value = value;
        }

        /**
         * <inheritdoc/>
         */
        public void SetValue(byte[] s)
        {
            this.value = s;
        }

        /**
         * <inheritdoc/>
         */
        public byte[] GetValue()
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
            throw new ODSException("Cannot write an InvalidTag.");
        }

        /**
         * <inheritdoc/>
         */
        public Tag<byte[]> CreateFromData(byte[] value)
        {
            this.value = value;
            return this;
        }

        /**
         * <inheritdoc/>
         */
        public byte GetID()
        {
            return 0;
        }
    }
}

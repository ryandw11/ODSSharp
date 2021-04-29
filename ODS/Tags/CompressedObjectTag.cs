using System;
using System.Text;
using ODS.ODSStreams;
using System.IO;
using System.Collections.Generic;

using ODS.Internal;
using ODS.Compression;
using ODS.Exceptions;

namespace ODS.Tags
{
    /// <summary>
    /// Any tags within the CompressedObject will be compressed using the specified compressor when being written to a file or memeory.
    /// <para>Tags within a compressed object cannot be obtained using <see cref="ObjectDataStructure.Get(string)"/> or similar methods.
    /// Attempting to will result in a <see cref="CompressedObjectException"/>.</para>
    /// </summary>
    public class CompressedObjectTag : Tag<List<ITag>>
    {
        private string name;
        private List<ITag> value;
        private ICompressor compressor;

        /**
         * <summary>Construct a compressed object tag.</summary>
         * <param name="name">The name of the tag.</param>
         * <param name="value">The list of Tags that the object contains.</param>
         * <param name="compressor">The compressor to use.</param>
         */
        public CompressedObjectTag(string name, List<ITag> value, ICompressor compressor)
        {
            this.name = name;
            this.value = value;
            this.compressor = compressor;
        }

        /**
         * <summary>Construct a compressed object tag with no existing tags.</summary>
         * <param name="name">The name of the tag.</param>
         * <param name="compressor">The compressor to use.</param>
         */
        public CompressedObjectTag(string name, ICompressor compressor)
        {
            this.name = name;
            this.value = new List<ITag>();
            this.compressor = compressor;
        }

        /**
         * <summary>
         *  <para>Construct a compressed object tag with no existing tags.</para>
         *  <para>GZIP Compression is used by default.</para>
         * </summary>
         * <param name="name">The name of the tag.</param>
         */
        public CompressedObjectTag(string name) : this(name, new GZIPCompression())
        {
        }

        /**
         * <inheritdoc/>
         */
        public void SetValue(List<ITag> s)
        {
            this.value = s;
        }

        /**
         * <inheritdoc/>
         */
        public List<ITag> GetValue()
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
         * <summary>Add a tag to the object.</summary>
         * <param name="tag">The tag to add.</param>
         */
        public void AddTag(ITag tag)
        {
            value.Add(tag);
        }

        /**
         * <summary>Get the tag at a specified index.</summary>
         * <param name="i">The index of the desired tag.</param>
         * <returns>The tag at a specified index.</returns>
         */
        public ITag GetTag(int i)
        {
            return value[i];
        }

        /**
         * <summary>Get a tag by name.</summary>
         * <param name="name">The name of the desired tag.</param>
         * <returns>A tag with a specific name. (Null if not found)</returns>
         */
        public ITag GetTag(string name)
        {
            return value.Find(tag => tag.GetName() == name);
        }

        /**
         * <summary>Remove a tag from the object.</summary>
         * <param name="tag">The tag to remove.</param>
         */
        public void RemoveTag(ITag tag)
        {
            value.Remove(tag);
        }

        /**
         * <summary>Remove a tag at a specified index.</summary>
         * <param name="tag">The index of the tag to remove.</param>
         */
        public void RemoveTag(int tag)
        {
            value.RemoveAt(tag);
        }

        /**
         * <summary>Remove all tags from the object.</summary>
         */
        public void RemoveAllTags()
        {
            value.Clear();
        }

        /**
         * <summary>Check to see if the object contains a tag with a certain name.</summary>
         * <param name="name">The name to check.</param>
         * <returns>If the object a tag with the desired name.</returns>
         */
        public bool HasTag(string name)
        {
            return value.Find(tag => tag.GetName() == name) != null;
        }

        /**
         * <summary>Get the compressor used.</summary>
         * <returns>The compressor used.</returns>
         */
        public ICompressor getCompressor()
        {
            return compressor;
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

            string compressorName = ODSUtil.GetCompressorName(compressor);
            if (compressorName == null)
                throw new ODSException("Unable to find compressor: " + compressor);
            // Write the data for the compressor into the temp writer.
            writer.Write((short)Encoding.UTF8.GetByteCount(compressorName));
            writer.Write(Encoding.UTF8.GetBytes(compressorName));

            // Temporary Compression Stream
            MemoryStream memStreamTemp = new MemoryStream();
            Stream compressedStream = compressor.GetCompressStream(memStreamTemp);
            BigBinaryWriter writerTemp = new BigBinaryWriter(compressedStream);

            foreach (ITag tag in value)
            {
                tag.WriteData(writerTemp);
            }

            writerTemp.Close();
            writer.Write(memStreamTemp.ToArray());

            dos.Write((int)writer.BaseStream.Length);
            writer.Close();
            dos.Write(memStream.ToArray());
        }

        /**
         * <inheritdoc/>
         */
        public Tag<List<ITag>> CreateFromData(byte[] value)
        {
            int length = value.Length;
            ICompressor compressor;
            byte[] bitData;
            using (BigBinaryReader bbr = new BigBinaryReader(new MemoryStream(value)))
            {
                short compressorLength = bbr.ReadInt16();

                length -= 2 + compressorLength;

                byte[] compressorBytes = new byte[compressorLength];
                bbr.Read(compressorBytes, 0, compressorLength);
                string compressionName = Encoding.UTF8.GetString(compressorBytes);
                compressor = ODSUtil.GetCompressor(compressionName);
                if (compressor == null)
                    throw new ODSException("Cannot find compressor: " + compressionName);
                bitData = bbr.ReadBytes((int) (bbr.BaseStream.Length - bbr.BaseStream.Position));
            }

            this.compressor = compressor;

            byte[] decompressedData;
            using(MemoryStream memStream = new MemoryStream())
            {
                using (Stream compressedStream = compressor.GetDecompressStream(new MemoryStream(bitData)))
                {
                    compressedStream.CopyTo(memStream);
                }
                decompressedData = memStream.ToArray();
            }
            
            this.value = InternalUtils.GetListData<ITag>(decompressedData);
            return this;
        }

        /**
         * <inheritdoc/>
         */
        public byte GetID()
        {
            return 12;
        }
    }
}

using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using ODS.Stream;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ODS
{
    /**
     * <summary>Primary class of the ObjectDataStructure library.</summary>
     */
    public class ObjectDataStructure
    {
        private FileInfo file;
        private Compression compression;

        /**
         * <summary>Construct the primary class with a file and compression type.</summary>
         * <param name="file">The file to use.</param>
         * <param name="compression">The compression that should be used.</param>
         */
        public ObjectDataStructure(FileInfo file, Compression compression)
        {
            this.file = file;
            this.compression = compression;
        }

        /**
         * <summary>Construct the primary class with a file. The default compression type is GZIP.</summary>
         * <param name="file">The file to use.</param>
         */
        public ObjectDataStructure(FileInfo file)
        {
            this.file = file;
            this.compression = Compression.GZIP;
        }

        private byte[] compress(byte[] data)
        {
            if(compression == Compression.GZIP)
            {
                MemoryStream stream = new MemoryStream();
                GZipStream zipStream = new GZipStream(stream, CompressionMode.Compress);
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return stream.ToArray();
            }
            else if(compression == Compression.ZLIB)
            {
                MemoryStream stream = new MemoryStream();
                DeflaterOutputStream zipStream = new DeflaterOutputStream(stream);
                zipStream.Write(data, 0, data.Length);
                zipStream.Close();
                return stream.ToArray();
            }
            return data;
        }

        private byte[] decompress(byte[] data)
        {
            if (compression == Compression.GZIP)
            {
                MemoryStream stream = new MemoryStream(data);
                GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress);
                MemoryStream output = new MemoryStream();
                zipStream.CopyTo(output);
                zipStream.Close();
                return output.ToArray();
            }
            else if (compression == Compression.ZLIB)
            {
                MemoryStream stream = new MemoryStream(data);
                InflaterInputStream zipStream = new InflaterInputStream(stream);
                MemoryStream output = new MemoryStream();
                zipStream.CopyTo(output);
                zipStream.Close();
                return output.ToArray();
            }
            return data;
        }

        /**
         * <summary>Grab a tag based upon the name. This will not work with object notation, use #GetObject(string)
         * instead.</summary>
         * <param name="name">The name of the tag to get.</param>
         * <returns>The tag. This will return null if no tag with the specified name is found or the file does not exist.</returns>
         */
        public ITag Get(string name)
        {
            if (!file.Exists)
            {
                return null;
            }
            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            MemoryStream compressed = new MemoryStream();
            fs.CopyTo(compressed);
            byte[] uncompressed = decompress(compressed.ToArray());
            MemoryStream memStream = new MemoryStream();
            memStream.Write(uncompressed, 0, uncompressed.Length);
            compressed.Close();

            ITag outf = getSubData(memStream, name);
            fs.Close();
            memStream.Close();
            return outf;
        }

        /**
         * <summary>Grab a tag based upon an object key. This method allows you to directly get sub-objects with little overhead.</summary>
         * <example>
         *      GetObject("primary.firstsub.secondsub");
         * </example>
         * <param name="key">They key to search for.</param>
         * <returns>The tag. This will return null if no tag with the specified key path is found or the file does not exist.</returns>
         */
        public ITag GetObject(string key)
        {
            if (!file.Exists)
            {
                return null;
            }
            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            MemoryStream compressed = new MemoryStream();
            fs.CopyTo(compressed);
            byte[] uncompressed = decompress(compressed.ToArray());

            ITag outf = getSubObjectData(uncompressed, key);
            fs.Close();
            return outf;
        }

        /**
         * <summary>Get all of the tags in the file.</summary>
         * <returns>All of the tags. This returns null if the file does not exist.</returns>
         */
        public List<ITag> GetAll()
        {
            if (!file.Exists)
            {
                return null;
            }
            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            MemoryStream compressed = new MemoryStream();
            fs.CopyTo(compressed);
            byte[] uncompressed = decompress(compressed.ToArray());
            compressed.Close();

            List<ITag> outf = GetListData<ITag>(uncompressed);
            fs.Close();
            return outf;
        }

        /**
         * <summary>Save tags to the file. This method will create a new file if one does not exist.
         * This will overwrite the existing file. To append tags
         * see #Append(ITag) and #AppendAll(List)</summary>
         * <param name="tags">The list of tags to save.</param>
         */
        public void Save(List<ITag> tags)
        {
            FileStream fs = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write);
            MemoryStream ms = new MemoryStream();
            BigBinaryWriter bw = new BigBinaryWriter(ms);
            foreach (ITag tag in tags)
            {
                tag.WriteData(bw);
            }
            bw.Close();
            byte[] data = compress(ms.ToArray());
            fs.Write(data, 0, data.Length);
            ms.Close();
            fs.Close();
        }

        /**
         * <summary>Append tags to the end of the file.</summary>
         * <param name="tag">The tag to be appended.</param>
         */
        public void Append(ITag tag)
        {
            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Write);
            MemoryStream compressed = new MemoryStream();
            fs.CopyTo(compressed);
            MemoryStream ms = new MemoryStream();
            byte[] decompressed = decompress(compressed.ToArray());
            ms.Write(decompressed, 0, decompressed.Length);
            BigBinaryWriter bw = new BigBinaryWriter(ms);
            fs.Close();

            ms.Seek(0, SeekOrigin.End);
            tag.WriteData(bw);
            bw.Close();

            FileStream appendFile = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write);
            byte[] data = compress(ms.ToArray());
            appendFile.SetLength(0);
            appendFile.Write(data, 0, data.Length);
            ms.Close();
            appendFile.Close();
        }

        /**
         * <summary>Append a list of tags to the end of the file.</summary>
         * <param name="tags">The list of tags.</param>
         */
        public void AppendAll(List<ITag> tags)
        {
            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Write);
            MemoryStream compressed = new MemoryStream();
            fs.CopyTo(compressed);
            MemoryStream ms = new MemoryStream();
            byte[] decompressed = decompress(compressed.ToArray());
            ms.Write(decompressed, 0, decompressed.Length);
            BigBinaryWriter bw = new BigBinaryWriter(ms);
            fs.Close();

            ms.Seek(0, SeekOrigin.End);
            foreach (ITag tag in tags)
            {
                tag.WriteData(bw);
            }
            bw.Close();

            FileStream appendFile = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write);
            byte[] data = compress(ms.ToArray());
            appendFile.SetLength(0);
            appendFile.Write(data, 0, data.Length);
            ms.Close();
            appendFile.Close();
        }

        /**
         * <summary>Find if a key exists within a file.</summary>
         * <param name="key">The key to find.</param>
         * <returns>If the key exists.</returns>
         */
        public bool Find(string key)
        {
            if (!file.Exists)
            {
                return false;
            }
            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            MemoryStream compressed = new MemoryStream();
            fs.CopyTo(compressed);
            byte[] uncompressed = decompress(compressed.ToArray()); 
            fs.Close();
            return findSubObjectData(uncompressed, key); ;
        }

        /**
         * <summary>Remove a tag from the list. **This will be updated in the future to support deleting object keys.**</summary>
         * <param name="name">The tag name to remove</param>
         * <returns>The index of where the data was deleted. This will return -1 if the file does not exist or if
         * the requested key cannot be found.</returns>
         */
        public int Delete(string name)
        {
            if (!file.Exists)
            {
                return -1;
            }
            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            MemoryStream compressed = new MemoryStream();
            fs.CopyTo(compressed);
            byte[] uncompressed = decompress(compressed.ToArray());
            fs.Close();

            MemoryStream ms = new MemoryStream();
            BigBinaryWriter bw = new BigBinaryWriter(ms);

            Tuple<int, byte[]> outf = deleteSubObjectData(uncompressed, name, bw);
            byte[] recompressed = compress(outf.Item2);

            FileStream newFile = new FileStream(file.FullName, FileMode.Open, FileAccess.Write, FileShare.Write);
            newFile.SetLength(0);
            newFile.Write(recompressed, 0, recompressed.Length);
            newFile.Close();
            return outf.Item1;
        }

        /**
         * <summary>Replace data with other data. Unlike the Java version, this method works with all compression types.</summary>
         * <param name="key">The key</param>
         * <param name="replacement">The data to replace the key</param>
         * <returns>If the replacement was successful. (Will return false if the file does not exist or the key cannot be found).</returns>
         */
        public bool ReplaceData(string key, ITag replacement)
        {
            if (!file.Exists)
            {
                return false;
            }
            int index = Delete(key);
            if (index == -1) return false;

            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            MemoryStream compressed = new MemoryStream();
            fs.CopyTo(compressed);
            byte[] uncompressed = decompress(compressed.ToArray());
            fs.Close();

            MemoryStream memStream = new MemoryStream();
            memStream.Write(uncompressed, 0, uncompressed.Length);
            BigBinaryWriter bbw = new BigBinaryWriter(memStream);
            bbw.Seek(index, SeekOrigin.Begin);
            replacement.WriteData(bbw);
            bbw.Close();

            FileStream newFile = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write);
            newFile.SetLength(0);
            byte[] recompressed = compress(memStream.ToArray());
            newFile.Write(recompressed, 0, recompressed.Length);

            newFile.Close();
            return true;
        }

        /**
         * Used to get a tag from the file.
         */
        private ITag getSubData(MemoryStream stream, string name)
        {
            BigBinaryReader binReader = new BigBinaryReader(stream);
            TagBuilder currentBuilder = new TagBuilder();

            binReader.BaseStream.Seek(0, SeekOrigin.Begin);

            while (binReader.BaseStream.Position != binReader.BaseStream.Length)
            {
                currentBuilder.setDataType(binReader.ReadByte());
                currentBuilder.setDataSize(binReader.ReadInt32());
                //TODO see if this is correct.
                currentBuilder.setStartingIndex(binReader.BaseStream.Position);
                currentBuilder.setNameSize(binReader.ReadInt16());

                if (currentBuilder.getNameSize() != Encoding.UTF8.GetByteCount(name))
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }

                byte[] nameBytes = binReader.ReadBytes(currentBuilder.getNameSize());
                String tagName = Encoding.UTF8.GetString(nameBytes);
                currentBuilder.setName(tagName);
                if (tagName != name)
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }

                byte[] value = binReader.ReadBytes((int) (currentBuilder.getStartingIndex() - stream.Position) + (int) currentBuilder.getDataSize());
                currentBuilder.setValueBytes(value);
                binReader.Close();
                return currentBuilder.proccess();
            }
            binReader.Close();
            return null;
        }

        /**
         * Used to get a tag from the file using the key system.
         */
        private ITag getSubObjectData(byte[] data, string key)
        {
            MemoryStream stream = new MemoryStream(data);
            BigBinaryReader binReader = new BigBinaryReader(stream);
            TagBuilder currentBuilder = new TagBuilder();

            binReader.BaseStream.Seek(0, SeekOrigin.Begin);

            string name = key.Split('.')[0];
            string otherKey = getKey(key.Split('.'));


            while (binReader.BaseStream.Position != binReader.BaseStream.Length)
            {
                currentBuilder.setDataType(binReader.ReadByte());
                currentBuilder.setDataSize(binReader.ReadInt32());
                //TODO see if this is correct.
                currentBuilder.setStartingIndex(binReader.BaseStream.Position);
                currentBuilder.setNameSize(binReader.ReadInt16());

                if (currentBuilder.getNameSize() != Encoding.UTF8.GetByteCount(name))
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }

                byte[] nameBytes = binReader.ReadBytes(currentBuilder.getNameSize());
                String tagName = Encoding.UTF8.GetString(nameBytes);
                currentBuilder.setName(tagName);
                if (tagName != name)
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }

                byte[] value = binReader.ReadBytes((int)(currentBuilder.getStartingIndex() - stream.Position) + (int)currentBuilder.getDataSize());
                currentBuilder.setValueBytes(value);
                binReader.Close();

                if (otherKey != null)
                    return getSubObjectData(currentBuilder.getValueBytes(), otherKey);
                return currentBuilder.proccess();
            }
            binReader.Close();
            return null;
        }

        private string getKey(string[] s)
        {
            List<string> list = new List<string>(s);
            list.Remove(list[0]);
            if (list.Count == 1) return list[0];
            if (list.Count < 1) return null;

            return string.Join(".", list);
        }

        /**
         * This is used to find the tags using the key system.
         */
        private bool findSubObjectData(byte[] data, string key)
        {
            MemoryStream stream = new MemoryStream(data);
            BigBinaryReader binReader = new BigBinaryReader(stream);
            TagBuilder currentBuilder = new TagBuilder();

            binReader.BaseStream.Seek(0, SeekOrigin.Begin);

            string name = key.Split('.')[0];
            string otherKey = getKey(key.Split('.'));


            while (binReader.BaseStream.Position != binReader.BaseStream.Length)
            {
                currentBuilder.setDataType(binReader.ReadByte());
                currentBuilder.setDataSize(binReader.ReadInt32());
                //TODO see if this is correct.
                currentBuilder.setStartingIndex(binReader.BaseStream.Position);
                currentBuilder.setNameSize(binReader.ReadInt16());

                if (currentBuilder.getNameSize() != Encoding.UTF8.GetByteCount(name))
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }

                byte[] nameBytes = binReader.ReadBytes(currentBuilder.getNameSize());
                String tagName = Encoding.UTF8.GetString(nameBytes);
                currentBuilder.setName(tagName);
                if (tagName != name)
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }

                byte[] value = binReader.ReadBytes((int)(currentBuilder.getStartingIndex() - stream.Position) + (int)currentBuilder.getDataSize());
                currentBuilder.setValueBytes(value);
                binReader.Close();

                if (otherKey != null)
                    return findSubObjectData(currentBuilder.getValueBytes(), otherKey);
                return true;
            }
            binReader.Close();
            return false;
        }


        /**
         * This is used to delete tags from the list. This does not support key format.
         */
        private Tuple<int, byte[]> deleteSubObjectData(byte[] data, string key, BigBinaryWriter newFileWriter)
        {
            MemoryStream stream = new MemoryStream(data);
            BigBinaryReader binReader = new BigBinaryReader(stream);
            TagBuilder currentBuilder = new TagBuilder();

            binReader.BaseStream.Seek(0, SeekOrigin.Begin);

            string name = key.Split('.')[0];
            string otherKey = getKey(key.Split('.'));


            while (binReader.BaseStream.Position != binReader.BaseStream.Length)
            {
                bool found = true;
                int start = (int)newFileWriter.BaseStream.Position;

                currentBuilder.setDataType(binReader.ReadByte());
                currentBuilder.setDataSize(binReader.ReadInt32());
                //TODO see if this is correct.
                currentBuilder.setStartingIndex(binReader.BaseStream.Position);
                currentBuilder.setNameSize(binReader.ReadInt16());

                if (currentBuilder.getNameSize() != Encoding.UTF8.GetByteCount(name))
                {
                    found = false;
                }

                byte[] nameBytes = binReader.ReadBytes(currentBuilder.getNameSize());
                String tagName = Encoding.UTF8.GetString(nameBytes);
                currentBuilder.setName(tagName);
                if (tagName != name)
                {
                    found = false;
                }

                byte[] value = binReader.ReadBytes((int)(currentBuilder.getStartingIndex() - stream.Position) + (int)currentBuilder.getDataSize());
                currentBuilder.setValueBytes(value);

                if (!found)
                {
                    addData(newFileWriter, currentBuilder);
                    currentBuilder = new TagBuilder();
                    continue;
                }

                binReader.Close();

                if (otherKey != null) { 
                    return deleteSubObjectData(currentBuilder.getValueBytes(), otherKey, newFileWriter);
                }
                Tuple<int, byte[]> finalData = new Tuple<int, byte[]>(start, ((MemoryStream) newFileWriter.BaseStream).ToArray());
                return finalData;
            }
            binReader.Close();
            return new Tuple<int, byte[]>(-1, new byte[0]);
        }

        private void addData(BigBinaryWriter writer, TagBuilder builder)
        {
            writer.Write((byte)builder.getDataType());
            writer.Write((int)builder.getDataSize());
            writer.Write((short)builder.getNameSize());
            writer.Write(Encoding.UTF8.GetBytes(builder.getName()));
            writer.Write(builder.getValueBytes());
        }

        /**
         * <summary>Get a list of Tags from an array of bytes. This is meant for internal use only.</summary>
         * <param name="data">The array of bytes.</param>
         */
        public static List<T> GetListData<T>(byte[] data) where T : ITag
        {
            MemoryStream stream = new MemoryStream(data);
            BigBinaryReader binReader = new BigBinaryReader(stream);
            TagBuilder currentBuilder = new TagBuilder();

            binReader.BaseStream.Seek(0, SeekOrigin.Begin);
            List<T> output = new List<T>();
            while (binReader.BaseStream.Position != binReader.BaseStream.Length)
            {
                currentBuilder.setDataType(binReader.ReadByte());
                currentBuilder.setDataSize(binReader.ReadInt32());
                //TODO see if this is correct.
                currentBuilder.setStartingIndex(binReader.BaseStream.Position);
                currentBuilder.setNameSize(binReader.ReadInt16());

                byte[] nameBytes = binReader.ReadBytes(currentBuilder.getNameSize());
                String tagName = Encoding.UTF8.GetString(nameBytes);
                currentBuilder.setName(tagName);

                byte[] value = binReader.ReadBytes((int)(currentBuilder.getStartingIndex() - stream.Position) + (int)currentBuilder.getDataSize());
                currentBuilder.setValueBytes(value);
                output.Add((T) currentBuilder.proccess());
            }
            binReader.Close();
            return output;
        }
    }
}

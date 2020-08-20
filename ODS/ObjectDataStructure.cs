using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using ODS.Stream;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ODS.Util;
using System.Diagnostics.Tracing;
using ODS.Exceptions;
using System.Collections.Specialized;
using ODS.Tags;

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
         * <summary>Grab a tag based upon an object key. This method allows you to directly get sub-objects with little overhead.</summary>
         * <example>
         *      GetObject("primary.firstsub.secondsub");
         * </example>
         * <param name="key">They key to search for.</param>
         * <returns>The tag. This will return null if no tag with the specified key path is found or the file does not exist.</returns>
         */
        public ITag Get(string key)
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

        // TODO stuff




        /**
         * <summary>Remove a tag from the list.</summary>
         * 
         * <param name="key">The key to remove.</param>
         * <returns>If the deletion was successfully done.</returns>
         */
        public bool Delete(string key)
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

            KeyScout counter = ScoutObjectData(uncompressed, key, new KeyScout());
            if (counter == null)
            {
                return false;
            }
            byte[] recompressed = compress(deleteSubObjectData(uncompressed, counter));

            FileStream newFile = new FileStream(file.FullName, FileMode.Open, FileAccess.Write, FileShare.Write);
            newFile.SetLength(0);
            newFile.Write(recompressed, 0, recompressed.Length);
            newFile.Close();
            return true;
        }

        /**
         * <summary>Replace a key with another tag.</summary>
         * <param name="key">The key</param>
         * <param name="replacement">The data to replace the key</param>
         * <returns>If the replacement was successful.</returns>
         */
        public bool ReplaceData(string key, ITag replacement)
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

            KeyScout counter = ScoutObjectData(uncompressed, key, new KeyScout());
            if (counter.GetEnd() == null)
            {
                return false;
            }

            MemoryStream memStream = new MemoryStream();
            BigBinaryWriter bbw = new BigBinaryWriter(memStream);
            replacement.WriteData(bbw);
            bbw.Close();

            byte[] replaceReturn = ReplaceSubObjectData(uncompressed, counter, memStream.ToArray());

            FileStream newFile = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write);
            newFile.SetLength(0);
            byte[] recompressed = compress(replaceReturn);
            newFile.Write(recompressed, 0, recompressed.Length);

            newFile.Close();
            return true;
        }

        /**
         * <summary>
         * This method can append, delete, and set tags.
         * <para>A note on keys when appending: <code>ObjectOne.ObjectTwo.tagName</code> When appending data <c>tagName</c> will not be the actual tag name.
         * The tag name written to the file is the name of the specified tag in the value parameter. Any parent objects that do not exist will be created. For example:
         * <code>ObjectOne.ObjectTwo.NewObject.tagName</code> If in the example above <c>NewObject</c> does not exist, than the object will be created with the value tag inside
         * of it. Please see the wiki for a more detailed explanation on this.</para>
         * <para>When value is null, the specified key is deleted. <c>The key MUST exist or an {@link ODSException} will be thrown.</c></para>
         * </summary>
         * 
         * <param name="key">
         * The key of the tag to append, delete, or set.
         * <para>When appending the key does not need to exist. ObjectTags that don't exist will be created automatically.</para>
         * <para>When the key is set to "" (An empty string) than it is assumed you want to append to the parent file.</para>
         * <para>Valid Tags:</para>
         *  <code>
         *  <para>ObjectOne.tagToDelete</para>
         *  <para>ObjectOne.NewObject.tagToAppend</para>
         *  <para>ObjectOne.tagToSet</para>
         *  </code>
         * </param>
         * 
         * <param name="value">The tag to append or replace the key with. <para>If this parameter is null than the key will be deleted.</para></param>
         * 
         */
        public void Set(string key, ITag value)
        {
            if(value == null)
            {
                bool output = Delete(key);
                if (!output)
                    throw new ODSException("The key " + key + " does not exist!");
                return;
            }
            if(key == "")
            {
                Save(new List<ITag>(){ value });
                return;
            }
            FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            MemoryStream compressed = new MemoryStream();
            fs.CopyTo(compressed);
            byte[] uncompressed = decompress(compressed.ToArray());
            fs.Close();

            KeyScout counter = ScoutObjectData(uncompressed, key, new KeyScout());
            if (counter.GetEnd() == null)
            {
                if(counter.GetChildren().Count < 1)
                {
                    Append(value);
                    return;
                }
                string existingKey = "";
                foreach (KeyScoutChild child in counter.GetChildren())
                {
                    if (existingKey.Length != 0)
                        existingKey += ".";
                    existingKey += child.GetName();
                }
                string newKey = key.Replace(existingKey + ".", "");
                ITag currentData;
                if(newKey.Split('.').Length > 1)
                {
                    ObjectTag output = null;
                    ObjectTag curTag = null;
                    List<string> keys = new List<string>(newKey.Split('.'));
                    int i = 0;
                    foreach (string s in keys){
                        if(i == 0)
                        {
                            output = new ObjectTag(s);
                            curTag = output;
                        }else if(i == keys.Count - 1)
                        {
                            curTag.AddTag(value);
                        }
                        else
                        {
                            ObjectTag tag = new ObjectTag(s);
                            curTag.AddTag(tag);
                            curTag = tag;
                        }
                        i++;
                    }
                    currentData = output;
                }
                else
                {
                    currentData = value;
                }
                // Actually replace the data and write it to the file.
                MemoryStream memStream = new MemoryStream();
                BigBinaryWriter bbw = new BigBinaryWriter(memStream);
                currentData.WriteData(bbw);
                bbw.Close();
                byte[] outputArray = SetSubObjectData(uncompressed, counter, memStream.ToArray());

                FileStream newFile = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write);
                newFile.SetLength(0);
                byte[] recompressed = compress(outputArray);
                newFile.Write(recompressed, 0, recompressed.Length);
                newFile.Close();
            }
            else
            {
                MemoryStream memStream = new MemoryStream();
                BigBinaryWriter bbw = new BigBinaryWriter(memStream);
                value.WriteData(bbw);
                bbw.Close();

                byte[] replaceReturn = ReplaceSubObjectData(uncompressed, counter, memStream.ToArray());
                FileStream newFile = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write);
                newFile.SetLength(0);
                byte[] recompressed = compress(replaceReturn);
                newFile.Write(recompressed, 0, recompressed.Length);
                newFile.Close();
            }

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
        private byte[] deleteSubObjectData(byte[] data, KeyScout counter)
        {
            counter.RemoveAmount(counter.GetEnd().GetSize() + 5);

            KeyScoutChild end = counter.GetEnd();

            byte[] array1 = new byte[data.Length - (5 + end.GetSize())];
            Array.Copy(data, 0, array1, 0, (end.GetStartingIndex() - 1));
            Array.Copy(data, end.GetStartingIndex() + 4 + end.GetSize(),
                array1, end.GetStartingIndex() - 1,
                data.Length - (end.GetStartingIndex() + 4 + end.GetSize()));

            foreach (KeyScoutChild child in counter.GetChildren())
            {
                int index = child.GetStartingIndex();
                int size = child.GetSize();
                array1[index] = (byte)(size >> 24);
                array1[index + 1] = (byte)(size >> 16);
                array1[index + 2] = (byte)(size >> 8);
                array1[index + 3] = (byte) size;
            }

            return array1;
        }

        /**
         * <summary>
         * Replace a tag with another tag.
         * </summary>
         * <param name="data">The input array of bytes</param>
         * <param name="counter">The scout object</param>
         * <param name="dataToReplace">The bytes of the replacement data.</param>
         * <returns>The output bytes.</returns>
         */
        private byte[] ReplaceSubObjectData(byte[] data, KeyScout counter, byte[] dataToReplace)
        {
            counter.RemoveAmount(counter.GetEnd().GetSize() + 5);
            counter.AddAmount(dataToReplace.Length);

            KeyScoutChild end = counter.GetEnd();


            byte[] array1 = new byte[data.Length - (5 + end.GetSize()) + dataToReplace.Length];
            //Copy all of the information before the removed data.
            Array.Copy(data, 0, array1, 0, (end.GetStartingIndex() - 1));
            Array.Copy(dataToReplace, 0, array1, end.GetStartingIndex() - 1, dataToReplace.Length);
            // copy all of the information after the removed data.
            Array.Copy(data, end.GetStartingIndex() + 4 + end.GetSize(),
                    array1, end.GetStartingIndex() - 1 + dataToReplace.Length,
                    data.Length - (end.GetStartingIndex() + 4 + end.GetSize()));

            foreach (KeyScoutChild child in counter.GetChildren())
            {
                int index = child.GetStartingIndex();
                int size = child.GetSize();
                array1[index] = (byte)(size >> 24);
                array1[index + 1] = (byte)(size >> 16);
                array1[index + 2] = (byte)(size >> 8);
                array1[index + 3] = (byte)(size);
            }

            return array1;
        }

        private byte[] SetSubObjectData(byte[] data, KeyScout counter, byte[] dataToReplace)
        {
            KeyScoutChild child = counter.GetChildren()[counter.GetChildren().Count - 1];


            byte[] array1 = new byte[data.Length + dataToReplace.Length];
            Array.Copy(data, 0, array1, 0, child.GetStartingIndex() + 4 + child.GetSize());
            Array.Copy(dataToReplace, 0, array1, child.GetStartingIndex() + 4 + child.GetSize(), dataToReplace.Length);
            Array.Copy(data, (child.GetStartingIndex() + 4 + child.GetSize()), array1, (child.GetStartingIndex() + 4 + child.GetSize()) + dataToReplace.Length,
                    data.Length - (child.GetStartingIndex() + 4 + child.GetSize()));

            counter.AddAmount(dataToReplace.Length);

            foreach (KeyScoutChild childs in counter.GetChildren())
            {
                int index = childs.GetStartingIndex();
                int size = childs.GetSize();
                array1[index] = (byte)(size >> 24);
                array1[index + 1] = (byte)(size >> 16);
                array1[index + 2] = (byte)(size >> 8);
                array1[index + 3] = (byte)(size);
            }

            return array1;
        }

        /**
         * <summary>
         * This object goes through the data and scouts out the information from the given key.
         * This method is recursive, which is why the parameter offset exists.
         * </summary>
         * 
         * <param name="data">The input array of bytes</param>
         * <param name="key">The key</param>
         * <param name="counter">The Scout object</param>
         * <returns>The key scout.</returns>
         */
        private KeyScout ScoutObjectData(byte[] data, string key, KeyScout counter)
        {
            return ScoutObjectData(data, key, counter, 0);
        }

        /**
         * <summary>
         * This object goes through the data and scouts out the information from the given key.
         * This method is recursive, which is why the parameter offset exists.
         * </summary>
         * 
         * <param name="data">The input array of bytes</param>
         * <param name="key">The key</param>
         * <param name="counter">The Scout object</param>
         * <param name="startIndex">The starting index for the count.</param>
         * <returns>The key scout.</returns>
         */
        private KeyScout ScoutObjectData(byte[] data, string key, KeyScout counter, int startIndex)
        {
            MemoryStream stream = new MemoryStream(data);
            BigBinaryReader binReader = new BigBinaryReader(stream);
            TagBuilder currentBuilder = new TagBuilder();

            binReader.BaseStream.Seek(0, SeekOrigin.Begin);

            string name = key.Split('.')[0];
            string otherKey = getKey(key.Split('.'));
            while(binReader.BaseStream.Position != binReader.BaseStream.Length)
            {
                KeyScoutChild child = new KeyScoutChild();
                currentBuilder.setDataType(binReader.ReadByte());
                child.SetStartingIndex((int) binReader.BaseStream.Position + startIndex);
                currentBuilder.setDataSize(binReader.ReadInt32());
                currentBuilder.setStartingIndex(binReader.BaseStream.Position);
                currentBuilder.setNameSize(binReader.ReadInt16());

                if (currentBuilder.getNameSize() != Encoding.UTF8.GetByteCount(name))
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }

                byte[] nameBytes = binReader.ReadBytes(currentBuilder.getNameSize());
                string tagName = Encoding.UTF8.GetString(nameBytes);
                currentBuilder.setName(tagName);
                if (tagName != name)
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }
                int binPos = (int)binReader.BaseStream.Position;
                byte[] value = binReader.ReadBytes((int)(currentBuilder.getStartingIndex() - stream.Position) + (int)currentBuilder.getDataSize());
                currentBuilder.setValueBytes(value);
                
                binReader.Close();


                if (otherKey != null)
                {
                    child.SetSize(currentBuilder.getDataSize());
                    child.SetName(currentBuilder.getName());
                    counter.AddChild(child);
                    return ScoutObjectData(currentBuilder.getValueBytes(), otherKey, counter, binPos + startIndex);
                }
                child.SetSize(currentBuilder.getDataSize());
                child.SetName(currentBuilder.getName());
                counter.SetEnd(child);
                return counter;
            }
            return counter;
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

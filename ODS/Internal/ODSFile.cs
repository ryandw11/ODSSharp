using System;
using System.IO;
using System.Collections.Generic;
using ODS.ODSStreams;
using ODS.Util;
using ODS.Exceptions;
using ODS.Tags;
using ODS.Compression;

namespace ODS.Internal
{
    /**
     * <summary>
     * This class handles the file storage type for ODS.
     * <para>This class is for internal use only. Use <see cref="ObjectDataStructure"/> instead of this class.</para>
     * </summary>
     */
    public class ODSFile : ODSInternal
    {

        private FileInfo file;
        private ICompressor compression;

        /**
         * <summary>Construct the primary class with a file and compression type.</summary>
         * <param name="file">The file to use.</param>
         * <param name="compression">The compression that should be used.</param>
         */
        public ODSFile(FileInfo file, ICompressor compression)
        {
            this.file = file;
            this.compression = compression;
        }

        /**
         * <summary>Construct the primary class with a file. The default compression type is GZIP.</summary>
         * <param name="file">The file to use.</param>
         */
        public ODSFile(FileInfo file)
        {
            this.file = file;
            this.compression = new GZIPCompression();
        }

        private System.IO.Stream GetCompressStream(System.IO.Stream stream)
        {
            return compression.GetCompressStream(stream);
        }

        private System.IO.Stream GetDecompressStream(System.IO.Stream stream)
        {
            return compression.GetDecompressStream(stream);
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
            using(FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (MemoryStream readStream = new MemoryStream())
            {

                using (Stream stream = GetDecompressStream(fs))
                    stream.CopyTo(readStream);

                readStream.Position = 0;

                ITag outf = InternalUtils.GetSubObjectData(readStream, key);

                return outf;
            }
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
            using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (MemoryStream readStream = new MemoryStream())
            {

                using (Stream stream = GetDecompressStream(fs))
                    stream.CopyTo(readStream);

                readStream.Position = 0;

                List<ITag> outf = InternalUtils.GetListData<ITag>(readStream);

                return outf;
            }
        }

        /**
         * <summary>Save tags to the file. This method will create a new file if one does not exist.
         * This will overwrite the existing file. To append tags
         * see #Append(ITag) and #AppendAll(List)</summary>
         * <param name="tags">The list of tags to save.</param>
         */
        public void Save(List<ITag> tags)
        {
            using (FileStream fs = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (Stream stream = GetCompressStream(fs))
            using (BigBinaryWriter bw = new BigBinaryWriter(stream))
            {
                foreach (ITag tag in tags)
                {
                    tag.WriteData(bw);
                }
            }
        }

        /**
         * <summary>Append tags to the end of the file.</summary>
         * <param name="tag">The tag to be appended.</param>
         */
        public void Append(ITag tag)
        {
            using (MemoryStream decompressedData = new MemoryStream())
            {
                using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Write))
                {
                    using (Stream compressStream = GetDecompressStream(fs))
                        compressStream.CopyTo(decompressedData);
                }
                BigBinaryWriter bw = new BigBinaryWriter(decompressedData);
                bw.Seek(0, SeekOrigin.End);
                tag.WriteData(bw);
                using (FileStream fs = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (Stream stream = GetCompressStream(fs))
                    {
                        decompressedData.Position = 0;
                        decompressedData.CopyTo(stream);
                    }
                }
            }
        }

        /**
         * <summary>Append a list of tags to the end of the file.</summary>
         * <param name="tags">The list of tags.</param>
         */
        public void AppendAll(List<ITag> tags)
        {
            using (MemoryStream decompressedData = new MemoryStream())
            {
                using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Write))
                {
                    using (Stream compressStream = GetDecompressStream(fs))
                        compressStream.CopyTo(decompressedData);
                }
                BigBinaryWriter bw = new BigBinaryWriter(decompressedData);
                bw.Seek(0, SeekOrigin.End);
                foreach (ITag tag in tags)
                {
                    tag.WriteData(bw);
                }
                using (FileStream fs = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (Stream stream = GetCompressStream(fs))
                    {
                        decompressedData.Position = 0;
                        decompressedData.CopyTo(stream);
                    }
                }
            }
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
            using (MemoryStream readStream = new MemoryStream())
            {
                using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream stream = GetDecompressStream(fs))
                    stream.CopyTo(readStream);
                readStream.Position = 0;
                return InternalUtils.FindSubObjectData(readStream, key);
            }
        }


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
            using (MemoryStream decompressedData = new MemoryStream())
            {
                using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream stream = GetDecompressStream(fs))
                    stream.CopyTo(decompressedData);

                KeyScout counter = InternalUtils.ScoutObjectData(decompressedData.ToArray(), key, new KeyScout());
                if (counter == null)
                {
                    return false;
                }
                byte[] recompressed = InternalUtils.deleteSubObjectData(decompressedData.ToArray(), counter);

                using (FileStream newFile = new FileStream(file.FullName, FileMode.Open, FileAccess.Write, FileShare.Write))
                {
                    newFile.SetLength(0);
                    using (Stream compressStream = GetCompressStream(newFile))
                        compressStream.Write(recompressed, 0, recompressed.Length);
                }
            }

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
            using (MemoryStream decompressed = new MemoryStream())
            {
                using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (Stream compStream = GetDecompressStream(fs))
                    compStream.CopyTo(decompressed);

                KeyScout counter = InternalUtils.ScoutObjectData(decompressed.ToArray(), key, new KeyScout());
                if (counter.GetEnd() == null)
                {
                    return false;
                }

                MemoryStream memStream = new MemoryStream();
                BigBinaryWriter bbw = new BigBinaryWriter(memStream);
                replacement.WriteData(bbw);
                bbw.Close();

                byte[] replaceReturn = InternalUtils.ReplaceSubObjectData(decompressed.ToArray(), counter, memStream.ToArray());

                using (FileStream newFile = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    newFile.SetLength(0);
                    using (Stream stream = GetCompressStream(newFile))
                        stream.Write(replaceReturn, 0, replaceReturn.Length);
                }
            }
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
            if (value == null)
            {
                bool output = Delete(key);
                if (!output)
                    throw new ODSException("The key " + key + " does not exist!");
                return;
            }
            if (key == "")
            {
                Save(new List<ITag>() { value });
                return;
            }
            byte[] uncompressed = new byte[0];

            using (FileStream fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            using(Stream stream = GetDecompressStream(fs))
            using(MemoryStream compressed = new MemoryStream())
            {
                
                stream.CopyTo(compressed);
                uncompressed = compressed.ToArray();
            }

            KeyScout counter = InternalUtils.ScoutObjectData(uncompressed, key, new KeyScout());
            if (counter.GetEnd() == null)
            {
                if (counter.GetChildren().Count < 1)
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
                if (newKey.Split('.').Length > 1)
                {
                    ObjectTag output = null;
                    ObjectTag curTag = null;
                    List<string> keys = new List<string>(newKey.Split('.'));
                    int i = 0;
                    foreach (string s in keys)
                    {
                        if (i == 0)
                        {
                            output = new ObjectTag(s);
                            curTag = output;
                        }
                        else if (i == keys.Count - 1)
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
                byte[] outputArray = InternalUtils.SetSubObjectData(uncompressed, counter, memStream.ToArray());

                using (FileStream newFile = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    newFile.SetLength(0);
                    using (Stream compressStream = GetCompressStream(newFile))
                        compressStream.Write(outputArray, 0, outputArray.Length);
                }
            }
            else
            {
                MemoryStream memStream = new MemoryStream();
                BigBinaryWriter bbw = new BigBinaryWriter(memStream);
                value.WriteData(bbw);
                bbw.Close();

                byte[] replaceReturn = InternalUtils.ReplaceSubObjectData(uncompressed, counter, memStream.ToArray());
                using (FileStream newFile = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    newFile.SetLength(0);
                    using (Stream compressStream = GetCompressStream(newFile))
                        compressStream.Write(replaceReturn, 0, replaceReturn.Length);
                }
            }

        }

        public byte[] Export(ICompressor compressor)
        {
            using (FileStream fileStream = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                Stream stream = GetDecompressStream(fileStream);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (Stream compressStream = compressor.GetCompressStream(memoryStream))
                    {
                        stream.CopyTo(compressStream);
                    }
                    return memoryStream.ToArray();
                }
            }
        }

        /// <summary>
        ///     <para>Import a file into this file.</para>
        ///     <para>This basically copies one file to another.</para>
        ///     <para>This <b>will</b> overwrite the current file.</para>
        /// </summary>
        /// <param name="file">The file to copy from.</param>
        /// <param name="compressor">The compression of the other file.</param>
        public void ImportFile(FileInfo file, ICompressor compressor)
        {
            using(FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (MemoryStream decompressedMem = new MemoryStream())
                {
                    using (Stream decompressStream = compressor.GetDecompressStream(fileStream))
                    {
                        decompressStream.CopyTo(decompressedMem);
                    }
                    using (FileStream outputFile = new FileStream(this.file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write))
                    {
                        using(Stream compressStream = this.compression.GetCompressStream(outputFile))
                        {
                            decompressedMem.Position = 0;
                            decompressedMem.CopyTo(compressStream);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     <para>Export to another file.</para>
        ///     <para>This basically copies the current file into another one.</para>
        /// </summary>
        /// <param name="file">The other file to copy to.</param>
        /// <param name="compressor">The desired compression of the copy file.</param>
        public void SaveToFile(FileInfo file, ICompressor compressor)
        {
            using (FileStream fileStream = new FileStream(this.file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                MemoryStream decompressedMem = new MemoryStream();
                using (Stream decompressStream = this.compression.GetDecompressStream(fileStream))
                {
                    decompressStream.CopyTo(decompressedMem);
                }
                
                using (FileStream outputFile = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (Stream compressStream = compressor.GetCompressStream(outputFile))
                    {
                        decompressedMem.Position = 0;
                        decompressedMem.CopyTo(compressStream);
                        
                    }
                }
                decompressedMem.Close();

            }
        }

        /// <summary>
        ///     <para>Clears all the data from the file.</para>
        ///     <para>This works internally by overwriting the file.</para>
        /// </summary>
        public void Clear()
        {
            this.file.Create();
        }
    }
}

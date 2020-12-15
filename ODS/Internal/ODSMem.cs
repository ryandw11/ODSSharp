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
    public class ODSMem : ODSInternal
    {

        private MemoryStream memStream;

        /**
         * <summary>Construct the primary class with a file and compression type.</summary>
         * <param name="file">The file to use.</param>
         * <param name="compression">The compression that should be used.</param>
         */
        public ODSMem(byte[] data, Compressor compressor)
        {
            using (MemoryStream mem = new MemoryStream(data))
            {
                using (Stream stream = compressor.GetDecompressStream(mem))
                    stream.CopyTo(mem);
                memStream = new MemoryStream(data);
            }
        }

        /**
         * <summary>Construct the primary class with a file. The default compression type is GZIP.</summary>
         * <param name="file">The file to use.</param>
         */
        public ODSMem()
        {
            memStream = new MemoryStream();
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

            memStream.Position = 0;

            ITag outf = InternalUtils.getSubObjectData(memStream, key);

            return outf;
        }

        /**
         * <summary>Get all of the tags in the file.</summary>
         * <returns>All of the tags. This returns null if the file does not exist.</returns>
         */
        public List<ITag> GetAll()
        {
            
            memStream.Position = 0;

            List<ITag> outf = InternalUtils.GetListData<ITag>(memStream);

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
            memStream.SetLength(0);
            memStream.Position = 0;
            BigBinaryWriter bw = new BigBinaryWriter(memStream);
            foreach (ITag tag in tags)
            {
                tag.WriteData(bw);
            }
            bw.Flush();
        }

        /**
         * <summary>Append tags to the end of the file.</summary>
         * <param name="tag">The tag to be appended.</param>
         */
        public void Append(ITag tag)
        {
            BigBinaryWriter bw = new BigBinaryWriter(memStream);
            bw.Seek(0, SeekOrigin.End);
            tag.WriteData(bw);
            bw.Flush();
        }

        /**
         * <summary>Append a list of tags to the end of the file.</summary>
         * <param name="tags">The list of tags.</param>
         */
        public void AppendAll(List<ITag> tags)
        {
            BigBinaryWriter bw = new BigBinaryWriter(memStream);
            bw.Seek(0, SeekOrigin.End);
            foreach (ITag tag in tags)
            {
                tag.WriteData(bw);
            }
            bw.Flush();
        }

        /**
         * <summary>Find if a key exists within a file.</summary>
         * <param name="key">The key to find.</param>
         * <returns>If the key exists.</returns>
         */
        public bool Find(string key)
        {
            memStream.Position = 0;
            return InternalUtils.findSubObjectData(memStream, key);
        }


        /**
         * <summary>Remove a tag from the list.</summary>
         * 
         * <param name="key">The key to remove.</param>
         * <returns>If the deletion was successfully done.</returns>
         */
        public bool Delete(string key)
        {
            KeyScout counter = InternalUtils.ScoutObjectData(memStream.ToArray(), key, new KeyScout());
            if (counter == null)
            {
                return false;
            }
            byte[] recompressed = InternalUtils.deleteSubObjectData(memStream.ToArray(), counter);

            memStream.SetLength(0);
            memStream.Position = 0;
            memStream.Write(recompressed, 0, recompressed.Length);

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
            KeyScout counter = InternalUtils.ScoutObjectData(memStream.ToArray(), key, new KeyScout());
            if (counter.GetEnd() == null)
            {
                return false;
            }

            MemoryStream tempStream = new MemoryStream();
            BigBinaryWriter bbw = new BigBinaryWriter(tempStream);
            replacement.WriteData(bbw);
            bbw.Close();

            byte[] replaceReturn = InternalUtils.ReplaceSubObjectData(memStream.ToArray(), counter, tempStream.ToArray());
            memStream.SetLength(0);
            memStream.Position = 0;
            memStream.Write(replaceReturn, 0, replaceReturn.Length);
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
            byte[] uncompressed = memStream.ToArray();

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
                MemoryStream tempStream = new MemoryStream();
                BigBinaryWriter bbw = new BigBinaryWriter(tempStream);
                currentData.WriteData(bbw);
                bbw.Close();
                byte[] outputArray = InternalUtils.SetSubObjectData(uncompressed, counter, tempStream.ToArray());

                memStream.SetLength(0);
                memStream.Position = 0;
                memStream.Write(outputArray, 0, outputArray.Length);
            }
            else
            {
                MemoryStream tempStream = new MemoryStream();
                BigBinaryWriter bbw = new BigBinaryWriter(tempStream);
                value.WriteData(bbw);
                bbw.Close();

                byte[] replaceReturn = InternalUtils.ReplaceSubObjectData(uncompressed, counter, tempStream.ToArray());

                memStream.SetLength(0);
                memStream.Position = 0;
                memStream.Write(replaceReturn, 0, replaceReturn.Length);
            }

        }

        public byte[] Export(Compressor compressor)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                using (Stream compressStream = compressor.GetCompressStream(mem))
                    memStream.CopyTo(compressStream);
                return mem.ToArray();
            }
        }
    }
}

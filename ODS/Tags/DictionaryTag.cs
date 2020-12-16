using System.Text;
using ODS.ODSStreams;
using System.IO;
using System.Collections.Generic;

using ODS.Internal;

namespace ODS.Tags
{
    /**
     * <summary>A tag that stores a disctionary. The key of the dictionary must be a string.</summary>
     * <remarks>This is the equivalent of a Map tag in the offical version.
     * This tag was renamed to reflect the C# api.
     * <para>Note: When grabbing a Dictionary from a file ODS assumes that it is a sorted dictionary.</para></remarks>
     * <typeparam name="T">The data type of the value of the dictionary (Must extend <see cref="ITag"/>).</typeparam>
     */
    public class DictionaryTag<T> : Tag<IDictionary<string, T>> where T : ITag
    {
        private string name;
        private IDictionary<string, T> value;

        /**
         * <summary>Construct a dictionary tag.</summary>
         * <param name="name">The name of the tag.</param>
         * <param name="value">The value of the tag.</param>
         */
        public DictionaryTag(string name, IDictionary<string, T> value)
        {
            this.name = name;
            this.value = value;
        }

        /**
         * <inheritdoc/>
         */
        public void SetValue(IDictionary<string, T> s)
        {
            this.value = s;
        }

        /**
         * <inheritdoc/>
         */
        public IDictionary<string, T> GetValue()
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
            
            foreach(KeyValuePair<string, T> entry in value)
            {
                entry.Value.SetName(entry.Key);
                entry.Value.WriteData(writer);
            }

            dos.Write((int)writer.BaseStream.Length);
            writer.Close();
            dos.Write(memStream.ToArray());
        }

        /**
         * <inheritdoc/>
         */
        public Tag<IDictionary<string, T>> CreateFromData(byte[] value)
        {
            List<T> data = InternalUtils.GetListData<T>(value);
            SortedDictionary<string, T> output = new SortedDictionary<string, T>();
            foreach(T tag in data)
            {
                output.Add(tag.GetName(), tag);
                tag.SetName("");
            }
            this.value = output;
            return this;
        }

        /**
         * <inheritdoc/>
         */
        public byte GetID()
        {
            return 10;
        }
    }
}

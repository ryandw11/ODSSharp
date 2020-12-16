using System;
using System.Text;
using ODS.ODSStreams;
using System.IO;
using System.Collections.Generic;

using ODS.Internal;

namespace ODS.Tags
{
    /**
     * <summary>A tag that contains a list of Tags.</summary>
     * <typeparam name="T">The type of tag that is stored. T must extend ITag.</typeparam>
     */
    public class ListTag<T> : Tag<List<T>> where T : ITag
    {
        private string name;
        private List<T> value;

        /**
         * <summary>Construct a list tag.</summary>
         * <param name="name">The name of the tag.</param>
         * <param name="value">The value of the tag.</param>
         */
        public ListTag(string name, List<T> value)
        {
            this.name = name;
            this.value = value;
        }

        /**
         * <inheritdoc/>
         */
        public void SetValue(List<T> s)
        {
            this.value = s;
        }

        /**
         * <inheritdoc/>
         */
        public List<T> GetValue()
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
         * <summary>Add a tag to the list.</summary>
         * 
         * <param name="tag">The tag to add.</param>
         */
        public void AddTag(T tag)
        {
            value.Add(tag);
        }

        /**
         * <summary>Get a tag at a certain index.</summary>
         * <param name="i">The index of the desired tag.</param>
         * <returns>The tag at index i.</returns>
         */
        public T GetTag(int i)
        {
            return value[i];
        }

        /**
         * <summary>Get a tag via its name.</summary>
         * <param name="name">The name of the tag to get.</param>
         * <returns>The tag with the desired name. (Returns the default if none is found).</returns>
         */
        public T GetTag(string name)
        {
            foreach(T tag in value){
                if (tag.GetName() == name)
                    return tag;
            }

            return default;
        }

       /**
        * <summary>Remove a tag via instance.</summary>
        * <param name="tag">The tag to remove.</param>
        */
        public void RemoveTag(T tag)
        {
            value.Remove(tag);
        }

        /**
         * <summary>Remove a tag via its index.</summary>
         * <param name="tag">The index of the tag to remove.</param>
         */
        public void RemoveTag(int tag)
        {
            value.RemoveAt(tag);
        }

        /**
         * <summary>Remove all tags for the list.</summary>
         */
        public void RemoveAllTags()
        {
            value.Clear();
        }

        /**
         * <summary>Get the index of the specified tag.</summary>
         * <param name="tag">The tag that you want the index of.</param>
         * <returns>The index of the specified tag.</returns>
         */
        public int IndexOf(T tag)
        {
            return value.IndexOf(tag);
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
            
            foreach(T tag in value)
            {
                tag.SetName("");
                tag.WriteData(writer);
            }

            dos.Write((int)writer.BaseStream.Length);
            writer.Close();
            dos.Write(memStream.ToArray());
        }

        /**
         * <inheritdoc/>
         */
        public Tag<List<T>> CreateFromData(byte[] value)
        {
            this.value = InternalUtils.GetListData<T>(value);
            return this;
        }

        /**
         * <inheritdoc/>
         */
        public byte GetID()
        {
            return 9;
        }
    }
}

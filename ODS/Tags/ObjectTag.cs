using System;
using System.Text;
using ODS.ODSStreams;
using System.IO;
using System.Collections.Generic;

using ODS.Internal;

namespace ODS.Tags
{
    /**
     * <summary>A tag that contains an object.</summary>
     */
    public class ObjectTag : Tag<List<ITag>>
    {
        private string name;
        private List<ITag> value;

        /**
         * <summary>Construct an object tag.</summary>
         * <param name="name">The name of the tag.</param>
         * <param name="value">The list of Tags that the object contains.</param>
         */
        public ObjectTag(string name, List<ITag> value)
        {
            this.name = name;
            this.value = value;
        }

        /**
         * <summary>Construct an object tag with no existing tags.</summary>
         * <param name="name">The name of the tag.</param>
         */
        public ObjectTag(string name)
        {
            this.name = name;
            this.value = new List<ITag>();
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
         * <inheritdoc/>
         */
        public void WriteData(BigBinaryWriter dos)
        {
            dos.Write(GetID());
            MemoryStream memStream = new MemoryStream();
            BigBinaryWriter writer = new BigBinaryWriter(memStream);
            writer.Write((short) Encoding.UTF8.GetByteCount(name));
            writer.Write(Encoding.UTF8.GetBytes(name));
            
            foreach(ITag tag in value)
            {
                tag.WriteData(writer);
            }

            dos.Write((int)writer.BaseStream.Length);
            writer.Close();
            dos.Write(memStream.ToArray());
        }

        /**
         * <inheritdoc/>
         */
        public Tag<List<ITag>> CreateFromData(byte[] value)
        {
            this.value = InternalUtils.GetListData<ITag>(value);
            return this;
        }

        /**
         * <inheritdoc/>
         */
        public byte GetID()
        {
            return 11;
        }
    }
}

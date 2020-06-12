using System;
using System.Text;
using ODS.Stream;
using System.IO;
using System.Collections.Generic;

namespace ODS.tags
{
    public class ObjectTag : Tag<List<ITag>>
    {
        private string name;
        private List<ITag> value;

        public ObjectTag(string name, List<ITag> value)
        {
            this.name = name;
            this.value = value;
        }

        public ObjectTag(string name)
        {
            this.name = name;
            this.value = new List<ITag>();
        }

        public void SetValue(List<ITag> s)
        {
            this.value = s;
        }

        public List<ITag> GetValue()
        {
            return value;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public String GetName()
        {
            return name;
        }

        public void AddTag(ITag tag)
        {
            value.Add(tag);
        }

        public ITag GetTag(int i)
        {
            return value[i];
        }

        public ITag GetTag(string name)
        {
            return value.Find(tag => tag.GetName() == name);
        }

        public void RemoveTag(ITag tag)
        {
            value.Remove(tag);
        }

        public void RemoveTag(int tag)
        {
            value.RemoveAt(tag);
        }

        public void RemoveAllTags()
        {
            value.Clear();
        }

        public bool HasTag(string name)
        {
            return value.Find(tag => tag.GetName() == name) != null;
        }

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

        public Tag<List<ITag>> CreateFromData(byte[] value)
        {
            this.value = ObjectDataStructure.GetListData<ITag>(value);
            return this;
        }

        public byte GetID()
        {
            return 11;
        }
    }
}

using System;
using System.Text;
using ODS.Stream;
using System.IO;
using System.Collections.Generic;

namespace ODS.tags
{
    public class ListTag<T> : Tag<List<T>> where T : ITag
    {
        private string name;
        private List<T> value;

        public ListTag(String name, List<T> value)
        {
            this.name = name;
            this.value = value;
        }

        public void SetValue(List<T> s)
        {
            this.value = s;
        }

        public List<T> GetValue()
        {
            return value;
        }

        public void SetName(String name)
        {
            this.name = name;
        }

        public String GetName()
        {
            return name;
        }

        public void AddTag(T tag)
        {
            value.Add(tag);
        }

        public T GetTag(int i)
        {
            return value[i];
        }

        public void RemoveTag(T tag)
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

        public int IndexOf(T tag)
        {
            return value.IndexOf(tag);
        }

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

        public Tag<List<T>> CreateFromData(byte[] value)
        {
            this.value = ObjectDataStructure.GetListData<T>(value);
            return this;
        }

        public byte GetID()
        {
            return 9;
        }
    }
}

using System.Text;
using ODS.Stream;
using System.IO;
using System.Collections.Generic;

namespace ODS.Tags
{
    /**
     * This is the equivalent of a Map tag in the offical version.
     * This tag was renamed to reflect the C# api.
     * Note: When grabbing a Dictionary from a file ODS assumes that it is a sorted dictionary.
     */
    public class DictionaryTag<T> : Tag<IDictionary<string, T>> where T : ITag
    {
        private string name;
        private IDictionary<string, T> value;

        public DictionaryTag(string name, IDictionary<string, T> value)
        {
            this.name = name;
            this.value = value;
        }

        public void SetValue(IDictionary<string, T> s)
        {
            this.value = s;
        }

        public IDictionary<string, T> GetValue()
        {
            return value;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public string GetName()
        {
            return name;
        }

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

        public Tag<IDictionary<string, T>> CreateFromData(byte[] value)
        {
            List<T> data = ObjectDataStructure.GetListData<T>(value);
            SortedDictionary<string, T> output = new SortedDictionary<string, T>();
            foreach(T tag in data)
            {
                output.Add(tag.GetName(), tag);
                tag.SetName("");
            }
            this.value = output;
            return this;
        }

        public byte GetID()
        {
            return 10;
        }
    }
}

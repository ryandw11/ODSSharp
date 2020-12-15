using System;
using System.Collections.Generic;
using System.Text;

using ODS.Compression;

namespace ODS.Internal
{
    interface ODSInternal
    {
        ITag Get(string key);
        List<ITag> GetAll();
        void Save(List<ITag> tags);
        void Append(ITag tag);
        void AppendAll(List<ITag> tags);
        bool Find(string key);
        bool Delete(string key);
        bool ReplaceData(string key, ITag replacement);
        void Set(string key, ITag value);
        byte[] Export(Compressor compressor);
    }
}

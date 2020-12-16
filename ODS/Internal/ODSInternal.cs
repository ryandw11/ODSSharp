using System;
using System.Collections.Generic;
using System.Text;

using ODS.Compression;

namespace ODS.Internal
{
    /**
     * <summary>This is the interface that handles the internal storage types.</summary>
     * <remarks>See <see cref="ObjectDataStructure"/> for comments on the API.</remarks>
     */
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
        byte[] Export(ICompressor compressor);
    }
}

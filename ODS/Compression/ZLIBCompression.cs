using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.IO;

namespace ODS.Compression
{
    /**
     * <summary>Compress the file using the ZLIB compression format.</summary>
     */
    public class ZLIBCompression : ICompressor
    {
        public System.IO.Stream GetCompressStream(System.IO.Stream stream)
        {
            return new DeflaterOutputStream(stream);
        }

        public System.IO.Stream GetDecompressStream(System.IO.Stream stream)
        {
            return new InflaterInputStream(stream);
        }
    }
}

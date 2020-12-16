using System.IO;
using System.IO.Compression;

namespace ODS.Compression
{
    /**
     * <summary>Compress the file using the GZIP compression format.</summary>
     */
    public class GZIPCompression : ICompressor
    {

        public Stream GetCompressStream(Stream stream)
        {
            return new GZipStream(stream, CompressionMode.Compress, true);
        }

        public Stream GetDecompressStream(Stream stream)
        {
            return new GZipStream(stream, CompressionMode.Decompress, true);
        }
    }
}

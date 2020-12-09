using System.IO;
using System.IO.Compression;

namespace ODS.Compression
{
    /**
     * <summary>Compress the file using the GZIP compression format.</summary>
     */
    public class GZIPCompression : Compressor
    {
        public byte[] Compress(byte[] data)
        {
            MemoryStream stream = new MemoryStream();
            GZipStream zipStream = new GZipStream(stream, CompressionMode.Compress);
            zipStream.Write(data, 0, data.Length);
            zipStream.Close();
            return stream.ToArray();
        }

        public byte[] Decompress(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress);
            MemoryStream output = new MemoryStream();
            zipStream.CopyTo(output);
            zipStream.Close();
            return output.ToArray();
        }
    }
}

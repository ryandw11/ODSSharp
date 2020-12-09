using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.IO;

namespace ODS.Compression
{
    /**
     * <summary>Compress the file using the ZLIB compression format.</summary>
     */
    public class ZLIBCompression : Compressor
    {
        public byte[] Compress(byte[] data)
        {
            MemoryStream stream = new MemoryStream();
            DeflaterOutputStream zipStream = new DeflaterOutputStream(stream);
            zipStream.Write(data, 0, data.Length);
            zipStream.Close();
            return stream.ToArray();
        }

        public byte[] Decompress(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            InflaterInputStream zipStream = new InflaterInputStream(stream);
            MemoryStream output = new MemoryStream();
            zipStream.CopyTo(output);
            zipStream.Close();
            return output.ToArray();
        }
    }
}

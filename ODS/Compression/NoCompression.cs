namespace ODS.Compression
{
    /**
     * <summary>This will not compress the file.</summary>
     */
    public class NoCompression : Compressor
    {
        public System.IO.Stream GetCompressStream(System.IO.Stream stream)
        {
            return stream;
        }

        public System.IO.Stream GetDecompressStream(System.IO.Stream stream)
        {
            return stream;
        }
    }
}

namespace ODS.Compression
{
    /**
     * <summary>This will not compress the file.</summary>
     */
    public class NoCompression : Compressor
    {
        public byte[] Compress(byte[] data)
        {
            return data;
        }

        public byte[] Decompress(byte[] data)
        {
            return data;
        }
    }
}

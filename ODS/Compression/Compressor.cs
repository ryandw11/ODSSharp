namespace ODS.Compression
{
    /**
     * <summary>
     * This interface allows different compression algorithms to be used.
     * Unlike the Java version of ODS, an input stream is not needed.
     * </summary>
     */
    public interface Compressor
    {
        /**
         * <summary>Compress an array of bytes.</summary>
         * <param name="data">The array of bytes to compress.</param>
         * <returns>The array of compressed bytes.</returns>
         */
        byte[] Compress(byte[] data);

        /**
         * <summary>Decompress an array of bytes.</summary>
         * <param name="data">The array of bytes to decompress.</param>
         * <returns>The array of decompressed bytes.</returns>
         */
        byte[] Decompress(byte[] data);
    }
}

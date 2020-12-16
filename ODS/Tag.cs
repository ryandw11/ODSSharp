namespace ODS
{
    /**
     * <summary>The generic tag interface.</summary>
     * <typeparam name="T">The type that the tag represents.</typeparam>
     */
    public interface Tag<T> : ITag
    {
        /**
         * <summary>Set the value of the tag.</summary>
         * <param name="t">The value.</param>
         */
        void SetValue(T t);

        /**
         * <summary>Get the value of the tag.</summary>
         * <returns>The value of the tag.</returns>
         */
        T GetValue();

        /**
         * <summary>Create the tag from an array of bytes. For internal use only.</summary>
         * <returns>The tag that is created from the value bytes.</returns>
         */
        Tag<T> CreateFromData(byte[] value);
    }
}

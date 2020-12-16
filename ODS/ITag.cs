using ODS.ODSStreams;

namespace ODS
{
    /**
     * <summary>The general tag interface.</summary>
     */
    public interface ITag
    {
        /**
         * Convert the tag into ODS format.
         * <summary>This is for internal use only.</summary>
         */
        void WriteData(BigBinaryWriter dos);

        /**
         * <summary>Set the name of the tag.</summary>
         * <param name="name">The name of the tag.</param>
         */
        void SetName(string name);

        /**
         * <summary>Get the name of the tag.</summary>
         * <returns>The name of the tag.</returns>
         */
        string GetName();

        /**
         * <summary>Get the id of the tag.</summary>
         * <returns>The byte id of the tag.</returns>
         */
        byte GetID();
    }
}

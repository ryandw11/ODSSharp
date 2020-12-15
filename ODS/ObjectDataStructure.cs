using System.IO;
using System.Collections.Generic;
using ODS.Compression;
using ODS.Internal;

namespace ODS
{
    /**
     * <summary>
     * Primary class of the ObjectDataStructure library.
     * <para>ObjectDataStructure has two storage types: File and Memory. File reads from a file while memory deals with
     * an array (or buffer) of bytes. This class deals with both types. The type is dependent on the constructor.</para>
     * <para>Most methods in ODS use what is called a key to reference objects within the file/buffer. A key allows you to grab specific
     * information within the file/buffer. For example: If you wanted a specific string inside of an object with lots of information,
     * you can get that specific string without any other data. If you have an object named Car and you wanted to get a string tag named
     * owner from the inside the object, then the key for that would be:</para>
     * <code>
     *  Car.owner
     * </code>
     * <para>Let's say that the owner is an object called 'Owner' and you want to get the age of the owner, you could do:</para>
     * <code>
     * Car.Owner.age
     * </code>
     * <para>You can obtain any tag using the key system, including ObjectTags. So the key `Car.Owner` would be valid.</para>
     * </summary>
     * 
     * <remarks>For exact information on the methods depending on storage type (viz. File or Memory) please visit the respective internal classes.</remarks>
     */
    public class ObjectDataStructure
    {
        private ODSInternal odsInternal;

        /**
         * <summary>Create ODS using the file storage type. Data will be written to and read from a file.</summary>
         * <param name="file">The file to use.</param>
         * <param name="compression">The compression that should be used.</param>
         */
        public ObjectDataStructure(FileInfo file, Compressor compression)
        {
            this.odsInternal = new ODSFile(file, compression);
        }

        /**
         * <summary>Create ODS using the file storage type. Data will be written to and read from a file.</summary>
         * <param name="file">The file to use.</param>
         */
        public ObjectDataStructure(FileInfo file)
        {
            this.odsInternal = new ODSFile(file);
        }

        /**
         * <summary>Create ODS using the memory storage type. Data will be written to and read from a memory stream.</summary>
         * <param name="data">Pre-existing data that should be inserted into the memory stream.</param>
         * <param name="compressor">The compression format the data uses.</param>
         */
        public ObjectDataStructure(byte[] data, Compressor compressor)
        {
            this.odsInternal = new ODSMem(data, compressor);
        }

        /**
         * <summary>Create ODS using the memory storage type. Data will be written to and read from a memory stream.</summary>
         */
        public ObjectDataStructure()
        {
            this.odsInternal = new ODSMem();
        }

        /*
         * TODO :: Finish updaing the comments. 
         * 
         * 
         */

        /**
         * <summary>Grab a tag based upon an object key. This method allows you to directly get sub-objects with little overhead.</summary>
         * <example>
         *      Get("primary.firstsub.secondsub");
         * </example>
         * <param name="key">They key to search for.</param>
         * <returns>The tag. This will return null if no tag with the specified key path is found or the file does not exist.</returns>
         */
        public ITag Get(string key)
        {
            return odsInternal.Get(key);
        }

        /**
         * <summary>Get all of the tags in the file.</summary>
         * <returns>All of the tags. This returns null if the file does not exist.</returns>
         */
        public List<ITag> GetAll()
        {
            return odsInternal.GetAll();
        }

        /**
         * <summary>Save tags to the file. This method will create a new file if one does not exist.
         * This will overwrite the existing file. To append tags
         * see #Append(ITag) and #AppendAll(List)</summary>
         * <param name="tags">The list of tags to save.</param>
         */
        public void Save(List<ITag> tags)
        {
            odsInternal.Save(tags);
        }

        /**
         * <summary>Append tags to the end of the file.</summary>
         * <param name="tag">The tag to be appended.</param>
         */
        public void Append(ITag tag)
        {
            odsInternal.Append(tag);
        }

        /**
         * <summary>Append a list of tags to the end of the file.</summary>
         * <param name="tags">The list of tags.</param>
         */
        public void AppendAll(List<ITag> tags)
        {
            odsInternal.AppendAll(tags);
        }

        /**
         * <summary>Find if a key exists within a file.</summary>
         * <param name="key">The key to find.</param>
         * <returns>If the key exists.</returns>
         */
        public bool Find(string key)
        {
            return odsInternal.Find(key);
        }

        /**
         * <summary>Remove a tag from the list.</summary>
         * 
         * <param name="key">The key to remove.</param>
         * <returns>If the deletion was successfully done.</returns>
         */
        public bool Delete(string key)
        {
            return odsInternal.Delete(key);
        }

        /**
         * <summary>Replace a key with another tag.</summary>
         * <param name="key">The key</param>
         * <param name="replacement">The data to replace the key</param>
         * <returns>If the replacement was successful.</returns>
         */
        public bool ReplaceData(string key, ITag replacement)
        {
            return odsInternal.ReplaceData(key, replacement);
        }

        /**
         * <summary>
         * This method can append, delete, and set tags.
         * <para>A note on keys when appending: <code>ObjectOne.ObjectTwo.tagName</code> When appending data <c>tagName</c> will not be the actual tag name.
         * The tag name written to the file is the name of the specified tag in the value parameter. Any parent objects that do not exist will be created. For example:
         * <code>ObjectOne.ObjectTwo.NewObject.tagName</code> If in the example above <c>NewObject</c> does not exist, than the object will be created with the value tag inside
         * of it. Please see the wiki for a more detailed explanation on this.</para>
         * <para>When value is null, the specified key is deleted. <c>The key MUST exist or an {@link ODSException} will be thrown.</c></para>
         * </summary>
         * 
         * <param name="key">
         * The key of the tag to append, delete, or set.
         * <para>When appending the key does not need to exist. ObjectTags that don't exist will be created automatically.</para>
         * <para>When the key is set to "" (An empty string) than it is assumed you want to append to the parent file.</para>
         * <para>Valid Tags:</para>
         *  <code>
         *  <para>ObjectOne.tagToDelete</para>
         *  <para>ObjectOne.NewObject.tagToAppend</para>
         *  <para>ObjectOne.tagToSet</para>
         *  </code>
         * </param>
         * 
         * <param name="value">The tag to append or replace the key with. <para>If this parameter is null than the key will be deleted.</para></param>
         * 
         */
        public void Set(string key, ITag value)
        {
            odsInternal.Set(key, value);
        }

        public byte[] Export(Compressor compressor)
        {
            return odsInternal.Export(compressor);
        }

    }
}

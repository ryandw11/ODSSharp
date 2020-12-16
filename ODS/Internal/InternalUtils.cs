using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using ODS.ODSStreams;
using ODS.Util;

namespace ODS.Internal
{
    /**
     * <summary>This class serves as a utility class for the internals of ODS.</summary>
     */
    class InternalUtils
    {
        /**
         * <summary>
         * Get a tag from a stream via a name.
         * 
         * <para>This method is deprecated. Use <see cref="GetSubObjectData(Stream, string)"/> instead.</para>
         * 
         * </summary>
         * 
         * <param name="stream">The stream to look for the tag in.</param>
         * <param name="name">The name of the object to find. (This is not the key!)</param>
         */
        [Obsolete]
        public static ITag getSubData(Stream stream, string name)
        {
            BigBinaryReader binReader = new BigBinaryReader(stream);
            TagBuilder currentBuilder = new TagBuilder();


            while (binReader.BaseStream.Position != binReader.BaseStream.Length)
            {
                currentBuilder.setDataType(binReader.ReadByte());
                currentBuilder.setDataSize(binReader.ReadInt32());
                //TODO see if this is correct.
                currentBuilder.setStartingIndex(binReader.BaseStream.Position);
                currentBuilder.setNameSize(binReader.ReadInt16());

                if (currentBuilder.getNameSize() != Encoding.UTF8.GetByteCount(name))
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }

                byte[] nameBytes = binReader.ReadBytes(currentBuilder.getNameSize());
                String tagName = Encoding.UTF8.GetString(nameBytes);
                currentBuilder.setName(tagName);
                if (tagName != name)
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }

                byte[] value = binReader.ReadBytes((int)(currentBuilder.getStartingIndex() - stream.Position) + (int)currentBuilder.getDataSize());
                currentBuilder.setValueBytes(value);
                return currentBuilder.proccess();
            }
            return null;
        }

        /**
         * <summary>Get a tag from a stream using the key system.</summary>
         * 
         * <param name="stream">The stream to read from. (Position is expected to be at 0.)</param>
         * <param name="key">The key of the tag that you want to find.</param>
         * <returns>Returns the desired tag.</returns>
         */
        public static ITag GetSubObjectData(Stream stream, string key)
        {
            BigBinaryReader binReader = new BigBinaryReader(stream);
            TagBuilder currentBuilder = new TagBuilder();

            string name = key.Split('.')[0];
            string otherKey = getKey(key.Split('.'));


            while (binReader.BaseStream.Position != binReader.BaseStream.Length)
            {
                currentBuilder.setDataType(binReader.ReadByte());
                currentBuilder.setDataSize(binReader.ReadInt32());
                //TODO see if this is correct.
                currentBuilder.setStartingIndex(stream.Position);
                currentBuilder.setNameSize(binReader.ReadInt16());

                if (currentBuilder.getNameSize() != Encoding.UTF8.GetByteCount(name))
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }

                byte[] nameBytes = binReader.ReadBytes(currentBuilder.getNameSize());
                string tagName = Encoding.UTF8.GetString(nameBytes);
                currentBuilder.setName(tagName);
                if (tagName != name)
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }

                if (otherKey != null)
                    return GetSubObjectData(stream, otherKey);

                byte[] value = binReader.ReadBytes((int)(currentBuilder.getStartingIndex() - stream.Position) + (int)currentBuilder.getDataSize());
                currentBuilder.setValueBytes(value);

                return currentBuilder.proccess();
            }
            return null;
        }

        private static string getKey(string[] s)
        {
            List<string> list = new List<string>(s);
            list.Remove(list[0]);
            if (list.Count == 1) return list[0];
            if (list.Count < 1) return null;

            return string.Join(".", list);
        }

        /**
         * <summary>Checks to see if a key exists within a stream.</summary>
         * <param name="stream">The stream to check in.</param>
         * <param name="key">The key to find.</param>
         * 
         * <returns>If the key exists.</returns>
         */
        public static bool FindSubObjectData(Stream stream, string key)
        {
            BigBinaryReader binReader = new BigBinaryReader(stream);
            TagBuilder currentBuilder = new TagBuilder();

            string name = key.Split('.')[0];
            string otherKey = getKey(key.Split('.'));


            while (binReader.BaseStream.Position != binReader.BaseStream.Length)
            {
                currentBuilder.setDataType(binReader.ReadByte());
                currentBuilder.setDataSize(binReader.ReadInt32());
                //TODO see if this is correct.
                currentBuilder.setStartingIndex(binReader.BaseStream.Position);
                currentBuilder.setNameSize(binReader.ReadInt16());

                if (currentBuilder.getNameSize() != Encoding.UTF8.GetByteCount(name))
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }

                byte[] nameBytes = binReader.ReadBytes(currentBuilder.getNameSize());
                string tagName = Encoding.UTF8.GetString(nameBytes);
                currentBuilder.setName(tagName);
                if (tagName != name)
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }

                

                if (otherKey != null)
                    return FindSubObjectData(stream, otherKey);

                byte[] value = binReader.ReadBytes((int)(currentBuilder.getStartingIndex() - stream.Position) + (int)currentBuilder.getDataSize());
                currentBuilder.setValueBytes(value);

                return true;
            }
            return false;
        }


        /**
         * <summary>Delete a tag from an array of bytes using the KeyScout.</summary>
         * 
         * <param name="data">The byte array to remove data from.</param>
         * <param name="counter">The KeyScout that contains information about the tag to remove.</param>
         * <returns>The byte array after the deletion.</returns>
         */
        public static byte[] deleteSubObjectData(byte[] data, KeyScout counter)
        {
            counter.RemoveAmount(counter.GetEnd().GetSize() + 5);

            KeyScoutChild end = counter.GetEnd();

            byte[] array1 = new byte[data.Length - (5 + end.GetSize())];
            Array.Copy(data, 0, array1, 0, (end.GetStartingIndex() - 1));
            Array.Copy(data, end.GetStartingIndex() + 4 + end.GetSize(),
                array1, end.GetStartingIndex() - 1,
                data.Length - (end.GetStartingIndex() + 4 + end.GetSize()));

            foreach (KeyScoutChild child in counter.GetChildren())
            {
                int index = child.GetStartingIndex();
                int size = child.GetSize();
                array1[index] = (byte)(size >> 24);
                array1[index + 1] = (byte)(size >> 16);
                array1[index + 2] = (byte)(size >> 8);
                array1[index + 3] = (byte)size;
            }

            return array1;
        }

        /**
         * <summary>
         * Replace a tag with another tag.
         * </summary>
         * <param name="data">The input array of bytes</param>
         * <param name="counter">The KeyScout object conatining the information of the tag to replace.</param>
         * <param name="dataToReplace">The bytes of the replacement data.</param>
         * <returns>The byte array after the replacement.</returns>
         */
        public static byte[] ReplaceSubObjectData(byte[] data, KeyScout counter, byte[] dataToReplace)
        {
            counter.RemoveAmount(counter.GetEnd().GetSize() + 5);
            counter.AddAmount(dataToReplace.Length);

            KeyScoutChild end = counter.GetEnd();


            byte[] array1 = new byte[data.Length - (5 + end.GetSize()) + dataToReplace.Length];
            //Copy all of the information before the removed data.
            Array.Copy(data, 0, array1, 0, (end.GetStartingIndex() - 1));
            Array.Copy(dataToReplace, 0, array1, end.GetStartingIndex() - 1, dataToReplace.Length);
            // copy all of the information after the removed data.
            Array.Copy(data, end.GetStartingIndex() + 4 + end.GetSize(),
                    array1, end.GetStartingIndex() - 1 + dataToReplace.Length,
                    data.Length - (end.GetStartingIndex() + 4 + end.GetSize()));

            foreach (KeyScoutChild child in counter.GetChildren())
            {
                int index = child.GetStartingIndex();
                int size = child.GetSize();
                array1[index] = (byte)(size >> 24);
                array1[index + 1] = (byte)(size >> 16);
                array1[index + 2] = (byte)(size >> 8);
                array1[index + 3] = (byte)(size);
            }

            return array1;
        }

        /**
         * <summary>Insert in a new tag.</summary>
         * <param name="data">The input array of bytes.</param>
         * <param name="counter">The KeyScout object containing information of the tag to set.</param>
         * <param name="dataToReplace">The bytes of the replacement data.</param>
         * 
         * <returns>The output bytes.</returns>
         */
        public static byte[] SetSubObjectData(byte[] data, KeyScout counter, byte[] dataToReplace)
        {
            KeyScoutChild child = counter.GetChildren()[counter.GetChildren().Count - 1];


            byte[] array1 = new byte[data.Length + dataToReplace.Length];
            Array.Copy(data, 0, array1, 0, child.GetStartingIndex() + 4 + child.GetSize());
            Array.Copy(dataToReplace, 0, array1, child.GetStartingIndex() + 4 + child.GetSize(), dataToReplace.Length);
            Array.Copy(data, (child.GetStartingIndex() + 4 + child.GetSize()), array1, (child.GetStartingIndex() + 4 + child.GetSize()) + dataToReplace.Length,
                    data.Length - (child.GetStartingIndex() + 4 + child.GetSize()));

            counter.AddAmount(dataToReplace.Length);

            foreach (KeyScoutChild childs in counter.GetChildren())
            {
                int index = childs.GetStartingIndex();
                int size = childs.GetSize();
                array1[index] = (byte)(size >> 24);
                array1[index + 1] = (byte)(size >> 16);
                array1[index + 2] = (byte)(size >> 8);
                array1[index + 3] = (byte)(size);
            }

            return array1;
        }

        /**
         * <summary>
         * This object goes through the data and scouts out the information from the given key.
         * This method is recursive, which is why the parameter offset exists.
         * </summary>
         * 
         * <param name="data">The input array of bytes</param>
         * <param name="key">The key</param>
         * <param name="counter">The Scout object</param>
         * <returns>The key scout.</returns>
         */
        public static KeyScout ScoutObjectData(byte[] data, string key, KeyScout counter)
        {
            return ScoutObjectData(data, key, counter, 0);
        }

        /**
         * <summary>
         * This object goes through the data and scouts out the information from the given key.
         * This method is recursive, which is why the parameter offset exists.
         * </summary>
         * 
         * <param name="data">The input array of bytes</param>
         * <param name="key">The key</param>
         * <param name="counter">The Scout object</param>
         * <param name="startIndex">The starting index for the count.</param>
         * <returns>The key scout.</returns>
         */
        public static KeyScout ScoutObjectData(byte[] data, string key, KeyScout counter, int startIndex)
        {
            MemoryStream stream = new MemoryStream(data);
            BigBinaryReader binReader = new BigBinaryReader(stream);
            TagBuilder currentBuilder = new TagBuilder();

            binReader.BaseStream.Seek(0, SeekOrigin.Begin);

            string name = key.Split('.')[0];
            string otherKey = getKey(key.Split('.'));
            while (binReader.BaseStream.Position != binReader.BaseStream.Length)
            {
                KeyScoutChild child = new KeyScoutChild();
                currentBuilder.setDataType(binReader.ReadByte());
                child.SetStartingIndex((int)binReader.BaseStream.Position + startIndex);
                currentBuilder.setDataSize(binReader.ReadInt32());
                currentBuilder.setStartingIndex(binReader.BaseStream.Position);
                currentBuilder.setNameSize(binReader.ReadInt16());

                if (currentBuilder.getNameSize() != Encoding.UTF8.GetByteCount(name))
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }

                byte[] nameBytes = binReader.ReadBytes(currentBuilder.getNameSize());
                string tagName = Encoding.UTF8.GetString(nameBytes);
                currentBuilder.setName(tagName);
                if (tagName != name)
                {
                    binReader.BaseStream.Seek((currentBuilder.getStartingIndex() - stream.Position) + currentBuilder.getDataSize(), SeekOrigin.Current);
                    currentBuilder = new TagBuilder();
                    continue;
                }
                int binPos = (int)binReader.BaseStream.Position;
                byte[] value = binReader.ReadBytes((int)(currentBuilder.getStartingIndex() - stream.Position) + (int)currentBuilder.getDataSize());
                currentBuilder.setValueBytes(value);

                binReader.Close();


                if (otherKey != null)
                {
                    child.SetSize(currentBuilder.getDataSize());
                    child.SetName(currentBuilder.getName());
                    counter.AddChild(child);
                    return ScoutObjectData(currentBuilder.getValueBytes(), otherKey, counter, binPos + startIndex);
                }
                child.SetSize(currentBuilder.getDataSize());
                child.SetName(currentBuilder.getName());
                counter.SetEnd(child);
                return counter;
            }
            return null;
        }

        /**
         * <summary>Get a list of Tags from an array of bytes. This is meant for internal use only.</summary>
         * <param name="data">The array of bytes.</param>
         * <returns>The list of tags.</returns>
         */
        public static List<T> GetListData<T>(byte[] data) where T : ITag
        {
            MemoryStream stream = new MemoryStream(data);
            List<T> output = GetListData<T>(stream);
            stream.Close();
            return output;
        }

        /**
         * <summary>Get a list of Tags from a stream. This is meant for internal use only.</summary>
         * <param name="stream">The stream to read from. (Position is expected to be at 0).</param>
         * <returns>The list of tags.</returns>
         */
        public static List<T> GetListData<T>(Stream stream) where T : ITag
        {
            BigBinaryReader binReader = new BigBinaryReader(stream);
            TagBuilder currentBuilder = new TagBuilder();

            binReader.BaseStream.Seek(0, SeekOrigin.Begin);
            List<T> output = new List<T>();
            while (binReader.BaseStream.Position != binReader.BaseStream.Length)
            {
                currentBuilder.setDataType(binReader.ReadByte());
                currentBuilder.setDataSize(binReader.ReadInt32());
                //TODO see if this is correct.
                currentBuilder.setStartingIndex(binReader.BaseStream.Position);
                currentBuilder.setNameSize(binReader.ReadInt16());

                byte[] nameBytes = binReader.ReadBytes(currentBuilder.getNameSize());
                String tagName = Encoding.UTF8.GetString(nameBytes);
                currentBuilder.setName(tagName);

                byte[] value = binReader.ReadBytes((int)(currentBuilder.getStartingIndex() - stream.Position) + (int)currentBuilder.getDataSize());
                currentBuilder.setValueBytes(value);
                output.Add((T)currentBuilder.proccess());
            }
            return output;
        }
    }
}

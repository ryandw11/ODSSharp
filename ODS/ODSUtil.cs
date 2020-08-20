using System;
using System.Collections.Generic;
using ODS.Tags;
using ODS.Serializer;
using System.Reflection;
using ODS.Exceptions;

namespace ODS
{
    /**
     * <summary>This is a utility class that provides wrappers for the different tags.</summary>
     */
    public class ODSUtil
    {
        internal static bool ignoreInvalidCustomTags = false; 
        private static List<ITag> customTags = new List<ITag>();

        /**
         * <summary>Wrap an object to a tag. The name is not set by this method. This method
         * cannot wrap Lists or Dictionaries. Objects are automatically serialized.</summary>
         * <param name="o">The object to wrap.</param>
         * <returns>The tag equivalent.</returns>
         */
        public static ITag Wrap(object o)
        {
            return Wrap("", o);
        }

        /**
         * <summary>Wrap an object to a tag. This method cannot wrap Lists or Dictionaries. Objects are automatically
         * serialized.</summary>
         * <param name="name">The name of the tag to be created.</param>
         * <param name="o">The object to wrap.</param>
         * <returns>The tag equivalent.</returns>
         */
        public static ITag Wrap(string name, object o)
        {
            //A switch statement cannot be used here.
            if(o.GetType() == typeof(byte))
            {
                return new ByteTag(name, (byte)o);
            }
            if(o.GetType() == typeof(char))
            {
                return new CharTag(name, (char)o);
            }
            if(o.GetType() == typeof(double))
            {
                return new DoubleTag(name, (double)o);
            }
            if (o.GetType() == typeof(float))
            {
                return new FloatTag(name, (float)o);
            }
            if (o.GetType() == typeof(int))
            {
                return new IntTag(name, (int)o);
            }
            if (o.GetType() == typeof(long))
            {
                return new LongTag(name, (long)o);
            }
            if (o.GetType() == typeof(short))
            {
                return new ShortTag(name, (short)o);
            }
            if (o.GetType() == typeof(string))
            {
                return new StringTag(name, (string)o);
            }
            if(o is List<object>)
            {
                return Wrap(name, (List<object>)o);
            }
            return Serialize(name, o);
        }

        /**
         * <summary>Unwrap a tag and turn it back into its original value. This method will not work for Lists or Dictionaries.</summary>
         * <param name="tag">The tag to unwrap.</param>
         * <typeparam name="T">The c# datatype represented by the tag.</typeparam>
         * <returns>The value of the tag.</returns>
         */
        public static T UnWrap<T>(Tag<T> tag)
        {
            if(tag.GetType() == typeof(ObjectTag))
            {
                ObjectTag objTag = (ObjectTag)tag;
                StringTag clazzName = (StringTag)objTag.GetTag("ODS_TAG");
                if (clazzName == null) throw new Exception("Cannot unwrap object: TagObject is not a serialized object");
                return (T)Deserialize<object>(objTag);
            }
            return tag.GetValue();
        }

        /**
         * <summary>Wrap a list into a list tag.</summary>
         * <param name="name">The name of the list tag.</param>
         * <param name="list">The list to turn into a list tag. The list can only contain objects that
         * can be converted into tags. The contents of the list are atuomatically wrapped. Note: this list cannot
         * contain Objects, other Lists, or Dictionaries.</param>
         * <typeparam name="T">The data type of the list.</typeparam>
         * <returns>The wrapped ListTag</returns>
         */
        public static ListTag<ITag> Wrap<T>(string name, List<T> list)
        {
            List<ITag> output = new List<ITag>();
            foreach(T t in list)
            {
                output.Add(Wrap(t));
            }
            return new ListTag<ITag>(name, output);
        }


        /**
         * <summary>Wrap all of the objects inside of the list. The same limitations apply as with the #Wrap(object) method.</summary>
         * <param name="list">The list to wrap.</param>
         * <typeparam name="T">The data type of the list.</typeparam>
         * <returns>The wrapped list.</returns>
         */
        public static List<ITag> Wrap<T>(List<T> list)
        {
            List<ITag> output = new List<ITag>();
            foreach(T t in list)
            {
                output.Add(Wrap(t));
            }
            return output;
        }

        /**
         * <summary>Unwrap a list full of tags. This method is not for unwrapping the ListTag to a normal list.</summary>
         * <param name="tags">The list of tags.</param>
         * <typeparam name="T">The data type of the tags. The list must be a uniform data type.</typeparam>
         * <returns>The unwrapped list.</returns>
         */
        public static List<T> UnWrapList<T>(List<Tag<T>> tags)
        {
            List<T> output = new List<T>();
            foreach(Tag<T> tag in tags)
            {
                output.Add(UnWrap(tag));
            }
            return output;
        }

        /**
         * <summary>Unwrap a ListTag into a list.</summary>
         * <param name="list">The ListTag to convert back into a list. The contents of the list are automatically unwrapped.</param>
         * <typeparam name="T">The data type of the list.</typeparam>
         * <returns>The unwrapped ListTag.</returns>
         */
        public static List<T> UnWrapListTag<T>(ListTag<Tag<T>> list)
        {
            List<T> output = new List<T>();
            foreach(Tag<T> tag in list.GetValue())
            {
                output.Add(UnWrap(tag));
            }
            return output;
        }

        /**
         * <summary>Serialize an object.</summary>
         * <param name="name">The name of the object.</param>
         * <param name="obj">The object to serialize.</param>
         * <returns>The serialized ObjectTag</returns>
         */
        public static ObjectTag Serialize(string name, object obj)
        {
            ObjectTag objectTag = new ObjectTag(name);
            Type type = obj.GetType();
            objectTag.AddTag(new StringTag("ODS_TAG", type.FullName));
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach(FieldInfo field in type.GetFields(flags))
            {
                if (field.GetCustomAttribute(typeof(ODSSerializeable)) == null) continue;
                objectTag.AddTag(Wrap(field.Name, field.GetValue(obj)));
            }

            if (objectTag.GetValue().Count < 2)
                throw new Exception("Cannot serialize object: No serializable fields detected!");
            return objectTag;
        }

        /**
         * <summary>Deserialize an ObjectTag back into an object.</summary>
         * <param name="tag">The object tag.</param>
         * <typeparam name="T">The Class to deserialize into.</typeparam>
         * <returns>The deserialized object.</returns>
         */
        public static T Deserialize<T>(ObjectTag tag)
        {
            if (!tag.HasTag("ODS_TAG"))
                throw new Exception("Cannot deserialze tag: This tag was not serialzed!");

            string classname = ((StringTag)tag.GetTag("ODS_TAG")).GetValue();
            Assembly asm = Assembly.GetEntryAssembly();
            Type mainType = asm.GetType(classname);

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            object obj = Activator.CreateInstance(mainType);
            foreach(FieldInfo field in mainType.GetFields(flags))
            {
                if (field.GetCustomAttribute(typeof(ODSSerializeable)) == null) continue;
                ITag fTag = tag.GetTag(field.Name);
                field.SetValue(obj, unwrap(fTag));
            }

            return (T) obj;
        }

        /**
         * <summary>This method is to unwrap tags manualy. Due to the limitations of C# I cannot use the UnWrap methods and generics cannot have wildcards.</summary>
         */
        private static object unwrap(ITag tag)
        {
            if (tag.GetType() == typeof(ObjectTag))
            {
                ObjectTag objTag = (ObjectTag)tag;
                StringTag clazzName = (StringTag)objTag.GetTag("ODS_TAG");
                if (clazzName == null) throw new Exception("Cannot unwrap object: TagObject is not a serialized object");
                return Deserialize<object>(objTag);
            }
            if(tag.GetType() == typeof(ByteTag))
            {
                return ((ByteTag)tag).GetValue();
            }
            if (tag.GetType() == typeof(CharTag))
            {
                return ((CharTag)tag).GetValue();
            }
            if (tag.GetType() == typeof(DoubleTag))
            {
                return ((DoubleTag)tag).GetValue();
            }
            if (tag.GetType() == typeof(FloatTag))
            {
                return ((FloatTag)tag).GetValue();
            }
            if (tag.GetType() == typeof(IntTag))
            {
                return ((IntTag)tag).GetValue();
            }
            if (tag.GetType() == typeof(LongTag))
            {
                return ((LongTag)tag).GetValue();
            }
            if (tag.GetType() == typeof(ShortTag))
            {
                return ((ShortTag)tag).GetValue();
            }
            if (tag.GetType() == typeof(StringTag))
            {
                return ((StringTag)tag).GetValue();
            }
            return null;
        }

        /**
         * <summary>Register a custom tag into the system. (This tag must respect the reserved tag ids).</summary>
         * 
         * <param name="tag">The tag class to add.</param>
         */
        public static void RegisterCustomTag(ITag tag)
        {
            if(tag.GetID() < 15)
            {
                throw new ODSException("Invalid Tag ID. ID cannot be 0 - 15");
            }
            customTags.Add(tag);
        }

        /**
         * <summary>Get the list of custom tags.</summary>
         * 
         * <returns>The list of custom tags.</returns>
         */
        public static List<ITag> GetCustomTags()
        {
            return customTags;
        }

        /**
         * <summary>
         * Informs ods whether or not to thrown an error when it comes across an undefined custom tag.
         * <para>Only enable this when developing something like a visualizer program.</para>
         * </summary>
         * 
         * <param name="value">Whether or not to thrown an error when it comes across an undefined custom tag.</param>
         */
        public static void AllowUndefinedTags(bool value)
        {
            ignoreInvalidCustomTags = value;
        }
    }
}

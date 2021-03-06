﻿using ODS.Exceptions;
using ODS.Tags;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace ODS
{
    /**
     * <summary>An internal class to handle ODS byte information and turn it into tags.</summary>
     */
    class TagBuilder
    {
        private int dataType;
        private int dataSize;
        private long startingIndex;
        private string name;
        private int nameSize;
        private byte[] valueBytes;

        public TagBuilder()
        {
            this.dataSize = -1;
            this.dataType = -1;
            this.startingIndex = -1;
            this.name = "";
            this.valueBytes = new byte[0];
        }

        public void setDataType(int dataType)
        {
            this.dataType = dataType;
        }

        public int getDataType()
        {
            return dataType;
        }

        public void setDataSize(int size)
        {
            this.dataSize = size;
        }

        public int getDataSize()
        {
            return dataSize;
        }

        public void setStartingIndex(long startingIndex)
        {
            this.startingIndex = startingIndex;
        }

        public long getStartingIndex()
        {
            return startingIndex;
        }

        public void setName(string s)
        {
            this.name = s;
        }

        public string getName()
        {
            return this.name;
        }

        public void setNameSize(int size)
        {
            this.nameSize = size;
        }

        public int getNameSize()
        {
            return this.nameSize;
        }

        public void setValueBytes(byte[] valueBytes)
        {
            this.valueBytes = valueBytes;
        }

        public byte[] getValueBytes()
        {
            return this.valueBytes;
        }

        public ITag proccess()
        {
            switch (getDataType())
            {
                case 1:
                    return new StringTag(this.name, "").CreateFromData(this.valueBytes);
                case 2:
                    return new IntTag(this.name, 0).CreateFromData(this.valueBytes);
                case 3:
                    return new FloatTag(this.name, 0).CreateFromData(this.valueBytes);
                case 4:
                    return new DoubleTag(this.name, 0).CreateFromData(this.valueBytes);
                case 5:
                    return new ShortTag(this.name, 0).CreateFromData(this.valueBytes);
                case 6:
                    return new LongTag(this.name, 0).CreateFromData(this.valueBytes);
                case 7:
                    return new CharTag(this.name, ' ').CreateFromData(this.valueBytes);
                case 8:
                    return new ByteTag(this.name, 0).CreateFromData(this.valueBytes);
                case 9:
                    return new ListTag<ITag>(this.name, new List<ITag>()).CreateFromData(this.valueBytes);
                case 10:
                    return new DictionaryTag<ITag>(this.name, new Dictionary<string, ITag>()).CreateFromData(this.valueBytes);
                case 11:
                    return new ObjectTag(this.name).CreateFromData(this.valueBytes);
                case 12:
                    return new CompressedObjectTag(this.name).CreateFromData(this.valueBytes);
                default:
                    foreach (ITag tag in ODSUtil.GetCustomTags())
                    {
                        if (getDataType() != tag.GetID()) continue;
                        Type t = tag.GetType();
                        ITag construct = (ITag) t.GetConstructor(new Type[] { typeof(string), typeof(byte[]) }).Invoke(new object[] { name, valueBytes });
                        return construct;
                    }
                    if (!ODSUtil.ignoreInvalidCustomTags)
                        throw new ODSException("Error: That data type does not exist! " + getDataType());
                    else
                        return new InvalidTag(name, null).CreateFromData(valueBytes);
            }
        }
    }
}

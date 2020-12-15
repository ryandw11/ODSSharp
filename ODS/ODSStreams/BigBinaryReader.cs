using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ODS.ODSStreams
{
    /**
     * <summary>This is similar to BinaryReader but in Big Endian format, which is what ODS uses.</summary>
     */
    public class BigBinaryReader : BinaryReader
    {
        public BigBinaryReader(Stream stream) : base(stream) { }

        public override int ReadInt32()
        {
            var data = base.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }

        public Int16 ReadInt16()
        {
            var data = base.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }

        public Int64 ReadInt64()
        {
            var data = base.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToInt64(data, 0);
        }

        public float ReadFloat()
        {
            var data = base.ReadBytes(4);
            Array.Reverse(data);
            return Convert.ToSingle(BitConverter.ToDouble(data, 0));
        }

        public override Double ReadDouble()
        {
            var data = base.ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToDouble(data, 0);
        }
    }
}

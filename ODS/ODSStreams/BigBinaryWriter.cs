using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ODS.ODSStreams
{
    /**
     * <summary>This is similar to BinaryWriter, but it writes in Big Endian format, which is what ODS uses.</summary>
     */
    public class BigBinaryWriter : BinaryWriter
    {
        public BigBinaryWriter(System.IO.Stream stream) : base(stream) { }

        public override void Write(Int16 value)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] data = BitConverter.GetBytes(value);
                Array.Reverse(data);
                base.Write(data);
            }
            else
            {
                base.Write(BitConverter.GetBytes(value));
            }
        }

        public override void Write(Int32 value)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] data = BitConverter.GetBytes(value);
                Array.Reverse(data);
                base.Write(data);
            }
            else
            {
                base.Write(BitConverter.GetBytes(value));
            }
        }

        public override void Write(Int64 value)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] data = BitConverter.GetBytes(value);
                Array.Reverse(data);
                base.Write(data);
            }
            else
            {
                base.Write(BitConverter.GetBytes(value));
            }
        }

        public override void Write(Double value)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] data = BitConverter.GetBytes(value);
                Array.Reverse(data);
                base.Write(data);
            }
            else
            {
                base.Write(BitConverter.GetBytes(value));
            }
        }

        public override void Write(float value)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] data = BitConverter.GetBytes(value);
                Array.Reverse(data);
                base.Write(data);
            }
            else
            {
                base.Write(BitConverter.GetBytes(value));
            }
        }
    }
}

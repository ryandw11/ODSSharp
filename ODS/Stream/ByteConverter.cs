using System;
using System.Collections.Generic;
using System.Text;

namespace ODS.Stream
{
    /**
     * <summary>This utility class swaps the Endianess of the formats.</summary>
     */
    public class ByteConverter
    {
        public static Int32 ToInt32(byte[] bits)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bits);
                return BitConverter.ToInt32(bits, 0);
            }
            else
            {
                return BitConverter.ToInt32(bits, 0);
            }
        }

        public static Int16 ToInt16(byte[] bits)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bits);
                return BitConverter.ToInt16(bits, 0);
            }
            else
            {
                return BitConverter.ToInt16(bits, 0);
            }
        }

        public static Int64 ToInt64(byte[] bits)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bits);
                return BitConverter.ToInt64(bits, 0);
            }
            else
            {
                return BitConverter.ToInt64(bits, 0);
            }
        }

        public static Double ToDouble(byte[] bits)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bits);
                return BitConverter.ToDouble(bits, 0);
            }
            else
            {
                return BitConverter.ToDouble(bits, 0);
            }
        }

        public static float ToFloat(byte[] bits)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bits);
                return BitConverter.ToSingle(bits, 0);
            }
            else
            {
                return Convert.ToSingle(BitConverter.ToDouble(bits, 0));
            }
        }
    }
}

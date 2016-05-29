using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BitConverterExtention
{
    public static class BitConverterExtention
    {
        public static byte[] ToBytes(this char value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(this bool value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(this byte value)
        {
            return new byte[1] { value };
        }

        public static byte[] ToBytes(this short value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(this ushort value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(this int value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(this uint value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(this long value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(this ulong value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(this float value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] ToBytes(this double value)
        {
            return BitConverter.GetBytes(value);
        }

        public static bool ToBoolean(this byte[] value, int startIndex)
        {
            return value[startIndex] != 0;
        }

        public static char ToChar(this byte[] value, int startIndex)
        {
            return BitConverter.ToChar(value, startIndex);
        }

        public static double ToDouble(this byte[] value, int startIndex)
        {
            return BitConverter.ToDouble(value, startIndex);
        }

        public static short ToInt16(this byte[] value, int startIndex)
        {
            return BitConverter.ToInt16(value, startIndex);
        }

        public static int ToInt32(this byte[] value, int startIndex)
        {
            return BitConverter.ToInt32(value, startIndex);
        }

        public static long ToInt64(this byte[] value, int startIndex)
        {
            return BitConverter.ToInt64(value, startIndex);
        }

        public static float ToSingle(this byte[] value, int startIndex)
        {
            return BitConverter.ToSingle(value, startIndex);
        }

        public static ushort ToUInt16(this byte[] value, int startIndex)
        {
            return BitConverter.ToUInt16(value, startIndex);
        }

        public static uint ToUInt32(this byte[] value, int startIndex)
        {
            return BitConverter.ToUInt32(value, startIndex);
        }
        public static string ToHexString(this byte[] value)
        {
            return BitConverter.ToString(value);
        }
        public static string[] ToHexStrings(this byte[] value)
        {
            return BitConverter.ToString(value).Split('-');
        }
        public static string ToHexString(this byte[] value, int startIndex)
        {
            return BitConverter.ToString(value, startIndex);
        }
        public static string[] ToHexStrings(this byte[] value, int startIndex)
        {
            return BitConverter.ToString(value, startIndex).Split('-');
        }
        public static string ToHexString(this byte[] value, int startIndex, int length)
        {
            return BitConverter.ToString(value, startIndex, length);
        }
        public static string[] ToHexStrings(this byte[] value, int startIndex, int length)
        {
            return BitConverter.ToString(value, startIndex, length).Split('-');
        }
        public static byte[] ToByteArray(this string value)
        {
            return value.ToCharArray().Select(Convert.ToByte).ToArray();
        }
        public static byte[] ToByteArray<T>(this T structure) where T : struct
        {
            byte[] bb = new byte[Marshal.SizeOf(typeof(T))];
            GCHandle gch = GCHandle.Alloc(bb, GCHandleType.Pinned);
            Marshal.StructureToPtr(structure, gch.AddrOfPinnedObject(), false);
            gch.Free();
            return bb;
        }
    }
}

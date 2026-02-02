using System;
using System.Collections.Generic;
using System.Text;

namespace Beyond.SparkBuffer
{
    public static class BinaryStream
    {
        public static string ReadCString(ref byte[] data, ref int pos)
        {
            int start = pos;
            while (data[pos] != 0) pos++;
            string s = Encoding.UTF8.GetString(data, start, pos - start);
            pos++;
            return s;
        }

        public static int ReadInt32(ref byte[] data, ref int pos)
        {
            var res = BitConverter.ToInt32(data, pos);
            pos += 4;
            return res;
        }

        public static long ReadInt64(ref byte[] data, ref int pos)
        {
            var res = BitConverter.ToInt64(data, pos);
            pos += 8;
            return res;
        }

        public static float ReadSingle(ref byte[] data, ref int pos)
        {
            var res = BitConverter.ToSingle(data, pos);
            pos += 4;
            return res;
        }

        public static double ReadDouble(ref byte[] data, ref int pos)
        {
            var res = BitConverter.ToDouble(data, pos);
            pos += 8;
            return res;
        }
    }
}

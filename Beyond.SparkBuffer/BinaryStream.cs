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

    }
}

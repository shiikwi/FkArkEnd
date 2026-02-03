using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace Beyond.ManifestBinary
{
    public static class Utils
    {
        public static string ReadLenUnicodeString(BinaryReader br)
        {
            int len = br.ReadInt32();
            return Encoding.Unicode.GetString(br.ReadBytes(len * 2));
        }

        public static int[] ReadIntArrayAt(BinaryReader br, long dataPos, int offset)
        {
            long SPos = br.BaseStream.Position;
            br.BaseStream.Position = dataPos + offset;
            int count = br.ReadInt32();
            int[] res = new int[count];
            for (int i = 0; i < count; i++)
                res[i] = br.ReadInt32();
            br.BaseStream.Position = SPos;
            return res;
        }

        public static string ReadStringAt(BinaryReader br, long dataPos, int offset)
        {
            long SPos = br.BaseStream.Position;
            br.BaseStream.Position = dataPos + offset;

            int len = br.ReadInt32();
            var res = Encoding.Unicode.GetString(br.ReadBytes(len));
            br.BaseStream.Position = SPos;
            return res;
        }

        public static string ReadCompressStringAt(BinaryReader br, long dataPos, int offset)
        {
            long SPos = br.BaseStream.Position;
            br.BaseStream.Position = dataPos + offset;
            int CompressLen = br.ReadInt32();
            byte[] rawData = br.ReadBytes(CompressLen);
            br.BaseStream.Position = SPos;
            var Data = DecompressDataToMemory(rawData);
            return Encoding.Unicode.GetString(Data);
        }

        public static byte[] DecompressDataToMemory(byte[] CompressData)
        {
            using (var ms = new MemoryStream(CompressData))
            using (var brotli = new BrotliStream(ms, CompressionMode.Decompress))
            using (var oms = new MemoryStream())
            {
                brotli.CopyTo(oms);
                byte[] res = oms.ToArray();

                return res;
            }
        }

    }
}

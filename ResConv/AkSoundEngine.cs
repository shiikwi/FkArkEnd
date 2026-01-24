using System;
using System.Collections.Generic;
using System.Text;

namespace ResConv
{
    public class AkSoundEngine
    {
        private const uint ENC_MAGIC = 0X4478293A;   //:)xD
        private const uint PLAIN_MAGIC = 0X4B504B41; //AKPK
        private const uint MULTIPLIER = 0x4E11C23;
        private const uint XOR_CONSTANT = 0X9C5A0B29;


        public static void LoadPCKPack(string inFile)
        {
            byte[] inData = File.ReadAllBytes(inFile);
            var outDir = Path.Combine(Path.GetDirectoryName(inFile)!, Path.GetFileNameWithoutExtension(inFile) + "Unpack");

            uint magic = BitConverter.ToUInt32(inData, 0);
            if (magic == PLAIN_MAGIC)
            {
                Console.WriteLine($"has been decrypted");
            }
            else if (magic != ENC_MAGIC)
            {
                throw new Exception("not a valid PCK file");
            }

            uint entrySize = BitConverter.ToUInt32(inData, 4);
            int headerLen = 12;
            DecryptPayLoad(inData, headerLen, entrySize, (int)entrySize);
            File.WriteAllBytes(inFile + ".dec", inData);
            var pck = new AkPckFile();
            pck.ReadPack(inData, outDir);
        }

        public static unsafe void DecryptPayLoad(byte[] data, int offset, uint seed, int len)
        {
            fixed (byte* pBase = data)
            {
                byte* pData = pBase + offset;
                int dwordCount = len / 4;

                uint currentSeed = seed;
                for (int i = 0; i < dwordCount; i++)
                {
                    uint key = GenXorKey(currentSeed);
                    uint* pInt = (uint*)(pData + (i * 4));
                    *pInt ^= key;
                    currentSeed++;
                }

                int remain = len % 4;
                if (remain > 0)
                {
                    uint key = GenXorKey(currentSeed);
                    byte* pByte = pData + (dwordCount * 4);
                    byte[] keyBytes = BitConverter.GetBytes(key);
                    for (int j = 0; j < remain; j++)
                    {
                        pByte[j] ^= keyBytes[j];
                    }
                }
            }
        }

        private static uint GenXorKey(uint seed)
        {
            uint v1 = (seed & 0xFF) ^ XOR_CONSTANT;
            uint v2 = MULTIPLIER * v1;
            uint v3 = v2 ^ ((seed >> 8) & 0xFF);
            uint v4 = MULTIPLIER * v3;
            uint v5 = v4 ^ ((seed >> 16) & 0xFF);
            uint v6 = MULTIPLIER * v5;
            uint v7 = v6 ^ (seed >> 24);
            return MULTIPLIER * v7;
        }

    }
}

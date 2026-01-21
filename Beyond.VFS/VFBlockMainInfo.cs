using System;
using System.Collections.Generic;
using System.Text;

namespace Beyond.VFS
{
    public struct FVFBlockChunkInfo
    {
        public string Md5Name;  //UInt128
        public string ContentMD5;  //UInt128
        public long Length;
        public byte BlockType;
        public List<FVFBlockFileInfo> Files;
        public FVFBlockChunkInfo()
        {
            Files = new List<FVFBlockFileInfo>();
        }
    }
    public struct FVFBlockFileInfo
    {
        public string FileName;
        public string FileNameHash;  //long
        public string FileChunkMD5Name;  //UInt128
        public string FileDataMD5;  //UInt128
        public long Offset;
        public long Len;
        public byte BlockType;
        public bool BUseEncrypt;
        public long IvSeed;
    }


    public class VFBlockMainInfo
    {
        public int Version;
        public string GroupCfgName;
        public string GroupCfgHashName;  //long
        public int GroupFileInfoNum;
        public long GroupChunksLength;
        public byte BlockType;
        public List<FVFBlockChunkInfo> AllChunks = new List<FVFBlockChunkInfo>();

        public static VFBlockMainInfo ReadFromByteBuf(byte[] decData)
        {
            using (MemoryStream ms = new MemoryStream(decData))
            using (var br = new BinaryReader(ms))
            {
                var main = new VFBlockMainInfo();

                main.Version = br.ReadInt32();
                br.BaseStream.Seek(12, SeekOrigin.Current); //Skip ChaCha iv Region
                main.GroupCfgName = ReadVfsString(br);
                main.GroupCfgHashName = ReadBEInt64(br);
                main.GroupFileInfoNum = br.ReadInt32();
                main.GroupChunksLength = br.ReadInt64();
                main.BlockType = br.ReadByte();

                int chunkCount = br.ReadInt32();
                for (int i = 0; i < chunkCount; i++)
                {
                    var chunk = new FVFBlockChunkInfo();
                    chunk.Md5Name = ReadBEUInt128(br);
                    chunk.ContentMD5 = ReadBEUInt128(br);
                    chunk.Length = br.ReadInt64();
                    chunk.BlockType = br.ReadByte();

                    int fileCount = br.ReadInt32();
                    for (int j = 0; j < fileCount; j++)
                    {
                        var file = new FVFBlockFileInfo();
                        file.FileName = ReadVfsString(br);
                        file.FileNameHash = ReadBEInt64(br);
                        file.FileChunkMD5Name = ReadBEUInt128(br);
                        file.FileDataMD5 = ReadBEUInt128(br);
                        file.Offset = br.ReadInt64();
                        file.Len = br.ReadInt64();
                        file.BlockType = br.ReadByte();
                        file.BUseEncrypt = br.ReadBoolean();

                        if (file.BUseEncrypt)
                        {
                            file.IvSeed = br.ReadInt64();
                        }

                        chunk.Files.Add(file);
                    }
                    main.AllChunks.Add(chunk);
                }
                return main;
            }
        }

        private static string ReadVfsString(BinaryReader br)
        {
            short len = br.ReadInt16();
            if (len <= 0) return string.Empty;
            byte[] strBytes = br.ReadBytes(len);
            return Encoding.UTF8.GetString(strBytes);
        }

        private static string ReadBEInt64(BinaryReader br)
        {
            byte[] data = br.ReadBytes(8);
            Array.Reverse(data);
            int startIndex = 0;
            while (startIndex < data.Length && (data[startIndex] == 0x00 || data[startIndex] == 0xFF))
            {
                startIndex++;
            }
            if (startIndex == data.Length)
            {
                return "00";
            }
            byte[] trimmed = new byte[data.Length - startIndex];
            Array.Copy(data, startIndex, trimmed, 0, trimmed.Length);
            Array.Reverse(trimmed);
            return BitConverter.ToString(trimmed).Replace("-", "");
        }

        private static string ReadBEUInt128(BinaryReader br)
        {
            byte[] high = br.ReadBytes(8);
            byte[] low = br.ReadBytes(8);
            return BitConverter.ToString(high).Replace("-", "") + BitConverter.ToString(low).Replace("-", "");
        }

    }
}

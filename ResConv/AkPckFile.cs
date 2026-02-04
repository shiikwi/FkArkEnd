using System;
using System.Text;

namespace AK.Wwise
{
    public class AkPckFile
    {
        public struct PckHead
        {
            public uint Magic;
            public uint Seed;  //EntrySize
            public uint Padding;
            public uint SizeFolder;
            public uint SizeA;  //bnk
            public uint SizeB;  //wem
            public uint SizeC;  //wem
        }

        //public struct PckEntry1
        //{
        //    public uint ID;
        //    public uint Flag1;
        //    public uint Size;
        //    public uint Offset;
        //    public uint Flag2;
        //}

        //public struct PckEntry2
        //{
        //    public ulong ID;
        //    public uint Flag1;
        //    public uint Size;
        //    public uint Offset;
        //    public uint Flag2;
        //}

        public struct PckEntry
        {
            public ulong ID;
            public uint Size;
            public uint Offset;
            public bool Is64Bit;
        }


        public void ReadPack(byte[] data, string outDir)
        {
            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                var head = new PckHead
                {
                    Magic = br.ReadUInt32(),
                    Seed = br.ReadUInt32(),
                    Padding = br.ReadUInt32(),
                    SizeFolder = br.ReadUInt32(),
                    SizeA = br.ReadUInt32(),
                    SizeB = br.ReadUInt32(),
                    SizeC = br.ReadUInt32()
                };

                var OffsetA = br.BaseStream.Position + head.SizeFolder;
                var OffsetB = OffsetA + head.SizeA;
                var OffsetC = OffsetB + head.SizeB;
                var OffsetData = OffsetC + head.SizeC;
                var DataLimit = data.Length - OffsetData - 4;

                Console.WriteLine($"OffsetA: {OffsetA:X}; OffsetB: {OffsetB:X}; OffsetC: {OffsetC:X}; OffsetData: {OffsetData:X}");
                List<PckEntry> AllEntry = new List<PckEntry>();

                if (head.SizeA > 4)
                {
                    br.BaseStream.Position = OffsetA;
                    AllEntry.AddRange(ReadEntries(br, false));
                }
                if (head.SizeB > 4)
                {
                    br.BaseStream.Position = OffsetB;
                    AllEntry.AddRange(ReadEntries(br, false));
                }
                if (head.SizeC > 4)
                {
                    br.BaseStream.Position = OffsetC;
                    AllEntry.AddRange(ReadEntries(br, true));
                }

                foreach (var entry in AllEntry)
                {
                    if (entry.Offset >= data.Length)
                    {
                        Console.WriteLine($"check {entry.ID:X}");
                        continue;
                    }
                    var content = new byte[entry.Size];
                    Array.Copy(data, entry.Offset, content, 0, entry.Size);
                    AkSoundEngine.DecryptPayLoad(content, 0, (uint)entry.ID, content.Length);
                    CheckSigSave(content, entry.ID, outDir);
                }
            }
        }

        private List<PckEntry> ReadEntries(BinaryReader br, bool is64Bit)
        {
            List<PckEntry> entries = new List<PckEntry>();
            uint count = br.ReadUInt32();

            for (int i = 0; i < count; i++)
            {
                var entry = new PckEntry();
                entry.Is64Bit = is64Bit;
                if (is64Bit)
                {
                    uint low = br.ReadUInt32();
                    uint high = br.ReadUInt32();
                    entry.ID = ((ulong)high << 32) | low;
                    br.ReadUInt32();  //flag1
                    entry.Size = br.ReadUInt32();
                    entry.Offset = br.ReadUInt32();
                    br.ReadUInt32();  //flag2
                }
                else
                {
                    entry.ID = br.ReadUInt32();
                    br.ReadUInt32();  //flag1
                    entry.Size = br.ReadUInt32();
                    entry.Offset = br.ReadUInt32();
                    br.ReadUInt32(); //flag2
                }
                entries.Add(entry);
            }
            return entries;
        }

        private void CheckSigSave(byte[] data, ulong id, string outDir)
        {
            string subDir = "other";
            string ext = ".bin";

            if(data.AsSpan(0, 4).SequenceEqual("RIFF"u8))
            {
                subDir = "wem";
                ext = ".wem";
            }else if(data.AsSpan(0, 4).SequenceEqual("BKHD"u8))
            {
                subDir = "bnk";
                ext = ".bnk";
            }

            string outPath = Path.Combine(outDir, subDir);
            if (!Directory.Exists(outPath)) Directory.CreateDirectory(outPath);
            string fullPath = Path.Combine(outPath, $"{id}{ext}");
            File.WriteAllBytes(fullPath, data);
            //Console.WriteLine($"Extracted {Path.GetFileName(fullPath)}");
        }

    }
}

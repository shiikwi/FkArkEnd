using System.IO.Compression;
using System.Text;

namespace Beyond.ManifestBinary
{
    public class ManifestDataBinary
    {
        public const uint HEAD1 = 0xFF11FF11;  //4279369489
        public const uint HEAD2 = 0XF1F2F3F4;  //4059231220
        public const string VERSION = "1.0.1";
        public ManifestScheme InitBinary(byte[] Encdata)
        {
            var data = Utils.DecompressDataToMemory(Encdata);
            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                var scheme = new ManifestScheme();
                if (br.ReadUInt32() != HEAD1) throw new Exception("Invalid Manifest HEAD1");

                scheme.Version = Utils.ReadLenUnicodeString(br);

                if (br.ReadUInt32() != HEAD2) throw new Exception("Invalid Manifest HEAD2");

                scheme.Hash = Utils.ReadLenUnicodeString(br);
                scheme.perforceCL = Utils.ReadLenUnicodeString(br);

                int assetInfoSize = br.ReadInt32();
                scheme.m_assetInfoAddress = br.BaseStream.Position;
                br.BaseStream.Seek(assetInfoSize, SeekOrigin.Current);

                int bundleSize = br.ReadInt32();
                scheme.m_bundleAddress = br.BaseStream.Position;
                br.BaseStream.Seek(bundleSize, SeekOrigin.Current);

                int bundleArraySize = br.ReadInt32();
                scheme.m_bundleArrayAddress = br.BaseStream.Position;
                br.BaseStream.Seek(bundleArraySize, SeekOrigin.Current);

                scheme.m_dataAddress = br.BaseStream.Position + 4;

                br.BaseStream.Position = scheme.m_bundleArrayAddress;
                int bundleCount = br.ReadInt32();
                for (int i = 0; i < bundleCount; i++)
                {
                    var b = new Bundle();

                    b.bundleIndex = br.ReadInt32();
                    int nameOffset = br.ReadInt32();
                    int depsOffset = br.ReadInt32();
                    int revDepsOffset = br.ReadInt32();
                    int dirDepsOffset = br.ReadInt32();
                    b.bundleFlags = br.ReadInt32();
                    b.hashName = br.ReadInt64();
                    b.hashVersion = br.ReadInt64();
                    b.category = (RootCategory)br.ReadInt32();
                    br.ReadInt32(); //Skip 0x00 padding;
                    long currPos = br.BaseStream.Position;

                    b.name = Utils.ReadStringAt(br, scheme.m_dataAddress, nameOffset);
                    b.dependencies = Utils.ReadIntArrayAt(br, scheme.m_dataAddress, depsOffset);
                    b.directReverseDependencies = Utils.ReadIntArrayAt(br, scheme.m_dataAddress, revDepsOffset);
                    b.directDependencies = Utils.ReadIntArrayAt(br, scheme.m_dataAddress, dirDepsOffset);
                    scheme.Bundles.Add(b);
                    br.BaseStream.Position = currPos;
                }

                br.BaseStream.Seek(scheme.m_assetInfoAddress, SeekOrigin.Begin);
                int assetCapacity = br.ReadInt32();
                br.BaseStream.Seek(assetCapacity * 8, SeekOrigin.Current);  //Skip Buckets
                int assetCount = (int)((scheme.m_assetInfoAddress + assetInfoSize) - br.BaseStream.Position) / 24;
                for (int i = 0; i < assetCount; i++)
                {
                    var info = new AssetInfo();
                    info.pathHashHead = br.ReadInt64();
                    int pathOffset = br.ReadInt32();
                    info.bundleIndex = br.ReadInt32();
                    info.assetSize = br.ReadInt32();
                    br.ReadInt32();

                    info.path = Utils.ReadCompressStringAt(br, scheme.m_dataAddress, pathOffset);
                    scheme.Assets.Add(info);
                }

                return scheme;
            }
        }

    }

}

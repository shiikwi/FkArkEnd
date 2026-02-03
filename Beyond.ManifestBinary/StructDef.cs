using System;
using System.Collections.Generic;
using System.Text;

namespace Beyond.ManifestBinary
{
    public enum RootCategory : byte
    {
        Main = 0,
        Initial = 1,
        E_Num = 2
    }

    public class Bundle
    {
        public int bundleIndex;
        public string name;
        public int[] dependencies;
        public int[] directReverseDependencies;
        public int[] directDependencies;
        public int bundleFlags;
        public long hashName;
        public long hashVersion;
        public RootCategory category;
    }

    public class AssetInfo
    {
        public long pathHashHead;
        public string path;
        public int bundleIndex;
        public int assetSize;
    }

    public class MappingStr
    {
        public long hash;
        public int offset;
    }

    public class ManifestScheme
    {
        public string Version;
        public string Hash;
        public string perforceCL;
        public long m_assetInfoAddress;
        public long m_bundleAddress;
        public long m_bundleArrayAddress;
        public long m_dataAddress;
        public List<Bundle> Bundles = new List<Bundle>();
        public List<AssetInfo> Assets = new List<AssetInfo>();
    }

}

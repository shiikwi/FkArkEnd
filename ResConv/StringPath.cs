using System;
using System.Collections.Generic;
using System.Text;
using Beyond.ManifestBinary;
using Newtonsoft.Json;

namespace Beyond.Resource
{
    public class StringPathHashBinary
    {
        private struct StringPathBin
        {
            public int StringPoolOffset;
            public int Capacity;
            public byte[] Slots;
            public byte[] Nodes;
        }

        public static void InitMain(string inFile)
        {
            var data = File.ReadAllBytes(inFile);
            var dict = InitData(data);

            var json = JsonConvert.SerializeObject(dict, Formatting.Indented);
            var outFile = Path.ChangeExtension(inFile, ".json");
            File.WriteAllText(outFile, json);
        }

        private static Dictionary<long, string> InitData(byte[] data)
        {
            var result = new Dictionary<long, string>();

            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                var strPath = new StringPathBin();
                strPath.StringPoolOffset = br.ReadInt32();
                strPath.Capacity = br.ReadInt32();
                strPath.Slots = br.ReadBytes(strPath.Capacity * 8);

                var mapList = new List<MappingStr>();
                int NodeCount = (int)((strPath.StringPoolOffset - br.BaseStream.Position) / 16);
                for (int i = 0; i < NodeCount; i++)
                {
                    var map = new MappingStr();
                    map.hash = br.ReadInt64();
                    map.offset = br.ReadInt32();
                    br.ReadInt32();  //Skip padding
                    mapList.Add(map);
                }

                foreach (var map in mapList)
                {
                    var str = Beyond.ManifestBinary.Utils.ReadStringAt(br, strPath.StringPoolOffset, map.offset);
                    result[map.hash] = str;
                }

                return result;
            }
        }

    }
}

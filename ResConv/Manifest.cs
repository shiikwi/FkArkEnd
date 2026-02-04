using System;
using System.Collections.Generic;
using System.Text;
using Beyond.ManifestBinary;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ManifestBinary
{
    public class Manifest
    {
        public static void ManifestRead(string inFile)
        {
            var inData = File.ReadAllBytes(inFile);
            var outFile = Path.ChangeExtension(inFile, ".json");
            ManifestDataBinary mani = new ManifestDataBinary();
            var Maniobj = mani.InitBinary(inData);
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter> { new StringEnumConverter() },
            };
            var json = JsonConvert.SerializeObject(Maniobj, settings);
            File.WriteAllText(outFile, json);
        }

    }
}

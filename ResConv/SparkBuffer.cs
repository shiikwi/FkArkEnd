using System;
using System.Collections.Generic;
using System.Text;
using Beyond.SparkBuffer;

namespace SparkBuffer
{
    public class SparkBufferManagement
    {
        public static void Deserialize(string inPath)
        {
            var data = File.ReadAllBytes(inPath);
            var spark = new SparkDeserializer(data);
            var json = spark.Load().ToString();

            var outPath = Path.ChangeExtension(inPath, ".json");
            File.WriteAllText(outPath, json);
        }
    }
}

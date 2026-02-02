using System;
using System.Collections.Generic;
using System.Text;

namespace Beyond.SparkBuffer.Runtime
{
    public class Field
    {
        public string Name;
        public SparkType Type;
        public int Offset;
        public virtual int GetSize() => Type switch
        {
            SparkType.Bool => 1,
            SparkType.Byte => 1,
            SparkType.Int => 4,
            SparkType.Float => 4,
            SparkType.Enum => 4,
            SparkType.Long => 8,
            SparkType.Double => 8,
            _ => 4
        };
    }

    public class ArrayField : Field
    {
        public SparkType ElementType;
        public int ElementTypeHash;
    }

    public class MapField : Field
    {
        public SparkType KeyType;
        public int KeyTypeHash;
        public SparkType ValueType;
        public int ValueTypeHash;
    }

    public class BeanField : Field
    {
        public BeanType beanType = new BeanType();
    }

    public class BeanType
    {
        public int TypeHash;
        public string Name;
    }

    public class SparkBean
    {
        public BeanType beanType = new BeanType();
        public List<Field> Fields = new List<Field>();
    }


    public class SparkEnum
    {
        public int TypeHash;
        public string Name;
        public Dictionary<string, int> Items = new Dictionary<string, int>();
        public Dictionary<int, string> ValueToName = new Dictionary<int, string>();
    }

    public class SparkRoot
    {
        public string rootName;
        public SparkType rootType;
        public int rootTypeHash;
        public SparkType subType1;
        public int subTypeHash1;
        public SparkType subType2;
        public int subTypeHash2;
    }

    public struct HashSlots
    {
        public int Offset;
        public int BucktSize;
    }

    public class SparkScheme
    {
        public Dictionary<int, SparkBean> Beans = new Dictionary<int, SparkBean>();
        public Dictionary<int, SparkEnum> Enums = new Dictionary<int, SparkEnum>();

        public SparkRoot Root = new SparkRoot();
        public Dictionary<int, string> StringPool = new Dictionary<int, string>();
    }
}

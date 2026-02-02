using Beyond.SparkBuffer.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Beyond.SparkBuffer
{
    public class SparkDeserializer
    {
        private byte[] data;
        private int _typeDefsPtr;
        private int _rootDefPtr;
        private int _dataPtr;
        private int _stringPtr;

        public SparkScheme scheme = new SparkScheme();

        public SparkDeserializer(byte[] bytes)
        {
            data = bytes;
        }

        public JToken Load()
        {
            ReadHeader();
            ExportTypeDefs();
            ExportRootDef();
            ExportStrings();
            return ExportDataFromRoot();
        }


        private void ReadHeader()
        {
            _typeDefsPtr = BitConverter.ToInt32(data, 0);
            _rootDefPtr = BitConverter.ToInt32(data, 4);
            _dataPtr = BitConverter.ToInt32(data, 8);
        }

        private void ExportTypeDefs()
        {
            int pos = Utils.Align(_typeDefsPtr, 4);
            int count = BinaryStream.ReadInt32(ref data, ref pos);

            for (int i = 0; i < count; i++)
            {
                byte tag = data[pos++];
                pos = Utils.Align(pos, 4);

                if (tag == (byte)SparkType.Bean)
                {
                    var bean = new SparkBean();
                    bean.beanType.TypeHash = BinaryStream.ReadInt32(ref data, ref pos);
                    bean.beanType.Name = BinaryStream.ReadCString(ref data, ref pos);
                    pos = Utils.Align(pos, 4);
                    int fieldCount = BinaryStream.ReadInt32(ref data, ref pos);
                    int currentPos = 0;
                    for (int j = 0; j < fieldCount; j++)
                    {
                        var field = ParseField(ref pos, ref currentPos);
                        bean.Fields.Add(field);
                    }

                    scheme.Beans[bean.beanType.TypeHash] = bean;
                }
                else if (tag == (byte)SparkType.Enum)
                {
                    var enu = new SparkEnum();
                    enu.TypeHash = BinaryStream.ReadInt32(ref data, ref pos);
                    enu.Name = BinaryStream.ReadCString(ref data, ref pos);
                    pos = Utils.Align(pos, 4);
                    int itemCount = BinaryStream.ReadInt32(ref data, ref pos);

                    for (int j = 0; j < itemCount; j++)
                    {
                        var fname = BinaryStream.ReadCString(ref data, ref pos);
                        pos = Utils.Align(pos, 4);
                        var fvalue = BinaryStream.ReadInt32(ref data, ref pos);
                        enu.Items[fname] = fvalue;
                    }

                    scheme.Enums[enu.TypeHash] = enu;
                }
            }
        }

        private void ExportRootDef()
        {
            int pos = _rootDefPtr;
            var root = new SparkRoot();
            root.rootType = (SparkType)data[pos++];
            root.rootName = BinaryStream.ReadCString(ref data, ref pos);
            switch (root.rootType)
            {
                case SparkType.Bean:
                    {
                        pos = Utils.Align(pos, 4);
                        root.rootTypeHash = BinaryStream.ReadInt32(ref data, ref pos);
                        break;
                    }
                case SparkType.Array:
                    {
                        root.subType1 = (SparkType)data[pos++];
                        if (root.subType1 == SparkType.Enum || root.subType1 == SparkType.Bean)
                        {
                            root.subTypeHash1 = BinaryStream.ReadInt32(ref data, ref pos);
                            root.rootTypeHash = root.subTypeHash1;
                        }
                        break;
                    }
                case SparkType.Map:
                    {
                        root.subType1 = (SparkType)data[pos++];
                        if (root.subType1 == SparkType.Enum)
                        {
                            pos = Utils.Align(pos, 4);
                            root.subTypeHash1 = BinaryStream.ReadInt32(ref data, ref pos);
                        }
                        root.subType2 = (SparkType)data[pos++];
                        if (root.subType2 == SparkType.Enum || root.subType2 == SparkType.Bean)
                        {
                            pos = Utils.Align(pos, 4);
                            root.subTypeHash2 = BinaryStream.ReadInt32(ref data, ref pos);
                        }
                        break;
                    }
            }
            scheme.Root = root;
            _stringPtr = pos;
        }


        private void ExportStrings()
        {
            int pos = Utils.Align(_stringPtr, 4);
            int StringCount = BinaryStream.ReadInt32(ref data, ref pos);
            //int bytes = data[pos];
            //if (bytes == 0) pos++;  // SparkType.Map skip 0x00
            for (int i = 0; i < StringCount; i++)
            {
                scheme.StringPool[pos] = BinaryStream.ReadCString(ref data, ref pos);
            }
        }

        private JToken ExportDataFromRoot()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter> { new StringEnumConverter() },
            };


#if true
            switch (scheme.Root.rootType)
            {
                case SparkType.Bean:
                    return ExportBeanData(_dataPtr, scheme.Root.rootTypeHash);
                case SparkType.Map:
                    return ExportMapData(_dataPtr, scheme.Root.subType1, scheme.Root.subTypeHash1, scheme.Root.subType2, scheme.Root.subTypeHash2);
                case SparkType.Array:
                    return ExportArrayData(_dataPtr, scheme.Root.subType1, scheme.Root.rootTypeHash);
                default:
                    return "Unknown Root type";
            }
#else
            return JsonConvert.SerializeObject(new
            {
                Header = new
                {
                    TypeDefsPtr = _typeDefsPtr,
                    RootDefPtr = _rootDefPtr,
                    DataPtr = _dataPtr
                },
                Root = scheme.Root,
                Enums = scheme.Enums,
                Beans = scheme.Beans,
                StringPool = scheme.StringPool
            }, settings);
#endif
        }

        private Field ParseField(ref int pos, ref int currentPos)
        {
            var fname = BinaryStream.ReadCString(ref data, ref pos);
            var type = (SparkType)data[pos++];

            Field field;

            switch (type)
            {
                case SparkType.Bean:
                case SparkType.Enum:
                    {
                        pos = Utils.Align(pos, 4);
                        field = new BeanField { beanType = new BeanType { TypeHash = BinaryStream.ReadInt32(ref data, ref pos) } };
                        break;
                    }

                case SparkType.Array:
                    {
                        var arrfield = new ArrayField();
                        arrfield.ElementType = (SparkType)data[pos++];
                        if (arrfield.ElementType == SparkType.Enum || arrfield.ElementType == SparkType.Bean)
                        {
                            pos = Utils.Align(pos, 4);
                            arrfield.ElementTypeHash = BinaryStream.ReadInt32(ref data, ref pos);
                        }
                        field = arrfield;
                        break;
                    }
                case SparkType.Map:
                    {
                        var mapfield = new MapField();
                        mapfield.KeyType = (SparkType)data[pos++];
                        if (mapfield.KeyType == SparkType.Enum || mapfield.KeyType == SparkType.Bean)
                        {
                            pos = Utils.Align(pos, 4);
                            mapfield.KeyTypeHash = BinaryStream.ReadInt32(ref data, ref pos);
                        }
                        mapfield.ValueType = (SparkType)data[pos++];
                        if (mapfield.ValueType == SparkType.Enum || mapfield.ValueType == SparkType.Bean)
                        {
                            pos = Utils.Align(pos, 4);
                            mapfield.ValueTypeHash = BinaryStream.ReadInt32(ref data, ref pos);
                        }
                        field = mapfield;
                        break;
                    }
                default:
                    field = new Field();
                    break;
            }

            field.Name = fname;
            field.Type = type;
            int align = Utils.GetAlignment(type);
            currentPos = Utils.Align(currentPos, align);
            field.Offset = currentPos;
            currentPos += field.GetSize();

            return field;
        }

        private JObject ExportMapData(int addr, SparkType keyType, int keyTypeHash, SparkType valueType, int valueTypeHash)
        {
            int pos = addr;
            var result = new JObject();
            var slots = new List<HashSlots>();
            int slotCount = BinaryStream.ReadInt32(ref data, ref pos);
            for (int i = 0; i < slotCount; i++)
            {
                var solt = new HashSlots();
                solt.Offset = BinaryStream.ReadInt32(ref data, ref pos);
                solt.BucktSize = BinaryStream.ReadInt32(ref data, ref pos);
                slots.Add(solt);
            }
            foreach (var slot in slots)
            {
                if (slot.BucktSize <= 0) continue;
                int entryPos = slot.Offset;
                for (int j = 0; j < slot.BucktSize; j++)
                {
                    JToken key = ExportElementDat(keyType, keyTypeHash, ref entryPos);
                    JToken val = ExportElementDat(valueType, valueTypeHash, ref entryPos);
                    result[key.ToString()] = val;
                }
            }
            return result;
        }

        private JObject ExportBeanData(int addr, int typeHash)
        {
            if (!scheme.Beans.TryGetValue(typeHash, out var bean))
                return new JObject { ["_error"] = $"Type {typeHash} not found" };
            var jobj = new JObject();

            foreach (var field in bean.Fields)
            {
                int fieldOffset = field.Offset + addr;

                int refHash = 0;
                if (field is BeanField bf) refHash = bf.beanType.TypeHash;
                else if (field is ArrayField af) { refHash = af.ElementTypeHash; }
                else if (field is MapField mf) { }

                jobj[field.Name] = ExportElementDat(field.Type, refHash, ref fieldOffset, field);
            }
            return jobj;
        }

        private JArray ExportArrayData(int addr, SparkType type, int typeHash)
        {
            int pos = addr;
            int align = Utils.GetAlignment(type);
            pos = Utils.Align(pos, align);

            var jarr = new JArray();
            int count = BinaryStream.ReadInt32(ref data, ref pos);
            for (int i = 0; i < count; i++)
            {
                jarr.Add(ExportElementDat(type, typeHash, ref pos));
            }
            return jarr;
        }

        private JToken ExportElementDat(SparkType type, int typeHash, ref int currentPos, Field field = null)
        {
            int align = Utils.GetAlignment(type);
            currentPos = Utils.Align(currentPos, align);

            switch (type)
            {
                case SparkType.Bool:
                    return data[currentPos++] != 0;
                case SparkType.Byte:
                    return data[currentPos++];
                case SparkType.Int:
                    return BinaryStream.ReadInt32(ref data, ref currentPos);
                case SparkType.Long:
                    return BinaryStream.ReadInt64(ref data, ref currentPos);
                case SparkType.Float:
                    return BinaryStream.ReadSingle(ref data, ref currentPos);
                case SparkType.Double:
                    return BinaryStream.ReadDouble(ref data, ref currentPos);
                case SparkType.Enum:
                    {
                        int val = BinaryStream.ReadInt32(ref data, ref currentPos);
                        if (scheme.Enums.TryGetValue(typeHash, out var enu))
                        {
                            var pair = enu.Items.FirstOrDefault(kvp => kvp.Value == val);
                            if (pair.Key != null) return pair.Key;
                        }
                        return val;
                    }
                case SparkType.String:
                    {
                        int Offset = BinaryStream.ReadInt32(ref data, ref currentPos);
                        if (Offset <= 0 || Offset == -1) return null;
                        if (scheme.StringPool.TryGetValue(Offset, out var str))
                            return str;
                        return string.Empty;
                    }
                case SparkType.Bean:
                    {
                        int OffPtr = BinaryStream.ReadInt32(ref data, ref currentPos);
                        if (OffPtr <= 0 || OffPtr == -1) return null;
                        return ExportBeanData(OffPtr, typeHash);
                    }
                case SparkType.Array:
                    {
                        int OffPtr = BinaryStream.ReadInt32(ref data, ref currentPos);
                        if (OffPtr <= 0 || OffPtr == -1) return null;
                        var etype = SparkType.Bean;
                        if (field is ArrayField af) etype = af.ElementType;
                        return ExportArrayData(OffPtr, etype, typeHash);
                    }
                case SparkType.Map:
                    {
                        int OffPtr = BinaryStream.ReadInt32(ref data, ref currentPos);
                        if (OffPtr <= 0 || OffPtr == -1) return null;
                        var mf = (MapField)field;
                        return ExportMapData(OffPtr, mf.KeyType, mf.KeyTypeHash, mf.ValueType, mf.ValueTypeHash);
                    }
                default:
                    return "Parse Error";
            }
        }


    }
}

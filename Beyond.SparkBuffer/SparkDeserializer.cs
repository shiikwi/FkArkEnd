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

        }


        private void ExportStrings()
        {

        }

        private JToken ExportDataFromRoot()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter> { new StringEnumConverter() },
            };

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
                        //var mapfield = new MapField();
                        //mapfield.KeyType = (SparkType)data[pos++];
                        throw new NotImplementedException("check mapping field file");
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
    }
}

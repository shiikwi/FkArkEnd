using System;
using System.Collections.Generic;
using System.Text;

namespace Beyond.SparkBuffer
{
    public enum SparkType : byte
    {
        Bool = 0,
        Byte = 1,
        Int = 2,
        Long = 3,
        Float = 4,
        Double = 5,
        Enum = 6,
        String = 7,
        Bean = 8,
        Array = 9,
        Map = 0x0A
    }

}

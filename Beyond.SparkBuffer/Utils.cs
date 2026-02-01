using System;
using System.Collections.Generic;
using System.Text;

namespace Beyond.SparkBuffer
{
    public static class Utils
    {
        public static int Align(int currPos, int Alim)
        {
            if (Alim <= 1) return currPos;
            int mod = currPos % Alim;
            return mod == 0 ? currPos : currPos + (Alim - mod);
        }

        public static int GetAlignment(SparkType type)
        {
            return type switch
            {
                SparkType.Long => 8,
                SparkType.Double => 8,
                SparkType.Int => 4,
                SparkType.Float => 4,
                SparkType.String => 4,
                SparkType.Bean => 4,
                SparkType.Array => 4,
                SparkType.Map => 4,
                _ => 1
            };
        }

    }
}

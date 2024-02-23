using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngineEditor.utilities
{
    public static class ID
    {
        public static int INVALID_ID => -1;
        public static bool isValid(int id) => id != INVALID_ID;
    }
    public static class MathUtil
    {
        public static float Epsilon => 0.00001f;

        //extention method
        public static bool isTheSameAs(this float value,float other)
        {
            return Math.Abs(value - other) < Epsilon;
        }

        public static bool isTheSameAs(this float? value, float? other)
        {
            if(!value.HasValue || !other.HasValue) return false;
            return Math.Abs(value.Value - other.Value) < Epsilon;
        }
    }
}

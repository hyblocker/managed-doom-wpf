using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace doom_sharpdx {
    public static class MathExtensions {
        public static float Clamp(float value, float min, float max) {
            if ( value.CompareTo(min) < 0 )
                return min;
            else if ( value.CompareTo(max) > 0 )
                return max;
            else
                return value;
        }
        public static int Clamp(int value, int min, int max) {
            if ( value.CompareTo(min) < 0 )
                return min;
            else if ( value.CompareTo(max) > 0 )
                return max;
            else
                return value;
        }
        public static double Clamp(double value, double min, double max) {
            if ( value.CompareTo(min) < 0 )
                return min;
            else if ( value.CompareTo(max) > 0 )
                return max;
            else
                return value;
        }
    }
}

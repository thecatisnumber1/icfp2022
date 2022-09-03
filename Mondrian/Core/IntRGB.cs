using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public struct IntRGB
    {
        public static readonly IMath<IntRGB> MATH = new Math();
        public static readonly IntRGB ZERO = new();

        public int R, G, B, A;

        public IntRGB(int r, int g, int b, int a)
        {
            R = r; G = g; B = b; A = a;
        }

        public static implicit operator IntRGB(RGBA color)
        {
            return new(color.R, color.G, color.B, color.A);
        }

        public static IntRGB operator +(IntRGB e1, IntRGB e2)
        {
            return new(e1.R + e2.R, e1.G + e2.G, e1.B + e2.B, e1.A + e2.A);
        }

        public static IntRGB operator -(IntRGB e1, IntRGB e2)
        {
            return new(e1.R - e2.R, e1.G - e2.G, e1.B - e2.B, e1.A - e2.A);
        }

        public static RGBA operator /(IntRGB e, int denom)
        {
            int div(int num)
            {
                return (2 * num + denom - 1) / (2 * denom);
            }

            return new RGBA(div(e.R), div(e.G), div(e.B), div(e.A));
        }

        private class Math : IMath<IntRGB>
        {
            public IntRGB Add(IntRGB e1, IntRGB e2)
            {
                return e1 + e2;
            }

            public IntRGB Sub(IntRGB e1, IntRGB e2)
            {
                return e1 - e2;
            }

            public IntRGB Zero()
            {
                return IntRGB.ZERO;
            }
        }
    }
}

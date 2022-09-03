namespace Core
{
    public class Image
    {
        private readonly RGBA[,] pixels;
        private readonly SumElement[,] summedAreaTable;

        public Image(RGBA[,] pixels)
        {
            this.pixels = pixels;
            summedAreaTable = new SumElement[pixels.GetLength(0) + 1, pixels.GetLength(1) + 1];

            for (int y = 0; y < Height; y++)
            {
                SumElement rowSum = new();
                for (int x = 0; x < Width; x++)
                {
                    rowSum += pixels[x, y];
                    summedAreaTable[x + 1, y + 1] = summedAreaTable[x + 1, y] + rowSum;
                }
            }
        }

        public RGBA this[Point p] => pixels[p.X, p.Y];

        public RGBA this[int x, int y] => pixels[x, y];

        public int Width => pixels.GetLength(0);

        public int Height => pixels.GetLength(1);

        public RGBA AverageColor(Rectangle rect)
        {
            return (summedAreaTable[rect.Right, rect.Top]
                - summedAreaTable[rect.Left, rect.Top]
                - summedAreaTable[rect.Right, rect.Bottom]
                + summedAreaTable[rect.Left, rect.Bottom]) / rect.Area;
        }

        private struct SumElement
        {
            public int R, G, B, A;

            public SumElement(int r, int g, int b, int a)
            {
                R = r; G = g; B = b; A = a;
            }

            public static implicit operator SumElement(RGBA color)
            {
                return new(color.R, color.G, color.B, color.A);
            }

            public static SumElement operator +(SumElement e1, SumElement e2)
            {
                return new(e1.R + e2.R, e1.G + e2.G, e1.B + e2.B, e1.A + e2.A);
            }

            public static SumElement operator -(SumElement e1, SumElement e2)
            {
                return new(e1.R - e2.R, e1.G - e2.G, e1.B - e2.B, e1.A - e2.A);
            }

            public static RGBA operator /(SumElement e, int denom)
            {
                int div(int num)
                {
                    return (2 * num + denom - 1) / (2 * denom);
                }

                return new RGBA(div(e.R), div(e.G), div(e.B), div(e.A));
            }
        } 
    }
}

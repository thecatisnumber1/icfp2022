namespace Core
{
    public class Image
    {
        private readonly RGBA[,] pixels;
        private readonly SummedAreaTable<IntRGB> summedAreaTable;

        public Image(RGBA[,] pixels)
        {
            this.pixels = pixels;
            IntRGB[,] upgradedPixels = new IntRGB[Width, Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    upgradedPixels[x, y] = pixels[x, y];
                }
            }
            summedAreaTable = new SummedAreaTable<IntRGB>(upgradedPixels, IntRGB.MATH);
        }

        public RGBA this[Point p] => pixels[p.X, p.Y];

        public RGBA this[int x, int y] => pixels[x, y];

        public int Width => pixels.GetLength(0);

        public int Height => pixels.GetLength(1);

        public RGBA AverageColor(Rectangle rect)
        {
            return summedAreaTable.GetSum(rect) / rect.Area;
        }

        public IntRGB ColorSum(Rectangle rect)
        {
            return summedAreaTable.GetSum(rect);
        }

        public Image Extract(Point bottomLeft, Point size)
        {
            RGBA[,] cut = new RGBA[size.X, size.Y];

            for (int x = bottomLeft.X; x < bottomLeft.X + size.X; x++)
            {
                for (int y = bottomLeft.Y; y < bottomLeft.Y + size.Y; y++)
                {
                    cut[x, y] = pixels[x, y];
                }
            }

            return new Image(cut);
        }
    }
}

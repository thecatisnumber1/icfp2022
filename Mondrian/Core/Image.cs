using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Image
    {
        private RGBA[,] pixels;

        public Image(RGBA[,] pixels)
        {
            this.pixels = pixels;
        }

        public RGBA this[Point p] => pixels[p.X, p.Y];

        public RGBA this[int x, int y] => pixels[x, y];

        public int Width => pixels.GetLength(0);

        public int Height => pixels.GetLength(1);
    }
}

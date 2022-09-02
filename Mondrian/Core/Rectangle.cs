using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Rectangle
    {
        public readonly Point BottomLeft, TopRight;

        //public Rectangle

        public Rectangle(Point bottomLeft, Point topRight)
        {
            BottomLeft = bottomLeft;
            TopRight = topRight;
        }

        public int Top => TopRight.Y;
        public int Bottom => BottomLeft.Y;
        public int Left => BottomLeft.X;
        public int Right => TopRight.X;

        public Point BottomRight => new Point(Right, Bottom);
        public Point TopLeft => new Point(Left, Top);

        public int Width => Right - Left;

        public int Height => Top - Bottom;

        public int Area => Width * Height;
    }
}

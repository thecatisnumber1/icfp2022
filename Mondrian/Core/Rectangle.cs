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

        public static Rectangle FromPoints(Point p1, Point p2)
        {
            int left = Math.Min(p1.X, p2.X);
            int bottom = Math.Min(p1.Y, p2.Y);
            return new Rectangle(new Point(left, bottom), new Point(p1.X + p2.X - left, p1.Y + p2.Y - bottom));
        }

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

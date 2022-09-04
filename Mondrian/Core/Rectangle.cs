using System.Text.Json.Serialization;

namespace Core
{
    public class Rectangle
    {
        [JsonInclude]
        public readonly Point BottomLeft,TopRight;

        public static Rectangle FromPoints(Point p1, Point p2)
        {
            int left = Math.Min(p1.X, p2.X);
            int bottom = Math.Min(p1.Y, p2.Y);
            return new Rectangle(new Point(left, bottom), new Point(p1.X + p2.X - left, p1.Y + p2.Y - bottom));
        }

        [JsonConstructor]
        public Rectangle(Point bottomLeft, Point topRight)
        {
            BottomLeft = bottomLeft;
            TopRight = topRight;
        }

        [JsonIgnore]
        public int Top => TopRight.Y;
        [JsonIgnore]
        public int Bottom => BottomLeft.Y;
        [JsonIgnore]
        public int Left => BottomLeft.X;
        [JsonIgnore]
        public int Right => TopRight.X;

        [JsonIgnore]
        public Point BottomRight => new Point(Right, Bottom);
        [JsonIgnore]
        public Point TopLeft => new Point(Left, Top);

        [JsonIgnore]
        public int Width => Right - Left;

        [JsonIgnore]
        public int Height => Top - Bottom;

        [JsonIgnore]
        public int Area => Width * Height;

        public override string ToString()
        {
            return $"({BottomLeft},{TopRight})";
        }
    }
}

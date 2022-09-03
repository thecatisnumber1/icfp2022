namespace Core
{
    public struct Point
    {
        public readonly int X;
        public readonly int Y;

        public Point(int x = 0, int y = 0)
        {
            X = x;
            Y = y;
        }

        public Point Clone()
        {
            return new Point(X, Y);
        }

        public Point GetDiff(Point other)
        {
            return new Point(Math.Max(X - other.X, 0), Math.Max(Y - other.Y, 0));
        }

        public bool IsStrictlyInside(Point bottomLeft, Point topRight)
        {
            return bottomLeft.X < X &&
                    X < topRight.X &&
                    bottomLeft.Y < Y &&
                    Y < topRight.Y;
        }

        public bool IsOnBoundary(Point bottomLeft, Point topRight)
        {
            return (bottomLeft.X == X && bottomLeft.Y <= this.Y && this.Y <= topRight.Y)
            || (topRight.X == this.X && bottomLeft.Y <= this.Y && this.Y <= topRight.Y)
            || (bottomLeft.Y == this.Y && bottomLeft.X <= this.X && this.X <= topRight.X)
            || (topRight.Y == this.Y && bottomLeft.X <= this.X && this.X <= topRight.X);
        }

        public bool IsInside(Point bottomLeft, Point topRight)
        {
            return IsStrictlyInside(bottomLeft, topRight) || IsOnBoundary(bottomLeft, topRight);
        }

        public int GetScalarSize()
        {
            return X * Y;
        }

        public Point Add(Point other)
        {
            return new Point(X + other.X, Y + other.Y);
        }

        public Point Subtract(Point other)
        {
            return new Point(X - other.X, Y - other.Y);
        }

        public override string ToString()
        {
            return $"[{X}, {Y}]";
        }
    }
}

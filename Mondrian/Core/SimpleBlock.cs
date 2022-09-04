namespace Core
{
    public class SimpleBlock : Block
    {
        // For initial-png-based blocks, this is the average color
        public RGBA Color { get; private set; }

        // For initial-png-based blocks, this is the section of image used
        public Image? Image { get; private set; }

        public SimpleBlock(string id, Point bottomLeft, Point topRight, RGBA color)
            : base(id, bottomLeft, topRight)
        {
            Color = color;
        }

        public SimpleBlock(string id, Point bottomLeft, Point topRight, Image image)
            : base(id, bottomLeft, topRight)
        {
            Color = image.AverageColor(new Rectangle(bottomLeft, topRight));
            Image = image;
        }

        public override SimpleBlock[] GetChildren()
        {
            return new SimpleBlock[] { this };
        }

        public RGBA GetColorAt(int x, int y)
        {
            if (Image == null)
            {
                return Color;
            }

            return Image[BottomLeft.X + x, BottomLeft.Y + y];
        }

        public SimpleBlock Clone()
        {
            if (Image == null)
            {
                return new SimpleBlock(ID, BottomLeft.Clone(), TopRight.Clone(), Color);
            }
            else
            {
                return new SimpleBlock(ID, BottomLeft.Clone(), TopRight.Clone(), Image.Clone());
            }
        }

        public SimpleBlock Shift(Point newBottomLeft, Point newTopRight)
        {
            if (Image == null)
            {
                return new SimpleBlock(ID, newBottomLeft.Clone(), newTopRight.Clone(), Color);
            }
            else
            {
                return new SimpleBlock(ID, newBottomLeft.Clone(), newTopRight.Clone(), Image.Clone());
            }
        }

        public SimpleBlock Cut(string idSuffix, Point newBottomLeft, Point newTopRight)
        {
            if (Image == null)
            {
                return new SimpleBlock(ID + idSuffix, newBottomLeft.Clone(), newTopRight.Clone(), Color);
            }
            else
            {
                int imageX = newBottomLeft.X - BottomLeft.X;
                int imageY = newBottomLeft.Y - BottomLeft.Y;
                var newImage = Image.Extract(new Point(imageX, imageY), newTopRight.GetDiff(newBottomLeft));

                return new SimpleBlock(ID + idSuffix, newBottomLeft.Clone(), newTopRight.Clone(), newImage);
            }
        }

        public override int GetHashCode()
        {
            // Do not consider HasRendered. It's not part of the actul state.
            return HashCode.Combine(ID, BottomLeft, TopRight, Size, Color);
        }
    }
}

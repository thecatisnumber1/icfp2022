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

        public SimpleBlock Clone()
        {
            return new SimpleBlock(ID, BottomLeft.Clone(), TopRight.Clone(), Color);
        }

        public override int GetHashCode()
        {
            // Do not consider HasRendered. It's not part of the actul state.
            return HashCode.Combine(ID, BottomLeft, TopRight, Size, Color);
        }
    }
}

namespace Core
{
    public class SimpleBlock : Block
    {
        public RGBA Color { get; private set; }

        public SimpleBlock(string id, Point bottomLeft, Point topRight, RGBA color)
            : base(id, bottomLeft, topRight)
        {
            Color = color;
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

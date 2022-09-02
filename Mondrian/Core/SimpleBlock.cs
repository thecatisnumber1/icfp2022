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
    }
}

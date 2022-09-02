namespace Core
{
    public class ComplexBlock : Block
    {
        public SimpleBlock[] SubBlocks { get; private set; }

        public ComplexBlock(string id, Point bottomLeft, Point topRight, SimpleBlock[] subBlocks)
            : base(id, bottomLeft, topRight)
        {
            SubBlocks = subBlocks;
        }

        public override SimpleBlock[] GetChildren()
        {
            return SubBlocks;
        }

        public SimpleBlock[] OffsetChildren(Point newBottomLeft)
        {
            return SubBlocks.Select(block => new SimpleBlock(
                "child",
                block.BottomLeft.Add(newBottomLeft).Subtract(BottomLeft),
                block.TopRight.Add(newBottomLeft).Subtract(BottomLeft),
                block.Color
               )).ToArray();
        }
    }
}

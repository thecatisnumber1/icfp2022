namespace Core
{
    public abstract class Block
    {
        public string ID { get; private set; }
        public Point BottomLeft { get; private set; }
        public Point TopRight { get; private set; }
        public Point Size { get; private set; }

        public bool HasRendered { get; internal set; }

        public Block(string id, Point bottomLeft, Point topRight)
        {
            ID = id;
            BottomLeft = bottomLeft;
            TopRight = topRight;
            Size = topRight.GetDiff(bottomLeft);

            if (BottomLeft.X > TopRight.X || BottomLeft.Y > TopRight.Y)
            {
                throw new Exception("Invalid Block");
            }

            // For ComplexBlocks, this can happen.
            // Probably should modify the point split code

            //if (Size.GetScalarSize() == 0)
            //{
            //    throw new Exception("Block size cannot be 0!");
            //}
        }

        public abstract SimpleBlock[] GetChildren();
    }
}

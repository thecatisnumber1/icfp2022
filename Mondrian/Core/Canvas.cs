namespace Core
{
    public class Canvas
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public RGBA BackgroundColor { get; private set; }
        public Dictionary<string, Block> Blocks { get; private set; }

        public Canvas(int width, int height, RGBA backgroundColor)
        {
            Width = width;
            Height = height;
            BackgroundColor = backgroundColor;

            Blocks = new Dictionary<string, Block>
            {
                { "0", new SimpleBlock("0", new Point(0, 0), new Point(width, height), backgroundColor) }
            };
        }

        public Point Size
        {
            get
            {
                return new Point(Width, Height);
            }
        }

        public IEnumerable<SimpleBlock> Simplify()
        {
            return Blocks.Values.SelectMany(block => block.GetChildren());
        }
    }
}

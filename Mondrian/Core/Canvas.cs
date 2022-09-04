namespace Core
{
    public class Canvas
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Size { get; private set; }
        public Dictionary<string, Block> Blocks { get; private set; }

        public Canvas(int width, int height, RGBA backgroundColor)
        {
            Width = width;
            Height = height;
            Size = width * height;

            Blocks = new Dictionary<string, Block>
            {
                { "0", new SimpleBlock("0", new Point(0, 0), new Point(width, height), backgroundColor) }
            };
        }

        public Canvas(InitialConfig config, Image? initialPng)
        {
            Width = config.width;
            Height = config.height;
            Size = Width * Height;

            Blocks = new Dictionary<string, Block>();

            foreach (var block in config.blocks)
            {
                var bl = new Point(block.bottomLeft[0], block.bottomLeft[1]);
                var tr = new Point(block.topRight[0], block.topRight[1]);

                if (block.color != null)
                {
                    Blocks[block.blockId] = new SimpleBlock(
                        block.blockId,
                        bl,
                        tr,
                        new RGBA(block.color[0], block.color[1], block.color[2], block.color[3]));
                }
                else if (block.pngBottomLeftPoint != null)
                {
                    var pngBL = new Point(block.pngBottomLeftPoint[0], block.pngBottomLeftPoint[1]);
                    var size = tr.GetDiff(bl);
                    Blocks[block.blockId] = new SimpleBlock(
                        block.blockId,
                        bl,
                        tr,
                        initialPng.Extract(pngBL, size));
                }
            }
        }

        public IEnumerable<SimpleBlock> Simplify()
        {
            return Blocks.Values.SelectMany(block => block.GetChildren());
        }
    }
}

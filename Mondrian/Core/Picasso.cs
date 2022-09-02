namespace Core
{
    public class Picasso
    {
        private Canvas canvas;
        private Stack<Instruction> instructions;
        // Target Image

        public Picasso(int width, int height)
        {
            canvas = new Canvas(width, height, new RGBA());
            instructions = new Stack<Instruction>();
        }

        public int Score
        {
            get
            {
                // TBD
                return 0;
            }
        }

        public IEnumerable<Block> AllBlocks
        {
            get
            {
                return canvas.Blocks.Values;
            }
        }

        public IEnumerable<Block> PointCut(Block block, Point point)
        {
            // TBD update blocks

            instructions.Push(new PointCutInstruction(block.ID, point));
            return new Block[0];
        }

        public void Color(Block block, RGBA color)
        {
            // TBD update blocks

            instructions.Push(new ColorInstruction(block.ID, color));
        }

        public RGBA AverageTargetColor(Block block)
        {
            // TBD
            return new RGBA();
        }

        public void UndoInstructions(int count)
        {
            for (int i = 0; i < count && instructions.Count > 0; i++)
            {
                instructions.Pop();
            }
        }
    }
}

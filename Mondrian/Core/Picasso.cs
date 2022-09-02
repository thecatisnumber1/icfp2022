using static Core.InstructionCostCalculator;

namespace Core
{
    public class Picasso
    {
        private readonly Canvas canvas;
        private readonly int canvasSize;

        private readonly Stack<Tuple<Instruction, int>> instructions;
        private int totalInstructionCost;

        private readonly Image image;

        public Picasso(Image img)
        {
            image = img;
            canvas = new Canvas(image.Width, image.Height, new RGBA());
            canvasSize = canvas.Size.GetScalarSize();
            instructions = new Stack<Tuple<Instruction, int>>();
        }

        public int Score
        {
            get
            {
                // TBD
                return totalInstructionCost + 0;
            }
        }

        public IEnumerable<Block> AllBlocks
        {
            get
            {
                return canvas.Blocks.Values;
            }
        }

        private void AddInstruction(Instruction instruction, Block? block)
        {
            int cost = GetCost(InstructionType.PointCut, block?.Size.GetScalarSize() ?? 1, canvasSize);

            instructions.Push(new Tuple<Instruction, int>(instruction, cost));
            totalInstructionCost += cost;
        }

        public IEnumerable<Block> PointCut(Block block, Point point)
        {
            // TBD update blocks

            AddInstruction(new PointCutInstruction(block.ID, point), block);

            return new Block[0];
        }

        public void Color(Block block, RGBA color)
        {
            // TBD update blocks

            AddInstruction(new ColorInstruction(block.ID, color), block);
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
                var instruction = instructions.Pop();
                totalInstructionCost -= instruction.Item2;
            }
        }
    }
}

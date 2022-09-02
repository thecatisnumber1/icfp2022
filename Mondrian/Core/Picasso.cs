using static Core.InstructionCostCalculator;

namespace Core
{
    public class Picasso
    {
        private Canvas canvas;
        private int canvasSize;
        private Stack<Tuple<Instruction, int>> instructions;
        private int totalInstructionCost;

        // Target Image

        public Picasso(int width, int height)
        {
            canvas = new Canvas(width, height, new RGBA());
            canvasSize = width * height;
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

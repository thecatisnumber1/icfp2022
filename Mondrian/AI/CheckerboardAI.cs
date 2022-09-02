using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace AI
{
    public class CheckerboardAI
    {
        public readonly int SAMPLE_SIZE = 10;
        public static Random r = new Random();

        public static void Solve(Picasso picasso, AIArgs args, LoggerBase logger)
        {
            throw new NotImplementedException();
        }

        public void Solve(Canvas canvas, Block block)
        {
            Rectangle rect = block.Rect();
            List<Point> samples = new List<Point>();
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                samples.Add(new Point(r.Next(rect.Left, rect.Right), r.Next(rect.Bottom, rect.Top)));
            }

            int bestScore = canvas.Score;
            Point? bestPoint = null;
            foreach (Point p in samples)
            {
                var results = SamplePoint(canvas, block, p);
                if (results.score < bestScore)
                {
                    bestScore = results.score;
                    bestPoint = p;
                }

                canvas.RemoveInstructions(results.instructionsUsed);
            }


            if (bestPoint != null)
            {
                var best = SamplePoint(canvas, block, bestPoint);
                foreach (Block subBlock in best.subBlocks)
                {
                    Solve(canvas, subBlock);
                }
            }
        }


        public (int score, int instructionsUsed, List<Block> subBlocks) SamplePoint(Canvas canvas, Block block, Point p)
        {
            List<Block> subBlocks = canvas.Cut(block, p);

            // Color each the average of the canvas underneath
            int instructionsUsed = 1;
            foreach (Block subBlock in subBlocks)
            {
                int preScore = canvas.Score;
                canvas.Color(subBlock, canvas.AverageTargetColor(subBlock));
                int postScore = canvas.Score;
                if (preScore < postScore)
                {
                    instructionsUsed++;
                }
                else
                {
                    canvas.RemoveInstructions(1);
                }
            }

            return (canvas.Score, instructionsUsed, subBlocks);
        }
    }

    public interface Instruction
    {
        public Instruction CutInstruction(Point p);
    }

    public interface Canvas
    {
        public int Score { get; }
        public List<Block> AllBlocks { get; }
        public List<Block> Cut(Block block, Point point);
        public void Color(Block block, Color color);
        public Color AverageTargetColor(Block block);

        // Remove the last two instructions
        public void RemoveInstructions(int count);
    }

    public interface Block
    {
        public Rectangle Rect();
    }

    public class Point
    {
        public int X { get; }
        public int Y { get; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public interface Color
    {

    }
}

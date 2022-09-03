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
        //public static readonly int SAMPLE_SIZE = 20;
        public static readonly int LEVELS = 5;
        public static Random r = new Random();

        public static void Solve(Core.Picasso picasso, AIArgs args, LoggerBase logger)
        {
            RecursiveSolveBad(picasso, picasso.AllBlocks.First(), 0);
        }

        public static void RecursiveSolveBad(Picasso picasso, Block block, int level)
        {
            if (level == LEVELS)
            {
                return;
            }

            Rectangle rect = new Rectangle(block.BottomLeft, block.TopRight);
            Point bestPoint = new Point((block.TopRight.X - block.BottomLeft.X) / 2 + block.BottomLeft.X, (block.TopRight.Y - block.BottomLeft.Y) / 2 + block.BottomLeft.Y);
            var best = SamplePoint(picasso, block, bestPoint);
            foreach (Block subBlock in best.subBlocks)
            {
                RecursiveSolveBad(picasso, subBlock, level + 1);
            }
        }


        public static (int score, int instructionsUsed, List<Block> subBlocks) SamplePoint(Picasso canvas, Block block, Point p)
        {
            List<Block> subBlocks = canvas.PointCut(block, p).ToList();

            // Color each the average of the canvas underneath
            int instructionsUsed = 1;
            foreach (Block subBlock in subBlocks)
            {
                int preScore = canvas.Score;
                canvas.Color(subBlock, canvas.AverageTargetColor(subBlock));
                int postScore = canvas.Score;
                //if (postScore < preScore)
                //{
                    instructionsUsed++;
                //}
                //else
                //{
                //    canvas.Undo(1);
                //}
            }

            return (canvas.Score, instructionsUsed, subBlocks);
        }
    }
}

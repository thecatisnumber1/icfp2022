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
        public static readonly int SAMPLE_SIZE = 20;
        public static Random r = new Random();

        public static void Solve(Core.Picasso picasso, AIArgs args, LoggerBase logger)
        {
            Solve(picasso, picasso.AllBlocks.First());
        }

        public static void Solve(Picasso picasso, Block block)
        {
            Rectangle rect = new Rectangle(block.BottomLeft, block.TopRight);
            List<Point> samples = new List<Point>();
            for (int i = 0; i < SAMPLE_SIZE; i++)
            {
                if (rect.Left + 1 < rect.Right - 1 && rect.Bottom + 1 < rect.Top - 1)
                {
                    samples.Add(new Point(r.Next(rect.Left + 1, rect.Right - 1), r.Next(rect.Bottom + 1, rect.Top - 1)));
                }
            }

            int bestScore = picasso.Score;
            Point? bestPoint = null;
            foreach (Point p in samples)
            {
                var results = SamplePoint(picasso, block, p);
                if (results.score < bestScore)
                {
                    bestScore = results.score;
                    bestPoint = p;
                }

                picasso.Undo(results.instructionsUsed);
            }


            if (bestPoint != null)
            {
                var best = SamplePoint(picasso, block, bestPoint.Value);
                foreach (Block subBlock in best.subBlocks)
                {
                    Solve(picasso, subBlock);
                }
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
                //canvas.Color(subBlock, new RGBA(r.Next(255), r.Next(255), r.Next(255), r.Next(255)));
                int postScore = canvas.Score;
                if (postScore < preScore)
                {
                    instructionsUsed++;
                }
                else
                {
                    canvas.Undo(1);
                }
            }

            return (canvas.Score, instructionsUsed, subBlocks);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace AI
{
    public class RoboLarsAI
    {
        public static void Solve(Picasso picasso, AIArgs args, LoggerBase logger)
        {
            int granularity = 40;
            List<Rectangle> rects = new List<Rectangle>();
            for (int col = 0; col < 400; col += granularity)
            {
                for (int row = 0; row < 400; row += granularity)
                {
                    if (col == 0 && row == 0)
                    {
                        continue;
                    }

                    rects.Add(new Rectangle(Point.ORIGIN, new Point(col, row)));
                }
            }

            rects = rects.OrderByDescending(x => x.TopRight.ManhattanDist(Point.ORIGIN)).ToList();
            for (int i = 0; i < rects.Count; i++)
            {
                if (i % 1000 == 0)
                {
                    logger.LogMessage(i.ToString());
                }
                Point point = rects[i];
                if (MaybeColor(picasso, new Rectangle(Point.ORIGIN, point), picasso.AllBlocks.First(), picasso.TargetImage[point]))
                {
                    logger.Render(picasso);
                }
            }
        }

        private static bool MaybeColor(Picasso picasso, Rectangle rect, Block block, RGBA color)
        {
            int preScore = picasso.Score;
            int instructionCount;
            if (rect.Right != 400 && rect.Top != 400)
            {
                List<Block> blocks1 = picasso.PointCut(block.ID, rect.TopRight).ToList();
                picasso.Color(blocks1[0].ID, color);
                Block firstMerge = picasso.Merge(blocks1[1].ID, blocks1[2].ID);
                Block secondMerge = picasso.Merge(blocks1[0].ID, blocks1[3].ID);
                picasso.Merge(firstMerge.ID, secondMerge.ID);
                instructionCount = 5;
            }
            else if (rect.Right != 400 && rect.Top == 400)
            {
                List<Block> blocks1 = picasso.VerticalCut(block.ID, rect.Right).ToList();
                picasso.Color(blocks1[0].ID, color);
                picasso.Merge(blocks1[0].ID, blocks1[1].ID);
                instructionCount = 3;
            }
            else if (rect.Right == 400 && rect.Top != 400)
            {
                List<Block> blocks1 = picasso.HorizontalCut(block.ID, rect.Top).ToList();
                picasso.Color(blocks1[0].ID, color);
                picasso.Merge(blocks1[0].ID, blocks1[1].ID);
                instructionCount = 3;
            }
            else
            {
                picasso.Color(block.ID, color);
                instructionCount = 1;
            }

            if (picasso.Score >= preScore)
            {
                picasso.Undo(instructionCount);
                return false;
            }

            return true;
        }
    }
}

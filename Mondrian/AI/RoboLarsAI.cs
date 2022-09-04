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
            PlaceAllRectangles(picasso, rects, logger);
        }

        private static void PlaceAllRectangles(Picasso picasso, List<Rectangle> rects, LoggerBase logger)
        {
            List<RGBA?> colors = ColorOptimizer.ChooseColorsSlow(rects, picasso.TargetImage);
            for (int i = 0; i < rects.Count; i++)
            {
                PlaceRectangle(picasso, rects[i], colors[i]);
            }
        }

        private static void PlaceRectangle(Picasso picasso, Rectangle rect, RGBA? color)
        {
            if (picasso.BlockCount > 1)
            {
                throw new Exception("Can't place a rectangle on a complex canvas!");
            }

            FirstCut(picasso, rect, color);
        }

        private static void FirstCut(Picasso picasso, Rectangle rect, RGBA? color)
        {
            if (rect.Left != 0 && rect.Bottom != 0)
            {
                List<Block> blocks0 = picasso.PointCut(picasso.AllBlocks.First().ID, rect.BottomLeft).ToList();
                Block zeroDotTwo = SecondCut(picasso, rect, blocks0[2], color);
                Block firstMerge = picasso.Merge(zeroDotTwo.ID, blocks0[1].ID);
                Block secondMerge = picasso.Merge(blocks0[0].ID, blocks0[3].ID);
                picasso.Merge(firstMerge.ID, secondMerge.ID);
            }
            else if (rect.Left != 0 && rect.Bottom == 0)
            {
                List<Block> blocks = picasso.VerticalCut(picasso.AllBlocks.First().ID, rect.Left).ToList();
                Block right = SecondCut(picasso, rect, blocks[1], color);
                picasso.Merge(right.ID, blocks[0].ID);
            }
            else if (rect.Left == 0 && rect.Bottom != 0)
            {
                List<Block> blocks = picasso.HorizontalCut(picasso.AllBlocks.First().ID, rect.Bottom).ToList();
                Block top = SecondCut(picasso, rect, blocks[1], color);
                picasso.Merge(top.ID, blocks[0].ID);
            }
            else
            {
                SecondCut(picasso, rect, picasso.AllBlocks.First(), color);
            }
        }

        private static Block SecondCut(Picasso picasso, Rectangle rect, Block block, RGBA? color)
        {
            if (rect.Right != 400 && rect.Top != 400)
            {
                List<Block> blocks1 = picasso.PointCut(block.ID, rect.TopRight).ToList();
                picasso.Color(blocks1[0].ID, color == null ? new RGBA(125, 254, 227, 255) : color.Value);
                Block firstMerge = picasso.Merge(blocks1[1].ID, blocks1[2].ID);
                Block secondMerge = picasso.Merge(blocks1[0].ID, blocks1[3].ID);
                return picasso.Merge(firstMerge.ID, secondMerge.ID);
            }
            else if (rect.Right != 400 && rect.Top == 400)
            {
                List<Block> blocks1 = picasso.VerticalCut(block.ID, rect.Right).ToList();
                picasso.Color(blocks1[0].ID, color == null ? new RGBA(125, 254, 227, 255) : color.Value);
                return picasso.Merge(blocks1[0].ID, blocks1[1].ID);
            }
            else if (rect.Right == 400 && rect.Top != 400)
            {
                List<Block> blocks1 = picasso.HorizontalCut(block.ID, rect.Top).ToList();
                picasso.Color(blocks1[0].ID, color == null ? new RGBA(125, 254, 227, 255) : color.Value);
                return picasso.Merge(blocks1[0].ID, blocks1[1].ID);
            }
            else
            {
                picasso.Color(block.ID, color == null ? new RGBA(125, 254, 227, 255) : color.Value);
                return block;
            }
        }
    }
}

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
        public static readonly int CANVAS_SIZE = 400;
        public static Random r = new Random();

        public static void Solve(Picasso picasso, AIArgs args, LoggerBase logger)
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < 300; i++)
            {
                points.Add(RandomPoint());
            }

            List<Point> corners = logger.UserSelectedRectangles.Select(x => x.TopRight).ToList();
            ClimbThatHill(picasso, points, logger);
        }

        public static readonly List<Point> DIRECTIONS = new List<Point>
        {
            new Point(0, -1),
            new Point(0, 1),
            new Point(-1, 0),
            new Point(1, 0)
        };

        private static void ClimbThatHill(Picasso picasso, List<Point> corners, LoggerBase logger)
        {
            bool improved = true;
            var colors = ColorOptimizer.ChooseColorsLars(corners, Point.ORIGIN, picasso.TargetImage);
            int totalInstructionCost = corners.Sum(ComputeRectInstructionCost);
            int bestScore = colors.score + totalInstructionCost;
            /*Picasso tempPic = new Picasso(picasso.TargetImage);
            PlaceAllRectangles(tempPic, corners.Select(x => new Rectangle(Point.ORIGIN, x)).ToList(), logger);

            if (Math.Abs(bestScore - tempPic.Score) > 0)
            {
                throw new Exception("predicatble");
            }*/

            int limit = 10;
            while (improved)
            {
                improved = false;
                for (int i = 0; i < corners.Count; i++)
                {
                    Point curPoint = corners[i];
                    foreach (Point d in DIRECTIONS)
                    {
                        Point scaledDelta = new Point(d.X * limit, d.Y * limit);
                        Point modifiedPoint = curPoint.Add(scaledDelta);
                        if (modifiedPoint.X > 0 && modifiedPoint.X <= CANVAS_SIZE && modifiedPoint.Y > 0 && modifiedPoint.Y <= CANVAS_SIZE)
                        {
                            corners[i] = modifiedPoint;
                            totalInstructionCost -= ComputeRectInstructionCost(curPoint);
                            totalInstructionCost += ComputeRectInstructionCost(modifiedPoint);
                            var tempColors = ColorOptimizer.ChooseColorsLars(corners, Point.ORIGIN, picasso.TargetImage);
                            int newScore = tempColors.score + totalInstructionCost;
                            
                            /*tempPic = new Picasso(picasso.TargetImage);
                            PlaceAllRectangles(tempPic, corners.Select(x => new Rectangle(Point.ORIGIN, x)).ToList(), logger);

                            if (Math.Abs(tempPic.Score - newScore) > 0)
                            {
                                throw new Exception("predicatble");
                            }*/
                            
                            if (newScore < bestScore)
                            {
                                bestScore = newScore;
                                improved = true;
                                curPoint = modifiedPoint;
                                colors = tempColors;

                                Picasso tempPic = new Picasso(picasso.TargetImage);
                                PlaceAllRectangles(tempPic, corners.Select(x => new Rectangle(Point.ORIGIN, x)).ToList(), colors.colors, logger);

                                if (tempPic.Score != newScore)
                                {
                                    throw new Exception("unpredicatble");
                                }
                                logger.Render(tempPic);
                            }
                            else
                            {
                                corners[i] = curPoint;
                                totalInstructionCost -= ComputeRectInstructionCost(modifiedPoint);
                                totalInstructionCost += ComputeRectInstructionCost(curPoint);
                            }
                        }
                    }
                }

                if (!improved && limit > 1)
                {
                    limit = (int)(limit * .75);
                    improved = true;
                    logger.LogMessage($"New \"limit\"={limit}");
                }
            }
        }

        private static int ComputeRectInstructionCost(Point topRight)
        {
            return ComputeRectInstructionCost(new Rectangle(Point.ORIGIN, topRight));
        }

        private static int ComputeRectInstructionCost(Rectangle rect)
        {
            if (rect.Right != CANVAS_SIZE && rect.Top != CANVAS_SIZE)
            {
                return PointCutCost(rect) + ColorCost(rect) + MergeCost(rect);
            }
            else if (rect.Right != CANVAS_SIZE && rect.Top == CANVAS_SIZE)
            {
                return VerticalCutCost(rect) + ColorCost(rect) + MergeCost(rect);
            }
            else if (rect.Right == CANVAS_SIZE && rect.Top != CANVAS_SIZE)
            {
                return HorizontalCutCost(rect) + ColorCost(rect) + MergeCost(rect);
            }
            else
            {
                return ColorCost(rect);
            }
        }

        private static int PointCutCost(Rectangle rect)
        {
            return InstructionCostCalculator.GetCost(InstructionType.PointCut, CANVAS_SIZE * CANVAS_SIZE, CANVAS_SIZE * CANVAS_SIZE);
        }

        private static int HorizontalCutCost(Rectangle rect)
        {
            return InstructionCostCalculator.GetCost(InstructionType.HorizontalCut, CANVAS_SIZE * CANVAS_SIZE, CANVAS_SIZE * CANVAS_SIZE);
        }

        private static int VerticalCutCost(Rectangle rect)
        {
            return InstructionCostCalculator.GetCost(InstructionType.VerticalCut, CANVAS_SIZE * CANVAS_SIZE, CANVAS_SIZE * CANVAS_SIZE);
        }

        private static int ColorCost(Rectangle rect)
        {
            return InstructionCostCalculator.GetCost(InstructionType.Color, rect.TopRight.GetDiff(rect.BottomLeft).GetScalarSize(), CANVAS_SIZE * CANVAS_SIZE);
        }

        private static int MergeCost(Rectangle rect)
        {
            Point center = rect.TopRight;

            // Figure out which rectangles were created for this rect
            if (rect.Right != CANVAS_SIZE && rect.Top != CANVAS_SIZE)
            {
                Rectangle rect1 = Rectangle.FromPoints(center, new Point(CANVAS_SIZE, 0));
                Rectangle rect2 = Rectangle.FromPoints(center, new Point(CANVAS_SIZE, CANVAS_SIZE));
                Rectangle rect3 = Rectangle.FromPoints(center, new Point(0, CANVAS_SIZE));
                Rectangle rect12 = MergeRects(rect1, rect2);
                Rectangle rect03 = MergeRects(rect, rect3);
                return MergeCost(rect1, rect2) + MergeCost(rect, rect3) + MergeCost(rect12, rect03);
            }
            else if (rect.Right != CANVAS_SIZE && rect.Top == CANVAS_SIZE)
            {
                Rectangle rect1 = Rectangle.FromPoints(center, new Point(CANVAS_SIZE, 0));
                return MergeCost(rect, rect1);
            }
            else if (rect.Right == CANVAS_SIZE && rect.Top != CANVAS_SIZE)
            {
                Rectangle rect1 = Rectangle.FromPoints(center, new Point(0, CANVAS_SIZE));
                return MergeCost(rect, rect1);
            }
            else
            {
                return 0;
            }
        }

        private static Rectangle MergeRects(Rectangle bottomLeft, Rectangle topRight)
        {
            return Rectangle.FromPoints(bottomLeft.BottomLeft, topRight.TopRight);
        }

        private static int MergeCost(Rectangle first, Rectangle second)
        {
            return InstructionCostCalculator.GetCost(InstructionType.Merge, Math.Max(first.Area, second.Area), CANVAS_SIZE * CANVAS_SIZE);
        }

        private static void PlaceAllRectangles(Picasso picasso, List<Rectangle> rects, List<RGBA?> colors, LoggerBase logger)
        {
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

        private static Point RandomPoint()
        {
            return new Point(r.Next(1, 399), r.Next(1, 399));
        }
    }
}

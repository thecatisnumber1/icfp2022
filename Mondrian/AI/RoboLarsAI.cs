﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static int NUM_POINTS = 200;
        public static List<Point> CORNERS = new List<Point>
        {
            new Point(0, 0),
            new Point(CANVAS_SIZE, 0),
            new Point(0, CANVAS_SIZE),
            new Point(CANVAS_SIZE, CANVAS_SIZE)
        };

        public static void NonInteractiveSolve(Picasso picasso, AIArgs args, LoggerBase logger)
        {
            List<Point> corners = GenerateInitialCorners();
            List<RGBA?> colors = DoSearch(picasso, logger, corners);
            SubmitSolution(picasso, args, logger, corners, colors);
        }

        public static void InteractiveSolve(Picasso picasso, AIArgs args, LoggerBase logger)
        {
            List<Point> corners = logger.UserSelectedRectangles.Select(x => x.TopRight).ToList();
            List<RGBA?> colors = DoSearch(picasso, logger, corners);
            SubmitSolution(picasso, args, logger, corners, colors);
        }

        private static List<RGBA?> DoSearch(Picasso picasso, LoggerBase logger, List<Point> corners)
        {
            List<RGBA?> colors;
            int totalScore;
            Stopwatch sw = Stopwatch.StartNew();
            do
            {
                (colors, totalScore) = ClimbThatHill(picasso.TargetImage, corners, logger);
            } while (Simplify(corners, picasso.TargetImage, logger, totalScore));
            logger.LogMessage(sw.Elapsed.ToString());
            return colors;
        }

        private static void SubmitSolution(Picasso picasso, AIArgs args, LoggerBase logger, List<Point> corners, List<RGBA?> colors)
        {
            AIUtils.RejoinAll(picasso);
            picasso.Color(picasso.AllBlocks.First().ID, new RGBA(255, 255, 255, 255));
            PlaceAllRectangles(picasso, corners.Select(x => new Rectangle(Point.ORIGIN, x)).ToList(), colors, logger);
            picasso.UndoUntilYouReachAColorInstructionThatWayTheScoreIsOptimal();
            logger.Render(picasso);

            logger.LogMessage(picasso.Score.ToString());
            List<string> instructions = picasso.SerializeInstructions();
            File.WriteAllLines($"{Guid.NewGuid}.sol", instructions);
            if (args.problemNum != -1)
            {
                Rest.CacheBests();
                int best = Rest.BestForProblem(args.problemNum);
                int score = picasso.Score;
                if (picasso.Score < best)
                {
                    logger.LogMessage($"Woo new top score! Previous: {best}.");
                    logger.LogMessage($"{Math.Round(score / (double)best * 100, 2)}% of previous high");
                }
                else
                {
                    logger.LogMessage($"Not good enough! Previous: {best}.");
                    logger.LogMessage($"{Math.Round(score / (double)best * 100, 2)}% of best solution.");
                }

                Rest.Upload(args.problemNum, string.Join("\n", picasso.SerializeInstructions()), picasso.Score);
            }
        }

        private static List<Point> GenerateInitialCorners()
        {
            List<Point> corners = new List<Point>();
            for (int i = 0; i < NUM_POINTS; i++)
            {
                corners.Add(RandomPoint());
            }

            return corners.OrderByDescending(x => x.ManhattanDist(Point.ORIGIN)).ToList();
        }

        public static readonly List<Point> DIRECTIONS = new List<Point>
        {
            new Point(0, -1),
            new Point(0, 1),
            new Point(-1, 0),
            new Point(1, 0)
        };

        private static (List<RGBA?> colors, int totalScore) ClimbThatHill(Image img, List<Point> corners, LoggerBase logger)
        {
            bool improved = true;
            var colors = ColorOptimizer.ChooseColorsLars(corners, Point.ORIGIN, img);
            int totalInstructionCost = corners.Sum(ComputeRectInstructionCost);
            int bestScore = colors.score + totalInstructionCost;
            int limit = 10;
            Stopwatch watch = Stopwatch.StartNew();
            bool renderedAtLeastOnce = false;
            while (improved)
            {
                improved = false;
                for (int i = 0; i < corners.Count; i++)
                {
                    Point curPoint = corners[i];
                    foreach (Point d in DIRECTIONS)
                    {
                        int scaleAmount = r.Next(1, limit + 1);
                        Point scaledDelta = new Point(d.X * scaleAmount, d.Y * scaleAmount);
                        Point modifiedPoint = curPoint.Add(scaledDelta);
                        if (modifiedPoint.X > 0 && modifiedPoint.X <= CANVAS_SIZE && modifiedPoint.Y > 0 && modifiedPoint.Y <= CANVAS_SIZE)
                        {
                            corners[i] = modifiedPoint;
                            totalInstructionCost -= ComputeRectInstructionCost(curPoint);
                            totalInstructionCost += ComputeRectInstructionCost(modifiedPoint);
                            var tempColors = ColorOptimizer.ChooseColorsLars(corners, Point.ORIGIN, img);
                            int newScore = tempColors.score + totalInstructionCost;
                            
                            if (newScore < bestScore)
                            {
                                bestScore = newScore;
                                improved = true;
                                curPoint = modifiedPoint;
                                colors = tempColors;

                                if (watch.Elapsed > TimeSpan.FromSeconds(3) || !renderedAtLeastOnce)
                                {
                                    renderedAtLeastOnce = true;
                                    Picasso leondardo = new Picasso(img, true);
                                    PlaceAllRectangles(leondardo, corners.Select(x => new Rectangle(Point.ORIGIN, x)).ToList(), colors.colors, logger);
                                    watch.Restart();

                                    if (leondardo.Score != newScore)
                                    {
                                        throw new Exception("unpredicatble");
                                    }
                                    logger.Render(leondardo);
                                }
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

            return (colors.colors, totalInstructionCost + colors.score);
        }

        private static bool Simplify(List<Point> corners, Image img, LoggerBase logger, int initialScore)
        {
            bool simplified = false;
            int bestScore = initialScore;
            int simplificationCount = 0;
            for (int i = 0; i < corners.Count; i++)
            {
                Point p = corners[i];
                corners.RemoveAt(i);
                int newScore = ColorOptimizer.ChooseColorsLars(corners, Point.ORIGIN, img).score;
                newScore += corners.Sum(ComputeRectInstructionCost);
                if (newScore < bestScore)
                {
                    simplificationCount++;
                    simplified = true;
                    bestScore = newScore;
                    i--;
                    continue;
                }
                else
                {
                    corners.Insert(i, p);
                }
            }

            logger.LogMessage($"Simplified {simplificationCount} times.");
            return simplified;
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
                return OptimalMergeScore(center).score;
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

        private static (bool vertical, int score) OptimalMergeScore(Point center)
        {
            return OptimalMergeScore(center, Point.ORIGIN);
        }

        private static (bool vertical, int score) OptimalMergeScore(Point opposite, Point corner)
        {
            Rectangle rect0 = Rectangle.FromPoints(corner, opposite);
            Rectangle rect1 = Rectangle.FromPoints(opposite, new Point(CANVAS_SIZE, 0));
            Rectangle rect2 = Rectangle.FromPoints(opposite, new Point(CANVAS_SIZE, CANVAS_SIZE));
            Rectangle rect3 = Rectangle.FromPoints(opposite, new Point(0, CANVAS_SIZE));
            Rectangle rect01 = MergeRects(rect0, rect1);
            Rectangle rect32 = MergeRects(rect3, rect2);
            Rectangle rect12 = MergeRects(rect1, rect2);
            Rectangle rect03 = MergeRects(rect0, rect3);
            int verticalCost =  MergeCost(rect1, rect2) + MergeCost(rect0, rect3) + MergeCost(rect12, rect03);
            int horizontalCost = MergeCost(rect0, rect1) + MergeCost(rect3, rect2) + MergeCost(rect01, rect32);
            return verticalCost < horizontalCost ? (true, verticalCost) : (false, horizontalCost);
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

            NewCut(picasso, rect, picasso.AllBlocks.First(), color);
        }

        private static void NewCut(Picasso picasso, Rectangle r, Block block, RGBA? color)
        {
            (Point corner, Point opposite) = FindCorner(r);
            if (IsOnCorner(opposite))
            {
                picasso.Color(block.ID, color == null ? new RGBA(125, 254, 227, 255) : color.Value);
            }
            else if (opposite.X > 0 && opposite.X < CANVAS_SIZE && opposite.Y > 0 && opposite.Y < CANVAS_SIZE)
            {
                List<Block> blocks1 = picasso.PointCut(block.ID, opposite).ToList();
                picasso.Color(blocks1[DirectionFrom(opposite, corner)].ID, color == null ? new RGBA(125, 254, 227, 255) : color.Value);
                if (OptimalMergeScore(opposite, corner).vertical)
                {
                    Block firstMerge = picasso.Merge(blocks1[1].ID, blocks1[2].ID);
                    Block secondMerge = picasso.Merge(blocks1[0].ID, blocks1[3].ID);
                    picasso.Merge(firstMerge.ID, secondMerge.ID);
                }
                else
                {
                    Block firstMerge = picasso.Merge(blocks1[0].ID, blocks1[1].ID);
                    Block secondMerge = picasso.Merge(blocks1[3].ID, blocks1[2].ID);
                    picasso.Merge(firstMerge.ID, secondMerge.ID);
                }
            }
            else if (opposite.X == CANVAS_SIZE || opposite.X == 0)
            {
                List<Block> blocks1 = picasso.HorizontalCut(block.ID, opposite.Y).ToList();
                if (corner.X < opposite.X)
                {
                    picasso.Color(blocks1[0].ID, color == null ? new RGBA(125, 254, 227, 255) : color.Value);
                }
                else
                {
                    picasso.Color(blocks1[1].ID, color == null ? new RGBA(125, 254, 227, 255) : color.Value);
                }

                picasso.Merge(blocks1[0].ID, blocks1[1].ID);
            }
            else if (opposite.Y == CANVAS_SIZE || opposite.Y == 0)
            {
                List<Block> blocks1 = picasso.VerticalCut(block.ID, opposite.X).ToList();
                if (corner.Y < opposite.X)
                {
                    picasso.Color(blocks1[0].ID, color == null ? new RGBA(125, 254, 227, 255) : color.Value);
                }
                else
                {
                    picasso.Color(blocks1[1].ID, color == null ? new RGBA(125, 254, 227, 255) : color.Value);
                }

                picasso.Merge(blocks1[0].ID, blocks1[1].ID);
            }
        }

        private static Block Cut(Picasso picasso, Rectangle rect, Block block, RGBA? color)
        {
            if (rect.Right != 400 && rect.Top != 400)
            {
                List<Block> blocks1 = picasso.PointCut(block.ID, rect.TopRight).ToList();
                picasso.Color(blocks1[0].ID, color == null ? new RGBA(125, 254, 227, 255) : color.Value);
                if (OptimalMergeScore(rect.TopRight).vertical)
                {
                    Block firstMerge = picasso.Merge(blocks1[1].ID, blocks1[2].ID);
                    Block secondMerge = picasso.Merge(blocks1[0].ID, blocks1[3].ID);
                    return picasso.Merge(firstMerge.ID, secondMerge.ID);
                }
                else
                {
                    Block firstMerge = picasso.Merge(blocks1[0].ID, blocks1[1].ID);
                    Block secondMerge = picasso.Merge(blocks1[3].ID, blocks1[2].ID);
                    return picasso.Merge(firstMerge.ID, secondMerge.ID);
                }
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

        private static int DirectionFrom(Point cutPoint, Point corner)
        {
            if (cutPoint.X > corner.X && cutPoint.Y > corner.Y)
            {
                return 0;
            }
            else if (cutPoint.X < corner.X && cutPoint.Y > corner.Y)
            {
                return 1;
            }
            else if (cutPoint.X < corner.X && cutPoint.Y < corner.Y)
            {
                return 2;
            }
            else if (cutPoint.X > corner.X && cutPoint.Y < corner.Y)
            {
                return 3;
            }

            throw new Exception("These two points don't have a direction.");
        }

        private static (Point corner, Point opposite) FindCorner(Rectangle rect)
        {
            foreach (Point p in CORNERS)
            {
                if (rect.BottomLeft == p)
                {
                    return (rect.BottomLeft, rect.TopRight);
                }
                else if (rect.BottomRight == p)
                {
                    return (rect.BottomRight, rect.TopLeft);
                }
                else if (rect.TopLeft == p)
                {
                    return (rect.TopLeft, rect.BottomRight);
                }
                else if (rect.TopRight == p)
                {
                    return (rect.TopRight, rect.BottomLeft);
                }
            }

            throw new Exception("This rect isn't in a corner!");
        }

        private static bool IsOnCorner(Point p)
        {
            return CORNERS.Contains(p);
        }

        private static Point RandomPoint()
        {
            return new Point(r.Next(1, 399), r.Next(1, 399));
        }
    }
}

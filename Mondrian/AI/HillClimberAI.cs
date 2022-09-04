using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace AI
{
    public class HillClimberAI
    {
        private static Random r = new Random();

        public static void Solve(Picasso picasso, AIArgs args, LoggerBase logger)
        {
            List<Rectangle> rects = logger.UserSelectedRectangles.ToList();
            bool simplified = true;
            while (simplified)
            {
                simplified = false;
                ClimbThatHill(picasso, rects, logger);
                for (int i = 0; i < rects.Count; i++)
                {
                    Picasso temp = new Picasso(picasso.TargetImage);
                    PlaceAllRectangles(temp, rects, logger);
                    int prevScore = temp.Score;
                    temp = new Picasso(picasso.TargetImage);
                    rects.RemoveAt(i);
                    PlaceAllRectangles(temp, rects, logger);
                    if (temp.Score < prevScore)
                    {
                        logger.LogMessage("Found a simplification!");
                        simplified = true;
                        break;
                    }
                }
            }
            
            
            if (args.problemNum != -1)
            {
                //Rest.Upload(args.problemNum, string.Join("\n", picasso.SerializeInstructions()));
            }
        }

        private static void ClimbThatHill(Picasso picasso, List<Rectangle> rects, LoggerBase logger)
        {
            bool improved = true;
            Picasso origCopy = new Picasso(picasso.TargetImage);
            PlaceAllRectangles(origCopy, rects, logger);
            int bestScore = origCopy.Score;
            int limit = 20;
            while (improved)
            {
                improved = false;
                for (int i = 0; i < rects.Count; i++)
                {
                    Rectangle curRect = rects[i];
                    foreach (RectMutation rm in RectMutation.AllMutations(limit, curRect))
                    {
                        if (rm.CanMutate())
                        {
                            curRect = rects[i];
                            rects.RemoveAt(i);
                            rects.Insert(i, rm.Mutate());
                            Picasso temp = new Picasso(picasso.TargetImage);
                            PlaceAllRectangles(temp, rects, logger);
                            if (temp.Score < bestScore)
                            {
                                bestScore = temp.Score;
                                improved = true;
                                logger.Render(temp);
                            }
                            else
                            {
                                rects.RemoveAt(i);
                                rects.Insert(i, curRect);
                            }
                        }
                    }
                }

                if (!improved && limit > 1)
                {
                    limit--;
                    improved = true;
                    logger.LogMessage($"New \"limit\"={limit}");
                }
            }
        }

        private static void PlaceAllRectangles(Picasso picasso, List<Rectangle> rects, LoggerBase logger)
        {
            foreach (Rectangle rect in rects)
            {
                PlaceRectangle(picasso, rect);
            }
        }

        private static void PlaceRectangle(Picasso picasso, Rectangle rect)
        {
            if (picasso.BlockCount > 1)
            {
                throw new Exception("Can't place a rectangle on a complex canvas!");
            }

            FirstCut(picasso, rect);
        }

        private static void FirstCut(Picasso picasso, Rectangle rect)
        {
            if (rect.Left != 0 && rect.Bottom != 0)
            {
                List<Block> blocks0 = picasso.PointCut(picasso.AllBlocks.First().ID, rect.BottomLeft).ToList();
                Block zeroDotTwo = SecondCut(picasso, rect, blocks0[2]);
                Block firstMerge = picasso.Merge(zeroDotTwo.ID, blocks0[1].ID);
                Block secondMerge = picasso.Merge(blocks0[0].ID, blocks0[3].ID);
                picasso.Merge(firstMerge.ID, secondMerge.ID);
            }
            else if (rect.Left != 0 && rect.Bottom == 0)
            {
                List<Block> blocks = picasso.VerticalCut(picasso.AllBlocks.First().ID, rect.Left).ToList();
                Block right = SecondCut(picasso, rect, blocks[1]);
                picasso.Merge(right.ID, blocks[0].ID);
            }
            else if (rect.Left == 0 && rect.Bottom != 0)
            {
                List<Block> blocks = picasso.HorizontalCut(picasso.AllBlocks.First().ID, rect.Bottom).ToList();
                Block top = SecondCut(picasso, rect, blocks[1]);
                picasso.Merge(top.ID, blocks[0].ID);
            }
            else
            {
                SecondCut(picasso, rect, picasso.AllBlocks.First());
            }
        }

        private static Block SecondCut(Picasso picasso, Rectangle rect, Block block)
        {
            if (rect.Right != 400 && rect.Top != 400)
            {
                List<Block> blocks1 = picasso.PointCut(block.ID, rect.TopRight).ToList();
                picasso.Color(blocks1[0].ID, picasso.AverageTargetColor(blocks1[0]));
                Block firstMerge = picasso.Merge(blocks1[1].ID, blocks1[2].ID);
                Block secondMerge = picasso.Merge(blocks1[0].ID, blocks1[3].ID);
                return picasso.Merge(firstMerge.ID, secondMerge.ID);
            }
            else if (rect.Right != 400 && rect.Top == 400)
            {
                List<Block> blocks1 = picasso.VerticalCut(block.ID, rect.Right).ToList();
                picasso.Color(blocks1[0].ID, picasso.AverageTargetColor(blocks1[0]));
                return picasso.Merge(blocks1[0].ID, blocks1[1].ID);
            }
            else if (rect.Right == 400 && rect.Top != 400)
            {
                List<Block> blocks1 = picasso.HorizontalCut(block.ID, rect.Top).ToList();
                picasso.Color(blocks1[0].ID, picasso.AverageTargetColor(blocks1[0]));
                return picasso.Merge(blocks1[0].ID, blocks1[1].ID);
            }
            else
            {
                picasso.Color(block.ID, picasso.AverageTargetColor(block));
                return block;
            }
        }

        private static Rectangle RandomRect()
        {
            Point p = RandomPoint();
            Point q = RandomPoint();
            while (q.X == p.X || q.Y == p.Y)
            {
                q = RandomPoint();
            }

            return new Rectangle(new Point(Math.Min(p.X, q.X), Math.Min(p.Y, q.Y)), new Point(Math.Max(p.X, q.X), Math.Max(p.Y, q.Y)));
        }

        private static Point RandomPoint()
        {
            return new Point(r.Next(0, 400), r.Next(0, 400));
        }

        abstract class RectMutation
        {
            public abstract bool CanMutate();
            public abstract Rectangle Mutate();

            public static List<RectMutation> AllMutations(int amount, Rectangle rect)
            {
                return new List<RectMutation>
                {
                    new TopMutation(amount, rect),
                    new TopMutation(-amount, rect),
                    new BottomMutation(amount, rect),
                    new BottomMutation(-amount, rect),
                    new RightMutation(amount, rect),
                    new RightMutation(-amount, rect),
                    new LeftMutation(amount, rect),
                    new LeftMutation(-amount, rect),
                };
            }
        }

        class TopMutation : RectMutation
        {
            private int amount;
            private Rectangle rect;

            public TopMutation(int amount, Rectangle rect)
            {
                this.amount = amount;
                this.rect = rect;
            }

            public override Rectangle Mutate()
            {
                return new Rectangle(rect.BottomLeft, new Point(rect.Right, rect.Top + amount));
            }

            public override bool CanMutate()
            {
                return rect.Top + amount > 0 && rect.Top + amount <= 400 && rect.Top + amount > rect.Bottom;
            }
        }

        class BottomMutation : RectMutation
        {
            private int amount;
            private Rectangle rect;

            public BottomMutation(int amount, Rectangle rect)
            {
                this.amount = amount;
                this.rect = rect;
            }

            public override Rectangle Mutate()
            {
                return new Rectangle(new Point(rect.Left, rect.Bottom + amount), rect.TopRight);
            }

            public override bool CanMutate()
            {
                return rect.Bottom + amount >= 0 && rect.Bottom + amount < 400 && rect.Bottom + amount < rect.Top;
            }
        }

        class LeftMutation : RectMutation
        {
            private int amount;
            private Rectangle rect;

            public LeftMutation(int amount, Rectangle rect)
            {
                this.amount = amount;
                this.rect = rect;
            }

            public override Rectangle Mutate()
            {
                return new Rectangle(new Point(rect.Left + amount, rect.Bottom), rect.TopRight);
            }

            public override bool CanMutate()
            {
                return rect.Left + amount >= 0 && rect.Left + amount < 400 && rect.Left + amount < rect.Right;
            }
        }

        class RightMutation : RectMutation
        {
            private int amount;
            private Rectangle rect;

            public RightMutation(int amount, Rectangle rect)
            {
                this.amount = amount;
                this.rect = rect;
            }

            public override Rectangle Mutate()
            {
                return new Rectangle(rect.BottomLeft, new Point(rect.Right + amount, rect.Top));
            }

            public override bool CanMutate()
            {
                return rect.Right + amount > 0 && rect.Right + amount <= 400 && rect.Right + amount > rect.Left;
            }
        }
    }
}

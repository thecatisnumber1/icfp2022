using Core;
using Mondrian;

namespace Tests
{
    [TestClass]
    public class PicassoTest
    {
        [TestMethod]
        public void BlackSquareTest()
        {
            Picasso p = new Picasso(Problems.GetProblem(1));
            p.Color(p.AllBlocks.First().ID, new RGBA(0, 0, 0, 255));
            Assert.AreEqual(170668, p.Score);
        }

        [TestMethod]
        public void SplitThenScoreTest()
        {
            // cut [0] [355, 113]
            // color[0.0][131, 131, 132, 255]
            Picasso p = new Picasso(Problems.GetProblem(1));
            List<Block> blocks = p.PointCut(p.AllBlocks.First().ID, new Point(355, 113)).ToList();
            p.Color(blocks[0].ID, new RGBA(131, 131, 132, 255));
            Assert.AreEqual(178128, p.Score);
        }

        [TestMethod]
        public void CheckerboardTest()
        {
            /*
            cut [0] [200, 200]
            color [0.0] [100, 116, 137, 255]
            color [0.1] [68, 99, 141, 255]
            color [0.2] [100, 116, 137, 255]
            */
            Picasso p = new Picasso(Problems.GetProblem(1));
            List<Block> blocks = p.PointCut(p.AllBlocks.First().ID, new Point(200, 200)).ToList();
            p.Color(blocks[0].ID, new RGBA(100, 116, 137, 255));
            p.Color(blocks[1].ID, new RGBA(68, 99, 141, 255));
            p.Color(blocks[2].ID, new RGBA(100, 116, 137, 255));
            Assert.AreEqual(152672, p.Score);
        }

        [TestMethod]
        public void CheckerboardTest2()
        {
            /*
            cut [0] [200, 200]
            color [0.0] [100, 116, 137, 255]
            color [0.1] [68, 99, 141, 255]
            color [0.2] [100, 116, 137, 255]
            cut [0.0] [100, 100]
            color [0.0.0] [78, 110, 152, 255]
            color [0.0.1] [78, 109, 152, 255]
            color [0.0.2] [122, 122, 123, 255]
            color [0.0.3] [122, 122, 122, 255]  
            */
            Picasso p = new Picasso(Problems.GetProblem(1));
            List<Block> blocks = p.PointCut(p.AllBlocks.First().ID, new Point(200, 200)).ToList();
            p.Color(blocks[0].ID, new RGBA(100, 116, 137, 255));
            p.Color(blocks[1].ID, new RGBA(68, 99, 141, 255));
            p.Color(blocks[2].ID, new RGBA(100, 116, 137, 255));
            blocks = p.PointCut(blocks[0].ID, new Point(100, 100)).ToList();
            p.Color(blocks[0].ID, new RGBA(78, 110, 152, 255));
            p.Color(blocks[1].ID, new RGBA(78, 109, 152, 255));
            p.Color(blocks[2].ID, new RGBA(122, 122, 123, 255));
            p.Color(blocks[3].ID, new RGBA(122, 122, 122, 255));
            Assert.AreEqual(152060, p.Score);
        }

        [TestMethod]
        public void CheckerboardTest3()
        {
            /*
            cut [0] [200, 200]
            color [0.0] [100, 116, 137, 255]
            color [0.1] [68, 99, 141, 255]
            color [0.2] [100, 116, 137, 255]
            cut [0.0] [100, 100]
            color [0.0.0] [78, 110, 152, 255]
            color [0.0.1] [78, 109, 152, 255]
            color [0.0.2] [122, 122, 123, 255]
            color [0.0.3] [122, 122, 122, 255]
            cut [0.0.0] [50, 50]
            color [0.0.0.0] [28, 92, 177, 255]
            color [0.0.0.1] [15, 78, 164, 255]
            */

            // From Test 2
            Picasso p = new Picasso(Problems.GetProblem(1));
            List<Block> blocks = p.PointCut(p.AllBlocks.First().ID, new Point(200, 200)).ToList();
            p.Color(blocks[0].ID, new RGBA(100, 116, 137, 255));
            p.Color(blocks[1].ID, new RGBA(68, 99, 141, 255));
            p.Color(blocks[2].ID, new RGBA(100, 116, 137, 255));
            blocks = p.PointCut(blocks[0].ID, new Point(100, 100)).ToList();
            p.Color(blocks[0].ID, new RGBA(78, 110, 152, 255));
            p.Color(blocks[1].ID, new RGBA(78, 109, 152, 255));
            p.Color(blocks[2].ID, new RGBA(122, 122, 123, 255));
            p.Color(blocks[3].ID, new RGBA(122, 122, 122, 255));

            // New stuff for Test 3
            blocks = p.PointCut(blocks[0].ID, new Point(50, 50)).ToList();
            p.Color(blocks[0].ID, new RGBA(28, 92, 177, 255));
            p.Color(blocks[1].ID, new RGBA(15, 78, 164, 255));
            Assert.AreEqual(151591, p.Score);
        }

        [TestMethod]
        public void MergeTest()
        {
            Picasso picasso = new Picasso(Problems.GetProblem(1));
            List<Rectangle> rects = new List<Rectangle>();
            for (int i = 0; i < 2; i++)
            {
                rects.Add(RandomRect());
            }

            PlaceAllRectangles(picasso, rects);
            int firstCall = picasso.Score;
            int secondCall = picasso.Score;
            Assert.AreEqual(firstCall, secondCall);
        }

        private static Random r = new Random();

        private static void PlaceAllRectangles(Picasso picasso, List<Rectangle> rects)
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

            if (picasso.BlockCount > 1)
            {
                throw new Exception("Can't place a rectangle on a complex canvas!");
            }
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
    }
}
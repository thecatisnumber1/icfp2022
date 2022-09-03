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
            /*
            cut [0] [256, 238]
            cut [0.2] [335, 383]
            color [0.2.0] [128, 128, 128, 255]
            merge [0.2.1] [0.2.2]
            merge [0.2.0] [0.2.3]
            merge [1] [2]
            merge [3] [0.1]
            merge [0.0] [0.3]
            merge [4] [5]
            cut [6] [11, 179]
            cut [6.2] [247, 216]
            color [6.2.0] [127, 127, 127, 255]
            merge [6.2.1] [6.2.2]
            merge [6.2.0] [6.2.3]
            merge [7] [8]
            merge [9] [6.1]
            merge [6.0] [6.3]
            merge [10] [11] 
            cost = 194338
            */
            Picasso picasso = new Picasso(Problems.GetProblem(1));
            List<Block> blocks0 = picasso.PointCut("0", new Point(256, 238)).ToList();
            List<Block> blocks1 = picasso.PointCut("0.2", new Point(335, 383)).ToList();
            picasso.Color("0.2.0", new RGBA(128, 128, 128, 255));
            picasso.Merge("0.2.1", "0.2.2");
            picasso.Merge("0.2.0", "0.2.3");
            picasso.Merge("1", "2");
            picasso.Merge("3", "0.1");
            picasso.Merge("0.0", "0.3");
            picasso.Merge("4", "5");
            picasso.PointCut("6", new Point(11, 179));
            picasso.PointCut("6.2", new Point(247, 216));
            picasso.Merge("6.2.1", "6.2.2");
            picasso.Merge("6.2.0", "6.2.3");
            picasso.Merge("7", "8");
            picasso.Merge("9", "6.1");
            picasso.Merge("6.0", "6.3");
            picasso.Merge("10", "11");
            Assert.AreEqual(194338, picasso.Score);
        }
    }
}
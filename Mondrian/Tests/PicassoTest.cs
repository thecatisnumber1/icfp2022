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
            p.Color(p.AllBlocks.First(), new RGBA(0, 0, 0, 255));
            Assert.AreEqual(170668, p.Score);
        }

        [TestMethod]
        public void SplitThenScoreTest()
        {
            // cut [0] [355, 113]
            // color[0.0][131, 131, 132, 255]
            Picasso p = new Picasso(Problems.GetProblem(1));
            List<Block> blocks = p.PointCut(p.AllBlocks.First(), new Point(355, 113)).ToList();
            p.Color(blocks[0], new RGBA(131, 131, 132, 255));
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
            List<Block> blocks = p.PointCut(p.AllBlocks.First(), new Point(200, 200)).ToList();
            p.Color(blocks[0], new RGBA(100, 116, 137, 255));
            p.Color(blocks[1], new RGBA(68, 99, 141, 255));
            p.Color(blocks[2], new RGBA(100, 116, 137, 255));
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
            List<Block> blocks = p.PointCut(p.AllBlocks.First(), new Point(200, 200)).ToList();
            p.Color(blocks[0], new RGBA(100, 116, 137, 255));
            p.Color(blocks[1], new RGBA(68, 99, 141, 255));
            p.Color(blocks[2], new RGBA(100, 116, 137, 255));
            blocks = p.PointCut(blocks[0], new Point(100, 100)).ToList();
            p.Color(blocks[0], new RGBA(78, 110, 152, 255));
            p.Color(blocks[1], new RGBA(78, 109, 152, 255));
            p.Color(blocks[2], new RGBA(122, 122, 123, 255));
            p.Color(blocks[3], new RGBA(122, 122, 122, 255));
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
            Picasso p = new Picasso(Problems.GetProblem(1));
            List<Block> blocks = p.PointCut(p.AllBlocks.First(), new Point(200, 200)).ToList();
            p.Color(blocks[0], new RGBA(100, 116, 137, 255));
            p.Color(blocks[1], new RGBA(68, 99, 141, 255));
            p.Color(blocks[2], new RGBA(100, 116, 137, 255));
            blocks = p.PointCut(blocks[0], new Point(100, 100)).ToList();
            p.Color(blocks[0], new RGBA(78, 110, 152, 255));
            p.Color(blocks[1], new RGBA(78, 109, 152, 255));
            p.Color(blocks[2], new RGBA(122, 122, 123, 255));
            p.Color(blocks[3], new RGBA(122, 122, 122, 255));
            blocks = p.PointCut(blocks[0], new Point(50, 50)).ToList();
            p.Color(blocks[0], new RGBA(28, 92, 177, 255));
            p.Color(blocks[1], new RGBA(15, 78, 164, 255));
            Assert.AreEqual(151591, p.Score);
        }
    }
}
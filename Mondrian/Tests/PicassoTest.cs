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
            Picasso p = new Picasso(Problems.GetProblem(1));
            // cut [0] [355, 113]
            // color[0.0][131, 131, 132, 255]
            List<Block> blocks = p.PointCut(p.AllBlocks.First(), new Point(355, 113)).ToList();
            p.Color(blocks[0], new RGBA(131, 131, 132, 255));
            Assert.AreEqual(178128, p.Score);
        }
    }
}
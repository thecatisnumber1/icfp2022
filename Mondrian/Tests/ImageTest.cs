using Core;

namespace Tests
{
    [TestClass]
    public class ImageTest
    {
        [TestMethod]
        public void AverageTest()
        {
            Image im = new(new RGBA[,] { { new(1, 10, 20, 40), new(1, 10, 20, 40) }, { new(3, 30, 40, 60), new(7, 70, 80, 100) } });

            Assert.AreEqual(new RGBA(1, 10, 20, 40), im.AverageColor(new Rectangle(new Point(0, 0), new Point(1, 1))));
            Assert.AreEqual(new RGBA(1, 10, 20, 40), im.AverageColor(new Rectangle(new Point(0, 0), new Point(1, 2))));
            Assert.AreEqual(new RGBA(2, 20, 30, 50), im.AverageColor(new Rectangle(new Point(0, 0), new Point(2, 1))));
            Assert.AreEqual(new RGBA(3, 30, 40, 60), im.AverageColor(new Rectangle(new Point(0, 0), new Point(2, 2))));
            Assert.AreEqual(new RGBA(7, 70, 80, 100), im.AverageColor(new Rectangle(new Point(1, 1), new Point(2, 2))));

            im = new(new RGBA[,] { { new(1, 0, 0, 0), new(1, 0, 0, 0), new(0, 0, 0, 0) } });
            Assert.AreEqual(new RGBA(1, 0, 0, 0), im.AverageColor(new Rectangle(new Point(0, 0), new Point(1, 3))));

            im = new(new RGBA[,] { { new(1, 0, 0, 0), new(0, 0, 0, 0), new(0, 0, 0, 0) } });
            Assert.AreEqual(new RGBA(0, 0, 0, 0), im.AverageColor(new Rectangle(new Point(0, 0), new Point(1, 3))));
        }
    }
}
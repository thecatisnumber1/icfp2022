using Core;
using Mondrian;

namespace Tests
{
    [TestClass]
    public class ColorOptimizerTest
    {
        [TestMethod]
        public void ColorChooserTest()
        {
            var problem = Problems.GetProblem(25);
            Rectangle rect = new Rectangle(Point.ORIGIN, new Point(400, 390));
            var slowResults = ColorOptimizer.ChooseColorsSlow(new List<Rectangle> { rect }, problem);
            var larsResults = ColorOptimizer.ChooseColorsLars(new List<Point> { rect.TopRight }, Point.ORIGIN, problem);
            CollectionAssert.AreEqual(slowResults, larsResults.colors);
        }
    }
}
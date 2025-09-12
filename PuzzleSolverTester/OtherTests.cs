using Microsoft.VisualStudio.TestTools.UnitTesting;
using PuzzleSolvers;

namespace PuzzleSolverTester
{
    [TestClass]
    public class OtherTests : PuzzleTestFixture
    {
        [TestMethod]
        public void TestBinairo()
        {
            // Standard Binairo
            AssertUniqueSolution(
                new Binairo(10).AddGivens("..1........0......11...........0.0..0.0......1.....11.......0.0.00..1.......1..10.......1...11....1."),
                0, 1, 1, 0, 0, 1, 0, 1, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 0, 1, 0, 1, 0);
        }
    }
}

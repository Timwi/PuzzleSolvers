using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PuzzleSolvers;

namespace PuzzleSolverTester
{
    public abstract class PuzzleTestFixture
    {
        protected static void AssertUniqueSolution(Puzzle puzzle, params int[] expectedSolution)
        {
            var solutions = puzzle.Solve().Take(2).ToArray();
            Assert.IsNotNull(solutions);
            Assert.AreEqual(1, solutions.Length);
            Assert.IsTrue(solutions[0].SequenceEqual(expectedSolution));
        }
    }
}

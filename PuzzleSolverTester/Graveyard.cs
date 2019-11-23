using System;
using System.Linq;
using PuzzleSolvers;
using RT.Util.Consoles;

namespace PuzzleSolverTester
{
    class Graveyard
    {
        public static void SandwichSudoku()
        {
            var startTime = DateTime.UtcNow;
            var count = 0;

            var puzzle = Puzzle.Sudoku(Puzzle.TranslateGivens("....................1...................5...................9...................."));

            // Row sandwich constraints
            var sums = new[] { 4, 33, 20, 17, 26, 10, 16, 24, 0 };
            for (var row = 0; row < sums.Length; row++)
                puzzle.Constraints.Add(new SandwichConstraint { Value1 = 1, Value2 = 9, AffectedCells = Enumerable.Range(0, 9).Select(col => col + 9 * row).ToArray(), Sum = sums[row] });

            // Column sandwich constraints
            sums = new[] { 8, 4, 17, 35, 14, 13, 3, 10, 25 };
            for (var col = 0; col < sums.Length; col++)
                puzzle.Constraints.Add(new SandwichConstraint { Value1 = 1, Value2 = 9, AffectedCells = Enumerable.Range(0, 9).Select(row => col + 9 * row).ToArray(), Sum = sums[col] });

            foreach (var solution in puzzle.Solve())
            {
                ConsoleUtil.WriteLine(puzzle.SolutionToConsoleString(solution));
                Console.WriteLine($"Took {(DateTime.UtcNow - startTime).TotalSeconds:0.0} sec");
                Console.WriteLine();
                count++;
            }

            Console.WriteLine($"Found {count} solutions in {(DateTime.UtcNow - startTime).TotalSeconds:0.0} sec");
            Console.WriteLine("Done. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}

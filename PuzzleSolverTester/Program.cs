using System;
using System.Linq;
using PuzzleSolvers;
using RT.Util.Consoles;

namespace PuzzleSolverTester
{
    class Program
    {
        static void Main()
        {
            var startTime = DateTime.UtcNow;
            var count = 0;

            var area = "C-G5,D4,F4,D6,F6".TranslateCoordinates(9);
            var puzzle = new Puzzle(81, 1, 9,
                Constraint.LatinSquare(9, 9),
                Constraint.Givens("..............................2.6.....13579.....4.8.............................."),
                new[] { new AntiKnightConstraint(9, 9, toroidal: true) },
                Enumerable.Range(0, 9).Select(i => new UniquenessConstraint(area.Select(cell => (cell % 9 + i) % 9 + 9 * ((cell / 9 + 2 * i) % 9)), backgroundColor: (ConsoleColor) (i + (i >= 6 ? 2 : 1)))));

            foreach (var solution in puzzle.Solve(showDebugOutput: null))
            {
                ConsoleUtil.WriteLine(puzzle.SudokuSolutionToConsoleString(solution));
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

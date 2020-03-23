using System;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.Consoles;

namespace PuzzleSolverTester
{
    class Program
    {
        static void Main()
        {
            var startTime = DateTime.UtcNow;
            var count = 0;

            var puzzle = new Puzzle(81, 1, 9,
                Constraint.Sudoku(),
                Constraint.KillerCages(@"ABBBBCDDEAFFFCCDDEAAFFCGGEEHHIIGGGEJHHIKLLLJJKKKKMLLNJOPPQMMRNNOOPQMSRRNOOQQSSSRN", 17, 21, null, 17, 28, null, 21, null, 21, null, 16, null, null, null, 32, null, null, 20, 26),
                Constraint.SandwichColumns(9, 9, 7, 0, 6, null, 8, 23, 30, null, 9),
                Constraint.SandwichRows(9, 9, 12, null, null, 0, null, 21, 8, 35, 13));

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

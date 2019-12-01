using System;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleSolverTester
{
    class Program
    {
        static void Main(string[] args)
        {
            //NUnitDirect.RunTestsOnAssembly(typeof(SudokuTests).Assembly);

            var startTime = DateTime.UtcNow;
            var count = 0;

            var puzzle = new Puzzle(81, 1, 9,
                    Constraint.Sudoku(),
                    Constraint.Givens("....4..9......9..6..9...7.....7...6.1...6...5.2...5.....3...4..2..4...5..7..5...."),
                    Ut.NewArray(new OddEvenConstraint(OddEvenType.AllSame, Constraint.TranslateCoordinates("A4,B3,B5,C2,C6,D1,D7,E2,E8,F3,F9,G4,G8,H5,H7,I6"))));

            foreach (var solution in puzzle.Solve())
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

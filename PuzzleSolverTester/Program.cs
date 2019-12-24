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

            /*
            // Very hard Sudoku with Killer cages that don’t cover the whole puzzle.
            // Solution:
            // 5 2 8 4 7 3 9 1 6
            // 1 6 4 8 9 5 3 7 2
            // 7 3 9 2 6 1 4 5 8
            // 9 4 5 3 1 6 8 2 7
            // 3 7 2 9 5 8 1 6 4
            // 8 1 6 7 2 4 5 3 9
            // 6 9 1 5 8 7 2 4 3
            // 4 8 7 1 3 2 6 9 5
            // 2 5 3 6 4 9 7 8 1
            var puzzle = new Puzzle(81, 1, 9,
                    Constraint.Sudoku(),
                    Constraint.KillerCage(8, Constraint.TranslateCoordinates("A1-2,B1")),
                    Constraint.KillerCage(9, Constraint.TranslateCoordinates("H1,I1-2")),
                    Constraint.KillerCage(10, Constraint.TranslateCoordinates("B-C2")),
                    Constraint.KillerCage(28, Constraint.TranslateCoordinates("D-F2,E3")),
                    Constraint.KillerCage(10, Constraint.TranslateCoordinates("G-H2")),
                    Constraint.KillerCage(18, Constraint.TranslateCoordinates("A-C4")),
                    Constraint.KillerCage(8, Constraint.TranslateCoordinates("E4-6")),
                    Constraint.KillerCage(17, Constraint.TranslateCoordinates("G-I4")),
                    Constraint.KillerCage(22, Constraint.TranslateCoordinates("B5,A-C6")),
                    Constraint.KillerCage(23, Constraint.TranslateCoordinates("H5,G-I6")),
                    Constraint.KillerCage(19, Constraint.TranslateCoordinates("A7-8,B7")),
                    Constraint.KillerCage(12, Constraint.TranslateCoordinates("H7,I7-8")),
                    Constraint.KillerCage(23, Constraint.TranslateCoordinates("B-C8-9")),
                    Constraint.KillerCage(7, Constraint.TranslateCoordinates("D8-9")),
                    Constraint.KillerCage(11, Constraint.TranslateCoordinates("F8-9")),
                    Constraint.KillerCage(30, Constraint.TranslateCoordinates("G-H8-9")));
            */

            var puzzle = new Puzzle(81, 1, 9, Constraint.Sudoku(), Ut.NewArray<Constraint>(
                new GivenConstraint(location: 3 + 9 * 4, value: 9),
                new SumOrProductConstraint(15, Enumerable.Range(0, 2).Select(i => 1 + i * 8)),
                new SumOrProductConstraint(9, Enumerable.Range(0, 3).Select(i => 2 + i * 8)),
                new SumOrProductConstraint(24, Enumerable.Range(0, 4).Select(i => 3 + i * 8)),
                new SumOrProductConstraint(6, Enumerable.Range(0, 5).Select(i => 4 + i * 8)),

                new SumOrProductConstraint(14, Enumerable.Range(0, 2).Select(i => 17 - i * 10)),
                new SumOrProductConstraint(9, Enumerable.Range(0, 3).Select(i => 26 - i * 10)),
                new SumOrProductConstraint(21, Enumerable.Range(0, 4).Select(i => 35 - i * 10)),
                new SumOrProductConstraint(29, Enumerable.Range(0, 5).Select(i => 44 - i * 10)),

                new SumOrProductConstraint(16, Enumerable.Range(0, 2).Select(i => 79 - i * 8)),
                new SumOrProductConstraint(10, Enumerable.Range(0, 3).Select(i => 78 - i * 8)),
                new SumOrProductConstraint(23, Enumerable.Range(0, 4).Select(i => 77 - i * 8)),
                new SumOrProductConstraint(49, Enumerable.Range(0, 5).Select(i => 76 - i * 8)),

                new SumOrProductConstraint(12, Enumerable.Range(0, 2).Select(i => 63 + i * 10)),
                new SumOrProductConstraint(18, Enumerable.Range(0, 3).Select(i => 54 + i * 10)),
                new SumOrProductConstraint(24, Enumerable.Range(0, 4).Select(i => 45 + i * 10)),
                new SumOrProductConstraint(8, Enumerable.Range(0, 5).Select(i => 36 + i * 10))
            ));

            foreach (var solution in puzzle.Solve(showDebugOutput: false))
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

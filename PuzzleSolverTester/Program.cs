using System;
using PuzzleSolvers;
using RT.Util.Consoles;

namespace PuzzleSolverTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var startTime = DateTime.UtcNow;
            var count = 0;

            var puzzle = Puzzle.ThermometerSudoku(Puzzle.TranslateGivens(".4.6.7.3...............................................7.....9....3.5.......1...."),
                Puzzle.TranslateCoordinates("A2-6"),
                Puzzle.TranslateCoordinates("C3-7"),
                Puzzle.TranslateCoordinates("E2-8"),
                Puzzle.TranslateCoordinates("G3-7"),
                Puzzle.TranslateCoordinates("I2-6")
            );

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

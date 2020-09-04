using System;
using RT.Util;
using RT.Util.ExtensionMethods;
using System.Collections.Generic;
using PuzzleSolvers;
using RT.Util.Consoles;
using RT.Util.Text;
using System.Linq;

namespace PuzzleSolverTester
{
    class Program
    {
        static void Main()
        {
            var tests = new List<(string name, Puzzle puzzle, int[] expectedSolution)>();

            tests.Add((
                "Sudoku",
                new Sudoku().AddGivens("3...5...8.9..7.5.....8.41...2.7.....5...28..47.....6...6....8....2...9.1.1.9.5..."),
                new[] { 3, 4, 6, 1, 5, 9, 2, 7, 8, 1, 9, 8, 2, 7, 6, 5, 4, 3, 2, 7, 5, 8, 3, 4, 1, 9, 6, 6, 2, 4, 7, 9, 1, 3, 8, 5, 5, 3, 9, 6, 2, 8, 7, 1, 4, 7, 8, 1, 5, 4, 3, 6, 2, 9, 9, 6, 3, 4, 1, 2, 8, 5, 7, 4, 5, 2, 3, 8, 7, 9, 6, 1, 8, 1, 7, 9, 6, 5, 4, 3, 2 }));
            tests.Add((
                "Jigsaw Sudoku",
                new JigsawSudoku("A-G1,E2,G2;A2-8,B6-7;B2-5,C5-7,D5,D7;C2-4,D2-4,E-F3,F2;H1-5,G3,E-G4;I1-6,G5-6,H6;E-F5-7,D6,G-H7;B-E8,A-E9;I7,F-I8-9")
                    .AddGivens("3648....71.......5....68.....5.19......9.............28....3......235..1.......9."),
                new[] { 3, 6, 4, 8, 9, 1, 5, 2, 7, 1, 8, 9, 3, 7, 4, 2, 6, 5, 5, 4, 2, 1, 6, 8, 7, 3, 9, 6, 2, 5, 7, 1, 9, 8, 4, 3, 2, 1, 3, 9, 8, 7, 4, 5, 6, 9, 3, 7, 4, 5, 6, 1, 8, 2, 8, 7, 6, 5, 2, 3, 9, 1, 4, 4, 9, 8, 2, 3, 5, 6, 7, 1, 7, 5, 1, 6, 4, 2, 3, 9, 8 }));
            tests.Add((
                "Anti-King Sudoku",
                new Sudoku().AddGivens("4...5...6.5.3.1.2...2...9...9.8.3.6.3.......9.6.9.5.7...4...8...2.1.9.4.9...3...2")
                    .AddConstraint(new AntiKingConstraint(9, 9)),
                new[] { 4, 1, 9, 2, 5, 8, 7, 3, 6, 6, 5, 7, 3, 9, 1, 4, 2, 8, 8, 3, 2, 4, 7, 6, 9, 1, 5, 7, 9, 1, 8, 2, 3, 5, 6, 4, 3, 4, 5, 6, 1, 7, 2, 8, 9, 2, 6, 8, 9, 4, 5, 3, 7, 1, 1, 7, 4, 5, 6, 2, 8, 9, 3, 5, 2, 3, 1, 8, 9, 6, 4, 7, 9, 8, 6, 7, 3, 4, 1, 5, 2 }));
            tests.Add((
                "Thermometer Sudoku",
                new Sudoku().AddGivens(".4.6.7.3...............................................7.....9....3.5.......1....").AddConstraints(LessThanConstraint.FromString("A2-6;C3-7;E2-8;G3-7;I2-6")),
                new[] { 9, 4, 8, 6, 2, 7, 1, 3, 5, 2, 6, 7, 5, 3, 1, 9, 8, 4, 5, 3, 1, 9, 4, 8, 2, 7, 6, 6, 9, 2, 8, 5, 4, 3, 1, 7, 7, 5, 3, 1, 6, 9, 4, 2, 8, 8, 1, 4, 2, 7, 3, 5, 6, 9, 3, 7, 5, 4, 8, 2, 6, 9, 1, 1, 8, 6, 3, 9, 5, 7, 4, 2, 4, 2, 9, 7, 1, 6, 8, 5, 3 }));
            tests.Add((
                "Little Killer Sudoku",
                new Sudoku().AddConstraints(
                    new SumConstraint(72, Enumerable.Range(0, 9).Select(i => i * 10)),
                    new SumConstraint(7, Constraint.TranslateCoordinates("B1,A2")),
                    new SumConstraint(20, Constraint.TranslateCoordinates("C1,B2,A3")),
                    new SumConstraint(5, Constraint.TranslateCoordinates("D1,C2,B3,A4")),
                    new SumConstraint(42, Constraint.TranslateCoordinates("E1,D2,C3,B4,A5")),
                    new SumConstraint(16, Constraint.TranslateCoordinates("A6,B7,C8,D9")),
                    new SumConstraint(15, Constraint.TranslateCoordinates("A7,B8,C9")),
                    new SumConstraint(15, Constraint.TranslateCoordinates("A8,B9")),
                    new SumConstraint(10, Constraint.TranslateCoordinates("I2,H1")),
                    new SumConstraint(20, Constraint.TranslateCoordinates("I3,H2,G1")),
                    new SumConstraint(23, Constraint.TranslateCoordinates("I4,H3,G2,F1")),
                    new SumConstraint(24, Enumerable.Range(0, 9).Select(i => 72 - i * 8)),
                    new SumConstraint(34, Constraint.TranslateCoordinates("E9,F8,G7,H6,I5")),
                    new SumConstraint(19, Constraint.TranslateCoordinates("F9,G8,H7,I6")),
                    new SumConstraint(16, Constraint.TranslateCoordinates("G9,H8,I7")),
                    new SumConstraint(4, Constraint.TranslateCoordinates("H9,I8"))),
                new[] { 7, 3, 5, 1, 9, 6, 8, 4, 2, 4, 9, 1, 8, 5, 2, 7, 3, 6, 6, 2, 8, 4, 3, 7, 1, 5, 9, 1, 8, 7, 9, 4, 3, 2, 6, 5, 9, 4, 2, 6, 7, 5, 3, 8, 1, 3, 5, 6, 2, 1, 8, 4, 9, 7, 5, 6, 3, 7, 8, 1, 9, 2, 4, 8, 1, 4, 5, 2, 9, 6, 7, 3, 2, 7, 9, 3, 6, 4, 5, 1, 8 }));
            tests.Add((
                "Killer Sudoku",
                new KillerSudoku("ABCCDDDDDABEEFFFQQABEEHIIINKBLMHHJINKKLMMMJXXPGLOOMJSXPGGGOTTSUVVWWWTTSURRRRRYYSU", 18, 22, 5, 35, 18, 15, 23, 10, 16, 16, 7, 20, 23, 13, 18, 13, 9, 19, 16, 20, 13, 17, 12, 15, 12)
                    .AddConstraints(Enumerable.Range(0, 9).Select(i => new GivenConstraint(i * 10, "478532958"[i] - '0'))),
                new[] { 4, 1, 2, 3, 7, 8, 6, 9, 5, 5, 7, 3, 6, 2, 9, 4, 8, 1, 9, 6, 8, 1, 5, 4, 2, 3, 7, 2, 8, 9, 5, 4, 1, 3, 7, 6, 1, 4, 5, 7, 3, 6, 8, 2, 9, 7, 3, 6, 9, 8, 2, 5, 1, 4, 6, 5, 7, 8, 1, 3, 9, 4, 2, 8, 9, 4, 2, 6, 7, 1, 5, 3, 3, 2, 1, 4, 9, 5, 7, 6, 8 }));
            tests.Add((
                "Equal-Sum Sudoku",
                new Sudoku().AddGivens("...3..79....5..3.2...2.7.14..4...8...1.......9.....62527...3.....3.2......64.....")
                    .AddConstraint(new EqualSumsConstraint("A1-5;B1-4;C1-3;G-I7;F-I8;E-I9;A9,B8,C7,D6,E5,F4,G3".Split(';').Select(str => Constraint.TranslateCoordinates(str)).ToArray())),
                new[] { 1, 2, 5, 3, 4, 6, 7, 9, 8, 4, 8, 7, 5, 9, 1, 3, 6, 2, 3, 6, 9, 2, 8, 7, 5, 1, 4, 7, 5, 4, 9, 6, 2, 8, 3, 1, 6, 1, 2, 8, 3, 5, 9, 4, 7, 9, 3, 8, 1, 7, 4, 6, 2, 5, 2, 7, 1, 6, 5, 3, 4, 8, 9, 8, 4, 3, 7, 2, 9, 1, 5, 6, 5, 9, 6, 4, 1, 8, 2, 7, 3 }));
            tests.Add((
                "Irregular Frame Sudoku",
                new FrameSumSudoku(minLength: 2, maxLength: 3, /* sum clues start here */ 11, 13, 13, 10, 8, 19, 13, 19, 10, 10, 18, 6, 6, 8, 23, 9, 13, 8, 11, 10, 6, 17, 18, 5, 10, 15, 16, 17, 6, 8, 9, 8, 19, 14, 17, 7),
                new[] { 3, 4, 5, 7, 2, 6, 8, 9, 1, 8, 9, 2, 3, 1, 4, 5, 6, 7, 1, 7, 6, 8, 5, 9, 3, 4, 2, 7, 3, 9, 6, 8, 5, 1, 2, 4, 6, 2, 8, 9, 4, 1, 7, 5, 3, 4, 5, 1, 2, 7, 3, 6, 8, 9, 2, 6, 4, 5, 3, 7, 9, 1, 8, 5, 1, 3, 4, 9, 8, 2, 7, 6, 9, 8, 7, 1, 6, 2, 4, 3, 5 }));
            tests.Add((
                "Sum-Or-Product Sudoku",
                new Sudoku().AddConstraints(
                    new GivenConstraint(cell: 3 + 9 * 4, value: 9),
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
                    new SumOrProductConstraint(8, Enumerable.Range(0, 5).Select(i => 36 + i * 10))),
                new[] { 8, 9, 3, 7, 2, 5, 4, 6, 1, 6, 4, 7, 1, 3, 9, 5, 2, 8, 2, 5, 1, 8, 6, 4, 7, 9, 3, 5, 3, 9, 6, 7, 1, 8, 4, 2, 1, 6, 8, 9, 4, 2, 3, 5, 7, 7, 2, 4, 3, 5, 8, 9, 1, 6, 3, 7, 2, 4, 9, 6, 1, 8, 5, 4, 1, 5, 2, 8, 7, 6, 3, 9, 9, 8, 6, 5, 1, 3, 2, 7, 4 }));
            tests.Add((
                "Sandwich 1-9 Sudoku",
                new SandwichSudoku(columnClues: new[] { 15, 9, 26, 8, 8, 12, 0, 12, 6 }, rowClues: new[] { 7, 14, 20, 2, 8, 26, 10, 31, 16 })
                    .AddGivens(@"............9......1............9...............8............5......5............"),
                new[] { 3, 8, 9, 2, 5, 1, 4, 7, 6, 4, 5, 7, 9, 6, 8, 1, 2, 3, 2, 1, 6, 3, 7, 4, 9, 8, 5, 6, 4, 8, 5, 3, 9, 2, 1, 7, 9, 3, 5, 1, 2, 7, 6, 4, 8, 7, 2, 1, 8, 4, 6, 5, 3, 9, 8, 9, 4, 6, 1, 3, 7, 5, 2, 1, 6, 2, 7, 8, 5, 3, 9, 4, 5, 7, 3, 4, 9, 2, 8, 6, 1 }));
            tests.Add((
                "Sandwich 4-6 Sudoku",
                new SandwichSudoku(crust1: 4, crust2: 6, columnClues: new[] { 26, 22, 11, 3, 2, 8, 18, 6, 12 }, rowClues: new[] { 35, 16, 10, 14, 6, 2, 1, 25, 7 })
                    .AddGivens((66, 1), (77, 4)),
                new[] { 4, 1, 3, 5, 8, 2, 9, 7, 6, 2, 5, 8, 6, 9, 7, 4, 3, 1, 9, 6, 7, 3, 4, 1, 5, 8, 2, 8, 3, 1, 4, 2, 5, 7, 6, 9, 7, 9, 5, 8, 6, 3, 2, 1, 4, 6, 2, 4, 7, 1, 9, 3, 5, 8, 5, 8, 2, 9, 3, 6, 1, 4, 7, 3, 4, 9, 1, 7, 8, 6, 2, 5, 1, 7, 6, 2, 5, 4, 8, 9, 3 }));
            tests.Add((
                "Sandwich Wraparound Sudoku",
                new SandwichSudoku(wraparound: true, columnClues: new[] { 21, 26, 25, 21, 9, 17, 4, 3, 9 }, rowClues: new[] { 13, 0, 29, 22, 18, 18, 35, 9, 35 })
                    .AddGivens((11, 9), (34, 3)),
                new[] { 6, 3, 2, 1, 8, 5, 9, 4, 7, 7, 1, 9, 6, 4, 2, 3, 8, 5, 8, 5, 4, 7, 3, 9, 6, 1, 2, 9, 2, 6, 5, 1, 8, 7, 3, 4, 5, 4, 1, 3, 7, 6, 2, 9, 8, 3, 8, 7, 9, 2, 4, 5, 6, 1, 2, 7, 3, 4, 9, 1, 8, 5, 6, 4, 9, 5, 8, 6, 7, 1, 2, 3, 1, 6, 8, 2, 5, 3, 4, 7, 9 }));
            var area = "C-G5,D4,F4,D6,F6".TranslateCoordinates(9);
            tests.Add((
                "Toroidal Anti-Knight Sudoku",
                new JigsawSudoku(Enumerable.Range(0, 9).Select(i => area.Select(cell => (cell % 9 + i) % 9 + 9 * ((cell / 9 + 2 * i) % 9)).ToArray()).ToArray())
                    .AddGivens("..............................2.6.....13579.....4.8..............................")
                    .AddConstraint(new AntiKnightConstraint(9, 9, toroidal: true)),
                new[] { 8, 7, 2, 6, 4, 9, 3, 1, 5, 7, 3, 6, 5, 9, 2, 1, 4, 8, 3, 4, 5, 7, 2, 1, 6, 8, 9, 4, 5, 8, 2, 3, 6, 7, 9, 1, 6, 8, 1, 3, 5, 7, 9, 2, 4, 9, 1, 3, 4, 7, 8, 2, 5, 6, 1, 2, 4, 9, 8, 3, 5, 6, 7, 2, 6, 9, 8, 1, 5, 4, 7, 3, 5, 9, 7, 1, 6, 4, 8, 3, 2 }));

            var tt = new TextTable { ColumnSpacing = 2 };
            var row = 0;
            tt.SetCell(1, 0, "Old algorithm (ms)".Color(ConsoleColor.White));
            tt.SetCell(2, 0, "New algorithm (ms)".Color(ConsoleColor.White));
            foreach (var (name, puzzle, expectedSolution) in tests)
            {
                row++;
                tt.SetCell(0, row, name);

                TimeSpan measure(IEnumerable<int[]> solver, int column)
                {
                    var startTime = DateTime.UtcNow;
                    var count = 0;
                    TimeSpan? firstSolve = null;
                    ConsoleColor? bg = null;

                    foreach (var solution in solver)
                    {
                        if (firstSolve == null)
                        {
                            if (!solution.SequenceEqual(expectedSolution))
                                bg = ConsoleColor.DarkMagenta;
                            firstSolve = DateTime.UtcNow - startTime;
                        }
                        else
                            bg = ConsoleColor.DarkRed;
                        count++;
                    }
                    var fullSolve = DateTime.UtcNow - startTime;
                    if (firstSolve == null)
                        bg = ConsoleColor.DarkCyan;
                    tt.SetCell(column, row, $"{(firstSolve ?? TimeSpan.Zero).TotalMilliseconds:0.0}/{fullSolve.TotalMilliseconds:0.0}", background: bg);
                    return fullSolve;
                }

                var t1 = measure(puzzle.Solve(), 1);
                var t2 = measure(puzzle.Solve(), 2);
                tt.SetCell(0, row, name.Color(ConsoleColor.White), background: t2 > t1 ? ConsoleColor.DarkRed : ConsoleColor.DarkGreen);

                Console.Clear();
                tt.WriteToConsole();
            }

            Console.WriteLine("Done. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}

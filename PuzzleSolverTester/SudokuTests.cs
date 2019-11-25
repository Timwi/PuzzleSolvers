using System;
using System.Linq;
using NUnit.Framework;
using PuzzleSolvers;
using RT.Util;

namespace PuzzleSolverTester
{
    [TestFixture]
    class SudokuTests
    {
        [Test]
        public void TestSudoku()
        {
            var solutions = Puzzle.Sudoku(Puzzle.TranslateGivens("3...5...8.9..7.5.....8.41...2.7.....5...28..47.....6...6....8....2...9.1.1.9.5...")).Solve().ToArray();
            Assert.IsNotNull(solutions);
            Assert.AreEqual(1, solutions.Length);
            Assert.IsTrue(solutions[0].SequenceEqual(new[] { 3, 4, 6, 1, 5, 9, 2, 7, 8, 1, 9, 8, 2, 7, 6, 5, 4, 3, 2, 7, 5, 8, 3, 4, 1, 9, 6, 6, 2, 4, 7, 9, 1, 3, 8, 5, 5, 3, 9, 6, 2, 8, 7, 1, 4, 7, 8, 1, 5, 4, 3, 6, 2, 9, 9, 6, 3, 4, 1, 2, 8, 5, 7, 4, 5, 2, 3, 8, 7, 9, 6, 1, 8, 1, 7, 9, 6, 5, 4, 3, 2 }));
        }

        [Test]
        public void TestJigsawSudoku()
        {
            var solutions = Puzzle.JigsawSudoku(
                Puzzle.TranslateGivens("3.......71.......5....68.....5.19......9.............28....3......235..1.......9."),
                new[] { "A-G1,E2,G2", "A2-8,B6-7", "B2-5,C5-7,D5,D7", "C2-4,D2-4,E-F3,F2", "H1-5,G3,E-G4", "I1-6,G5-6,H6", "E-F5-7,D6,G-H7", "B-E8,A-E9", "I7,F-I8-9" }.Select(coords => Puzzle.TranslateCoordinates(coords)).ToArray()
            )
                .Solve().ToArray();
            Assert.IsNotNull(solutions);
            Assert.AreEqual(1, solutions.Length);
            Assert.IsTrue(solutions[0].SequenceEqual(new[] { 3, 6, 4, 8, 9, 1, 5, 2, 7, 1, 8, 9, 3, 7, 4, 2, 6, 5, 5, 4, 2, 1, 6, 8, 7, 3, 9, 6, 2, 5, 7, 1, 9, 8, 4, 3, 2, 1, 3, 9, 8, 7, 4, 5, 6, 9, 3, 7, 4, 5, 6, 1, 8, 2, 8, 7, 6, 5, 2, 3, 9, 1, 4, 4, 9, 8, 2, 3, 5, 6, 7, 1, 7, 5, 1, 6, 4, 2, 3, 9, 8 }));
        }

        [Test]
        public void TestNoTouchSudoku()
        {
            var solutions = Puzzle.NoTouchSudoku(Puzzle.TranslateGivens("4...5...6.5.3.1.2...2...9...9.8.3.6.3.......9.6.9.5.7...4...8...2.1.9.4.9...3...2")).Solve().ToArray();
            Assert.IsNotNull(solutions);
            Assert.AreEqual(1, solutions.Length);
            Assert.IsTrue(solutions[0].SequenceEqual(new[] { 4, 1, 9, 2, 5, 8, 7, 3, 6, 6, 5, 7, 3, 9, 1, 4, 2, 8, 8, 3, 2, 4, 7, 6, 9, 1, 5, 7, 9, 1, 8, 2, 3, 5, 6, 4, 3, 4, 5, 6, 1, 7, 2, 8, 9, 2, 6, 8, 9, 4, 5, 3, 7, 1, 1, 7, 4, 5, 6, 2, 8, 9, 3, 5, 2, 3, 1, 8, 9, 6, 4, 7, 9, 8, 6, 7, 3, 4, 1, 5, 2 }));
        }

        [Test]
        public void TestThermometerSudoku1()
        {
            var solutions = Puzzle.ThermometerSudoku(Puzzle.TranslateGivens(".4.6.7.3...............................................7.....9....3.5.......1...."),
                Puzzle.TranslateCoordinates("A2-6"),
                Puzzle.TranslateCoordinates("C3-7"),
                Puzzle.TranslateCoordinates("E2-8"),
                Puzzle.TranslateCoordinates("G3-7"),
                Puzzle.TranslateCoordinates("I2-6")
            )
                .Solve().ToArray();
            Assert.IsNotNull(solutions);
            Assert.AreEqual(1, solutions.Length);
            Assert.IsTrue(solutions[0].SequenceEqual(new[] { 9, 4, 8, 6, 2, 7, 1, 3, 5, 2, 6, 7, 5, 3, 1, 9, 8, 4, 5, 3, 1, 9, 4, 8, 2, 7, 6, 6, 9, 2, 8, 5, 4, 3, 1, 7, 7, 5, 3, 1, 6, 9, 4, 2, 8, 8, 1, 4, 2, 7, 3, 5, 6, 9, 3, 7, 5, 4, 8, 2, 6, 9, 1, 1, 8, 6, 3, 9, 5, 7, 4, 2, 4, 2, 9, 7, 1, 6, 8, 5, 3 }));
        }

        [Test]
        public void TestThermometerSudoku2()
        {
            var solutions = Puzzle.ThermometerSudoku(
                Puzzle.TranslateCoordinates("D4,C4,B4,A4,A3,A2,A1"),
                Puzzle.TranslateCoordinates("D4,D3,D2,D1,C1,B1"),
                Puzzle.TranslateCoordinates("F1-2"),
                Puzzle.TranslateCoordinates("G1,H1-3,G3,F3"),
                Puzzle.TranslateCoordinates("E-I5"),
                Puzzle.TranslateCoordinates("E5-8"),
                Puzzle.TranslateCoordinates("A8,A7,A-B6"),
                Puzzle.TranslateCoordinates("C7,C6"),
                Puzzle.TranslateCoordinates("B-C8"),
                Puzzle.TranslateCoordinates("I8,I7,I6"),
                Puzzle.TranslateCoordinates("I8-9,H9,G9,F9,E9")
            )
                .Solve().ToArray();
            Assert.IsNotNull(solutions);
            Assert.AreEqual(1, solutions.Length);
            Assert.IsTrue(solutions[0].SequenceEqual(new[] { 9, 8, 7, 6, 3, 4, 1, 2, 5, 6, 2, 1, 5, 7, 8, 3, 4, 9, 5, 4, 3, 2, 1, 9, 8, 7, 6, 4, 3, 2, 1, 8, 6, 5, 9, 7, 1, 7, 5, 9, 2, 3, 4, 6, 8, 8, 9, 6, 7, 4, 5, 2, 1, 3, 7, 6, 4, 8, 5, 1, 9, 3, 2, 3, 5, 9, 4, 6, 2, 7, 8, 1, 2, 1, 8, 3, 9, 7, 6, 5, 4 }));
        }

        [Test]
        public void TestSumSudoku()
        {
            // Tests a sum Sudoku with no givens and no uniqueness constraints
            var puzzle = Puzzle.Sudoku();
            puzzle.Constraints.Add(new SumConstraint { Sum = 72, AffectedCells = Enumerable.Range(0, 9).Select(i => i * 10).ToArray() });
            puzzle.Constraints.Add(new SumConstraint { Sum = 7, AffectedCells = Puzzle.TranslateCoordinates("B1,A2") });
            puzzle.Constraints.Add(new SumConstraint { Sum = 20, AffectedCells = Puzzle.TranslateCoordinates("C1,B2,A3") });
            puzzle.Constraints.Add(new SumConstraint { Sum = 5, AffectedCells = Puzzle.TranslateCoordinates("D1,C2,B3,A4") });
            puzzle.Constraints.Add(new SumConstraint { Sum = 42, AffectedCells = Puzzle.TranslateCoordinates("E1,D2,C3,B4,A5") });
            puzzle.Constraints.Add(new SumConstraint { Sum = 16, AffectedCells = Puzzle.TranslateCoordinates("A6,B7,C8,D9") });
            puzzle.Constraints.Add(new SumConstraint { Sum = 15, AffectedCells = Puzzle.TranslateCoordinates("A7,B8,C9") });
            puzzle.Constraints.Add(new SumConstraint { Sum = 15, AffectedCells = Puzzle.TranslateCoordinates("A8,B9") });
            puzzle.Constraints.Add(new SumConstraint { Sum = 10, AffectedCells = Puzzle.TranslateCoordinates("I2,H1") });
            puzzle.Constraints.Add(new SumConstraint { Sum = 20, AffectedCells = Puzzle.TranslateCoordinates("I3,H2,G1") });
            puzzle.Constraints.Add(new SumConstraint { Sum = 23, AffectedCells = Puzzle.TranslateCoordinates("I4,H3,G2,F1") });
            puzzle.Constraints.Add(new SumConstraint { Sum = 24, AffectedCells = Enumerable.Range(0, 9).Select(i => 72 - i * 8).ToArray() });
            puzzle.Constraints.Add(new SumConstraint { Sum = 34, AffectedCells = Puzzle.TranslateCoordinates("E9,F8,G7,H6,I5") });
            puzzle.Constraints.Add(new SumConstraint { Sum = 19, AffectedCells = Puzzle.TranslateCoordinates("F9,G8,H7,I6") });
            puzzle.Constraints.Add(new SumConstraint { Sum = 16, AffectedCells = Puzzle.TranslateCoordinates("G9,H8,I7") });
            puzzle.Constraints.Add(new SumConstraint { Sum = 4, AffectedCells = Puzzle.TranslateCoordinates("H9,I8") });
            var solutions = puzzle.Solve().ToArray();
            Assert.IsNotNull(solutions);
            Assert.AreEqual(1, solutions.Length);
            Assert.IsTrue(solutions[0].SequenceEqual(new[] { 7, 3, 5, 1, 9, 6, 8, 4, 2, 4, 9, 1, 8, 5, 2, 7, 3, 6, 6, 2, 8, 4, 3, 7, 1, 5, 9, 1, 8, 7, 9, 4, 3, 2, 6, 5, 9, 4, 2, 6, 7, 5, 3, 8, 1, 3, 5, 6, 2, 1, 8, 4, 9, 7, 5, 6, 3, 7, 8, 1, 9, 2, 4, 8, 1, 4, 5, 2, 9, 6, 7, 3, 2, 7, 9, 3, 6, 4, 5, 1, 8 }));
        }

        [Test]
        public void TestKillerSudoku()
        {
            // Tests a Sudoku with sum-and-uniqueness constraints
            var puzzle = Puzzle.Sudoku();
            puzzle.Constraints.AddRange(Constraint.KillerCage(12, Puzzle.TranslateCoordinates("A1-2,B1"), ConsoleColor.DarkBlue));
            puzzle.Constraints.AddRange(Constraint.KillerCage(18, Puzzle.TranslateCoordinates("H1-2,G2"), ConsoleColor.DarkBlue));
            puzzle.Constraints.AddRange(Constraint.KillerCage(19, Puzzle.TranslateCoordinates("C3-5,D-E3"), ConsoleColor.DarkBlue));
            puzzle.Constraints.AddRange(Constraint.KillerCage(10, Puzzle.TranslateCoordinates("H5-6"), ConsoleColor.DarkBlue));
            puzzle.Constraints.AddRange(Constraint.KillerCage(16, Puzzle.TranslateCoordinates("C-E7"), ConsoleColor.DarkBlue));
            puzzle.Constraints.AddRange(Constraint.KillerCage(12, Puzzle.TranslateCoordinates("A-B9"), ConsoleColor.DarkBlue));
            puzzle.Constraints.AddRange(Constraint.KillerCage(10, Puzzle.TranslateCoordinates("I8-9,H9"), ConsoleColor.DarkBlue));
            puzzle.Constraints.AddRange(Constraint.KillerCage(13, Puzzle.TranslateCoordinates("C-D1,D2"), ConsoleColor.DarkGreen));
            puzzle.Constraints.AddRange(Constraint.KillerCage(13, Puzzle.TranslateCoordinates("I1-2"), ConsoleColor.DarkGreen));
            puzzle.Constraints.AddRange(Constraint.KillerCage(15, Puzzle.TranslateCoordinates("A-B5"), ConsoleColor.DarkGreen));
            puzzle.Constraints.AddRange(Constraint.KillerCage(10, Puzzle.TranslateCoordinates("G3-5"), ConsoleColor.DarkGreen));
            puzzle.Constraints.AddRange(Constraint.KillerCage(10, Puzzle.TranslateCoordinates("E-F8"), ConsoleColor.DarkGreen));
            puzzle.Constraints.AddRange(Constraint.KillerCage(11, Puzzle.TranslateCoordinates("E1-2"), ConsoleColor.DarkCyan));
            puzzle.Constraints.AddRange(Constraint.KillerCage(14, Puzzle.TranslateCoordinates("I4-6"), ConsoleColor.DarkCyan));
            puzzle.Constraints.AddRange(Constraint.KillerCage(13, Puzzle.TranslateCoordinates("A6-7,B6"), ConsoleColor.DarkCyan));
            puzzle.Constraints.AddRange(Constraint.KillerCage(9, Puzzle.TranslateCoordinates("D-F9"), ConsoleColor.DarkCyan));
            puzzle.Constraints.AddRange(Constraint.KillerCage(15, Puzzle.TranslateCoordinates("F-G1,F2"), ConsoleColor.DarkRed));
            puzzle.Constraints.AddRange(Constraint.KillerCage(15, Puzzle.TranslateCoordinates("B2-3,C2"), ConsoleColor.DarkRed));
            puzzle.Constraints.AddRange(Constraint.KillerCage(18, Puzzle.TranslateCoordinates("F5-7,E6,G6"), ConsoleColor.DarkRed));
            puzzle.Constraints.AddRange(Constraint.KillerCage(12, Puzzle.TranslateCoordinates("A-B8,B7"), ConsoleColor.DarkRed));
            puzzle.Constraints.AddRange(Constraint.KillerCage(18, Puzzle.TranslateCoordinates("D-E4-5"), ConsoleColor.DarkMagenta));
            puzzle.Constraints.AddRange(Constraint.KillerCage(17, Puzzle.TranslateCoordinates("H3-4,I3"), ConsoleColor.DarkMagenta));
            puzzle.Constraints.AddRange(Constraint.KillerCage(24, Puzzle.TranslateCoordinates("C8-9,D8"), ConsoleColor.DarkMagenta));
            puzzle.Constraints.AddRange(Constraint.KillerCage(17, Puzzle.TranslateCoordinates("A3-4,B4"), ConsoleColor.DarkYellow));
            puzzle.Constraints.AddRange(Constraint.KillerCage(12, Puzzle.TranslateCoordinates("F3-4"), ConsoleColor.DarkYellow));
            puzzle.Constraints.AddRange(Constraint.KillerCage(17, Puzzle.TranslateCoordinates("C-D6"), ConsoleColor.DarkYellow));
            puzzle.Constraints.AddRange(Constraint.KillerCage(35, Puzzle.TranslateCoordinates("G-H7-8,G9,I7"), ConsoleColor.DarkYellow));
            var solutions = puzzle.Solve().ToArray();
            Assert.IsNotNull(solutions);
            Assert.AreEqual(1, solutions.Length);
            Assert.IsTrue(solutions[0].SequenceEqual(new[] { 8, 3, 4, 2, 6, 1, 5, 7, 9, 1, 2, 6, 7, 5, 9, 8, 3, 4, 9, 7, 5, 3, 8, 4, 1, 2, 6, 3, 5, 2, 6, 4, 8, 7, 9, 1, 6, 9, 1, 5, 3, 7, 2, 4, 8, 7, 4, 8, 9, 1, 2, 3, 6, 5, 2, 1, 3, 4, 9, 5, 6, 8, 7, 5, 6, 9, 8, 7, 3, 4, 1, 2, 4, 8, 7, 1, 2, 6, 9, 5, 3 }));
        }

        [Test]
        public void TestEqualSumsSudoku()
        {
            // Tests a Sudoku in which several regions must have the same sum, but the sum is not given
            var puzzle = Puzzle.Sudoku(Puzzle.TranslateGivens("...3..79....5..3.2...2.7.14..4...8...1.......9.....62527...3.....3.2......64....."));
            puzzle.Constraints.Add(new EqualSumsConstraint
            {
                Regions = Ut.NewArray(
                    Puzzle.TranslateCoordinates("A1-5"),
                    Puzzle.TranslateCoordinates("B1-4"),
                    Puzzle.TranslateCoordinates("C1-3"),
                    Puzzle.TranslateCoordinates("G-I7"),
                    Puzzle.TranslateCoordinates("F-I8"),
                    Puzzle.TranslateCoordinates("E-I9"),
                    Puzzle.TranslateCoordinates("A9,B8,C7,D6,E5,F4,G3")
                )
            });
            var solutions = puzzle.Solve().ToArray();
            Assert.IsNotNull(solutions);
            Assert.AreEqual(1, solutions.Length);
            Assert.IsTrue(solutions[0].SequenceEqual(new[] { 1, 2, 5, 3, 4, 6, 7, 9, 8, 4, 8, 7, 5, 9, 1, 3, 6, 2, 3, 6, 9, 2, 8, 7, 5, 1, 4, 7, 5, 4, 9, 6, 2, 8, 3, 1, 6, 1, 2, 8, 3, 5, 9, 4, 7, 9, 3, 8, 1, 7, 4, 6, 2, 5, 2, 7, 1, 6, 5, 3, 4, 8, 9, 8, 4, 3, 7, 2, 9, 1, 5, 6, 5, 9, 6, 4, 1, 8, 2, 7, 3 }));
        }

        [Test]
        public void TestIrregularFrameSudoku()
        {
            var solutions = Puzzle.FrameSudoku(null, 2, 3, 11, 13, 13, 10, 8, 19, 13, 19, 10, 10, 18, 6, 6, 8, 23, 9, 13, 8, 11, 10, 6, 17, 18, 5, 10, 15, 16, 17, 6, 8, 9, 8, 19, 14, 17, 7).Solve().ToArray();
            Assert.IsNotNull(solutions);
            Assert.AreEqual(1, solutions.Length);
            Assert.IsTrue(solutions[0].SequenceEqual(new[] { 3, 4, 5, 7, 2, 6, 8, 9, 1, 8, 9, 2, 3, 1, 4, 5, 6, 7, 1, 7, 6, 8, 5, 9, 3, 4, 2, 7, 3, 9, 6, 8, 5, 1, 2, 4, 6, 2, 8, 9, 4, 1, 7, 5, 3, 4, 5, 1, 2, 7, 3, 6, 8, 9, 2, 6, 4, 5, 3, 7, 9, 1, 8, 5, 1, 3, 4, 9, 8, 2, 7, 6, 9, 8, 7, 1, 6, 2, 4, 3, 5 }));
        }
    }
}

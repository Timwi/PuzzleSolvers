using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PuzzleSolvers;

namespace PuzzleSolverTester
{
    [TestClass]
    public class SudokuTests : PuzzleTestFixture
    {
        [TestMethod]
        public void TestSudoku()
        {
            // Standard Sudoku
            assertUniqueSolution(
                new Sudoku().AddGivens("3...5...8.9..7.5.....8.41...2.7.....5...28..47.....6...6....8....2...9.1.1.9.5..."),
                3, 4, 6, 1, 5, 9, 2, 7, 8, 1, 9, 8, 2, 7, 6, 5, 4, 3, 2, 7, 5, 8, 3, 4, 1, 9, 6, 6, 2, 4, 7, 9, 1, 3, 8, 5, 5, 3, 9, 6, 2, 8, 7, 1, 4, 7, 8, 1, 5, 4, 3, 6, 2, 9, 9, 6, 3, 4, 1, 2, 8, 5, 7, 4, 5, 2, 3, 8, 7, 9, 6, 1, 8, 1, 7, 9, 6, 5, 4, 3, 2);
        }

        [TestMethod]
        public void TestJigsawSudoku()
        {
            // Jigsaw Sudoku; taken from Cracking the Cryptic: https://www.youtube.com/watch?v=wuduuLVGKDQ
            // This test has additional givens added to make it run faster. The version from the video takes this solver several seconds.
            assertUniqueSolution(
                new JigsawSudoku("A-G1,E2,G2;A2-8,B6-7;B2-5,C5-7,D5,D7;C2-4,D2-4,E-F3,F2;H1-5,G3,E-G4;I1-6,G5-6,H6;E-F5-7,D6,G-H7;B-E8,A-E9;I7,F-I8-9")
                    .AddGivens("3648....71.......5....68.....5.19......9.............28....3......235..1.......9."),
                3, 6, 4, 8, 9, 1, 5, 2, 7, 1, 8, 9, 3, 7, 4, 2, 6, 5, 5, 4, 2, 1, 6, 8, 7, 3, 9, 6, 2, 5, 7, 1, 9, 8, 4, 3, 2, 1, 3, 9, 8, 7, 4, 5, 6, 9, 3, 7, 4, 5, 6, 1, 8, 2, 8, 7, 6, 5, 2, 3, 9, 1, 4, 4, 9, 8, 2, 3, 5, 6, 7, 1, 7, 5, 1, 6, 4, 2, 3, 9, 8);
        }

        [TestMethod]
        public void TestAntiKingSudoku()
        {
            // No-touch Sudoku; taken from Cracking the Cryptic: https://www.youtube.com/watch?v=L4tWOtwLhP4
            assertUniqueSolution(
                new Sudoku().AddGivens("4...5...6.5.3.1.2...2...9...9.8.3.6.3.......9.6.9.5.7...4...8...2.1.9.4.9...3...2")
                    .AddConstraint(new AntiKingConstraint(9, 9)),
                4, 1, 9, 2, 5, 8, 7, 3, 6, 6, 5, 7, 3, 9, 1, 4, 2, 8, 8, 3, 2, 4, 7, 6, 9, 1, 5, 7, 9, 1, 8, 2, 3, 5, 6, 4, 3, 4, 5, 6, 1, 7, 2, 8, 9, 2, 6, 8, 9, 4, 5, 3, 7, 1, 1, 7, 4, 5, 6, 2, 8, 9, 3, 5, 2, 3, 1, 8, 9, 6, 4, 7, 9, 8, 6, 7, 3, 4, 1, 5, 2);
        }

        [TestMethod]
        public void TestThermometerSudoku()
        {
            // Thermometer Sudoku with givens; taken from Cracking the Cryptic: https://www.youtube.com/watch?v=RRSFXCpBBek
            // There’s also a version with more thermometers and no givens (https://www.youtube.com/watch?v=KTth49YrQVU) but it takes this solver several seconds.
            assertUniqueSolution(
                new Sudoku().AddGivens(".4.6.7.3...............................................7.....9....3.5.......1....").AddConstraints(LessThanConstraint.FromString("A2-6;C3-7;E2-8;G3-7;I2-6")),
                9, 4, 8, 6, 2, 7, 1, 3, 5, 2, 6, 7, 5, 3, 1, 9, 8, 4, 5, 3, 1, 9, 4, 8, 2, 7, 6, 6, 9, 2, 8, 5, 4, 3, 1, 7, 7, 5, 3, 1, 6, 9, 4, 2, 8, 8, 1, 4, 2, 7, 3, 5, 6, 9, 3, 7, 5, 4, 8, 2, 6, 9, 1, 1, 8, 6, 3, 9, 5, 7, 4, 2, 4, 2, 9, 7, 1, 6, 8, 5, 3);
        }

        [TestMethod]
        public void TestSumSudoku()
        {
            // Sum Sudoku, or “Little Killer” Sudoku, with no givens and no extra uniqueness constraints
            // Taken from Cracking the Cryptic: https://www.youtube.com/watch?v=4T_zQkNp5X0
            assertUniqueSolution(
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
                7, 3, 5, 1, 9, 6, 8, 4, 2, 4, 9, 1, 8, 5, 2, 7, 3, 6, 6, 2, 8, 4, 3, 7, 1, 5, 9, 1, 8, 7, 9, 4, 3, 2, 6, 5, 9, 4, 2, 6, 7, 5, 3, 8, 1, 3, 5, 6, 2, 1, 8, 4, 9, 7, 5, 6, 3, 7, 8, 1, 9, 2, 4, 8, 1, 4, 5, 2, 9, 6, 7, 3, 2, 7, 9, 3, 6, 4, 5, 1, 8);
        }

        [TestMethod]
        public void TestKillerSudoku()
        {
            // Killer Sudoku, where each cage has a sum and a uniqueness constraint
            // Taken from DailyKillerSudoku.com
            var sudoku = new KillerSudoku("ABCCDDDDDABEEFFFQQABEEHIIINKBLMHHJINKKLMMMJXXPGLOOMJSXPGGGOTTSUVVWWWTTSURRRRRYYSU", 18, 22, 5, 35, 18, 15, 23, 10, 16, 16, 7, 20, 23, 13, 18, 13, 9, 19, 16, 20, 13, 17, 12, 15, 12);
            for (var i = 0; i < 9; i++)
                sudoku.AddConstraint(new GivenConstraint(i * 10, "478532958"[i] - '0'));
            assertUniqueSolution(sudoku,
                4, 1, 2, 3, 7, 8, 6, 9, 5, 5, 7, 3, 6, 2, 9, 4, 8, 1, 9, 6, 8, 1, 5, 4, 2, 3, 7, 2, 8, 9, 5, 4, 1, 3, 7, 6, 1, 4, 5, 7, 3, 6, 8, 2, 9, 7, 3, 6, 9, 8, 2, 5, 1, 4, 6, 5, 7, 8, 1, 3, 9, 4, 2, 8, 9, 4, 2, 6, 7, 1, 5, 3, 3, 2, 1, 4, 9, 5, 7, 6, 8);
        }

        [TestMethod]
        public void TestEqualSumsSudoku()
        {
            // Tests a Sudoku in which several regions must have the same sum, but the sum is not given.
            // Taken from Cracking the Cryptic: https://www.youtube.com/watch?v=nkSXluRoV0w
            assertUniqueSolution(
                new Sudoku().AddGivens("...3..79....5..3.2...2.7.14..4...8...1.......9.....62527...3.....3.2......64.....")
                    .AddConstraint(new EqualSumsConstraint("A1-5;B1-4;C1-3;G-I7;F-I8;E-I9;A9,B8,C7,D6,E5,F4,G3".Split(';').Select(str => Constraint.TranslateCoordinates(str)).ToArray())),
                1, 2, 5, 3, 4, 6, 7, 9, 8, 4, 8, 7, 5, 9, 1, 3, 6, 2, 3, 6, 9, 2, 8, 7, 5, 1, 4, 7, 5, 4, 9, 6, 2, 8, 3, 1, 6, 1, 2, 8, 3, 5, 9, 4, 7, 9, 3, 8, 1, 7, 4, 6, 2, 5, 2, 7, 1, 6, 5, 3, 4, 8, 9, 8, 4, 3, 7, 2, 9, 1, 5, 6, 5, 9, 6, 4, 1, 8, 2, 7, 3);
        }

        [TestMethod]
        public void TestIrregularFrameSudoku()
        {
            // Irregular Frame Sudoku; taken from Cracking the Cryptic: https://www.youtube.com/watch?v=dW5mYUi0XkQ
            assertUniqueSolution(
                new FrameSumSudoku(minLength: 2, maxLength: 3, /* sum clues start here */ 11, 13, 13, 10, 8, 19, 13, 19, 10, 10, 18, 6, 6, 8, 23, 9, 13, 8, 11, 10, 6, 17, 18, 5, 10, 15, 16, 17, 6, 8, 9, 8, 19, 14, 17, 7),
                3, 4, 5, 7, 2, 6, 8, 9, 1, 8, 9, 2, 3, 1, 4, 5, 6, 7, 1, 7, 6, 8, 5, 9, 3, 4, 2, 7, 3, 9, 6, 8, 5, 1, 2, 4, 6, 2, 8, 9, 4, 1, 7, 5, 3, 4, 5, 1, 2, 7, 3, 6, 8, 9, 2, 6, 4, 5, 3, 7, 9, 1, 8, 5, 1, 3, 4, 9, 8, 2, 7, 6, 9, 8, 7, 1, 6, 2, 4, 3, 5);
        }

        [TestMethod]
        public void TestOddEvenSudoku()
        {
            // Here’s an interesting Sudoku which has two solutions — one in which all the constrained cells are odd, and one in which they are all even.
            // Taken from Cracking the Cryptic: https://www.youtube.com/watch?v=pzVy93NhOzY
            var solutions = new Sudoku().AddGivens("....4..9......9..6..9...7.....7...6.1...6...5.2...5.....3...4..2..4...5..7..5....")
                .AddConstraint(new OddEvenConstraint(OddEvenType.AllSame, Constraint.TranslateCoordinates("A4,B3,B5,C2,C6,D1,D7,E2,E8,F3,F9,G4,G8,H5,H7,I6")))
                .Solve().Take(3).ToArray();

            Assert.IsNotNull(solutions);
            Assert.AreEqual(2, solutions.Length);

            // Solution where the constrained cells are all even
            Assert.IsTrue(solutions.Any(s => s.SequenceEqual(new[] { 3, 1, 2, 6, 4, 7, 5, 9, 8, 7, 4, 8, 5, 2, 9, 1, 3, 6, 5, 6, 9, 1, 3, 8, 7, 4, 2, 4, 3, 5, 7, 9, 2, 8, 6, 1, 1, 8, 7, 3, 6, 4, 9, 2, 5, 9, 2, 6, 8, 1, 5, 3, 7, 4, 6, 5, 3, 2, 7, 1, 4, 8, 9, 2, 9, 1, 4, 8, 3, 6, 5, 7, 8, 7, 4, 9, 5, 6, 2, 1, 3 })));
            // Solution where the constrained cells are all odd
            Assert.IsTrue(solutions.Any(s => s.SequenceEqual(new[] { 7, 6, 2, 5, 4, 8, 3, 9, 1, 4, 3, 1, 2, 7, 9, 5, 8, 6, 8, 5, 9, 6, 1, 3, 7, 2, 4, 3, 4, 5, 7, 8, 2, 1, 6, 9, 1, 9, 8, 3, 6, 4, 2, 7, 5, 6, 2, 7, 1, 9, 5, 8, 4, 3, 5, 8, 3, 9, 2, 6, 4, 1, 7, 2, 1, 6, 4, 3, 7, 9, 5, 8, 9, 7, 4, 8, 5, 1, 6, 3, 2 })));
        }

        [TestMethod]
        public void TestSumOrProductSudoku()
        {
            // Tests OrConstraint by combining a SumConstraint and a ProductConstraint (SumOrProductConstraint is just a helper class for exactly that)
            // Taken from Cracking the Cryptic: https://www.youtube.com/watch?v=53VH-vLWBUg
            assertUniqueSolution(new Sudoku().AddConstraints(
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
                new SumOrProductConstraint(8, Enumerable.Range(0, 5).Select(i => 36 + i * 10))
            ), 8, 9, 3, 7, 2, 5, 4, 6, 1, 6, 4, 7, 1, 3, 9, 5, 2, 8, 2, 5, 1, 8, 6, 4, 7, 9, 3, 5, 3, 9, 6, 7, 1, 8, 4, 2, 1, 6, 8, 9, 4, 2, 3, 5, 7, 7, 2, 4, 3, 5, 8, 9, 1, 6, 3, 7, 2, 4, 9, 6, 1, 8, 5, 4, 1, 5, 2, 8, 7, 6, 3, 9, 9, 8, 6, 5, 1, 3, 2, 7, 4);
        }

        [TestMethod]
        public void TestSandwichSudoku()
        {
            // Tests SandwichUniquenessConstraint with sums sandwiched between 1 and 9 (the typical variant).
            // Taken from Cracking the Cryptic: https://www.youtube.com/watch?v=qUZnq5nP0zI
            assertUniqueSolution(new SandwichSudoku(columnClues: new[] { 15, 9, 26, 8, 8, 12, 0, 12, 6 }, rowClues: new[] { 7, 14, 20, 2, 8, 26, 10, 31, 16 })
                .AddGivens(@"............9......1............9...............8............5......5............"),
                3, 8, 9, 2, 5, 1, 4, 7, 6, 4, 5, 7, 9, 6, 8, 1, 2, 3, 2, 1, 6, 3, 7, 4, 9, 8, 5, 6, 4, 8, 5, 3, 9, 2, 1, 7, 9, 3, 5, 1, 2, 7, 6, 4, 8, 7, 2, 1, 8, 4, 6, 5, 3, 9, 8, 9, 4, 6, 1, 3, 7, 5, 2, 1, 6, 2, 7, 8, 5, 3, 9, 4, 5, 7, 3, 4, 9, 2, 8, 6, 1);
        }

        [TestMethod]
        public void TestSandwich46Sudoku()
        {
            // Tests SandwichUniquenessConstraint with sums sandwiched between 4 and 6.
            // Taken from Cracking the Cryptic: https://www.youtube.com/watch?v=ZzH2_tjQAHE
            assertUniqueSolution(new SandwichSudoku(crust1: 4, crust2: 6, columnClues: new[] { 26, 22, 11, 3, 2, 8, 18, 6, 12 }, rowClues: new[] { 35, 16, 10, 14, 6, 2, 1, 25, 7 })
                .AddGivens((66, 1), (77, 4)),
                4, 1, 3, 5, 8, 2, 9, 7, 6, 2, 5, 8, 6, 9, 7, 4, 3, 1, 9, 6, 7, 3, 4, 1, 5, 8, 2, 8, 3, 1, 4, 2, 5, 7, 6, 9, 7, 9, 5, 8, 6, 3, 2, 1, 4, 6, 2, 4, 7, 1, 9, 3, 5, 8, 5, 8, 2, 9, 3, 6, 1, 4, 7, 3, 4, 9, 1, 7, 8, 6, 2, 5, 1, 7, 6, 2, 5, 4, 8, 9, 3);
        }

        [TestMethod]
        public void TestSandwichWraparoundSudoku()
        {
            // Tests SandwichWraparoundUniquenessConstraint — the one where the Sudoku grid behaves like a torus.
            // Taken from Cracking the Cryptic: https://www.youtube.com/watch?v=L7KTfeKsxS4
            assertUniqueSolution(new SandwichSudoku(wraparound: true, columnClues: new[] { 21, 26, 25, 21, 9, 17, 4, 3, 9 }, rowClues: new[] { 13, 0, 29, 22, 18, 18, 35, 9, 35 })
                .AddGivens((11, 9), (34, 3)),
                6, 3, 2, 1, 8, 5, 9, 4, 7, 7, 1, 9, 6, 4, 2, 3, 8, 5, 8, 5, 4, 7, 3, 9, 6, 1, 2, 9, 2, 6, 5, 1, 8, 7, 3, 4, 5, 4, 1, 3, 7, 6, 2, 9, 8, 3, 8, 7, 9, 2, 4, 5, 6, 1, 2, 7, 3, 4, 9, 1, 8, 5, 6, 4, 9, 5, 8, 6, 7, 1, 2, 3, 1, 6, 8, 2, 5, 3, 4, 7, 9);
        }

        [TestMethod]
        public void TestToroidalAntiKnightSudoku()
        {
            // Tests the toroidal version of the anti-knight constraint. 
            // Puzzle taken from Logic Masters: https://logic-masters.de/Raetselportal/Raetsel/zeigen.php?id=0003F0
            var area = "C-G5,D4,F4,D6,F6".TranslateCoordinates(9);
            assertUniqueSolution(new JigsawSudoku(Enumerable.Range(0, 9).Select(i => area.Select(cell => (cell % 9 + i) % 9 + 9 * ((cell / 9 + 2 * i) % 9)).ToArray()).ToArray())
                .AddGivens("..............................2.6.....13579.....4.8..............................")
                .AddConstraint(new AntiKnightConstraint(9, 9, toroidal: true)),
                8, 7, 2, 6, 4, 9, 3, 1, 5, 7, 3, 6, 5, 9, 2, 1, 4, 8, 3, 4, 5, 7, 2, 1, 6, 8, 9, 4, 5, 8, 2, 3, 6, 7, 9, 1, 6, 8, 1, 3, 5, 7, 9, 2, 4, 9, 1, 3, 4, 7, 8, 2, 5, 6, 1, 2, 4, 9, 8, 3, 5, 6, 7, 2, 6, 9, 8, 1, 5, 4, 7, 3, 5, 9, 7, 1, 6, 4, 8, 3, 2);
        }
    }
}

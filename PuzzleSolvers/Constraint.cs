using System;
using RT.Util;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Abstract base class for all constraints in a puzzle.</summary>
    public abstract class Constraint
    {
        /// <summary>
        ///     Constraint implementations must modify <paramref name="takens"/> to mark values as taken that are known to be
        ///     impossible given the specified incomplete grid.</summary>
        /// <param name="takens">
        ///     The array to be modified. The first dimension equates to the cells in the puzzle. The second dimension equates
        ///     to the possible values for the cell, indexed from 0. In a standard Sudoku, indexes 0 to 8 are used for the
        ///     numbers 1 to 9. Only set values to <c>true</c> that are now impossible to satisfy; implementations must not
        ///     change other values back to <c>false</c>.</param>
        /// <param name="grid">
        ///     The incomplete grid at the current point during the algorithm. Implementations must not modify this array. In
        ///     order to communicate that a cell must have a specific value, mark all other possible values on that cell as
        ///     taken in the <paramref name="takens"/> array.</param>
        /// <param name="ix">
        ///     If <c>null</c>, this method was called either at the very start of the algorithm or because this constraint
        ///     was returned from another constraint. In such a case, the method must examine all filled-in values in the
        ///     provided grid. Otherwise, specifies which value has just been placed and allows the method to update <paramref
        ///     name="takens"/> based only on the value in that square.</param>
        /// <param name="minValue">
        ///     The minimum value that squares can have in this puzzle. For standard Sudoku, this is 1. This is also the
        ///     difference between the real-life values in the grid and the indexes used in the <paramref name="takens"/>
        ///     array.</param>
        /// <param name="maxValue">
        ///     The maximum value that squares can have in this puzzle. For standard Sudoku, this is 9.</param>
        /// <returns>
        ///     <para>
        ///         Implementations must return <c>null</c> if the constraint remains valid for the remainder of filling this
        ///         grid, or a collection of constraints that this constraint shall be replaced with (can be empty).</para>
        ///     <para>
        ///         For example, in <see cref="EqualSumsConstraint"/>, since the sum is not initially known, the constraint
        ///         waits until one of its regions is filled and then uses this return value to replace itself with several
        ///         <see cref="SumConstraint"/>s to ensure the other regions have the same sum.</para>
        ///     <para>
        ///         The algorithm will automatically call this method again on all the new constraints for all cells already
        ///         placed in the grid. The constraints returned MUST NOT themselves return yet more constraints at that
        ///         point.</para></returns>
        public abstract IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue);

        /// <summary>
        ///     Constraint implementations may override this to have <see cref="Puzzle.SudokuSolutionToConsoleString(int[],
        ///     int)"/> color a cell a certain text color.</summary>
        /// <param name="ix">
        ///     The cell to be colored.</param>
        /// <returns>
        ///     <c>null</c> for no special coloring, otherwise a <see cref="ConsoleColor"/> value.</returns>
        public virtual ConsoleColor? CellColor(int ix) => null;

        /// <summary>
        ///     Constraint implementations may override this to have <see cref="Puzzle.SudokuSolutionToConsoleString(int[],
        ///     int)"/> color a cell a certain background color.</summary>
        /// <param name="ix">
        ///     The cell to be colored.</param>
        /// <returns>
        ///     <c>null</c> for no special coloring, otherwise a <see cref="ConsoleColor"/> value.</returns>
        public virtual ConsoleColor? CellBackgroundColor(int ix) => null;

        /// <summary>
        ///     Converts a convenient coordinate notation into puzzle-grid indices.</summary>
        /// <param name="str">
        ///     A string such as <c>"A1-4,B-E1,E2"</c>. The letters are assumed to be columns (starting from A) while the
        ///     digits are rows (starting from 1). Ranges can be combined to create rectangles; thus, <c>A-C1-3</c> generates
        ///     a 3×3 square.</param>
        /// <param name="gridWidth">
        ///     The width of the puzzle grid. For a standard Sudoku, this is 9.</param>
        /// <returns>
        ///     An array of cell indices.</returns>
        public static IEnumerable<int> TranslateCoordinates(string str, int gridWidth = 9)
        {
            foreach (var part in str.Split(','))
            {
                var m = Regex.Match(part, @"^\s*(?<col>[A-I](\s*-\s*(?<colr>[A-I]))?)\s*(?<row>[1-9](\s*-\s*(?<rowr>[1-9]))?)\s*$");
                if (!m.Success)
                    throw new ArgumentException(string.Format(@"The region “{0}” is not in a valid format. Expected a column letter (or a range of columns, e.g. A-D) followed by a row digit (or a range of rows, e.g. 1-4).", part), nameof(str));
                var col = m.Groups["col"].Value[0] - 'A';
                var colr = m.Groups["colr"].Success ? m.Groups["colr"].Value[0] - 'A' : col;
                var row = m.Groups["row"].Value[0] - '1';
                var rowr = m.Groups["rowr"].Success ? m.Groups["rowr"].Value[0] - '1' : row;
                for (var c = col; c <= colr; c++)
                    for (var r = row; r <= rowr; r++)
                        yield return c + gridWidth * r;
            }
        }

        /// <summary>
        ///     Returns a collection of <see cref="GivenConstraint"/>s for the non-<c>null</c> values in the provided array.</summary>
        /// <param name="givens">
        ///     An array containing given (pre-filled) values for a puzzle. Use <c>null</c> to indicate that a cell doesn’t
        ///     have a given.</param>
        public static IEnumerable<Constraint> Givens(int?[] givens)
        {
            if (givens == null)
                throw new ArgumentNullException(nameof(givens));
            for (var i = 0; i < givens.Length; i++)
                if (givens[i] != null)
                    yield return new GivenConstraint(i, givens[i].Value);
        }

        /// <summary>
        ///     Returns a collection of <see cref="GivenConstraint"/>s from the specified string representation.</summary>
        /// <param name="givens">
        ///     A string such as <c>"3...5...8.9..7.5.....8.41...2.7.....5...28..47.....6...6....8....2...9.1.1.9.5..."</c>.
        ///     Each digit is a given value, while periods (<c>.</c>) indicate no given for that cell.</param>
        /// <returns>
        ///     An array of nullable integers containing the givens.</returns>
        public static IEnumerable<Constraint> Givens(string givens)
        {
            if (givens == null)
                throw new ArgumentNullException(nameof(givens));
            if (!givens.All(ch => ch == '.' || (ch >= '0' && ch <= '9')))
                throw new ArgumentException("‘givens’ must contain only digits 0–9 and periods (.) for cells with no given.", "givens");
            return Givens(givens.Select(ch => ch == '.' ? (int?) null : ch - '0').ToArray());
        }

        /// <summary>
        ///     Returns a collection of <see cref="UniquenessConstraint"/>s that span the rows and columns of a rectangular
        ///     grid of the specified dimensions.</summary>
        public static IEnumerable<Constraint> LatinSquare(int width, int height)
        {
            // Rows
            for (var row = 0; row < height; row++)
                yield return new UniquenessConstraint(Enumerable.Range(0, width).Select(col => row * width + col));

            // Columns
            for (var col = 0; col < width; col++)
                yield return new UniquenessConstraint(Enumerable.Range(0, height).Select(row => row * width + col));
        }

        /// <summary>Returns all the <see cref="UniquenessConstraint"/>s for a standard 3×3 Sudoku.</summary>
        public static IEnumerable<Constraint> Sudoku()
        {
            foreach (var c in LatinSquare(9, 9))
                yield return c;

            // 3×3 regions
            for (var r = 0; r < 9; r++)
                yield return new UniquenessConstraint(Enumerable.Range(0, 9).Select(i => i % 3 + 3 * (r % 3) + 9 * (i / 3 + 3 * (r / 3))));
        }

        /// <summary>
        ///     Constructs a cage (region) for a Killer Sudoku. This is just a <see cref="SumConstraint"/> and a <see
        ///     cref="UniquenessConstraint"/> for the same region.</summary>
        /// <param name="sum">
        ///     The desired sum for the cage.</param>
        /// <param name="affectedCells">
        ///     The set of cells contained in this cage. Use <see cref="TranslateCoordinates(string, int)"/> for convenience.</param>
        /// <param name="backgroundColor">
        ///     An optional background color to use when outputting a solution using <see
        ///     cref="Puzzle.SudokuSolutionToConsoleString(int[], int)"/>.</param>
        /// <returns>
        ///     A collection containing the two required constraints.</returns>
        public static IEnumerable<Constraint> KillerCage(int sum, int[] affectedCells, ConsoleColor? backgroundColor = null)
        {
            yield return new SumConstraint(sum, affectedCells);
            yield return new UniquenessConstraint(affectedCells, backgroundColor);
        }

        /// <summary>
        ///     Returns all the <see cref="UniquenessConstraint"/>s for a 3×3 Sudoku with irregularly shaped regions.</summary>
        /// <param name="regions">
        ///     The regions that subdivide this Sudoku. There must be exactly 9 and they must each cover exactly 9 cells.</param>
        public static IEnumerable<Constraint> JigsawSudoku(params IEnumerable<int>[] regions)
        {
            if (regions == null)
                throw new ArgumentNullException(nameof(regions));
            if (regions.Length != 9)
                throw new ArgumentOutOfRangeException("There must be exactly 9 regions in a Jigsaw Sudoku.", nameof(regions));

            ConsoleColor convertToColor(int number)
            {
                number++;
                if (number >= 7)
                    number++;
                return (ConsoleColor) number;
            }

            var regionConstraints = new List<UniquenessConstraint>();
            foreach (var region in regions)
            {
                regionConstraints.Add(new UniquenessConstraint(region, convertToColor(regionConstraints.Count)));
                if (regionConstraints.Last().AffectedCells.Length != 9)
                    throw new ArgumentException(string.Format(@"A region ({0}) does not cover exactly 9 cells.", region.JoinString(", ")), nameof(regions));
            }
            var missingCells = Enumerable.Range(0, 81).Except(regionConstraints.SelectMany(c => c.AffectedCells)).ToArray();
            if (missingCells.Length != 0)
                throw new ArgumentException(string.Format(
                    missingCells.Length == 1 ? @"A cell ({0}) is not covered by any region." : @"Some cells ({0}) are not covered by any region.",
                    missingCells.JoinString(", ")), nameof(regions));

            return regionConstraints.Concat(LatinSquare(9, 9));
        }

        /// <summary>
        ///     Generates a Frame-Sum Sudoku, which is surrounded by numbers. Every number indicates the sum of the first few
        ///     cells visible from that location. Between <paramref name="minLength"/> and <paramref name="maxLength"/> cells
        ///     may be included in that sum.</summary>
        /// <param name="minLength">
        ///     The minimum number of cells included in the sum in each clue.</param>
        /// <param name="maxLength">
        ///     The maximum number of cells included in the sum in each clue.</param>
        /// <param name="gridWidth">
        ///     The width of the grid. For a standard Sudoku, this is 9.</param>
        /// <param name="gridHeight">
        ///     The height of the grid. For a standard Sudoku, this is 9.</param>
        /// <param name="cluesClockwiseFromTopLeft">
        ///     The clues outside the grid, given in clockwise order from the top of the left column.</param>
        public static IEnumerable<Constraint> FrameSums(int minLength, int maxLength, int gridWidth, int gridHeight, params int[] cluesClockwiseFromTopLeft)
        {
            if (cluesClockwiseFromTopLeft == null)
                throw new ArgumentNullException(nameof(cluesClockwiseFromTopLeft));
            if (cluesClockwiseFromTopLeft.Length != 2 * gridWidth + 2 * gridHeight)
                throw new ArgumentException(string.Format(@"‘cluesClockwiseFromTopLeft’ must provide exactly {0} clues.", 2 * gridWidth + 2 * gridHeight), nameof(cluesClockwiseFromTopLeft));

            for (var col = 0; col < gridWidth; col++)
            {
                yield return new SumAlternativeConstraint(cluesClockwiseFromTopLeft[col], Enumerable.Range(minLength, maxLength - minLength + 1).Select(len => Enumerable.Range(0, len).Select(row => gridWidth * row + col)).ToArray());
                yield return new SumAlternativeConstraint(cluesClockwiseFromTopLeft[2 * gridWidth + gridHeight - 1 - col], Enumerable.Range(minLength, maxLength - minLength + 1).Select(len => Enumerable.Range(0, len).Select(row => gridWidth * (gridHeight - 1 - row) + col)).ToArray());
            }
            for (var row = 0; row < gridHeight; row++)
            {
                yield return new SumAlternativeConstraint(cluesClockwiseFromTopLeft[gridWidth + row], Enumerable.Range(minLength, maxLength - minLength + 1).Select(len => Enumerable.Range(0, len).Select(col => gridWidth * row + (gridWidth - 1 - col))).ToArray());
                yield return new SumAlternativeConstraint(cluesClockwiseFromTopLeft[2 * gridHeight + 2 * gridWidth - 1 - row], Enumerable.Range(minLength, maxLength - minLength + 1).Select(len => Enumerable.Range(0, len).Select(col => gridWidth * row + col)).ToArray());
            }
        }

        /// <summary>
        ///     Generates the constraints for several sum cages (as used in, for example, a Killer Sudoku).</summary>
        /// <param name="field">
        ///     A string containing letters A, B, etc. identifying the cages.</param>
        /// <param name="sums">
        ///     The sums associated with each cage in the same order as the letters.</param>
        /// <returns>
        ///     Constraints for all of the specified Killer cages. The regions are also associated with 6 different background
        ///     colors in the same order as the letters.</returns>
        public static IEnumerable<Constraint> KillerCages(string field, params int[] sums)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));
            if (field.Any(ch => ch < 'A' || ch > 'Z'))
                throw new ArgumentException("The ‘field’ must contain only letter A-Z.", nameof(field));
            if (sums == null)
                throw new ArgumentNullException(nameof(sums));

            var cages = new Dictionary<char, List<int>>();
            for (var cell = 0; cell < field.Length; cell++)
                cages.AddSafe(field[cell], cell);

            if (Enumerable.Range(0, sums.Length).Any(i => !cages.ContainsKey((char) ('A' + i))))
                throw new ArgumentException("The ‘field’ must contain every letter from A up to however many elements are in ‘sums’.", nameof(field));

            foreach (var kvp in cages)
                foreach (var c in KillerCage(sums[kvp.Key - 'A'], kvp.Value.ToArray(), (ConsoleColor) ((kvp.Key - 'A') % 6 + 1)))
                    yield return c;
        }
    }
}

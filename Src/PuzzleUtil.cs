using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>Contains extension methods for code related to PuzzleSolvers.</summary>
    public static class PuzzleUtil
    {
        /// <summary>
        ///     Converts a convenient coordinate notation into a puzzle-grid index.</summary>
        /// <param name="str">
        ///     A string such as <c>"E2"</c>. The letter is assumed to be a column (starting from A) while the digit is a row
        ///     (starting from 1).</param>
        /// <param name="gridWidth">
        ///     The width of the puzzle grid. For a standard Sudoku, this is 9.</param>
        /// <returns>
        ///     A cell index.</returns>
        public static int TranslateCoordinate(this string str, int gridWidth = 9) => Constraint.TranslateCoordinate(str, gridWidth);

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
        public static IEnumerable<int> TranslateCoordinates(this string str, int gridWidth = 9) => Constraint.TranslateCoordinates(str, gridWidth);

        /// <summary>
        ///     Expresses the given cell as letter-number coordinates (top-left corner is A1).</summary>
        /// <param name="cell">
        ///     The index of the cell to turn into coordinates.</param>
        /// <param name="gridWidth">
        ///     The width of the puzzle grid.</param>
        public static string AsCoordinate(this int cell, int gridWidth = 9) => $"{(char) ('A' + cell % gridWidth)}{cell / gridWidth + 1}";

        /// <summary>
        ///     Generates all combinations of <paramref name="numDigits"/> digits, each between <paramref name="minValue"/>
        ///     and <paramref name="maxValue"/>.</summary>
        /// <param name="minValue">
        ///     Minimum value to use.</param>
        /// <param name="maxValue">
        ///     Maximum value to use.</param>
        /// <param name="numDigits">
        ///     Number of digits in each combination.</param>
        /// <param name="allowDuplicates">
        ///     If <c>true</c>, duplicated digits are allowed.</param>
        public static IEnumerable<int[]> Combinations(int minValue, int maxValue, int numDigits, bool allowDuplicates = false)
        {
            IEnumerable<int[]> recurse(int[] sofar, int ix)
            {
                if (ix == numDigits)
                {
                    yield return sofar.ToArray();
                    yield break;
                }
                for (var v = minValue; v <= maxValue; v++)
                    if (allowDuplicates || !sofar.Take(ix).Contains(v))
                    {
                        sofar[ix] = v;
                        foreach (var s in recurse(sofar, ix + 1))
                            yield return s;
                    }
            }
            return recurse(new int[numDigits], 0);
        }

        /// <summary>Returns the set of cells adjacent to the specified cell (including diagonals).</summary>
        public static IEnumerable<int> Adjacent(int cell, int gridWidth = 9, int gridHeight = 9)
        {
            var x = cell % gridWidth;
            var y = cell / gridHeight;
            for (var xx = x - 1; xx <= x + 1; xx++)
                if (xx >= 0 && xx < gridWidth)
                    for (var yy = y - 1; yy <= y + 1; yy++)
                        if (yy >= 0 && yy < gridHeight && (xx != x || yy != y))
                            yield return xx + gridWidth * yy;
        }

        /// <summary>Returns the set of cells orthogonally adjacent to the specified cell (no diagonals).</summary>
        public static IEnumerable<int> Orthogonal(int cell, int gridWidth = 9, int gridHeight = 9)
        {
            var x = cell % gridWidth;
            var y = cell / gridHeight;
            for (var xx = x - 1; xx <= x + 1; xx++)
                if (xx >= 0 && xx < gridWidth)
                    for (var yy = y - 1; yy <= y + 1; yy++)
                        if (yy >= 0 && yy < gridHeight && (xx == x || yy == y) && (xx != x || yy != y))
                            yield return xx + gridWidth * yy;
        }

        /// <summary>Specifies the delta-x for directions Up, Right, Down, Left in order.</summary>
        public static readonly int[] Dxs = [0, 1, 0, -1];
        /// <summary>Specifies the delta-y for directions Up, Right, Down, Left in order.</summary>
        public static readonly int[] Dys = [-1, 0, 1, 0];
    }
}

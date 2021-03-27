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
    }
}

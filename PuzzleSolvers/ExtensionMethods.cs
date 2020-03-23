using System.Collections.Generic;

namespace PuzzleSolvers
{
    /// <summary>Contains extension methods for code related to PuzzleSolvers.</summary>
    public static class ExtensionMethods
    {
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
    }
}

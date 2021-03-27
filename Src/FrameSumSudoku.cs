using System;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a Frame-Sum Sudoku, which is surrounded by numbers. Every number indicates the sum of the first few
    ///     cells visible from that location. Between <c>minLength</c> and <c>maxLength</c> cells may be included in that sum.</summary>
    public class FrameSumSudoku : Sudoku
    {
        /// <summary>
        ///     Constructor.</summary>
        /// <param name="minLength">
        ///     The minimum number of cells included in the sum.</param>
        /// <param name="maxLength">
        ///     The maximum number of cells included in the sum.</param>
        /// <param name="cluesClockwiseFromTopLeft">
        ///     The 36 clues surrounding the grid in clockwise order, starting with the clues above the columns.</param>
        public FrameSumSudoku(int minLength, int maxLength, params int[] cluesClockwiseFromTopLeft)
            : this(minLength, maxLength, cluesClockwiseFromTopLeft, 1) { }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="minLength">
        ///     The minimum number of cells included in the sum.</param>
        /// <param name="maxLength">
        ///     The maximum number of cells included in the sum.</param>
        /// <param name="cluesClockwiseFromTopLeft">
        ///     The 36 clues surrounding the grid in clockwise order, starting with the clues above the columns.</param>
        /// <param name="minValue">
        ///     The minimum value used in the puzzle.</param>
        public FrameSumSudoku(int minLength, int maxLength, int[] cluesClockwiseFromTopLeft, int minValue = 1)
            : base(minValue)
        {
            if (minLength < 1 || minLength > 9)
                throw new ArgumentException(@"‘minLength’ must be in the range 1–9.", nameof(minLength));
            if (maxLength < minLength || maxLength > 9)
                throw new ArgumentException(@"‘maxLength’ must be in the range minLength–9.", nameof(maxLength));
            if (cluesClockwiseFromTopLeft == null)
                throw new ArgumentNullException(nameof(cluesClockwiseFromTopLeft));
            if (cluesClockwiseFromTopLeft.Length != 4 * 9)
                throw new ArgumentException(string.Format(@"‘cluesClockwiseFromTopLeft’ must provide exactly {0} clues.", 4 * 9), nameof(cluesClockwiseFromTopLeft));

            const int gridWidth = 9;
            const int gridHeight = 9;
            for (var col = 0; col < gridWidth; col++)
            {
                Constraints.Add(new SumAlternativeConstraint(cluesClockwiseFromTopLeft[col], Enumerable.Range(minLength, maxLength - minLength + 1).Select(len => Enumerable.Range(0, len).Select(row => gridWidth * row + col)).ToArray()));
                Constraints.Add(new SumAlternativeConstraint(cluesClockwiseFromTopLeft[2 * gridWidth + gridHeight - 1 - col], Enumerable.Range(minLength, maxLength - minLength + 1).Select(len => Enumerable.Range(0, len).Select(row => gridWidth * (gridHeight - 1 - row) + col)).ToArray()));
            }
            for (var row = 0; row < gridHeight; row++)
            {
                Constraints.Add(new SumAlternativeConstraint(cluesClockwiseFromTopLeft[gridWidth + row], Enumerable.Range(minLength, maxLength - minLength + 1).Select(len => Enumerable.Range(0, len).Select(col => gridWidth * row + (gridWidth - 1 - col))).ToArray()));
                Constraints.Add(new SumAlternativeConstraint(cluesClockwiseFromTopLeft[2 * gridHeight + 2 * gridWidth - 1 - row], Enumerable.Range(minLength, maxLength - minLength + 1).Select(len => Enumerable.Range(0, len).Select(col => gridWidth * row + col)).ToArray()));
            }
        }
    }
}

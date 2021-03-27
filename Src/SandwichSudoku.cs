using System;
using System.Linq;
using RT.Util;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a Sudoku variant in which numbers written outside the grid describe the sum of the digits in that
    ///     row/column that are sandwiched between two specific values (usually 1 and 9).</summary>
    public class SandwichSudoku : Sudoku
    {
        /// <summary>
        ///     Generates a Sandwich Sudoku in which all the sandwich constraints for every column and row are specified.</summary>
        /// <param name="columnClues">
        ///     Sandwich clues for the columns (from left to right).</param>
        /// <param name="rowClues">
        ///     Sandwich clues for the rows (from top to bottom).</param>
        /// <param name="crust1">
        ///     The lower value of the two that “sandwich” the sum.</param>
        /// <param name="crust2">
        ///     The higher value of the two that “sandwich” the sum.</param>
        /// <param name="minValue">
        ///     Minimum value to be used in the grid.</param>
        /// <param name="wraparound">
        ///     If <c>false</c>, the clues are assumed to be the sum between the crusts no matter which order they are in in a
        ///     row/column. If <c>true</c>, the clues are assumed to be the sum of the digits going from <paramref
        ///     name="crust1"/> to <paramref name="crust2"/>, wrapping around the edge of the grid if they are in the “wrong”
        ///     order.</param>
        public SandwichSudoku(int[] columnClues, int[] rowClues, int crust1 = 1, int crust2 = 9, int minValue = 1, bool wraparound = false) : this(
            columnClues == null ? throw new ArgumentNullException(nameof(columnClues)) : columnClues.Select(i => i.Nullable()).ToArray(),
            rowClues == null ? throw new ArgumentNullException(nameof(rowClues)) : rowClues.Select(i => i.Nullable()).ToArray(),
            crust1, crust2, minValue, wraparound)
        { }

        /// <summary>
        ///     Generates a Sandwich Sudoku in which some columns and rows have sandwich constraints.</summary>
        /// <param name="columnClues">
        ///     Sandwich clues for the columns (from left to right).</param>
        /// <param name="rowClues">
        ///     Sandwich clues for the rows (from top to bottom).</param>
        /// <param name="crust1">
        ///     The lower value of the two that “sandwich” the sum.</param>
        /// <param name="crust2">
        ///     The higher value of the two that “sandwich” the sum.</param>
        /// <param name="minValue">
        ///     Minimum value to be used in the grid.</param>
        /// <param name="wraparound">
        ///     If <c>false</c>, the clues are assumed to be the sum between the crusts no matter which order they are in in a
        ///     row/column. If <c>true</c>, the clues are assumed to be the sum of the digits going from <paramref
        ///     name="crust1"/> to <paramref name="crust2"/>, wrapping around the edge of the grid if they are in the “wrong”
        ///     order.</param>
        public SandwichSudoku(int?[] columnClues, int?[] rowClues, int crust1 = 1, int crust2 = 9, int minValue = 1, bool wraparound = false) : base(minValue)
        {
            if (columnClues == null)
                throw new ArgumentNullException(nameof(columnClues));
            if (columnClues.Length != 9)
                throw new ArgumentOutOfRangeException("‘columnClues’ must have exactly 9 values.", nameof(columnClues));
            if (rowClues == null)
                throw new ArgumentNullException(nameof(rowClues));
            if (rowClues.Length != 9)
                throw new ArgumentOutOfRangeException("‘rowClues’ must have exactly 9 values.", nameof(rowClues));
            if (crust1 < minValue || crust1 > minValue + 8)
                throw new ArgumentOutOfRangeException("‘crust1’ must be in the range of ‘minValue’ to ‘minValue + 8’.", nameof(crust1));
            if (crust2 <= crust1 || crust2 > minValue + 8)
                throw new ArgumentOutOfRangeException("‘crust2’ must be in the range of ‘crust1 + 1’ to ‘minValue + 8’.", nameof(crust2));

            for (var col = 0; col < 9; col++)
                if (columnClues[col] != null)
                    AddConstraint(wraparound
                        ? new SandwichWraparoundUniquenessConstraint(crust1, crust2, columnClues[col].Value, Enumerable.Range(0, 9).Select(row => row * 9 + col), minValue, minValue + 8)
                        : (Constraint) new SandwichUniquenessConstraint(crust1, crust2, columnClues[col].Value, Enumerable.Range(0, 9).Select(row => row * 9 + col), minValue, minValue + 8));
            for (var row = 0; row < 9; row++)
                if (rowClues[row] != null)
                    AddConstraint(wraparound
                        ? new SandwichWraparoundUniquenessConstraint(crust1, crust2, rowClues[row].Value, Enumerable.Range(0, 9).Select(col => row * 9 + col), minValue, minValue + 8)
                        : (Constraint) new SandwichUniquenessConstraint(crust1, crust2, rowClues[row].Value, Enumerable.Range(0, 9).Select(col => row * 9 + col), minValue, minValue + 8));
        }
    }
}

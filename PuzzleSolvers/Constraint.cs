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
        /// <summary>The group of cells affected by this constraint, or <c>null</c> if it affects all of them.</summary>
        public int[] AffectedCells { get; private set; }

        /// <summary>
        ///     Constructor for derived types.</summary>
        /// <param name="affectedCells">
        ///     The set of cells affected by this constraint.</param>
        protected Constraint(IEnumerable<int> affectedCells)
        {
            AffectedCells = affectedCells?.ToArray();
        }

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
                var m = Regex.Match(part, @"^\s*(?<col>[A-Z](\s*-\s*(?<colr>[A-Z]))?)\s*(?:(?<row>\d+)(\s*-\s*(?<rowr>\d+))?)\s*$");
                if (!m.Success)
                    throw new ArgumentException(string.Format(@"The region “{0}” is not in a valid format. Expected a column letter (or a range of columns, e.g. A-D) followed by a row digit (or a range of rows, e.g. 1-4).", part), nameof(str));
                var col = m.Groups["col"].Value[0] - 'A';
                var colr = m.Groups["colr"].Success ? m.Groups["colr"].Value[0] - 'A' : col;
                var row = int.Parse(m.Groups["row"].Value) - 1;
                var rowr = m.Groups["rowr"].Success ? int.Parse(m.Groups["rowr"].Value) - 1 : row;
                for (var c = col; c <= colr; c++)
                    for (var r = row; r <= rowr; r++)
                        yield return c + gridWidth * r;
            }
        }
    }
}

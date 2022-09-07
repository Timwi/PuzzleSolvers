using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PuzzleSolvers
{
    /// <summary>Abstract base class for all constraints in a puzzle.</summary>
    public abstract class Constraint
    {
        /// <summary>The group of cells affected by this constraint, or <c>null</c> if it affects all of them.</summary>
        public int[] AffectedCells { get; protected set; }

        /// <summary>
        ///     Constructor for derived types.</summary>
        /// <param name="affectedCells">
        ///     The set of cells affected by this constraint, or <c>null</c> if it affects all of them.</param>
        protected Constraint(IEnumerable<int> affectedCells)
        {
            AffectedCells = affectedCells?.ToArray();
        }

        /// <summary>
        ///     Constraint implementations must use <see cref="SolverState.MarkImpossible(int, int)"/> to mark values that are
        ///     known to be impossible given the incomplete grid exposed by <see cref="SolverState.this[int]"/>.</summary>
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
        public abstract ConstraintResult Process(SolverState state);

        /// <summary>
        ///     By default, a constraint is only evaluated once for every digit placed in the grid, but not when another
        ///     constraint merely rules out a possibility. Derived types can override this and return <c>true</c> to indicate
        ///     to the solver that the constraint should be reevaluated (meaning: have <see cref="Process"/> called on it
        ///     again) when another constraint rules out a value in one of the affected cells of this constraint.</summary>
        public virtual bool CanReevaluate => false;

        /// <summary>
        ///     Indicates an approximate number of possible combinations of digits this constraint can still accommodate. This
        ///     will help the solver prioritize cells when multiple cells have the same number of combinations individually.</summary>
        public virtual int? NumCombinations => null;

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

        /// <summary>
        ///     Converts a convenient coordinate notation into a puzzle-grid index.</summary>
        /// <param name="str">
        ///     A string such as <c>"E2"</c>. The letter is assumed to be a column (starting from A) while the digit is a row
        ///     (starting from 1).</param>
        /// <param name="gridWidth">
        ///     The width of the puzzle grid. For a standard Sudoku, this is 9.</param>
        /// <returns>
        ///     A cell index.</returns>
        public static int TranslateCoordinate(string str, int gridWidth = 9)
        {
            var m = Regex.Match(str, @"^\s*(?<col>[A-Z])\s*(?<row>\d+)\s*$");
            if (!m.Success)
                throw new ArgumentException(string.Format(@"The coordinate “{0}” is not in a valid format. Expected a column letter followed by a row digit.", str), nameof(str));
            return m.Groups["col"].Value[0] - 'A' + gridWidth * (int.Parse(m.Groups["row"].Value) - 1);
        }
    }
}

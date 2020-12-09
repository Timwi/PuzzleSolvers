using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement puzzle (such as a Thermometer Sudoku) where a series of cells must be
    ///     in ascending order.</summary>
    public class LessThanConstraint : Constraint
    {
        /// <summary>
        ///     Contains the region of cells affected by this constraint (the “thermometer”). The order of cells in this array
        ///     is significant as the constraint assumes these to be ordered from smallest to largest value.</summary>
        public new int[] AffectedCells => base.AffectedCells;

        /// <summary>Constructor.</summary>
        public LessThanConstraint(IEnumerable<int> affectedCells) : base(affectedCells) { }

        /// <summary>
        ///     Generates multiple <see cref="LessThanConstraint"/> from a semicolon-separated list of coordinate regions
        ///     represented in the format understood by <see cref="Constraint.TranslateCoordinates(string, int)"/>.</summary>
        /// <param name="representation">
        ///     The string representation of the set of regions.</param>
        /// <param name="gridWidth">
        ///     The width of the grid. If not specified, the default value is 9.</param>
        public static IEnumerable<LessThanConstraint> FromString(string representation, int gridWidth = 9)
        {
            if (representation == null)
                throw new ArgumentNullException(nameof(representation));
            foreach (var piece in representation.Split(';'))
                yield return new LessThanConstraint(TranslateCoordinates(piece, gridWidth));
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            // At the start, mark cells along the sequence. For example, the second cell can’t be 1, the third can’t be 1 or 2, etc.
            if (ix == null)
            {
                var min = minValue;
                var max = maxValue - AffectedCells.Length + 1;
                for (var q = 0; q < AffectedCells.Length; q++)
                {
                    for (var v = 0; v < takens[AffectedCells[q]].Length; v++)
                        if (v + minValue < min || v + minValue > max)
                            takens[AffectedCells[q]][v] = true;
                    min++;
                    max++;
                }
            }

            // Also make sure that all the values in the grid are considered.
            for (var p = 0; p < AffectedCells.Length; p++)
            {
                // Consider all placed values only if ix == null (performance optimization).
                if ((ix != null && AffectedCells[p] != ix.Value) || grid[AffectedCells[p]] == null)
                    continue;

                var val = grid[AffectedCells[p]].Value;
                for (var q = 0; q < AffectedCells.Length; q++)
                    for (var v = 0; v < takens[AffectedCells[q]].Length; v++)
                        if ((q < p && v > val - p + q) || (q > p && v < val - p + q))
                            takens[AffectedCells[q]][v] = true;
            }
            return null;
        }
    }
}

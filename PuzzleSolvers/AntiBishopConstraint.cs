using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number placement puzzle in which cells that are diagonal from each other (like a
    ///     bishop’s move in chess) cannot contain the same value.</summary>
    public sealed class AntiBishopConstraint : Constraint
    {
        /// <summary>Optionally specifies a limited set of values that are affected by the anti-bishop constraint.</summary>
        public int[] AffectedValues { get; private set; }
        /// <summary>The width of the grid.</summary>
        public int GridWidth { get; private set; }
        /// <summary>The height of the grid.</summary>
        public int GridHeight { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="gridWidth">
        ///     See <see cref="GridWidth"/>.</param>
        /// <param name="gridHeight">
        ///     See <see cref="GridHeight"/>.</param>
        /// <param name="affectedValues">
        ///     See <see cref="AffectedValues"/>.</param>
        /// <param name="color">
        ///     See <see cref="Constraint.CellColor"/>.</param>
        /// <param name="backgroundColor">
        ///     See <see cref="Constraint.CellBackgroundColor"/>.</param>
        public AntiBishopConstraint(int gridWidth, int gridHeight, int[] affectedValues = null, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
            : base(Enumerable.Range(0, gridWidth * gridHeight), color, backgroundColor)
        {
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            AffectedValues = affectedValues;
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            for (var cell = ix ?? 0; cell <= (ix ?? (grid.Length - 1)); cell++)
            {
                if (grid[cell] == null || (AffectedValues != null && !AffectedValues.Contains(grid[cell].Value + minValue)))
                    continue;
                var x = cell % GridWidth;
                var y = cell / GridWidth;

                for (var i = 0; i < grid.Length; i++)
                {
                    var xx = i % GridWidth;
                    var yy = i / GridWidth;
                    if (xx + yy == x + y || xx - yy == x - y)
                        takens[i][grid[cell].Value] = true;
                }
            }
            return null;
        }
    }
}

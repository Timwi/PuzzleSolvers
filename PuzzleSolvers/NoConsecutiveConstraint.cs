using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement grid puzzle where no adjacent cells (NOT including diagonals) can
    ///     have numerically consecutive values.</summary>
    public class NoConsecutiveConstraint : Constraint
    {
        /// <summary>
        ///     If not <c>null</c>, the constraint is limited to these values in the grid. This must include all affected
        ///     consecutive digits; for example, if this contains 2 and 3, then 1 and 2 can still be adjacent. If this
        ///     contains a single digit, the constraint is entirely ineffectual.</summary>
        public int[] AffectedValues { get; private set; }
        /// <summary>The width of the grid this constraint applies to.</summary>
        public int GridWidth { get; private set; }
        /// <summary>The height of the grid this constraint applies to.</summary>
        public int GridHeight { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="width">
        ///     The width of the grid.</param>
        /// <param name="height">
        ///     The height of the grid.</param>
        /// <param name="affectedValues">
        ///     If not <c>null</c>, the constraint is limited to these values in the grid.</param>
        public NoConsecutiveConstraint(int width, int height, int[] affectedValues = null)
            : base(Enumerable.Range(0, width * height))
        {
            AffectedValues = affectedValues;
            GridWidth = width;
            GridHeight = height;
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

                for (var dx = -1; dx <= 1; dx++)
                    if (x + dx >= 0 && x + dx < GridWidth)
                        for (var dy = dx == 0 ? -1 : 0; dy <= (dx == 0 ? 1 : 0); dy++)
                            if (y + dy >= 0 && y + dy < GridHeight)
                            {
                                var tIx = (x + dx) + GridWidth * (y + dy);
                                if (grid[cell].Value > 0 && (AffectedValues == null || AffectedValues.Contains(grid[cell].Value - 1 + minValue)))
                                    takens[tIx][grid[cell].Value - 1] = true;
                                if (grid[cell].Value < takens[tIx].Length - 1 && (AffectedValues == null || AffectedValues.Contains(grid[cell].Value + 1 + minValue)))
                                    takens[tIx][grid[cell].Value + 1] = true;
                            }
            }
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement grid puzzle where no adjacent cells (including diagonals) can have
    ///     the same value.</summary>
    public class AntiKingConstraint : Constraint
    {
        /// <summary>If not <c>null</c>, the constraint is limited to these values in the grid.</summary>
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
        public AntiKingConstraint(int width, int height, int[] affectedValues = null)
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
                        for (var dy = -1; dy <= 1; dy++)
                            if (y + dy >= 0 && y + dy < GridHeight)
                                takens[(x + dx) + GridWidth * (y + dy)][grid[cell].Value] = true;
            }
            return null;
        }
    }
}

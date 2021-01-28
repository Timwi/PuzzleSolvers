using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement grid puzzle where no adjacent cells can have numerically consecutive
    ///     values.</summary>
    public class NoConsecutiveConstraint : Constraint
    {
        /// <summary>
        ///     If not <c>null</c>, the constraint is limited to these values in the grid. This must include all affected
        ///     consecutive digits; for example, if this contains 2 and 3, then 1 and 2 can still be adjacent. If this
        ///     contains a single digit, the constraint is entirely ineffectual.</summary>
        public int[] AffectedValues { get; private set; }
        /// <summary>
        ///     Optionally specifies a limited set of cells that are affected by the no-consecutive constraint.</summary>
        /// <remarks>
        ///     Note this differs from <see cref="Constraint.AffectedCells"/> as that will contain all affected cells plus
        ///     those that are adjacent to them.</remarks>
        public new int[] AffectedCells { get; private set; }
        /// <summary>The width of the grid this constraint applies to.</summary>
        public int GridWidth { get; private set; }
        /// <summary>The height of the grid this constraint applies to.</summary>
        public int GridHeight { get; private set; }
        /// <summary>
        ///     If <c>true</c>, the constraint also applies to cells diagonally adjacent to one another. If <c>false</c>, only
        ///     orthogonally adjacent cells are affected.</summary>
        public bool IncludeDiagonals { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="gridWidth">
        ///     See <see cref="GridWidth"/>.</param>
        /// <param name="gridHeight">
        ///     See <see cref="GridHeight"/>.</param>
        /// <param name="includeDiagonals">
        ///     See <see cref="IncludeDiagonals"/>.</param>
        /// <param name="affectedValues">
        ///     See <see cref="AffectedValues"/>.</param>
        /// <param name="affectedCells">
        ///     See <see cref="AffectedCells"/>. If <c>null</c>, the default is to affect the entire grid.</param>
        public NoConsecutiveConstraint(int gridWidth, int gridHeight, bool includeDiagonals, int[] affectedValues = null, int[] affectedCells = null)
            : base(affectedCells?.SelectMany(cell => adjacentCells(cell, gridWidth, gridHeight, includeDiagonals)).Distinct())
        {
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            IncludeDiagonals = includeDiagonals;
            AffectedValues = affectedValues;
            AffectedCells = affectedCells?.ToArray();
        }

        private static IEnumerable<int> adjacentCells(int cell, int gridWidth, int gridHeight, bool includeDiagonals)
        {
            var x = cell % gridWidth;
            var y = cell / gridWidth;
            for (var yy = y - 1; yy <= y + 1; yy++)
                if (yy >= 0 && yy < gridHeight)
                    for (var xx = x - 1; xx <= x + 1; xx++)
                        if (xx >= 0 && xx < gridWidth && (xx != x || yy != y) && (includeDiagonals || xx == x || yy == y))
                            yield return xx + gridWidth * yy;
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            for (var cellIx = 0; cellIx < (ix != null ? 1 : base.AffectedCells != null ? base.AffectedCells.Length : grid.Length); cellIx++)
            {
                var cell = ix ?? (base.AffectedCells != null ? base.AffectedCells[cellIx] : cellIx);
                if (grid[cell] == null || (AffectedValues != null && !AffectedValues.Contains(grid[cell].Value + minValue)))
                    continue;

                var x = cell % GridWidth;
                var y = cell / GridWidth;

                for (var dx = -1; dx <= 1; dx++)
                    if (x + dx >= 0 && x + dx < GridWidth)
                        for (var dy = -1; dy <= 1; dy++)
                            if ((dx == 0 || dy == 0 || IncludeDiagonals) && (dx != 0 || dy != 0) && y + dy >= 0 && y + dy < GridHeight)
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

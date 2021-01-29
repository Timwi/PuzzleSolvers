﻿using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number placement puzzle in which cells that are at specific relative placements from
    ///     one another cannot contain the same value.</summary>
    public abstract class AntiChessConstraint : Constraint
    {
        /// <summary>Optionally specifies a limited set of values that are affected by the constraint.</summary>
        public int[] AffectedValues { get; private set; }
        /// <summary>
        ///     Optionally specifies a limited set of cells on which the constraint is enforced.</summary>
        /// <remarks>
        ///     Note this differs from <see cref="Constraint.AffectedCells"/> as that will contain all affected cells plus
        ///     those that are compared against.</remarks>
        public int[] EnforcedCells { get; private set; }
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
        /// <param name="enforcedCells">
        ///     See <see cref="EnforcedCells"/>. If <c>null</c>, the default is to enforce the entire grid.</param>
        public AntiChessConstraint(int gridWidth, int gridHeight, int[] affectedValues = null, IEnumerable<int> enforcedCells = null)
            : base(null)
        {
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            AffectedValues = affectedValues;
            EnforcedCells = enforcedCells?.ToArray();
            AffectedCells = enforcedCells?.SelectMany(getRelatedCells).Distinct().ToArray();
        }

        /// <summary>Specifies what cells a specific cell needs to be compared against for the constraint.</summary>
        protected abstract IEnumerable<int> getRelatedCells(int cell);

        /// <summary>Override; see base.</summary>
        public override sealed IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            for (var cellIx = 0; cellIx < (ix != null ? 1 : AffectedCells != null ? AffectedCells.Length : grid.Length); cellIx++)
            {
                var cell = ix ?? (AffectedCells != null ? AffectedCells[cellIx] : cellIx);
                if (grid[cell] == null || (AffectedValues != null && !AffectedValues.Contains(grid[cell].Value + minValue)))
                    continue;
                foreach (var relatedCell in getRelatedCells(cell))
                    if (EnforcedCells == null || EnforcedCells.Contains(relatedCell) || EnforcedCells.Contains(cell))
                        takens[relatedCell][grid[cell].Value] = true;
            }
            return null;
        }
    }
}

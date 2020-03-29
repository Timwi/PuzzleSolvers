﻿using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement grid puzzle where no adjacent cells (including diagonals) can have
    ///     the same value.</summary>
    public class NoTouchConstraint : Constraint
    {
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
        public NoTouchConstraint(int width, int height) : base(Enumerable.Range(0, width * height)) { GridWidth = width; GridHeight = height; }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            for (var cell = ix ?? 0; cell <= (ix ?? (grid.Length - 1)); cell++)
            {
                if (grid[cell] == null)
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
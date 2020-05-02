using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number placement puzzle in which cells that are a knight’s move away from each other
    ///     cannot contain the same value.</summary>
    public sealed class AntiKnightConstraint : Constraint
    {
        /// <summary>Optionally specifies a limited set of values that are affected by the anti-knight constraint.</summary>
        public int[] AffectedValues { get; private set; }
        /// <summary>The width of the grid.</summary>
        public int GridWidth { get; private set; }
        /// <summary>The height of the grid.</summary>
        public int GridHeight { get; private set; }
        /// <summary>
        ///     If <c>true</c>, the constraint considers the grid to be toroidal, meaning that it wraps around the left/right
        ///     and top/bottom edges. Thus, in a Sudoku-sized grid, A1 would be a knight’s move away from B8 and H2. If
        ///     <c>false</c>, the knight’s move cannot extend beyond the bounds of the grid.</summary>
        public bool Toroidal { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="gridWidth">
        ///     See <see cref="GridWidth"/>.</param>
        /// <param name="gridHeight">
        ///     See <see cref="GridHeight"/>.</param>
        /// <param name="affectedValues">
        ///     See <see cref="AffectedValues"/>.</param>
        /// <param name="toroidal">
        ///     See <see cref="Toroidal"/>.</param>
        /// <param name="color">
        ///     See <see cref="Constraint.CellColor"/>.</param>
        /// <param name="backgroundColor">
        ///     See <see cref="Constraint.CellBackgroundColor"/>.</param>
        public AntiKnightConstraint(int gridWidth, int gridHeight, int[] affectedValues = null, bool toroidal = false, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
            : base(Enumerable.Range(0, gridWidth * gridHeight), color, backgroundColor)
        {
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            AffectedValues = affectedValues;
            Toroidal = toroidal;
        }

        private static readonly int[] _dxs = new[] { -2, -1, 1, 2 };
        private static readonly int[] _dys1 = new[] { -2, 2 };
        private static readonly int[] _dys2 = new[] { -1, 1 };

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            for (var cell = ix ?? 0; cell <= (ix ?? (grid.Length - 1)); cell++)
            {
                if (grid[cell] == null || (AffectedValues != null && !AffectedValues.Contains(grid[cell].Value + minValue)))
                    return null;
                var x = cell % GridWidth;
                var y = cell / GridWidth;
                foreach (var dx in _dxs)
                    if (Toroidal || (x + dx >= 0 && x + dx < GridWidth))
                        foreach (var dy in (dx == 1 || dx == -1) ? _dys1 : _dys2)
                            if (Toroidal || (y + dy >= 0 && y + dy < GridHeight))
                            {
                                var ix2 = (x + dx + GridWidth) % GridWidth + GridWidth * ((y + dy + GridHeight) % GridHeight);
                                if (AffectedCells.Contains(ix2))
                                    takens[ix2][grid[cell].Value] = true;
                            }
            }
            return null;
        }
    }
}

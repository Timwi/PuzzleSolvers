using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number placement puzzle in which cells that are a knight’s move away from each other
    ///     cannot contain the same value.</summary>
    public sealed class AntiKnightConstraint : Constraint
    {
        /// <summary>Optionally specifies a limited set of values that are affected by the anti-knight constraint.</summary>
        public int[] AffectedValues { get; private set; }
        /// <summary>
        ///     Optionally specifies a limited set of cells that are affected by the anti-knight constraint.</summary>
        /// <remarks>
        ///     Note this differs from <see cref="Constraint.AffectedCells"/> as that will contain all affected cells plus
        ///     those that are a knight’s move away.</remarks>
        public new int[] AffectedCells { get; private set; }
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
        /// <param name="affectedCells">
        ///     See <see cref="AffectedCells"/>. If <c>null</c>, the default is to affect the entire grid.</param>
        /// <param name="toroidal">
        ///     See <see cref="Toroidal"/>.</param>
        public AntiKnightConstraint(int gridWidth, int gridHeight, int[] affectedValues = null, IEnumerable<int> affectedCells = null, bool toroidal = false)
            : base(affectedCells?.SelectMany(cell => knightsMoves(cell, gridWidth, gridHeight, toroidal).Concat(cell)).Distinct())
        {
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            AffectedValues = affectedValues;
            AffectedCells = affectedCells?.ToArray();
            Toroidal = toroidal;
        }

        private static readonly int[] _dxs = new[] { -2, -1, 1, 2 };
        private static readonly int[] _dys1 = new[] { -2, 2 };
        private static readonly int[] _dys2 = new[] { -1, 1 };

        private static IEnumerable<int> knightsMoves(int cell, int gridWidth, int gridHeight, bool toroidal)
        {
            var x = cell % gridWidth;
            var y = cell / gridWidth;
            foreach (var dx in _dxs)
                if (toroidal || (x + dx >= 0 && x + dx < gridWidth))
                    foreach (var dy in (dx == 1 || dx == -1) ? _dys1 : _dys2)
                        if (toroidal || (y + dy >= 0 && y + dy < gridHeight))
                            yield return (x + dx + gridWidth) % gridWidth + gridWidth * ((y + dy + gridHeight) % gridHeight);
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            for (var cellIx = 0; cellIx < (ix != null ? 1 : base.AffectedCells != null ? base.AffectedCells.Length : grid.Length); cellIx++)
            {
                var cell = ix ?? (base.AffectedCells != null ? base.AffectedCells[cellIx] : cellIx);
                if (grid[cell] == null || (AffectedValues != null && !AffectedValues.Contains(grid[cell].Value + minValue)))
                    continue;
                foreach (var knightsCell in knightsMoves(cell, GridWidth, GridHeight, Toroidal))
                    if (AffectedCells == null || AffectedCells.Contains(knightsCell) || AffectedCells.Contains(cell))
                        takens[knightsCell][grid[cell].Value] = true;
            }
            return null;
        }
    }
}

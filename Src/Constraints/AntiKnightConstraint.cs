using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number placement puzzle in which cells that are a knight’s move away from each other
    ///     cannot contain the same value.</summary>
    public class AntiKnightConstraint : AntiChessConstraint
    {
        /// <summary>
        ///     If <c>true</c>, the constraint considers the grid to be toroidal, meaning that it wraps around the left/right
        ///     and top/bottom edges. Thus, in a Sudoku-sized grid, A1 would be a knight’s move away from B8 and H2. If
        ///     <c>false</c>, the knight’s move cannot extend beyond the bounds of the grid.</summary>
        public bool Toroidal { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="gridWidth">
        ///     See <see cref="AntiChessConstraint.GridWidth"/>.</param>
        /// <param name="gridHeight">
        ///     See <see cref="AntiChessConstraint.GridHeight"/>.</param>
        /// <param name="affectedValues">
        ///     See <see cref="AntiChessConstraint.AffectedValues"/>.</param>
        /// <param name="enforcedCells">
        ///     See <see cref="AntiChessConstraint.EnforcedCells"/>. If <c>null</c>, the default is to enforce the entire
        ///     grid.</param>
        /// <param name="toroidal">
        ///     See <see cref="Toroidal"/>.</param>
        public AntiKnightConstraint(int gridWidth, int gridHeight, int[] affectedValues = null, IEnumerable<int> enforcedCells = null, bool toroidal = false)
            : base(gridWidth, gridHeight, affectedValues, enforcedCells)
        {
            Toroidal = toroidal;
        }

        private static readonly int[] _dxs = new[] { -2, -1, 1, 2 };
        private static readonly int[] _dys1 = new[] { -2, 2 };
        private static readonly int[] _dys2 = new[] { -1, 1 };

        /// <summary>
        ///     Returns the set of cells that are a knight’s move away from the specified cell.</summary>
        /// <param name="cell">
        ///     The cell from which to generate knight’s moves.</param>
        /// <param name="gridWidth">
        ///     Width of the grid.</param>
        /// <param name="gridHeight">
        ///     Height of the grid.</param>
        /// <param name="toroidal">
        ///     If <c>true</c>, the grid is considered to be toroidal, i.e., the knight’s move can leave the grid and reenter
        ///     on the opposite edge.</param>
        public static IEnumerable<int> KnightsMoves(int cell, int gridWidth, int gridHeight, bool toroidal)
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
        protected override IEnumerable<int> getRelatedCells(int cell) => KnightsMoves(cell, GridWidth, GridHeight, Toroidal);
    }
}

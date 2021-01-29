using System.Collections.Generic;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number placement puzzle in which cells that are diagonal from each other (like a
    ///     bishop’s move in chess) cannot contain the same value.</summary>
    public sealed class AntiBishopConstraint : AntiChessConstraint
    {
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
        public AntiBishopConstraint(int gridWidth, int gridHeight, int[] affectedValues = null, IEnumerable<int> enforcedCells = null)
            : base(gridWidth, gridHeight, affectedValues, enforcedCells) { }

        /// <summary>Returns all cells reachable from the specified cell by a bishop’s move in chess.</summary>
        public static IEnumerable<int> BishopsMoves(int cell, int gridWidth, int gridHeight)
        {
            var x = cell % gridWidth;
            var y = cell / gridWidth;
            for (var i = 0; i < gridWidth * gridHeight; i++)
            {
                if (i == cell)
                    continue;
                var xx = i % gridWidth;
                var yy = i / gridWidth;
                if (xx + yy == x + y || xx - yy == x - y)
                    yield return i;
            }
        }

        /// <summary>Override; see base.</summary>
        protected override IEnumerable<int> getRelatedCells(int cell) => BishopsMoves(cell, GridWidth, GridHeight);
    }
}

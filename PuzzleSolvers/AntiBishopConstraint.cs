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
        /// <summary>
        ///     Optionally specifies a limited set of cells that are affected by the anti-bishop constraint.</summary>
        /// <remarks>
        ///     Note this differs from <see cref="Constraint.AffectedCells"/> as that will contain all affected cells plus
        ///     those that are a bishop’s move away.</remarks>
        public new int[] AffectedCells { get; private set; }
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
        /// <param name="affectedCells">
        ///     See <see cref="AffectedCells"/>. If <c>null</c>, the default is to affect the entire grid.</param>
        public AntiBishopConstraint(int gridWidth, int gridHeight, int[] affectedValues = null, IEnumerable<int> affectedCells = null)
            : base(affectedCells?.SelectMany(cell => bishopsMoves(cell, gridWidth, gridHeight)).Distinct())
        {
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            AffectedValues = affectedValues;
            AffectedCells = affectedCells?.ToArray();
        }

        private static IEnumerable<int> bishopsMoves(int cell, int gridWidth, int gridHeight)
        {
            var x = cell % gridWidth;
            var y = cell / gridWidth;
            for (var i = 0; i < gridWidth * gridHeight; i++)
            {
                var xx = i % gridWidth;
                var yy = i / gridWidth;
                if (xx + yy == x + y || xx - yy == x - y)
                    yield return i;
            }
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            for (var cellIx = 0; cellIx < (ix != null ? 1 : base.AffectedCells != null ? base.AffectedCells.Length : grid.Length); cellIx++)
            {
                var cell = ix ?? (base.AffectedCells != null ? base.AffectedCells[cellIx] : cellIx);
                if (grid[cell] == null || (AffectedValues != null && !AffectedValues.Contains(grid[cell].Value + minValue)))
                    continue;
                foreach (var bishopsCell in bishopsMoves(cell, GridWidth, GridHeight))
                    if (AffectedCells == null || AffectedCells.Contains(bishopsCell) || AffectedCells.Contains(cell))
                        takens[bishopsCell][grid[cell].Value] = true;
            }
            return null;
        }
    }
}

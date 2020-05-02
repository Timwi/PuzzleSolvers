using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    public sealed class AntiBishopConstraint : Constraint
    {
        public int[] AffectedValues { get; private set; }
        public int GridWidth { get; private set; }
        public int GridHeight { get; private set; }

        public AntiBishopConstraint(int gridWidth, int gridHeight, int[] affectedValues = null, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
            : base(Enumerable.Range(0, gridWidth * gridHeight), color, backgroundColor)
        {
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            AffectedValues = affectedValues;
        }

        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            for (var cell = ix ?? 0; cell <= (ix ?? (grid.Length - 1)); cell++)
            {
                if (grid[cell] == null || (AffectedValues != null && !AffectedValues.Contains(grid[cell].Value + minValue)))
                    continue;
                var x = cell % GridWidth;
                var y = cell / GridWidth;

                for (var i = 0; i < grid.Length; i++)
                {
                    var xx = i % GridWidth;
                    var yy = i / GridWidth;
                    if (xx + yy == x + y || xx - yy == x - y)
                        takens[i][grid[cell].Value] = true;
                }
            }
            return null;
        }
    }
}
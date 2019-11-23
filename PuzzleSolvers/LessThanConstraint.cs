using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuzzleSolvers
{
    public class LessThanConstraint : Constraint
    {
        // Every cell is assumed to be greater than the previous; in other words, the values in these cells must be in ascending order
        public int[] AffectedCells;
        public ConsoleColor? BackgroundColor;

        public override void MarkInitialTakens(bool[][] takens, int minValue, int maxValue)
        {
            var min = minValue;
            var max = maxValue - AffectedCells.Length + 1;
            for (var i = 0; i < AffectedCells.Length; i++)
            {
                var t = takens[AffectedCells[i]];
                for (var val = 0; val < t.Length; val++)
                    if (val + minValue < min || val + minValue > max)
                        t[val] = true;
                min++;
                max++;
            }
        }

        public override IEnumerable<Constraint> MarkTaken(bool[][] takens, int?[] grid, int ix, int val, int minValue, int maxValue)
        {
            var p = Array.IndexOf(AffectedCells, ix);
            if (p == -1)
                return null;

            for (var i = 0; i < AffectedCells.Length; i++)
            {
                if (i < p)
                {
                    var vMax = takens[AffectedCells[i]].Length;
                    for (var v = Math.Max(0, val - p + i + 1); v < vMax; v++)
                        takens[AffectedCells[i]][v] = true;
                }
                else if (i > p)
                {
                    var vMax = Math.Min(takens[AffectedCells[i]].Length, val + i - p);
                    for (var v = 0; v < vMax; v++)
                        takens[AffectedCells[i]][v] = true;
                }
            }
            return null;
        }

        public override ConsoleColor? CellBackgroundColor(int ix) => AffectedCells.Contains(ix) ? BackgroundColor : null;
    }
}

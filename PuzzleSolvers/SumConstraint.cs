using System;
using System.Collections.Generic;
using System.Text;

namespace PuzzleSolvers
{
    public class SumConstraint : Constraint
    {
        public int[] AffectedCells;
        public int Sum;

        public override void MarkInitialTakens(bool[][] takens, int minValue, int maxValue)
        {
            var minValuePerCell = Sum - maxValue * (AffectedCells.Length - 1);
            var maxValuePerCell = Sum - minValue * (AffectedCells.Length - 1);
            foreach (var cell in AffectedCells)
                for (var v = 0; v < takens[cell].Length; v++)
                    if (v + minValue < minValuePerCell || v + minValue > maxValuePerCell)
                        takens[cell][v] = true;
        }

        public override IEnumerable<Constraint> MarkTaken(bool[][] takens, int?[] grid, int ix, int val, int minValue, int maxValue)
        {
            var sumAlready = 0;
            var stillNeed = AffectedCells.Length;
            foreach (var cell in AffectedCells)
                if (grid[cell] != null)
                {
                    sumAlready += grid[cell].Value + minValue;
                    stillNeed--;
                }

            var minValuePerCell = Sum - sumAlready - maxValue * (stillNeed - 1);
            var maxValuePerCell = Sum - sumAlready - minValue * (stillNeed - 1);
            foreach (var cell in AffectedCells)
                if (grid[cell] == null)
                    for (var v = 0; v < takens[cell].Length; v++)
                        if (v + minValue < minValuePerCell || v + minValue > maxValuePerCell)
                            takens[cell][v] = true;

            return null;
        }
    }
}

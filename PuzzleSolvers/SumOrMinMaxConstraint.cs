using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    public class SumOrMinMaxConstraint : Constraint
    {
        public SumOrMinMaxConstraint()
        {
            throw new NotImplementedException(@"The implementation for this constraint has an unknown bug.");
        }

        public int SumOrMinMax;
        public bool IsMin;
        public int[] AffectedCells;

        public override void MarkInitialTakens(bool[][] takens, int minValue, int maxValue)
        {
            var minValuePerCell = SumOrMinMax - maxValue * (AffectedCells.Length - 1);
            if (IsMin)
                minValuePerCell = Math.Min(minValuePerCell, SumOrMinMax);

            var maxValuePerCell = SumOrMinMax - minValue * (AffectedCells.Length - 1);
            if (!IsMin)
                maxValuePerCell = Math.Max(maxValuePerCell, SumOrMinMax);

            foreach (var cell in AffectedCells)
                for (var v = 0; v < takens[cell].Length; v++)
                    if (v + minValue < minValuePerCell || v + minValue > maxValuePerCell)
                        takens[cell][v] = true;
        }

        public override IEnumerable<Constraint> MarkTaken(bool[][] takens, int?[] grid, int ix, int val, int minValue, int maxValue)
        {
            var sumAlready = 0;
            var minAlready = int.MaxValue;
            var maxAlready = 0;
            var stillNeed = AffectedCells.Length;
            foreach (var cell in AffectedCells)
                if (grid[cell] != null)
                {
                    sumAlready += grid[cell].Value + minValue;
                    stillNeed--;
                    minAlready = Math.Min(minAlready, grid[cell].Value);
                    maxAlready = Math.Max(maxAlready, grid[cell].Value);
                }

            if (stillNeed > 1)
                return null;

            // If min/max is already violated, make sure that the sum is correct
            var minMaxViolated = IsMin ? minAlready < SumOrMinMax : maxAlready > SumOrMinMax;
            var minMaxExact = (IsMin ? minAlready : maxAlready) == SumOrMinMax;

            var possibleValuesForLastCell = new List<int>();
            possibleValuesForLastCell.Add(SumOrMinMax - sumAlready);
            if (minMaxExact)
                possibleValuesForLastCell.AddRange(IsMin ? Enumerable.Range(minValue, SumOrMinMax - minValue) : Enumerable.Range(SumOrMinMax, maxValue - SumOrMinMax));
            else if (!minMaxViolated)
                possibleValuesForLastCell.Add(SumOrMinMax);

            foreach (var cell in AffectedCells)
                if (grid[cell] == null)
                    for (var v = 0; v < takens[cell].Length; v++)
                        if (!possibleValuesForLastCell.Contains(v + minValue))
                            takens[cell][v] = true;

            return null;
        }
    }
}

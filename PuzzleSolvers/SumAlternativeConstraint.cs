using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    public class SumAlternativeConstraint : Constraint
    {
        public int[][] AffectedCellGroups;
        public int Sum;

        private int[] _distinctAffectedCells;

        public override void MarkInitialTakens(bool[][] takens, int minValue, int maxValue)
        {
            _distinctAffectedCells = AffectedCellGroups.SelectMany(x => x).Distinct().ToArray();

            // Mark values as taken that are impossible for ALL of the groups of cells
            foreach (var cell in _distinctAffectedCells)
                for (var v = 0; v < takens[cell].Length; v++)
                    if (Enumerable.Range(0, AffectedCellGroups.Length).All(grIx =>
                        // If any of the groups don’t affect this cell, we cannot assume that any value is impossible
                        AffectedCellGroups[grIx].Contains(cell) && (
                            // Mark values that are too small (e.g. if you need a sum of 17 with two cells, and maxValue is 9, you need at least an 8)
                            v + minValue < (Sum - maxValue * (AffectedCellGroups[grIx].Length - 1)) ||
                            // Mark values that are too large (e.g. if you need a sum of 3 with two cells, and minValue is 1, you can’t have more than 2)
                            v + minValue > (Sum - minValue * (AffectedCellGroups[grIx].Length - 1)))))
                        takens[cell][v] = true;
        }

        public override IEnumerable<Constraint> MarkTaken(bool[][] takens, int?[] grid, int ix, int val, int minValue, int maxValue)
        {
            var sumsAlready = new int[AffectedCellGroups.Length];
            var stillNeed = AffectedCellGroups.Select(gr => gr.Length).ToArray();
            foreach (var cell in _distinctAffectedCells)
                if (grid[cell] != null)
                    for (var grIx = 0; grIx < AffectedCellGroups.Length; grIx++)
                        if (AffectedCellGroups[grIx].Contains(cell))
                        {
                            sumsAlready[grIx] += grid[cell].Value + minValue;
                            stillNeed[grIx]--;

                            // If any of the sums is already correct, this constraint is already satisfied.
                            if (stillNeed[grIx] == 0 && sumsAlready[grIx] == Sum)
                                return null;
                        }

            // We need to mark values as taken that are impossible for ALL of the affected groups of cells,
            // EXCEPT for groups in which all the cells have already been filled because their sum is wrong
            // (if it were correct, the check further up would have already returned).

            foreach (var cell in _distinctAffectedCells)
                if (grid[cell] == null)
                    for (var v = 0; v < takens[cell].Length; v++)
                        if (!takens[cell][v] && Enumerable.Range(0, AffectedCellGroups.Length).All(grIx =>
                            // Groups in which all cells have already been filled no longer contribute to this constraint
                            stillNeed[grIx] == 0 ||
                            // The rest of this condition is the same as in MarkInitialTakens()
                            AffectedCellGroups[grIx].Contains(cell) && (
                                v + minValue < (Sum - sumsAlready[grIx] - maxValue * (stillNeed[grIx] - 1)) ||
                                v + minValue > (Sum - sumsAlready[grIx] - minValue * (stillNeed[grIx] - 1)))))
                            takens[cell][v] = true;

            return null;
        }
    }
}

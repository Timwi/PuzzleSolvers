using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement puzzle where either one of several regions must have a specified sum.</summary>
    public class SumAlternativeConstraint : Constraint
    {
        /// <summary>The desired sum.</summary>
        public int Sum { get; private set; }

        /// <summary>
        ///     Contains the regions affected by the constraint. At least one of these regions must have the sum specified by
        ///     <see cref="Sum"/>. Each region is an array of cell indices.</summary>
        public int[][] Regions { get; private set; }

        private readonly int[] _distinctAffectedCells;

        /// <summary>Constructor.</summary>
        public SumAlternativeConstraint(int sum, params IEnumerable<int>[] regions)
        {
            Sum = sum;
            Regions = regions.Select(r => r.ToArray()).ToArray();
            _distinctAffectedCells = Regions.SelectMany(x => x).Distinct().ToArray();
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix != null && !Regions.Any(region => region.Contains(ix.Value)))
                return null;

            var sumsAlready = new int[Regions.Length];
            var stillNeed = Regions.Select(gr => gr.Length).ToArray();
            foreach (var cell in _distinctAffectedCells)
                if (grid[cell] != null)
                    for (var grIx = 0; grIx < Regions.Length; grIx++)
                        if (Regions[grIx].Contains(cell))
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
                        if (!takens[cell][v] && Enumerable.Range(0, Regions.Length).All(grIx =>
                            // Groups in which all cells have already been filled no longer contribute to this constraint
                            stillNeed[grIx] == 0 ||
                            // If ANY of the groups don’t affect this cell, we cannot assume that any value is impossible
                            Regions[grIx].Contains(cell) && (
                                // Mark values that are too small (e.g. if you need a sum of 17 with two cells, and maxValue is 9, you need at least an 8)
                                v + minValue < (Sum - sumsAlready[grIx] - maxValue * (stillNeed[grIx] - 1)) ||
                                // Mark values that are too large (e.g. if you need a sum of 3 with two cells, and minValue is 1, you can’t have more than 2)
                                v + minValue > (Sum - sumsAlready[grIx] - minValue * (stillNeed[grIx] - 1)))))
                            takens[cell][v] = true;

            return null;
        }
    }
}

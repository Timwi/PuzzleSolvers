using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in which a specific region of cells must sum up to the value in another cell. In a variant
    ///     Sudoku, this is often represented as an arrow coming out of a circle, and hence, the variant is known as “Arrow
    ///     Sudoku”.</summary>
    public class IndirectSumConstraint : Constraint
    {
        /// <summary>The cell that must contain the desired sum.</summary>
        public int SumCell { get; private set; }

        /// <summary>The set of cells that must sum up to the value in <see cref="SumCell"/>.</summary>
        public int[] Region { get; private set; }

        /// <summary>Constructor.</summary>
        public IndirectSumConstraint(int sumCell, IEnumerable<int> region) : base(region.Concat(new[] { sumCell }))
        {
            SumCell = sumCell;
            Region = region?.ToArray();
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix == null)
            {
                for (var v = 0; v < takens[SumCell].Length; v++)
                    if (v + minValue < minValue * Region.Length || v + minValue > maxValue * Region.Length)
                        takens[SumCell][v] = true;
            }
            else
            {
                if (grid[SumCell] == null)
                {
                    var minValuePerCell = Region.Sum(c => grid[c] == null ? minValue : grid[c].Value + minValue);
                    var maxValuePerCell = Region.Sum(c => grid[c] == null ? maxValue : grid[c].Value + minValue);
                    for (var v = 0; v < takens[SumCell].Length; v++)
                        if (v + minValue < minValuePerCell || v + minValue > maxValuePerCell)
                            takens[SumCell][v] = true;
                }
                else
                {
                    var sumAlready = 0;
                    var stillNeed = Region.Length;
                    foreach (var cell in Region)
                        if (grid[cell] != null)
                        {
                            sumAlready += grid[cell].Value + minValue;
                            stillNeed--;
                        }

                    var minValuePerCell = grid[SumCell].Value + minValue - sumAlready - maxValue * (stillNeed - 1);
                    var maxValuePerCell = grid[SumCell].Value + minValue - sumAlready - minValue * (stillNeed - 1);
                    foreach (var cell in Region)
                        if (grid[cell] == null)
                            for (var v = 0; v < takens[cell].Length; v++)
                                if (v + minValue < minValuePerCell || v + minValue > maxValuePerCell)
                                    takens[cell][v] = true;
                }
            }
            return null;
        }
    }
}

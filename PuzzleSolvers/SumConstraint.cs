using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in which a specific region of cells must sum up to a specified value.</summary>
    /// <remarks>
    ///     Technically this constraint is equivalent to a <see cref="SumAlternativeConstraint"/> with a single region.</remarks>
    public class SumConstraint : Constraint
    {
        /// <summary>The region of cells that must have the sum specified by <see cref="Sum"/>.</summary>
        public new int[] AffectedCells => base.AffectedCells;
        /// <summary>The desired sum.</summary>
        public int Sum { get; private set; }

        /// <summary>Constructor.</summary>
        public SumConstraint(int sum, IEnumerable<int> affectedCells) : base(affectedCells) { Sum = sum; }

        /// <summary>Override; see base;</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix != null && !AffectedCells.Contains(ix.Value))
                return null;

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

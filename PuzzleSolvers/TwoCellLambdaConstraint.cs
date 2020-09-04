using System;
using System.Collections.Generic;

namespace PuzzleSolvers
{
    /// <summary>Can be used to describe any constraint that applies to the whole puzzle using a lambda expression.</summary>
    public sealed class TwoCellLambdaConstraint : Constraint
    {
        /// <summary>A function that determines whether a pair of values is valid in the relevant cells.</summary>
        public Func<int, int, bool> IsValid { get; private set; }

        /// <summary>Constructor.</summary>
        public TwoCellLambdaConstraint(int affectedCell1, int affectedCell2, Func<int, int, bool> isValid)
            : base(new[] { affectedCell1, affectedCell2 })
        {
            IsValid = isValid;
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix == AffectedCells[0])
                for (var v = 0; v < takens[AffectedCells[1]].Length; v++)
                    if (!IsValid(grid[ix.Value].Value + minValue, v + minValue))
                        takens[AffectedCells[1]][v] = true;
            if (ix == AffectedCells[1])
                for (var v = 0; v < takens[AffectedCells[0]].Length; v++)
                    if (!IsValid(v + minValue, grid[ix.Value].Value + minValue))
                        takens[AffectedCells[0]][v] = true;
            return null;
        }
    }
}

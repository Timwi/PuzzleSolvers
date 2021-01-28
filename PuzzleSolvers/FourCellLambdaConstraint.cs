using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Constrains four cells to values that satisfy a lambda expression.</summary>
    /// <remarks>
    ///     This constraint is not very efficient as it will only be evaluated once all but one of the cells is filled in.</remarks>
    public sealed class FourCellLambdaConstraint : Constraint
    {
        /// <summary>A function that determines whether a set of values is valid in the relevant cells.</summary>
        public Func<int, int, int, int, bool> IsValid { get; private set; }

        /// <summary>Constructor.</summary>
        public FourCellLambdaConstraint(int affectedCell1, int affectedCell2, int affectedCell3, int affectedCell4, Func<int, int, int, int, bool> isValid)
            : base(new[] { affectedCell1, affectedCell2, affectedCell3, affectedCell4 })
        {
            IsValid = isValid;
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix == null)
                return null;

            var unknowns = AffectedCells.Count(af => grid[af] == null);
            if (unknowns != 1)
                return null;
            var unknown = AffectedCells.First(af => grid[af] == null);

            for (var v = 0; v < takens[unknown].Length; v++)
                if (!IsValid(
                    (grid[AffectedCells[0]] ?? v) + minValue,
                    (grid[AffectedCells[1]] ?? v) + minValue,
                    (grid[AffectedCells[2]] ?? v) + minValue,
                    (grid[AffectedCells[3]] ?? v) + minValue
                ))
                    takens[unknown][v] = true;
            return null;
        }
    }
}

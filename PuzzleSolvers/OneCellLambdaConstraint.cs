using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>Constrains a single cell to values that satisfy a lambda expression.</summary>
    public sealed class OneCellLambdaConstraint : Constraint
    {
        /// <summary>A function that determines whether a value is valid in the relevant cell.</summary>
        public Func<int, bool> IsValid { get; private set; }

        /// <summary>Constructor.</summary>
        public OneCellLambdaConstraint(int affectedCell, Func<int, bool> isValid) : base(new[] { affectedCell })
        {
            IsValid = isValid;
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix == null)
            {
                for (var v = 0; v < takens[AffectedCells[0]].Length; v++)
                    if (!IsValid(v + minValue))
                        takens[AffectedCells[0]][v] = true;
            }
            return Enumerable.Empty<Constraint>();
        }
    }
}

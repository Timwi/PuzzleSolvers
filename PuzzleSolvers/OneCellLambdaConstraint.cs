using System;
using System.Collections.Generic;

namespace PuzzleSolvers
{
    /// <summary>Can be used to describe any constraint that applies to the whole puzzle using a lambda expression.</summary>
    public sealed class OneCellLambdaConstraint : Constraint
    {
        /// <summary>A function that determines whether a value is valid in the relevant cell.</summary>
        public Func<int, bool> IsValid { get; private set; }

        /// <summary>Constructor.</summary>
        public OneCellLambdaConstraint(int affectedCell, Func<int, bool> isValid, ConsoleColor? color = null, ConsoleColor? backgroundColor = null) : base(new[] { affectedCell }, color, backgroundColor)
        {
            IsValid = isValid;
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix == null)
                for (var v = 0; v < takens[AffectedCells[0]].Length; v++)
                    if (!IsValid(v + minValue))
                        takens[AffectedCells[0]][v] = true;
            return null;
        }
    }
}

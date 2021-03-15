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
        public override IEnumerable<Constraint> MarkTakens(SolverState state)
        {
            if (state.LastPlaced == null)
                state.MarkImpossible(AffectedCells[0], value => !IsValid(value));
            return Enumerable.Empty<Constraint>();
        }
    }
}

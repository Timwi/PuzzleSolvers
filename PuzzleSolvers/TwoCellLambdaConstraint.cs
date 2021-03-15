using System;
using System.Collections.Generic;

namespace PuzzleSolvers
{
    /// <summary>Constrains two cells to values that satisfy a lambda expression.</summary>
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
        public override IEnumerable<Constraint> MarkTakens(SolverState state)
        {
            if (state.LastPlaced == AffectedCells[0])
                state.MarkImpossible(AffectedCells[1], value => !IsValid(state.LastPlacedValue, value));
            else if (state.LastPlaced == AffectedCells[1])
                state.MarkImpossible(AffectedCells[0], value => !IsValid(state.LastPlacedValue, value));
            return null;
        }
    }
}

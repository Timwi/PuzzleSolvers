using System;

namespace PuzzleSolvers
{
    /// <summary>Constrains two cells to values that satisfy a lambda expression.</summary>
    public class TwoCellLambdaConstraint(int affectedCell1, int affectedCell2, Func<int, int, bool> isValid) : Constraint([affectedCell1, affectedCell2])
    {
        /// <summary>A function that determines whether a pair of values is valid in the relevant cells.</summary>
        public Func<int, int, bool> IsValid { get; private set; } = isValid;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            if (state.LastPlacedCell == AffectedCells[0])
                state.MarkImpossible(AffectedCells[1], value => !IsValid(state.LastPlacedValue, value));
            else if (state.LastPlacedCell == AffectedCells[1])
                state.MarkImpossible(AffectedCells[0], value => !IsValid(value, state.LastPlacedValue));
            return null;
        }
    }
}

using System;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Constrains six cells to values that satisfy a lambda expression.</summary>
    /// <remarks>
    ///     This constraint is not very efficient as it will only be evaluated once all but one of the cells is filled in.</remarks>
    public class SixCellLambdaConstraint(int affectedCell1, int affectedCell2, int affectedCell3, int affectedCell4, int affectedCell5, int affectedCell6, Func<int, int, int, int, int, int, bool> isValid)
        : Constraint([affectedCell1, affectedCell2, affectedCell3, affectedCell4, affectedCell5, affectedCell6])
    {
        /// <summary>A function that determines whether a set of values is valid in the relevant cells.</summary>
        public Func<int, int, int, int, int, int, bool> IsValid { get; private set; } = isValid;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            if (state.LastPlacedCell == null)
                return null;

            var unknowns = AffectedCells.Count(af => state[af] == null);
            if (unknowns != 1)
                return null;
            var unknown = AffectedCells.First(af => state[af] == null);

            state.MarkImpossible(unknown, value => !IsValid(
                state[AffectedCells[0]] ?? value,
                state[AffectedCells[1]] ?? value,
                state[AffectedCells[2]] ?? value,
                state[AffectedCells[3]] ?? value,
                state[AffectedCells[4]] ?? value,
                state[AffectedCells[5]] ?? value
            ));
            return null;
        }
    }
}

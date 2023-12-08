using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Constrains a set of values to occur a specified maximum number of times within a specified region.</summary>
    /// <param name="cells">
    ///     Region of cells affected by the constraint.</param>
    /// <param name="isRestrictedValue">
    ///     Determines which value(s) may only occur up to <paramref name="maxAllowed"/> number of times within <paramref
    ///     name="cells"/>.</param>
    /// <param name="maxAllowed">
    ///     The maximum number of times a restricted value may occur within <paramref name="cells"/>.</param>
    public class MaximumCountConstraint(IEnumerable<int> cells, Func<int, bool> isRestrictedValue, int maxAllowed) : Constraint(cells)
    {
        /// <summary>
        ///     The value that may only occur up to <see cref="MaxAllowed"/> number of times within <see
        ///     cref="Constraint.AffectedCells"/>.</summary>
        public Func<int, bool> IsRestrictedValue { get; private set; } = isRestrictedValue;
        /// <summary>
        ///     The maximum number of times a restricted value (<see cref="IsRestrictedValue"/>) may occur within <see
        ///     cref="Constraint.AffectedCells"/>.</summary>
        public int MaxAllowed { get; private set; } = maxAllowed;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            var countAlready = AffectedCells.Count(c => state[c] is int cv && IsRestrictedValue(cv));
            if (countAlready > MaxAllowed)
                return ConstraintResult.Violation;
            if (countAlready == MaxAllowed)
            {
                for (var ix = 0; ix < AffectedCells.Length; ix++)
                    state.MarkImpossible(AffectedCells[ix], IsRestrictedValue);
                return ConstraintResult.Remove;
            }
            return null;
        }
    }
}

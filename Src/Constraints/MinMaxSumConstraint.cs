using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in which the sum of values in a specific region must have a specified minimum or maximum.</summary>
    /// <remarks>
    ///     Note that if <paramref name="min"/> and <paramref name="max"/> are both set to <c>true</c>, the result is a <see
    ///     cref="SumConstraint"/>. If they are both <c>false</c>, this constraint has no effect.</remarks>
    public class MinMaxSumConstraint(int sum, bool min, bool max, IEnumerable<int> affectedCells) : Constraint(affectedCells)
    {
        /// <summary>The region of cells that must have the minimum or maximum sum specified by <see cref="Sum"/>.</summary>
        public new int[] AffectedCells => base.AffectedCells;
        /// <summary>The desired minimum or maximum sum.</summary>
        public int Sum { get; private set; } = sum;
        /// <summary>Specifies whether <see cref="Sum"/> is a minimum.</summary>
        public bool Min { get; private set; } = min;
        /// <summary>Specifies whether <see cref="Sum"/> is a maximum.</summary>
        public bool Max { get; private set; } = max;

        /// <inheritdoc/>
        public override bool CanReevaluate => true;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            if (!Min && !Max)
                return ConstraintResult.Remove;

            var minPossibleSum = Max ? AffectedCells.Sum(state.MinPossible) : 0;
            var maxPossibleSum = Min ? AffectedCells.Sum(state.MaxPossible) : 0;

            foreach (var cell in AffectedCells)
                if (state[cell] == null)
                {
                    var minOther = Max ? minPossibleSum - state.MinPossible(cell) : 0;
                    var maxOther = Min ? maxPossibleSum - state.MaxPossible(cell) : 0;
                    state.MarkImpossible(cell, value => (Max && minOther + value > Sum) || (Min && maxOther + value < Sum));
                }

            return null;
        }
    }

    /// <summary>
    ///     Describes a constraint in which a region of cells must sum up to a specified value.</summary>
    /// <remarks>
    ///     Technically this constraint is equivalent to a <see cref="SumAlternativeConstraint"/> with a single region.</remarks>
    public class SumConstraint(int sum, IEnumerable<int> affectedCells) : MinMaxSumConstraint(sum, true, true, affectedCells) { }

    /// <summary>Describes a constraint in which a region of cells must sum up to a specified minimum value or more.</summary>
    public class MinSumConstraint(int sum, IEnumerable<int> affectedCells) : MinMaxSumConstraint(sum, true, false, affectedCells) { }

    /// <summary>Describes a constraint in which a region of cells must not sum up to more than a specified maximum value.</summary>
    public class MaxSumConstraint(int sum, IEnumerable<int> affectedCells) : MinMaxSumConstraint(sum, false, true, affectedCells) { }
}

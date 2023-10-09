using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Constrains three cells to values that satisfy a lambda expression.</summary>
    public class ThreeCellLambdaConstraint : Constraint
    {
        /// <summary>A function that determines whether a set of values is valid in the relevant cells.</summary>
        public Func<int, int, int, bool> IsValid { get; private set; }

        /// <summary>Constructor.</summary>
        public ThreeCellLambdaConstraint(int affectedCell1, int affectedCell2, int affectedCell3, Func<int, int, int, bool> isValid)
            : base(new[] { affectedCell1, affectedCell2, affectedCell3 })
        {
            IsValid = isValid;
        }

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            var unknown1Ix = AffectedCells.IndexOf(af => state[af] == null);
            if (unknown1Ix == -1)
                // All cells are filled in
                return IsValid(AffectedCells[0], AffectedCells[1], AffectedCells[2]) ? ConstraintResult.Remove : ConstraintResult.Violation;

            var unknown2Ix = AffectedCells.IndexOf(af => state[af] == null, unknown1Ix + 1);
            if (unknown2Ix == -1)
            {
                // All but one cell are filled in: reduce the last cell to its set of possibilities
                state.MarkImpossible(AffectedCells[unknown1Ix], value => !IsValid(
                    state[AffectedCells[0]] ?? value,
                    state[AffectedCells[1]] ?? value,
                    state[AffectedCells[2]] ?? value
                ));
                return ConstraintResult.Remove;
            }

            var combinations = new List<int?[]>();
            foreach (var v1 in state.Possible(AffectedCells[0]))
                foreach (var v2 in state.Possible(AffectedCells[1]))
                    foreach (var v3 in state.Possible(AffectedCells[2]))
                        if (IsValid(v1, v2, v3))
                            combinations.Add(new int?[] { v1, v2, v3 });
            return new CombinationsConstraint(AffectedCells, combinations);
        }
    }
}

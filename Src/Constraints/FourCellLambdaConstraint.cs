using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Constrains four cells to values that satisfy a lambda expression.</summary>
    /// <remarks>
    ///     This constraint is not very efficient as it will only be evaluated once all but one of the cells is filled in.</remarks>
    public class FourCellLambdaConstraint : Constraint
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
        public override ConstraintResult Process(SolverState state)
        {
            if (state.LastPlacedCell == null)
                return null;

            var unknowns = AffectedCells.Count(af => state[af] == null);

            if (unknowns == 1)
            {
                var unknown = AffectedCells.First(af => state[af] == null);

                state.MarkImpossible(unknown, value => !IsValid(
                    state[AffectedCells[0]] ?? value,
                    state[AffectedCells[1]] ?? value,
                    state[AffectedCells[2]] ?? value,
                    state[AffectedCells[3]] ?? value
                ));
            }
            else if (unknowns == 2)
            {
                var unkn1 = AffectedCells.First(af => state[af] == null);
                var unkn1Ix = AffectedCells.IndexOf(unkn1);
                var unkn2 = AffectedCells.Last(af => state[af] == null);
                var numValues = state.MaxValue - state.MinValue + 1;
                var possibles = new bool[numValues * numValues];
                for (var val1 = state.MinValue; val1 <= state.MaxValue; val1++)
                    for (var val2 = state.MinValue; val2 <= state.MaxValue; val2++)
                        possibles[(val1 - state.MinValue) + numValues * (val2 - state.MinValue)] = IsValid(
                            state[AffectedCells[0]] ?? (unkn1Ix == 0 ? val1 : val2),
                            state[AffectedCells[1]] ?? (unkn1Ix == 1 ? val1 : val2),
                            state[AffectedCells[2]] ?? (unkn1Ix == 2 ? val1 : val2),
                            state[AffectedCells[3]] ?? (unkn1Ix == 3 ? val1 : val2));
                state.MarkImpossible(unkn1, value1 => Enumerable.Range(0, numValues).All(value2 => !possibles[(value1 - state.MinValue) + numValues * value2]));
                state.MarkImpossible(unkn2, value2 => Enumerable.Range(0, numValues).All(value1 => !possibles[value1 + numValues * (value2 - state.MinValue)]));
            }

            return null;
        }
    }
}

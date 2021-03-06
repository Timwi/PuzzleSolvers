﻿using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>Override; see base.</summary>
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
                state[AffectedCells[2]] ?? value
            ));
            return null;
        }
    }
}

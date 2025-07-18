﻿using System.Collections.Generic;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes the function signature required for a <see cref="LambdaConstraint"/>. See <see
    ///     cref="Constraint.Process(SolverState)"/> for parameter and return value documentation.</summary>
    public delegate ConstraintResult CustomConstraint(SolverState state);

    /// <summary>Can be used to describe any constraint that applies to the whole puzzle using a lambda expression.</summary>
    public class LambdaConstraint(CustomConstraint lambda, IEnumerable<int> affectedCells = null) : Constraint(affectedCells)
    {
        /// <summary>The function used to evaluate this constraint.</summary>
        public CustomConstraint Lambda { get; private set; } = lambda;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state) => Lambda(state);
    }
}

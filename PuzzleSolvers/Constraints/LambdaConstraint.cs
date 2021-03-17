using System.Collections.Generic;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes the function signature required for a <see cref="LambdaConstraint"/>. See <see
    ///     cref="Constraint.MarkTakens(SolverState)"/> for parameter and return value documentation.</summary>
    public delegate IEnumerable<Constraint> CustomConstraint(SolverState state);

    /// <summary>Can be used to describe any constraint that applies to the whole puzzle using a lambda expression.</summary>
    public sealed class LambdaConstraint : Constraint
    {
        /// <summary>The function used to evaluate this constraint.</summary>
        public CustomConstraint Lambda { get; private set; }

        /// <summary>Constructor.</summary>
        public LambdaConstraint(CustomConstraint lambda, IEnumerable<int> affectedCells = null) : base(affectedCells)
        {
            Lambda = lambda;
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(SolverState state) => Lambda(state);
    }
}

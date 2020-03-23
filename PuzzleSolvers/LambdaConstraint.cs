using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes the function signature required for a <see cref="LambdaConstraint"/>. See <see
    ///     cref="Constraint.MarkTakens(bool[][], int?[], int?, int, int)"/> for parameter and return value documentation.</summary>
    public delegate IEnumerable<Constraint> CustomConstraint(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue);

    /// <summary>Can be used to describe any constraint that applies to the whole puzzle using a lambda expression.</summary>
    public sealed class LambdaConstraint : Constraint
    {
        /// <summary>The function used to evaluate this constraint.</summary>
        public CustomConstraint Lambda { get; private set; }

        /// <summary>Constructor.</summary>
        public LambdaConstraint(CustomConstraint lambda, IEnumerable<int> affectedCells = null, ConsoleColor? color = null, ConsoleColor? backgroundColor = null) : base(affectedCells, color, backgroundColor)
        {
            Lambda = lambda;
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue) =>
            Lambda(takens, grid, ix, minValue, maxValue);
    }
}

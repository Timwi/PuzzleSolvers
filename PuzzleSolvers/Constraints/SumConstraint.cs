using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in which a specific region of cells must sum up to a specified value.</summary>
    /// <remarks>
    ///     Technically this constraint is equivalent to a <see cref="SumAlternativeConstraint"/> with a single region.</remarks>
    public class SumConstraint : Constraint
    {
        /// <summary>The region of cells that must have the sum specified by <see cref="Sum"/>.</summary>
        public new int[] AffectedCells => base.AffectedCells;
        /// <summary>The desired sum.</summary>
        public int Sum { get; private set; }

        /// <summary>Constructor.</summary>
        public SumConstraint(int sum, IEnumerable<int> affectedCells) : base(affectedCells) { Sum = sum; }

        /// <summary>Override; see base.</summary>
        public override bool CanReevaluate => true;

        /// <summary>Override; see base.</summary>
        public override ConstraintResult Process(SolverState state)
        {
            var minPossibleSum = AffectedCells.Sum(state.MinPossible);
            var maxPossibleSum = AffectedCells.Sum(state.MaxPossible);

            for (var ix = 0; ix < AffectedCells.Length; ix++)
            {
                var cell = AffectedCells[ix];
                if (state[cell] != null)
                    continue;
                var minOther = minPossibleSum - state.MinPossible(cell);
                var maxOther = maxPossibleSum - state.MaxPossible(cell);
                state.MarkImpossible(cell, value => minOther + value > Sum || maxOther + value < Sum);
            }

            return null;
        }
    }
}

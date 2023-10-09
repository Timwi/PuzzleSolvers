using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint that mandates that there must be an equal number of odd and even values in a row. This is a
    ///     subset of the rules of Binairo.</summary>
    /// <remarks>
    ///     This constraint enforces a single row or column. For a full Binairo puzzle, you will need instances of this
    ///     constraint for every row and every column.</remarks>
    public class ParityEvennessConstraint : Constraint
    {
        /// <summary>Constructor.</summary>
        public ParityEvennessConstraint(IEnumerable<int> affectedCells) : base(affectedCells)
        {
            if (AffectedCells.Length % 2 != 0)
                throw new ArgumentException("ParityEvennessConstraint requires an even number of cells.", nameof(affectedCells));
        }

        private static readonly int[] _parities = new[] { 0, 1 };

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            if (state.LastPlacedCell == null)
                return null;
            foreach (var parity in _parities)
            {
                var count = AffectedCells.Count(cell => state[cell] != null && state[cell].Value % 2 == parity);
                if (count == AffectedCells.Length / 2)
                {
                    foreach (var cell in AffectedCells)
                        if (state[cell] == null)
                            for (var v = state.MinValue; v <= state.MaxValue; v++)
                                if (v % 2 == parity)
                                    state.MarkImpossible(cell, v);
                }
            }
            return null;
        }
    }
}

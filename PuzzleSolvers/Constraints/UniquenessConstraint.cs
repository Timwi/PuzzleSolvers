using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Describes a constraint that mandates that a region of cells must have different values.</summary>
    public class UniquenessConstraint : Constraint
    {
        /// <summary>The cells that must have different values.</summary>
        public new int[] AffectedCells => base.AffectedCells;

        /// <summary>Constructor.</summary>
        public UniquenessConstraint(IEnumerable<int> affectedCells) : base(affectedCells)
        {
        }

        /// <summary>Override.</summary>
        public override string ToString() => $"Uniqueness: {AffectedCells.JoinString(", ")}";

        /// <summary>Override; see base.</summary>
        public override bool CanReevaluate => true;

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(SolverState state)
        {
            if (state.LastPlacedCell != null)
            {
                foreach (var cell in AffectedCells)
                    if (cell != state.LastPlacedCell.Value)
                        state.MarkImpossible(cell, state.LastPlacedValue);
            }
            else
            {
                // In case this constraint was returned from another constraint when some of the grid is already filled in, make sure to enforce uniqueness correctly.
                foreach (var cell1 in AffectedCells)
                    if (state[cell1] != null)
                        foreach (var cell2 in AffectedCells)
                            if (cell2 != cell1)
                                state.MarkImpossible(cell2, state[cell1].Value);
            }

            // Special case: if the number of values equals the number of cells, we can detect when there’s only one place to put a certain number
            if (state.MaxValue - state.MinValue + 1 == AffectedCells.Length)
            {
                for (var v = state.MinValue; v <= state.MaxValue; v++)
                {
                    int? cell = null;
                    for (var ix = 0; ix < AffectedCells.Length; ix++)
                    {
                        if (!state.IsImpossible(AffectedCells[ix], v))
                        {
                            if (cell == null)
                                cell = AffectedCells[ix];
                            else
                                goto busted;
                        }
                    }

                    if (cell == null)
                        throw new ConstraintViolationException();

                    // We found a value that can have only one place
                    state.MustBe(cell.Value, v);

                    busted:;
                }
            }
            return null;
        }
    }
}

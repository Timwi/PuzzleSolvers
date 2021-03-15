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
        public override IEnumerable<Constraint> MarkTakens(SolverState state)
        {
            if (state.LastPlaced != null)
            {
                foreach (var cell in AffectedCells)
                    if (cell != state.LastPlaced.Value)
                        state.MarkImpossible(cell, state.LastPlacedValue);

                //// Special case: if the number of values equals the number of cells, we can detect when there’s only one place to put a certain number
                //if (maxValue - minValue + 1 == AffectedCells.Length)
                //{
                //    for (var v = 0; v <= minValue - maxValue; v++)
                //    {
                //        int? c = null;
                //        foreach (var cell in AffectedCells)
                //            if (!takens[cell][v])
                //            {
                //                if (c == null)
                //                    c = cell;
                //                else
                //                    goto busted;
                //            }
                //        if (c != null)
                //            for (var v2 = 0; v2 <= minValue - maxValue; v2++)
                //                if (v2 != v)
                //                    takens[c.Value][v2] = true;

                //        busted:;
                //    }
                //}
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
            return null;
        }
    }
}

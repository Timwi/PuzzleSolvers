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

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix != null)
            {
                if (!AffectedCells.Contains(ix.Value))
                    return null;

                foreach (var cell in AffectedCells)
                    if (cell != ix.Value)
                        takens[cell][grid[ix.Value].Value] = true;

                // Special case: if the number of values equals the number of cells, we can detect when there’s only one place to put a certain number
                if (maxValue - minValue + 1 == AffectedCells.Length)
                {
                    for (var v = 0; v <= minValue - maxValue; v++)
                    {
                        int? c = null;
                        foreach (var cell in AffectedCells)
                            if (!takens[cell][v])
                            {
                                if (c == null)
                                    c = cell;
                                else
                                    goto busted;
                            }
                        if (c != null)
                            for (var v2 = 0; v2 <= minValue - maxValue; v2++)
                                if (v2 != v)
                                    takens[c.Value][v2] = true;

                        busted:;
                    }
                }
            }
            else
            {
                var values = ix == null ? AffectedCells.Where(cell => grid[cell] != null).Select(cell => grid[cell].Value).ToHashSet() : null;
                if (values.Count > 0)
                    foreach (var cell in AffectedCells)
                        for (var v = 0; v < takens[cell].Length; v++)
                            if (values.Contains(v))
                                takens[cell][v] = true;
            }
            return null;
        }
    }
}

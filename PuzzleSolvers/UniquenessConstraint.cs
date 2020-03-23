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
        public UniquenessConstraint(IEnumerable<int> affectedCells, ConsoleColor? color = null, ConsoleColor? backgroundColor = null) : base(affectedCells, color, backgroundColor)
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

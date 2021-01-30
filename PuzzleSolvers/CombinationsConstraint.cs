using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in which a specified set of cells must have one of a specified set of exact value
    ///     combinations.</summary>
    public class CombinationsConstraint : Constraint
    {
        /// <summary>The set of combinations allowed for the specified set of cells.</summary>
        public int[][] Combinations { get; private set; }

        /// <summary>Constructor.</summary>
        public CombinationsConstraint(IEnumerable<int> affectedCells, IEnumerable<int[]> combinations) : base(affectedCells)
        {
            Combinations = (combinations as int[][]) ?? combinations.ToArray();
            if (Combinations.Any(comb => comb.Length != AffectedCells.Length))
                throw new ArgumentException($"The combinations passed to a CombinationsConstraint must match the size of the region ({AffectedCells.Length}).");
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix == null)
            {
                for (var i = 0; i < AffectedCells.Length; i++)
                    if (grid[AffectedCells[i]] == null)
                        for (var v = 0; v < takens[AffectedCells[i]].Length; v++)
                            if (!Combinations.Any(cmb => cmb[i] == v + minValue))
                                takens[AffectedCells[i]][v] = true;
                return null;
            }
            else
            {
                var newCombinations = ix == null ? null : Combinations
                    .Where(cmb => Enumerable.Range(0, AffectedCells.Length).All(i => grid[AffectedCells[i]] == null ? !takens[AffectedCells[i]][cmb[i] - minValue] : (grid[AffectedCells[i]].Value == cmb[i] - minValue)))
                    .ToArray();
                return new[] { new CombinationsConstraint(AffectedCells, newCombinations) };
            }
        }
    }
}

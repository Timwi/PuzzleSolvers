using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

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
            // This will determine which values are still possible in which cells.
            // (It’s the inverse of ‘takens’, and limited to only the affected cells.)
            var poss = Ut.NewArray(AffectedCells.Length, i => new bool[takens[i].Length]);

            // If any combination can be ruled out, this will contain the remaining combinations still available.
            List<int[]> newComb = null;

            for (var i = 0; i < Combinations.Length; i++)
            {
                // Can this combination be ruled out?
                if (AffectedCells.Any((cellIx, lstIx) => grid[cellIx] == null ? (takens[cellIx][Combinations[i][lstIx] - minValue]) : (grid[cellIx].Value + minValue != Combinations[i][lstIx])))
                {
                    if (newComb == null)
                        newComb = new List<int[]>(Combinations.Take(i));
                }
                else
                {
                    if (newComb != null)
                        newComb.Add(Combinations[i]);

                    // Remember the possibilities for each cell
                    for (var lstIx = 0; lstIx < Combinations[i].Length; lstIx++)
                        poss[lstIx][Combinations[i][lstIx] - minValue] = true;
                }
            }

            // Mark any cell values that are no longer possible as taken
            var anyChanges = false;
            for (var lstIx = 0; lstIx < poss.Length; lstIx++)
                if (grid[AffectedCells[lstIx]] == null)
                    for (var v = 0; v < poss[lstIx].Length; v++)
                        if (!poss[lstIx][v] && !takens[AffectedCells[lstIx]][v])
                        {
                            takens[AffectedCells[lstIx]][v] = true;
                            anyChanges = true;
                        }

            if (newComb != null)
                return new[] { new CombinationsConstraint(AffectedCells, newComb.ToArray()) };
            if (anyChanges)
                return new[] { this };
            return null;
        }

        /// <summary>Override.</summary>
        public override string ToString() => $"{Combinations.Length} combinations";
    }
}

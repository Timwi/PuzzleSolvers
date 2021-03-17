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
        public int?[][] Combinations { get; private set; }

        /// <summary>Constructor.</summary>
        public CombinationsConstraint(IEnumerable<int> affectedCells, IEnumerable<int?[]> combinations) : base(affectedCells)
        {
            Combinations = (combinations as int?[][]) ?? combinations.ToArray();
            if (Combinations.Any(comb => comb.Length != AffectedCells.Length))
                throw new ArgumentException($"The combinations passed to a CombinationsConstraint must match the size of the region ({AffectedCells.Length}).");
        }

        /// <summary>Constructor.</summary>
        public CombinationsConstraint(IEnumerable<int> affectedCells, IEnumerable<int[]> combinations) : this(affectedCells, combinations.Select(comb => comb.Select(Ut.Nullable).ToArray())) { }

        /// <summary>Override; see base.</summary>
        public override bool CanReevaluate => true;

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(SolverState state)
        {
            // This will determine which values are still possible in which of the affected cells.
            var poss = Ut.NewArray(AffectedCells.Length, i => new bool[state.MaxValue - state.MinValue + 1]);

            // If any combination can be ruled out, this will contain the remaining combinations still available.
            List<int?[]> newComb = null;

            for (var i = 0; i < Combinations.Length; i++)
            {
                // Can this combination be ruled out?
                if (AffectedCells.Any((cellIx, lstIx) => Combinations[i][lstIx] != null && (state[cellIx] == null ? state.IsImpossible(cellIx, Combinations[i][lstIx].Value) : (state[cellIx].Value != Combinations[i][lstIx].Value))))
                {
                    if (newComb == null)
                        newComb = new List<int?[]>(Combinations.Take(i));
                }
                else
                {
                    if (newComb != null)
                        newComb.Add(Combinations[i]);

                    // Remember the possibilities for each cell
                    for (var lstIx = 0; lstIx < Combinations[i].Length; lstIx++)
                        if (Combinations[i][lstIx] == null)
                            poss[lstIx] = null;
                        else if (poss[lstIx] != null)
                            poss[lstIx][Combinations[i][lstIx].Value - state.MinValue] = true;
                }
            }

            // Mark any cell values that are no longer possible as taken
            for (var lstIx = 0; lstIx < poss.Length; lstIx++)
                if (state[AffectedCells[lstIx]] == null && poss[lstIx] != null)
                    for (var v = 0; v < poss[lstIx].Length; v++)
                        if (!poss[lstIx][v] && !state.IsImpossible(AffectedCells[lstIx], v + state.MinValue))
                            state.MarkImpossible(AffectedCells[lstIx], v + state.MinValue);

            if (newComb != null)
                return new[] { new CombinationsConstraint(AffectedCells, newComb.ToArray()) };
            return null;
        }

        /// <summary>Override.</summary>
        public override string ToString() => $"{Combinations.Length} combinations";
    }
}

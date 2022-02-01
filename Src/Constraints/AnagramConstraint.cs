using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in which a specified set of cells must have one of a specified set of value
    ///     combinations, but they may be reordered arbitrarily.</summary>
    public class AnagramConstraint : Constraint
    {
        /// <summary>The set of combinations allowed for the specified set of cells.</summary>
        public List<int>[] Combinations { get; private set; }

        /// <summary>Constructor.</summary>
        public AnagramConstraint(IEnumerable<int> affectedCells, IEnumerable<List<int>> combinations) : base(affectedCells)
        {
            if (AffectedCells == null)
                throw new ArgumentException("AnagramConstraint must be given a set of affected cells.");
            Combinations = (combinations as List<int>[]) ?? combinations.ToArray();
            if (Combinations.Any(comb => comb.Count != AffectedCells.Length))
                throw new ArgumentException($"The combinations passed to an AnagramConstraint must match the size of the region ({AffectedCells.Length}).");
        }

        /// <summary>Override; see base.</summary>
        public override bool CanReevaluate => true;

        /// <summary>Override; see base.</summary>
        public override ConstraintResult Process(SolverState state)
        {
            // This will determine which values are still possible in any of the remaining anagrams.
            var poss = new bool[state.MaxValue - state.MinValue + 1];

            // If any combination can be ruled out, this will contain the remaining combinations still available.
            List<List<int>> newComb = null;

            for (var cmbIx = 0; cmbIx < Combinations.Length; cmbIx++)
            {
                // Can this combination be ruled out?
                var canBeRuledOut = false;
                var copy = Combinations[cmbIx].ToList();
                for (var cIx = 0; cIx < AffectedCells.Length && !canBeRuledOut; cIx++)
                    if (state[AffectedCells[cIx]] != null && !copy.Remove(state[AffectedCells[cIx]].Value))
                        canBeRuledOut = true;

                if (canBeRuledOut)
                {
                    if (newComb == null)
                        newComb = new List<List<int>>(Combinations.Take(cmbIx));
                }
                else
                {
                    if (newComb != null)
                        newComb.Add(Combinations[cmbIx]);

                    // Remember the remaining possible values
                    for (var i = 0; i < copy.Count; i++)
                        poss[copy[i] - state.MinValue] = true;
                }
            }

            // Mark any cell values that are no longer possible as taken
            foreach (var cell in AffectedCells)
                state.MarkImpossible(cell, v => !poss[v - state.MinValue]);

            if (newComb != null)
                return new[] { new AnagramConstraint(AffectedCells, newComb.ToArray()) };

            return null;
        }

        /// <summary>Override.</summary>
        public override string ToString() => $"{Combinations.Length} combinations";
    }
}

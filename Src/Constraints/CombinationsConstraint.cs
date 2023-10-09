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

        /// <inheritdoc/>
        public override int? NumCombinations => Combinations.Length;

        /// <summary>Constructor.</summary>
        public CombinationsConstraint(IEnumerable<int> affectedCells, IEnumerable<int?[]> combinations) : base(affectedCells)
        {
            Combinations = (combinations as int?[][]) ?? combinations.ToArray();
            if (Combinations.Any(comb => comb.Length != AffectedCells.Length))
                throw new ArgumentException($"The combinations passed to a CombinationsConstraint must match the size of the region ({AffectedCells.Length}).");
        }

        /// <summary>Constructor.</summary>
        public CombinationsConstraint(IEnumerable<int> affectedCells, IEnumerable<int[]> combinations) : this(affectedCells, combinations.Select(comb => comb.Select(Ut.Nullable).ToArray())) { }

        /// <inheritdoc/>
        public override bool CanReevaluate => true;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            // This will determine which values are still possible in which of the affected cells.
            var poss = Ut.NewArray(AffectedCells.Length, i => new bool[state.MaxValue - state.MinValue + 1]);

            // If any combination can be ruled out, this will contain the remaining combinations still available.
            List<int?[]> newComb = null;

            for (var i = 0; i < Combinations.Length; i++)
            {
                // Can this combination be ruled out?
                if (AffectedCells.Any((cellIx, lstIx) => Combinations[i][lstIx] != null && (state[cellIx] == null ? state.IsImpossible(cellIx, Combinations[i][lstIx].Value) : (state[cellIx].Value != Combinations[i][lstIx].Value))))
                    newComb ??= new List<int?[]>(Combinations.Take(i));
                else
                {
                    newComb?.Add(Combinations[i]);

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
                if (poss[lstIx] != null)
                    state.MarkImpossible(AffectedCells[lstIx], v => !poss[lstIx][v - state.MinValue]);

            if (newComb != null)
                return newComb.Count == 1 ? ConstraintResult.Remove : new[] { new CombinationsConstraint(AffectedCells, newComb.ToArray()) };
            return null;
        }

        /// <summary>Override.</summary>
        public override string ToString() => $"{Combinations.Length} combinations";
    }
}

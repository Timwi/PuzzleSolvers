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

        private List<int>[] _optimizationList = null;

        /// <summary>Override; see base.</summary>
        public override ConstraintResult Process(SolverState state)
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

            // Special case: if the number of values equals the number of cells, we can detect further optimizations
            if (state.MaxValue - state.MinValue + 1 == AffectedCells.Length)
            {
                if (_optimizationList == null || _optimizationList.Length != state.MaxValue - state.MinValue + 1)
                    _optimizationList = new List<int>[state.MaxValue - state.MinValue + 1];
                var cells = _optimizationList;
                for (var i = 0; i < cells.Length; i++)
                    if (cells[i] != null)
                        cells[i].Clear();

                foreach (var cell in AffectedCells)
                    for (var v = state.MinValue; v <= state.MaxValue; v++)
                        if (!state.IsImpossible(cell, v))
                        {
                            if (cells[v - state.MinValue] == null)
                                cells[v - state.MinValue] = new List<int>();
                            cells[v - state.MinValue].Add(cell);
                        }

                for (var v1 = 0; v1 <= state.MaxValue - state.MinValue; v1++)
                {
                    // Detect if a value can only be in one place
                    if (cells[v1]?.Count == 1)
                        state.MustBe(cells[v1][0], v1 + state.MinValue);

                    for (var v2 = v1 + 1; v2 <= state.MaxValue - state.MinValue; v2++)
                    {
                        // Detect if two values can only be in two places (“pair”)
                        if (cells[v1]?.Count == 2 && cells[v2]?.Count == 2 && cells[v1].All(cells[v2].Contains))
                            foreach (var c in cells[v1])
                                state.MarkImpossible(c, v => v != v1 + state.MinValue && v != v2 + state.MinValue);

                        for (var v3 = v2 + 1; v3 <= state.MaxValue - state.MinValue; v3++)
                        {
                            // Detect if three values can only be in three places (“triplet”)
                            if (cells[v1]?.Count <= 3 && cells[v2]?.Count <= 3 && cells[v3]?.Count <= 3)
                            {
                                var hashSet = new HashSet<int>(cells[v1]);
                                hashSet.AddRange(cells[v2]);
                                hashSet.AddRange(cells[v3]);
                                if (hashSet.Count <= 3)
                                    foreach (var c in hashSet)
                                        state.MarkImpossible(c, v => v != v1 + state.MinValue && v != v2 + state.MinValue && v != v3 + state.MinValue);
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}

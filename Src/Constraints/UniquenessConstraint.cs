using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers;

/// <summary>Describes a constraint that mandates that a region of cells must have different values.</summary>
public class UniquenessConstraint(IEnumerable<int> affectedCells) : Constraint(affectedCells)
{
    /// <summary>The cells that must have different values.</summary>
    public new int[] AffectedCells => base.AffectedCells;

    /// <summary>Override.</summary>
    public override string ToString() => $"Uniqueness: {AffectedCells.JoinString(", ")}";

    /// <inheritdoc/>
    public override bool CanReevaluate => true;

    /// <inheritdoc/>
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
            var cells = new List<int>[state.MaxValue - state.MinValue + 1];

            foreach (var cell in AffectedCells)
                for (var v = state.MinValue; v <= state.MaxValue; v++)
                    if (!state.IsImpossible(cell, v))
                        (cells[v - state.MinValue] ??= []).Add(cell);

            for (var v1 = 0; v1 <= state.MaxValue - state.MinValue; v1++)
                if (cells[v1] is { } c1)
                {
                    // Detect if a value can only be in one place
                    if (c1.Count == 1)
                        state.MustBe(c1[0], v1 + state.MinValue);

                    for (var v2 = v1 + 1; v2 <= state.MaxValue - state.MinValue; v2++)
                        if (cells[v2] is { } c2)
                        {
                            // Detect if two values can only be in two places (“pair”)
                            if (c1.Count == 2 && c1.SequenceEqual(c2))
                                foreach (var c in c1)
                                    state.MarkImpossible(c, v => v != v1 + state.MinValue && v != v2 + state.MinValue);

                            for (var v3 = v2 + 1; v3 <= state.MaxValue - state.MinValue; v3++)
                                if (cells[v3] is { } c3)
                                {
                                    // Detect if three values can only be in three places (“triplet”)
                                    if (c1.Count <= 3 && c2.Count <= 3 && c3.Count <= 3)
                                    {
                                        var hashSet = new HashSet<int>(c1);
                                        hashSet.AddRange(c2);
                                        hashSet.AddRange(c3);
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

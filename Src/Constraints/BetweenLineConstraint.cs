using System;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a “between line” constraint: every digit within the <see cref="InnerCells"/> must lie numerically
    ///     between the values in cells <see cref="Cap1"/> and <see cref="Cap2"/> (exclusive).</summary>
    public class BetweenLineConstraint : Constraint
    {
        /// <summary>
        ///     The cell containing one of the caps of the between line.</summary>
        /// <remarks>
        ///     The constraint does not require this to be the lower or upper cap of the between line.</remarks>
        public int Cap1 { get; private set; }
        /// <summary>
        ///     The cell containing the other cap of the between line.</summary>
        /// <remarks>
        ///     The constraint does not require this to be the lower or upper cap of the between line.</remarks>
        public int Cap2 { get; private set; }
        /// <summary>The set of cells along the between line.</summary>
        public int[] InnerCells { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="cap1">
        ///     The cell containing one of the caps of the between line.</param>
        /// <param name="cap2">
        ///     The cell containing the other cap of the between line.</param>
        /// <param name="innerCells">
        ///     The set of cells along the between line.</param>
        public BetweenLineConstraint(int cap1, int cap2, int[] innerCells) : base(innerCells.Concat([cap1, cap2]))
        {
            if (cap1 == cap2)
                throw new ArgumentException("cap1 and cap2 can’t be equal.", nameof(cap2));
            Cap1 = cap1;
            Cap2 = cap2;
            InnerCells = innerCells ?? throw new ArgumentNullException(nameof(innerCells));
        }

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            var ix = state.LastPlacedCell;
            if (ix == null)
            {
                // The inside of the line cannot contain the min or max value
                for (var icIx = 0; icIx < InnerCells.Length; icIx++)
                {
                    state.MarkImpossible(InnerCells[icIx], state.MinValue);
                    state.MarkImpossible(InnerCells[icIx], state.MaxValue);
                }
                return null;
            }

            if (ix == Cap1 || ix == Cap2)
            {
                // If both caps are filled in, all the inner cells must be between them.
                if (state[Cap1] != null && state[Cap2] != null)
                {
                    var min = Math.Min(state[Cap1].Value, state[Cap2].Value);
                    var max = Math.Max(state[Cap1].Value, state[Cap2].Value);
                    for (var icIx = 0; icIx < InnerCells.Length; icIx++)
                        state.MarkImpossible(InnerCells[icIx], v => v <= min || v >= max);
                }
                // If one cap is filled in, all the inner cells must be different from it.
                else
                {
                    for (var icIx = 0; icIx < InnerCells.Length; icIx++)
                        state.MarkImpossible(InnerCells[icIx], state.LastPlacedValue);
                }
            }

            var curMin = InnerCells.Where(c => state[c] != null).MinOrNull(c => state[c].Value);
            if (curMin == null)
                return null;
            var curMax = InnerCells.Where(c => state[c] != null).Max(c => state[c].Value);

            // If neither cap is filled in, both must be outside the range but we don’t yet know which one is the min and which one is the max
            if (state[Cap1] == null && state[Cap2] == null)
            {
                state.MarkImpossible(Cap1, v => v >= curMin.Value && v <= curMax);
                state.MarkImpossible(Cap2, v => v >= curMin.Value && v <= curMax);
            }
            else
            {
                // Check if Cap1 is filled in and Cap2 not, and then also check the reverse
                var cap1 = Cap1;
                var cap2 = Cap2;
                iter:
                if (state[cap1] == null && state[cap2] != null)
                {
                    if (state[cap2].Value > curMax)
                    {
                        // grid[cap1] must be < curMin
                        state.MarkImpossible(cap1, v => v >= curMin.Value);
                        // The rest of the inner cells must be < state[cap2]
                        foreach (var cell in InnerCells)
                            state.MarkImpossible(cell, v => v >= state[cap2].Value);
                    }
                    else if (state[cap2].Value < curMin.Value)
                    {
                        // grid[cap1] must be > curMax
                        state.MarkImpossible(cap1, v => v <= curMax);
                        // The rest of the inner cells must be > state[cap2]
                        foreach (var cell in InnerCells)
                            state.MarkImpossible(cell, v => v <= state[cap2].Value);
                    }
                    else
                        throw new InvalidOperationException("BetweenLineConstraint encountered an internal bug.");
                }
                if (cap1 == Cap1)
                {
                    cap1 = Cap2;
                    cap2 = Cap1;
                    goto iter;
                }
            }
            return null;
        }
    }
}

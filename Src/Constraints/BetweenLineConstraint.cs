using System;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    public class BetweenLineConstraint : Constraint
    {
        public int Cap1 { get; private set; }
        public int Cap2 { get; private set; }
        public int[] InnerCells { get; private set; }
        public BetweenLineConstraint(int cap1, int cap2, int[] innerCells) : base(innerCells.Concat(new[] { cap1, cap2 }))
        {
            if (cap1 == cap2)
                throw new ArgumentException("cap1 and cap2 can’t be equal.", nameof(cap2));
            Cap1 = cap1;
            Cap2 = cap2;
            InnerCells = innerCells ?? throw new ArgumentNullException(nameof(innerCells));
        }

        public override ConstraintResult Process(SolverState state)
        {
            var ix = state.LastPlacedCell;
            if (state.LastPlacedCell == null)
            {
                // The inside of the line cannot contain a 1 or a 9
                for (var icIx = 0; icIx < InnerCells.Length; icIx++)
                {
                    state.MarkImpossible(InnerCells[icIx], 1);
                    state.MarkImpossible(InnerCells[icIx], 9);
                }
                return null;
            }

            // If both caps are filled in, all the inner cells must simply be between them.
            if (state[Cap1] != null && state[Cap2] != null)
            {
                // We don’t need to recompute this multiple times.
                if (!(ix == Cap1 || ix == Cap2))
                    return null;
                var min = Math.Min(state[Cap1].Value, state[Cap2].Value);
                var max = Math.Max(state[Cap1].Value, state[Cap2].Value);
                for (var icIx = 0; icIx < InnerCells.Length; icIx++)
                    state.MarkImpossible(InnerCells[icIx], v => v <= min || v >= max);
            }
            // If one cap is filled in, all the inner cells must simply be different from it.
            else if (state[Cap1] != null || state[Cap2] != null)
            {
                // We don’t need to recompute this multiple times.
                if (!(ix == Cap1 || ix == Cap2))
                    return null;
                var v = state[Cap1] ?? state[Cap2].Value;
                for (var icIx = 0; icIx < InnerCells.Length; icIx++)
                    state.MarkImpossible(InnerCells[icIx], v);
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
                    }
                    else if (state[cap2].Value < curMin.Value)
                    {
                        // grid[cap1] must be > curMax
                        state.MarkImpossible(cap1, v => v <= curMax);
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

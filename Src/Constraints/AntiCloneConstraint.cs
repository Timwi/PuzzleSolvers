using System;

namespace PuzzleSolvers
{
    /// <summary>Describes a constraint that mandates that multiple same-size regions cannot contain the exact same digits.</summary>
    public class AntiCloneConstraint : Constraint
    {
        /// <summary>
        ///     Constructor.</summary>
        /// <param name="regions">
        ///     The regions that cannot contain identical values. Note that containing the same values in a different order is
        ///     still allowed. Note also that regions of different sizes are treated separately.</param>
        public AntiCloneConstraint(int[][] regions) : base(null)
        {
            if (regions == null)
                throw new ArgumentNullException(nameof(regions));
            if (regions.Length == 0)
                throw new ArgumentException($"‘{nameof(regions)}’ cannot be empty.", nameof(regions));
            for (var rgIx = 0; rgIx < regions.Length; rgIx++)
                for (var i = 0; i < regions[rgIx].Length; i++)
                    for (var j = i + 1; j < regions[rgIx].Length; j++)
                        if (regions[rgIx][i] == regions[rgIx][j])
                            throw new ArgumentException($"Region #{rgIx} in ‘{nameof(regions)}’ cannot contain a duplicate cell.");
            Regions = regions;
        }

        /// <summary>Determines the regions that must not be equal.</summary>
        public int[][] Regions { get; private set; }

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            var nn = new int[Regions.Length];   // How many values are null (not filled in) in each region (capped at 2)
            var ixs = new int[Regions.Length];  // Which index within the region is null (not filled in) in case there is exactly one
            for (var rgIx = 0; rgIx < Regions.Length; rgIx++)
                for (var ix = 0; ix < Regions[rgIx].Length && nn[rgIx] < 2; ix++)
                    if (state[Regions[rgIx][ix]] == null)
                    {
                        nn[rgIx]++;
                        ixs[rgIx] = ix;
                    }

            for (var rgIx1 = 0; rgIx1 < Regions.Length; rgIx1++)
                if (nn[rgIx1] < 2)
                    for (var rgIx2 = rgIx1 + 1; rgIx2 < Regions.Length; rgIx2++)
                        if (nn[rgIx2] < 2)
                        {
                            // Both already filled in: don’t care (assume already satisfied)
                            if (nn[rgIx1] == 0 && nn[rgIx2] == 0)
                                continue;
                            // Any unmatched values already filled in: already satisfied
                            for (var ix = 0; ix < Regions[rgIx1].Length; ix++)
                                if (state[Regions[rgIx1][ix]] is int v1 && state[Regions[rgIx2][ix]] is int v2 && v1 != v2)
                                    goto next;

                            // One filled in and one missing a value: make sure the missing value won’t be equal
                            if (nn[rgIx1] == 0)
                                state.MarkImpossible(Regions[rgIx2][ixs[rgIx2]], state[Regions[rgIx1][ixs[rgIx2]]].Value);
                            else if (nn[rgIx2] == 0)
                                state.MarkImpossible(Regions[rgIx1][ixs[rgIx1]], state[Regions[rgIx2][ixs[rgIx1]]].Value);
                            else    // Both missing a value: special case if it’s the same cell
                            {
                                var ix1 = ixs[rgIx1];
                                var ix2 = ixs[rgIx2];
                                if (Regions[rgIx2][ix2] != Regions[rgIx1][ix1])
                                    continue;
                                if (ix1 == ix2)
                                    return ConstraintResult.Violation;
                                var susVal = state[Regions[rgIx2][ix1]].Value;
                                if (susVal != state[Regions[rgIx1][ix2]].Value)
                                    continue;
                                state.MarkImpossible(Regions[rgIx1][ix2], susVal);
                                state.MarkImpossible(Regions[rgIx2][ix1], susVal);
                            }

                            next:;
                        }
            return null;
        }
    }
}

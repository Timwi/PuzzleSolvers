using System;

namespace PuzzleSolvers
{
    /// <summary>
    /// Describes a constraint that mandates that two same-size regions cannot contain the exact same digits.
    /// </summary>
    public class AntiCloneConstraint : Constraint
    {
        /// <summary>Constructor.</summary>
        public AntiCloneConstraint(int[][] regions) : base(null)
        {
            if (regions == null)
                throw new ArgumentNullException(nameof(regions));
            if (regions.Length == 0)
                throw new ArgumentException("‘regions’ cannot be empty.", nameof(regions));
            var size = regions[0].Length;
            for (var i = 1; i < regions.Length; i++)
                if (regions[i].Length != size)
                    throw new ArgumentException("‘regions’ must contain regions of equal sizes.", nameof(regions));
            Regions = regions;
        }

        /// <summary>Determines the regions that must not be equal.</summary>
        public int[][] Regions { get; private set; }

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            for (var regionIx = 0; regionIx < Regions.Length; regionIx++)
            {
                for (var cellIx = 0; cellIx < Regions[regionIx].Length; cellIx++)
                    if (state[Regions[regionIx][cellIx]] == null)
                        goto next2;
                for (var region2Ix = 0; region2Ix < Regions.Length; region2Ix++)
                {
                    if (region2Ix == regionIx || Regions[regionIx].Length != Regions[region2Ix].Length)
                        continue;
                    int? nullCellIx = null;
                    for (var cellIx = 0; cellIx < Regions[region2Ix].Length; cellIx++)
                    {
                        if (state[Regions[region2Ix][cellIx]] == null)
                        {
                            if (nullCellIx != null)
                                goto next;
                            nullCellIx = cellIx;
                        }
                        else if (state[Regions[region2Ix][cellIx]] != state[Regions[regionIx][cellIx]])
                            goto next;
                    }
                    if (nullCellIx != null)
                        state.MarkImpossible(Regions[region2Ix][nullCellIx.Value], state[Regions[regionIx][nullCellIx.Value]].Value);
                    next:;
                }
                next2:;
            }
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement puzzle where either one of several regions must have a specified sum.</summary>
    public class SumAlternativeConstraint : Constraint
    {
        /// <summary>The desired sum.</summary>
        public int Sum { get; private set; }

        /// <summary>
        ///     Contains the regions affected by the constraint. At least one of these regions must have the sum specified by
        ///     <see cref="Sum"/>. Each region is an array of cell indices.</summary>
        public int[][] Regions { get; private set; }

        /// <summary>Constructor.</summary>
        public SumAlternativeConstraint(int sum, params IEnumerable<int>[] regions) : base(regions.SelectMany(r => r).Distinct())
        {
            Sum = sum;
            Regions = regions.Select(r => r.ToArray()).ToArray();
        }

        private SumAlternativeConstraint(int sum, IEnumerable<int[]> regions) : base(regions.SelectMany(r => r).Distinct())
        {
            Sum = sum;
            Regions = regions.ToArray();
        }

        /// <summary>Override; see base.</summary>
        public override bool CanReevaluate => true;

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(SolverState state)
        {
            // This can only happen if the user has specified a single region at the start.
            if (Regions.Length == 1)
                return new[] { new SumConstraint(Sum, Regions[0]) };

            // Find out which regions can now be ruled out
            List<int[]> newRegions = null;
            for (var i = 0; i < Regions.Length; i++)
            {
                var region = Regions[i];
                int min, max;
                if ((min = region.Sum(state.MinPossible)) > Sum || (max = region.Sum(state.MaxPossible)) < Sum)
                {
                    if (newRegions == null)
                        newRegions = new List<int[]>(Regions.Take(i));
                }
                else
                {
                    if (min == Sum && max == Sum)   // The constraint is already satisfied
                        return Enumerable.Empty<Constraint>();
                    if (newRegions != null)
                        newRegions.Add(region);
                }
            }
            if (newRegions != null)
            {
                if (newRegions.Count == 0)
                    // This can only happen if some regions overlap and the algorithm filled in one of the shared cells.
                    throw new ConstraintViolationException();
                else if (newRegions.Count == 1)
                    return new[] { new SumConstraint(Sum, newRegions[0]) };
                else
                    return new[] { new SumAlternativeConstraint(Sum, newRegions) };
            }
            return null;
        }
    }
}

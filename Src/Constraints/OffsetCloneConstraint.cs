using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint where two regions of the grid must be identical (“clones”) except that one is plus or minus
    ///     a consistent offset from the other.</summary>
    /// <remarks>
    ///     For best results, this constraint should be last in the list of constraints in the puzzle so that it can re-use
    ///     deductions obtained from the other constraints.</remarks>
    public class OffsetCloneConstraint : Constraint
    {
        /// <summary>The first area that needs to be cloned.</summary>
        public int[] Area1 { get; private set; }
        /// <summary>The other area that needs to be identical to the first.</summary>
        public int[] Area2 { get; private set; }

        /// <summary>Constructor.</summary>
        public OffsetCloneConstraint(IEnumerable<int> area1, IEnumerable<int> area2) : base(area1.Concat(area2))
        {
            if (area1 == null)
                throw new ArgumentNullException(nameof(area1));
            if (area2 == null)
                throw new ArgumentNullException(nameof(area2));
            Area1 = area1.ToArray();
            Area2 = area2.ToArray();
            if (Area1.Length != Area2.Length)
                throw new ArgumentException("The two clone areas must have the same size.", nameof(area2));
        }

        /// <inheritdoc/>
        public override bool CanReevaluate => true;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            // Determine if we know the offset
            for (var i = 0; i < Area1.Length; i++)
                if (state[Area1[i]] != null && state[Area2[i]] != null)
                {
                    // We found the offset. Process the entire region and stop here.
                    var offset = state[Area2[i]].Value - state[Area1[i]].Value;
                    for (var ix = 0; ix < Area1.Length; ix++)
                    {
                        state.MarkImpossible(Area1[ix], value => state.IsImpossible(Area2[ix], value + offset));
                        state.MarkImpossible(Area2[ix], value => state.IsImpossible(Area1[ix], value - offset));
                    }
                    return null;
                }

            // We do not know the offset.
            return null;
        }
    }
}

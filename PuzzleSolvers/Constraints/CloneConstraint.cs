using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint where two regions of the grid must be identical (“clones”).</summary>
    /// <remarks>
    ///     For best results, this constraint should be last in the list of constraints in the puzzle so that it can re-use
    ///     deductions obtained from the other constraints.</remarks>
    public class CloneConstraint : Constraint
    {
        /// <summary>The first area that needs to be cloned.</summary>
        public int[] Area1 { get; private set; }
        /// <summary>The other area that needs to be identical to the first.</summary>
        public int[] Area2 { get; private set; }

        /// <summary>Constructor.</summary>
        public CloneConstraint(IEnumerable<int> area1, IEnumerable<int> area2) : base(area1.Concat(area2))
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

        /// <summary>Override; see base.</summary>
        public override bool CanReevaluate => true;

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(SolverState state)
        {
            for (var ix = 0; ix < Area1.Length; ix++)
            {
                state.MarkImpossible(Area1[ix], value => state.IsImpossible(Area2[ix], value));
                state.MarkImpossible(Area2[ix], value => state.IsImpossible(Area1[ix], value));
            }
            return null;
        }
    }
}

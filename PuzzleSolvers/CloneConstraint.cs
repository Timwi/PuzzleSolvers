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
        public CloneConstraint(IEnumerable<int> area1, IEnumerable<int> area2, ConsoleColor? color = null, ConsoleColor? backgroundColor = null) : base(area1.Concat(area2), color, backgroundColor)
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
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix != null)
            {
                var p1 = Array.IndexOf(Area1, ix.Value);
                var p2 = Array.IndexOf(Area2, ix.Value);
                if (p1 != -1 || p2 != -1)
                {
                    var otherIx = p1 == -1 ? Area1[p2] : Area2[p1];
                    for (var v = 0; v < takens[otherIx].Length; v++)
                        if (v + minValue != grid[ix.Value].Value)
                            takens[otherIx][v] = true;
                }
            }

            for (var a1ix = 0; a1ix < Area1.Length; a1ix++)
                if (grid[Area1[a1ix]] == null)
                    for (var v = 0; v < takens[Area1[a1ix]].Length; v++)
                        if (takens[Area1[a1ix]][v])
                            takens[Area2[a1ix]][v] = true;

            for (var a2ix = 0; a2ix < Area2.Length; a2ix++)
                if (grid[Area2[a2ix]] == null)
                    for (var v = 0; v < takens[Area2[a2ix]].Length; v++)
                        if (takens[Area2[a2ix]][v])
                            takens[Area1[a2ix]][v] = true;

            return null;
        }
    }
}

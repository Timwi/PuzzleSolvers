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

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            // Determine if we know the offset
            int offset;
            for (var i = 0; i < Area1.Length; i++)
                if (grid[Area1[i]] != null && grid[Area2[i]] != null)
                {
                    offset = grid[Area2[i]].Value - grid[Area1[i]].Value;
                    goto offsetFound;
                }
            return null;
            offsetFound:

            // Mark the consequences of a value just placed
            if (ix != null)
            {
                var p1 = Array.IndexOf(Area1, ix.Value);
                var p2 = Array.IndexOf(Area2, ix.Value);
                if (p1 != -1 || p2 != -1)
                {
                    var otherIx = p1 == -1 ? Area1[p2] : Area2[p1];
                    var useOffset = p1 == -1 ? -offset : offset;
                    for (var v = 0; v < takens[otherIx].Length; v++)
                        if (v != grid[ix.Value].Value + useOffset)
                            takens[otherIx][v] = true;
                }
            }

            // Mark the consequences of other takens that were set by other constraints, and values that are simply out of range
            for (var a1ix = 0; a1ix < Area1.Length; a1ix++)
                if (grid[Area1[a1ix]] == null)
                    for (var v = 0; v < takens[Area1[a1ix]].Length; v++)
                        if (v + offset < 0 || v + offset >= takens[Area2[a1ix]].Length || takens[Area2[a1ix]][v + offset])
                            takens[Area1[a1ix]][v] = true;

            for (var a2ix = 0; a2ix < Area2.Length; a2ix++)
                if (grid[Area2[a2ix]] == null)
                    for (var v = 0; v < takens[Area2[a2ix]].Length; v++)
                        if (v - offset < 0 || v - offset >= takens[Area2[a2ix]].Length || takens[Area1[a2ix]][v - offset])
                            takens[Area2[a2ix]][v] = true;

            return null;
        }
    }
}

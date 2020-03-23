using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a composite constraint in which one of a set of constraints must be met.</summary>
    public class OrConstraint : Constraint
    {
        /// <summary>The set of constraints, of which at least one must be satisfied.</summary>
        public Constraint[] Subconstraints { get; private set; }

        /// <summary>Constructor.</summary>
        public OrConstraint(IEnumerable<Constraint> subconstraints) : base(subconstraints.SelectMany(c => c.AffectedCells).Distinct()) { Subconstraints = subconstraints.ToArray(); }

        /// <summary>Constructor.</summary>
        public OrConstraint(params Constraint[] subconstraints) : base(subconstraints.SelectMany(c => c.AffectedCells).Distinct()) { Subconstraints = subconstraints.ToArray(); }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix != null && !AffectedCells.Contains(ix.Value))
                return null;

            var takensCopies = new bool[Subconstraints.Length][][];
            for (var sc = 0; sc < Subconstraints.Length; sc++)
            {
                takensCopies[sc] = takens.Select(arr => arr.ToArray()).ToArray();
                Subconstraints[sc].MarkTakens(takensCopies[sc], grid, ix, minValue, maxValue);
            }

            foreach (var cell in AffectedCells)
                for (var v = 0; v < takens[cell].Length; v++)
                    takens[cell][v] = takens[cell][v] || takensCopies.All(tc => tc[cell][v]);

            return null;
        }
    }
}

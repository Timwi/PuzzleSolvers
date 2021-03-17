using System.Collections.Generic;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in which a specific region of cells must have a specified value as either its sum or its
    ///     product.</summary>
    public class SumOrProductConstraint : OrConstraint
    {
        /// <summary>The region of cells that must have the sum or product specified by <see cref="SumOrProduct"/>.</summary>
        public new int[] AffectedCells => base.AffectedCells;
        /// <summary>The desired sum or product.</summary>
        public int SumOrProduct { get; private set; }

        /// <summary>Constructor.</summary>
        public SumOrProductConstraint(int sumOrProduct, IEnumerable<int> affectedCells)
            : base(new SumConstraint(sumOrProduct, affectedCells), new ProductConstraint(sumOrProduct, affectedCells))
        {
            SumOrProduct = sumOrProduct;
        }
    }
}

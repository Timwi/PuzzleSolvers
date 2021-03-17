using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in which a specific region of cells must have a specified product (multiplication).</summary>
    public class ProductConstraint : Constraint
    {
        /// <summary>The region of cells that must have the product specified by <see cref="Product"/>.</summary>
        public new int[] AffectedCells => base.AffectedCells;
        /// <summary>The desired product.</summary>
        public int Product { get; private set; }

        /// <summary>Constructor.</summary>
        public ProductConstraint(int product, IEnumerable<int> affectedCells) : base(affectedCells) { Product = product; }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(SolverState state)
        {
            if (state.LastPlacedCell != null && !AffectedCells.Contains(state.LastPlacedCell.Value))
                return null;

            var productAlready = 1;
            var cellsLeftToFill = 0;
            foreach (var cell in AffectedCells)
                if (state[cell] is int value)
                    productAlready *= value;
                else
                    cellsLeftToFill++;
            if (cellsLeftToFill == 0 || (productAlready == 0 && Product == 0))
                return null;

            var alreadyBroken = productAlready == 0 || (Product % productAlready != 0);

            foreach (var cell in AffectedCells)
                state.MarkImpossible(cell, value =>
                    alreadyBroken ||
                    // The last remaining cell must have the exact required value
                    (cellsLeftToFill == 1 && productAlready * value != Product) ||
                    // The remaining cells must be factors of whatever is left to multiply
                    (cellsLeftToFill > 1 && value == 0 ? (Product != 0) : ((Product / productAlready) % value != 0)));
            return null;
        }
    }
}

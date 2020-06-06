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
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix != null && !AffectedCells.Contains(ix.Value))
                return null;

            var productAlready = 1;
            var cellsLeftToFill = 0;
            foreach (var cell in AffectedCells)
                if (grid[cell] != null)
                    productAlready *= (grid[cell].Value + minValue);
                else
                    cellsLeftToFill++;
            if (cellsLeftToFill == 0 || (productAlready == 0 && Product == 0))
                return null;

            var alreadyBroken = productAlready == 0 || (Product % productAlready != 0);

            foreach (var cell in AffectedCells)
                if (grid[cell] == null)
                    for (var v = 0; v < takens[cell].Length; v++)
                    {
                        if (alreadyBroken)
                            takens[cell][v] = true;
                        // The last remaining cell must have the exact required value
                        else if (cellsLeftToFill == 1 && productAlready * (v + minValue) != Product)
                            takens[cell][v] = true;
                        // The remaining cells must be factors of whatever is left to multiply
                        else if (cellsLeftToFill > 1 && (v + minValue) == 0 ? (Product != 0) : ((Product / productAlready) % (v + minValue) != 0))
                            takens[cell][v] = true;
                    }
            return null;
        }
    }
}

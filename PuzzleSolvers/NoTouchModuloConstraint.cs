using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement grid puzzle where the values in orthogonally adjacent cells (NOT
    ///     including diagonals) cannot have the same remainder modulo a fixed quotient.</summary>
    public class NoTouchModuloConstraint : Constraint
    {
        /// <summary>The width of the grid this constraint applies to.</summary>
        public int GridWidth { get; private set; }
        /// <summary>The height of the grid this constraint applies to.</summary>
        public int GridHeight { get; private set; }
        /// <summary>The quotient modulo which the constraint is computed.</summary>
        public int Modulo { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="width">
        ///     The width of the grid.</param>
        /// <param name="height">
        ///     The height of the grid.</param>
        /// <param name="modulo">
        ///     The quotient modulo which the constraint is computed.</param>
        public NoTouchModuloConstraint(int width, int height, int modulo) : base(Enumerable.Range(0, width * height)) { GridWidth = width; GridHeight = height; Modulo = modulo; }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix == null)
                return null;
            var cell = ix.Value;
            var x = cell % GridWidth;
            var y = cell / GridWidth;

            for (var dx = -1; dx <= 1; dx++)
                if (x + dx >= 0 && x + dx < GridWidth)
                    for (var dy = (dx == 0 ? -1 : 0); dy <= (dx == 0 ? 1 : 0); dy++)
                        if (y + dy >= 0 && y + dy < GridHeight)
                        {
                            var i = (x + dx) + GridWidth * (y + dy);
                            for (var v = 0; v < takens[i].Length; v++)
                                if (((v + minValue) % Modulo + Modulo) % Modulo == ((grid[cell].Value + minValue) % Modulo + Modulo) % Modulo)
                                    takens[i][v] = true;
                        }
            return null;
        }
    }
}

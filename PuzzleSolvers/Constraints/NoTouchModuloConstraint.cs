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
        public override IEnumerable<Constraint> MarkTakens(SolverState state)
        {
            if (state.LastPlacedCell is int cell)
            {
                var x = cell % GridWidth;
                var y = cell / GridWidth;
                var refMod = (state[cell].Value % Modulo + Modulo) % Modulo;

                for (var dx = -1; dx <= 1; dx++)
                    if (x + dx >= 0 && x + dx < GridWidth)
                        for (var dy = (dx == 0 ? -1 : 0); dy <= (dx == 0 ? 1 : 0); dy++)
                            if (y + dy >= 0 && y + dy < GridHeight)
                                state.MarkImpossible((x + dx) + GridWidth * (y + dy), v => (v % Modulo + Modulo) % Modulo == refMod);
            }
            return null;
        }
    }
}

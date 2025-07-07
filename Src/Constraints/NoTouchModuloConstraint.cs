using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement grid puzzle where the values in orthogonally adjacent cells (NOT
    ///     including diagonals) cannot have the same remainder modulo a fixed quotient.</summary>
    /// <param name="width">
    ///     The width of the grid.</param>
    /// <param name="height">
    ///     The height of the grid.</param>
    /// <param name="modulo">
    ///     The quotient modulo which the constraint is computed.</param>
    public class NoTouchModuloConstraint(int width, int height, int modulo) : Constraint(Enumerable.Range(0, width * height))
    {
        /// <summary>The width of the grid this constraint applies to.</summary>
        public int GridWidth { get; private set; } = width;
        /// <summary>The height of the grid this constraint applies to.</summary>
        public int GridHeight { get; private set; } = height;
        /// <summary>The quotient modulo which the constraint is computed.</summary>
        public int Modulo { get; private set; } = modulo;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
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

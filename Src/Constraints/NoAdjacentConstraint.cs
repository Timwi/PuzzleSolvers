using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Represents a constraint in a number-placement puzzle that prohibits any of the <paramref name="values"/> from
    ///     appearing adjacent to one another.</summary>
    /// <param name="width">
    ///     The width of the puzzle grid.</param>
    /// <param name="height">
    ///     The height of the puzzle grid.</param>
    /// <param name="values">
    ///     The set of values that are not allowed to appear adjacent to one another.</param>
    public class NoAdjacentConstraint(int width, int height, params int[] values) : Constraint(null)
    {
        /// <summary>The width of the puzzle grid.</summary>
        public int Width { get; private set; } = width;
        /// <summary>The height of the puzzle grid.</summary>
        public int Height { get; private set; } = height;
        /// <summary>The set of values that are not allowed to appear adjacent to one another.</summary>
        public int[] Values { get; private set; } = values;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            if (state.LastPlacedCell == null)
                return null;
            if (!Values.Contains(state.LastPlacedValue))
                return null;

            var cell = state.LastPlacedCell.Value;
            var x = cell % Width;
            var y = cell / Width;
            if (x > 0)
                state.MarkImpossible(cell - 1, Values.Contains);
            if (x < Width - 1)
                state.MarkImpossible(cell + 1, Values.Contains);
            if (y > 0)
                state.MarkImpossible(cell - Width, Values.Contains);
            if (y < Height - 1)
                state.MarkImpossible(cell + Width, Values.Contains);
            return null;
        }
    }
}

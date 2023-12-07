using System.Linq;

namespace PuzzleSolvers
{
    public class NoAdjacentConstraint(int width, int height, params int[] values) : Constraint(null)
    {
        public int Width { get; private set; } = width;
        public int Height { get; private set; } = height;
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

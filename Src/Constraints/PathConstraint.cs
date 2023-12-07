namespace PuzzleSolvers
{
    /// <summary>
    ///     Represents a constraint in a path puzzle such as <see cref="Masyu"/> that mandates that lines must join up into a
    ///     path (the path cannot have a dangling end within the grid). Paths can still exit the grid at the edge.</summary>
    /// <param name="width">
    ///     Width of the whole puzzle grid.</param>
    /// <param name="height">
    ///     Height of the whole puzzle grid.</param>
    public class PathConstraint(int width, int height) : Constraint(null)
    {
        /// <summary>Specifies the width of the puzzle grid.</summary>
        public int Width { get; private set; } = width;
        /// <summary>Specifies the height of the puzzle grid.</summary>
        public int Height { get; private set; } = height;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            if (state.LastPlacedCell == null)
            {
                // Make sure that a line can’t crash into the edge of the grid
                for (var xx = 0; xx < Width; xx++)
                {
                    state.MarkImpossible(xx + 0 * Width, v => v < 7 && (Path.ToBits[v] & 1) != 0);
                    state.MarkImpossible(xx + (Height - 1) * Width, v => v < 7 && (Path.ToBits[v] & 4) != 0);
                }
                for (var yy = 0; yy < Height; yy++)
                {
                    state.MarkImpossible(0 + yy * Width, v => v < 7 && (Path.ToBits[v] & 8) != 0);
                    state.MarkImpossible(Width - 1 + yy * Width, v => v < 7 && (Path.ToBits[v] & 2) != 0);
                }
                return null;
            }

            // Make sure that a line can’t stop in its tracks (lines in adjacent cells must join up)
            if (state.LastPlacedCell.Value % Width > 0)
                state.MarkImpossible(state.LastPlacedCell.Value - 1, v => (v < 7 && (Path.ToBits[v] & 2) != 0) != (state.LastPlacedValue < 7 && (Path.ToBits[state.LastPlacedValue] & 8) != 0));
            if (state.LastPlacedCell.Value % Width < Width - 1)
                state.MarkImpossible(state.LastPlacedCell.Value + 1, v => (v < 7 && (Path.ToBits[v] & 8) != 0) != (state.LastPlacedValue < 7 && (Path.ToBits[state.LastPlacedValue] & 2) != 0));
            if (state.LastPlacedCell.Value / Width > 0)
                state.MarkImpossible(state.LastPlacedCell.Value - Width, v => (v < 7 && (Path.ToBits[v] & 4) != 0) != (state.LastPlacedValue < 7 && (Path.ToBits[state.LastPlacedValue] & 1) != 0));
            if (state.LastPlacedCell.Value / Width < Height - 1)
                state.MarkImpossible(state.LastPlacedCell.Value + Width, v => (v < 7 && (Path.ToBits[v] & 1) != 0) != (state.LastPlacedValue < 7 && (Path.ToBits[state.LastPlacedValue] & 4) != 0));

            return null;
        }
    }
}

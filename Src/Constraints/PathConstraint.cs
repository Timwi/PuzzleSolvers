namespace PuzzleSolvers
{
    /// <summary>
    ///     Represents a constraint in a path puzzle such as <see cref="Masyu"/>, <see cref="Numberlink"/> or <see
    ///     cref="Yajilin"/> that mandates that lines must join up into a path (the path cannot have a dangling end within the
    ///     grid). The values 0–6 are used to represent pieces of a path (see <see cref="Path"/> for constants to help deal
    ///     with these values). Paths can still exit the grid at the edge, or join up with a value greater than 6.</summary>
    /// <param name="width">
    ///     Width of the whole puzzle grid.</param>
    /// <param name="height">
    ///     Height of the whole puzzle grid.</param>
    /// <param name="canHitNonPathCells">
    ///     Determines whether a path is allowed to “hit” a non-path cell (as in <see cref="Numberlink"/>) or whether the path
    ///     must navigate around all non-path cells (as in <see cref="Yajilin"/>).</param>
    public class PathConstraint(int width, int height, bool canHitNonPathCells) : Constraint(null)
    {
        /// <summary>Specifies the width of the puzzle grid.</summary>
        public int Width { get; private set; } = width;
        /// <summary>Specifies the height of the puzzle grid.</summary>
        public int Height { get; private set; } = height;
        /// <summary>
        ///     Determines whether a path is allowed to “hit” a non-path cell (as in <see cref="Numberlink"/>) or whether the
        ///     path must navigate around all non-path cells (as in <see cref="Yajilin"/>).</summary>
        public bool CanHitNonPathCells { get; private set; } = canHitNonPathCells;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            if (state.LastPlacedCell == null)
                return null;

            // Make sure that a line can’t stop in its tracks (lines in adjacent cells must join up)
            if (CanHitNonPathCells)
            {
                if (state.LastPlacedValue < 7)
                {
                    if (state.LastPlacedCell.Value % Width > 0)
                        state.MarkImpossible(state.LastPlacedCell.Value - 1, v => v < 7 && ((Path.ToBits[v] & 2) != 0) != ((Path.ToBits[state.LastPlacedValue] & 8) != 0));
                    if (state.LastPlacedCell.Value % Width < Width - 1)
                        state.MarkImpossible(state.LastPlacedCell.Value + 1, v => v < 7 && ((Path.ToBits[v] & 8) != 0) != ((Path.ToBits[state.LastPlacedValue] & 2) != 0));
                    if (state.LastPlacedCell.Value / Width > 0)
                        state.MarkImpossible(state.LastPlacedCell.Value - Width, v => v < 7 && ((Path.ToBits[v] & 4) != 0) != ((Path.ToBits[state.LastPlacedValue] & 1) != 0));
                    if (state.LastPlacedCell.Value / Width < Height - 1)
                        state.MarkImpossible(state.LastPlacedCell.Value + Width, v => v < 7 && ((Path.ToBits[v] & 1) != 0) != ((Path.ToBits[state.LastPlacedValue] & 4) != 0));
                }
            }
            else
            {
                if (state.LastPlacedCell.Value % Width > 0)
                    state.MarkImpossible(state.LastPlacedCell.Value - 1, v => (v < 7 && (Path.ToBits[v] & 2) != 0) != (state.LastPlacedValue < 7 && (Path.ToBits[state.LastPlacedValue] & 8) != 0));
                if (state.LastPlacedCell.Value % Width < Width - 1)
                    state.MarkImpossible(state.LastPlacedCell.Value + 1, v => (v < 7 && (Path.ToBits[v] & 8) != 0) != (state.LastPlacedValue < 7 && (Path.ToBits[state.LastPlacedValue] & 2) != 0));
                if (state.LastPlacedCell.Value / Width > 0)
                    state.MarkImpossible(state.LastPlacedCell.Value - Width, v => (v < 7 && (Path.ToBits[v] & 4) != 0) != (state.LastPlacedValue < 7 && (Path.ToBits[state.LastPlacedValue] & 1) != 0));
                if (state.LastPlacedCell.Value / Width < Height - 1)
                    state.MarkImpossible(state.LastPlacedCell.Value + Width, v => (v < 7 && (Path.ToBits[v] & 1) != 0) != (state.LastPlacedValue < 7 && (Path.ToBits[state.LastPlacedValue] & 4) != 0));
            }

            return null;
        }
    }
}

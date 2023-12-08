using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Represents a constraint in a path puzzle such as <see cref="Masyu"/> that mandates that the path must form a
    ///     single connected loop.</summary>
    /// <param name="width">
    ///     Width of the whole puzzle grid.</param>
    /// <param name="height">
    ///     Height of the whole puzzle grid.</param>
    public class SingleLoopConstraint(int width, int height) : PathConstraint(width, height, false)
    {
        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            var r = base.Process(state);

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
                return r;
            }

            if (!state.LastPlacedValue.IsBetween(1, 6))
                return r;

            // Ensure there’s only one loop. Follow the line we just placed; if it leads us back to the current cell,
            // we’ve completed a loop, so set the entire rest of the puzzle to 0s to ensure no other loops are made.
            var cell = state.LastPlacedCell.Value;
            var x = cell % Width;
            var y = cell / Width;
            var dir = -1;
            while (true)
            {
                var newDir = Enumerable.Range(0, 4).First(d => d != dir && (Path.ToBits[state[x + Width * y].Value] & (1 << d)) != 0);
                x += PuzzleUtil.Dxs[newDir];
                y += PuzzleUtil.Dys[newDir];
                if (state[x + Width * y] == null)
                    break;
                if (x + Width * y == cell)
                {
                    for (var i = 0; i < Width * Height; i++)
                        if (state[i] == null)
                            state.MustBe(i, 0);
                    break;
                }
                dir = (newDir + 2) % 4;
            }
            return r;
        }
    }
}

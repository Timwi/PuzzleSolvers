using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Represents a constraint in a path puzzle such as <see cref="Masyu"/> that mandates that the path must form a
    ///     single connected loop.</summary>
    public class SingleLoopConstraint : PathConstraint
    {
        /// <summary>
        ///     Constructor.</summary>
        /// <param name="width">
        ///     Width of the whole puzzle grid.</param>
        /// <param name="height">
        ///     Height of the whole puzzle grid.</param>
        public SingleLoopConstraint(int width, int height) : base(width, height) { }

        private static readonly int[] dxs = [0, 1, 0, -1];
        private static readonly int[] dys = [-1, 0, 1, 0];

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            var r = base.Process(state);

            if (state.LastPlacedCell == null || state.LastPlacedValue == 0)
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
                x += dxs[newDir];
                y += dys[newDir];
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

using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint that prevents a specified set of <paramref name="values"/> from forming a 2×2 area within a
    ///     2D grid.</summary>
    /// <param name="width">
    ///     Width of the puzzle grid.</param>
    /// <param name="height">
    ///     Height of the puzzle grid.</param>
    /// <param name="values">
    ///     The set of values that cannot form a 2×2 area.</param>
    public class No2x2sConstraint(int width, int height, params int[] values) : Constraint(null)
    {
        /// <summary>Width of the puzzle grid.</summary>
        public int Width { get; private set; } = width;
        /// <summary>Height of the puzzle grid.</summary>
        public int Height { get; private set; } = height;
        /// <summary>The set of values that cannot form a 2×2 area.</summary>
        public int[] Values { get; private set; } = values;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            for (var topLeft = 0; topLeft < Width * Height; topLeft++)
                if (topLeft % Width < Width - 1 && topLeft / Width < Height - 1)
                {
                    var cells = new[] { topLeft, topLeft + 1, topLeft + Width, topLeft + Width + 1 };
                    var still = cells.Where(c => !state.AllSame(c, Values.Contains, out bool b) || !b).ToArray();
                    if (still.Length == 0)
                        return ConstraintResult.Violation;
                    if (still.Length > 1)
                        continue;
                    state.MarkImpossible(still[0], Values.Contains);
                }
            return null;
        }
    }
}

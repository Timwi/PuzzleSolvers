using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint that will solve a Kyudoku puzzle given a 6×6 grid of numbers 1–9. The solution will contain
    ///     a 0 for marked cells and a 1 for struck-out cells.</summary>
    public class Kyudoku6x6Constraint : Constraint
    {
        /// <summary>Initial grid of numbers to operate on.</summary>
        public int[] NumberGrid { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="numberGrid">
        ///     See <see cref="NumberGrid"/>.</param>
        public Kyudoku6x6Constraint(int[] numberGrid) : base(Enumerable.Range(0, 36))
        {
            if (numberGrid == null)
                throw new ArgumentNullException(nameof(numberGrid));
            if (numberGrid.Length != 36)
                throw new ArgumentException("‘numberGrid’ must have exactly 36 elements.", nameof(numberGrid));
            NumberGrid = numberGrid;
        }

        /// <summary>See <see cref="Constraint.Process(SolverState)"/>.</summary>
        public override ConstraintResult Process(SolverState state)
        {
            if (state.GridSize != 36)
                throw new NotImplementedException("Kyudoku6x6Constraint only works on 6×6 grids.");
            if (state.MinValue != 0 || state.MaxValue != 1)
                throw new NotImplementedException("Kyudoku6x6Constraint requires ‘MinValue’ to be 0 and ‘MaxValue’ to be 1.");

            // Check if any cell is now the only occurrence of a specific digit
            for (var v = 1; v <= 9; v++)
            {
                var cnt = Enumerable.Range(0, 36).Where(i => state[i] != 1 && NumberGrid[i] == v).ToArray();
                if (cnt.Length == 1)
                    state.MarkImpossible(cnt[0], 1);
            }

            if (state.LastPlacedCell != null && state.LastPlacedValue != 1)
            {
                // Shade all other cells of the same value
                for (var i = 0; i < 36; i++)
                    if (i != state.LastPlacedCell.Value && NumberGrid[i] == NumberGrid[state.LastPlacedCell.Value])
                        state.MarkImpossible(i, 0);

                // Shade all cells in the same row/column that would take the total above 9
                {
                    var row = state.LastPlacedCell.Value / 6;
                    var rowTotal = Enumerable.Range(0, 6).Select(col => state[6 * row + col] == 0 ? NumberGrid[6 * row + col] : 0).Sum();
                    for (var col = 0; col < 6; col++)
                        if (state[6 * row + col] == null && rowTotal + NumberGrid[6 * row + col] > 9)
                            state.MarkImpossible(6 * row + col, 0);
                }
                {
                    var col = state.LastPlacedCell.Value % 6;
                    var colTotal = Enumerable.Range(0, 6).Select(row => state[6 * row + col] == 0 ? NumberGrid[6 * row + col] : 0).Sum();
                    for (var row = 0; row < 6; row++)
                        if (state[6 * row + col] == null && colTotal + NumberGrid[6 * row + col] > 9)
                            state.MarkImpossible(6 * row + col, 0);
                }
            }
            return null;
        }
    }
}

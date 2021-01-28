using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint that will solve a Kyudoku puzzle given a 6×6 grid of numbers 1–9. The solution will contain
    ///     a 0 for marked cells and a 1 for struck cells.</summary>
    public sealed class Kyudoku6x6Constraint : Constraint
    {
        /// <summary>Initial grid of numbers to operate on.</summary>
        public int[] NumberGrid { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="numberGrid">
        ///     See <see cref="NumberGrid"/>.</param>
        public Kyudoku6x6Constraint(int[] numberGrid) : base(Enumerable.Range(0, 36))
        {
            NumberGrid = numberGrid;
        }

        /// <summary>See <see cref="Constraint.MarkTakens(bool[][], int?[], int?, int, int)"/>.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix == null)
            {
                // Check if another cell is now the only occurrence of a specific digit
                for (var v = 1; v <= 9; v++)
                {
                    var cnt = Enumerable.Range(0, 36).Where(i => grid[i] != 1 && NumberGrid[i] == v).ToArray();
                    if (cnt.Length == 1)
                        takens[cnt[0]][1] = true;
                }
                return null;
            }

            var isShaded = grid[ix.Value].Value == 1;
            if (isShaded)
            {
                // Check if another cell is now the only occurrence of a specific digit
                for (var v = 1; v <= 9; v++)
                {
                    var cnt = Enumerable.Range(0, 36).Where(i => grid[i] != 1 && NumberGrid[i] == v).ToArray();
                    if (cnt.Length == 1)
                        takens[cnt[0]][1] = true;
                }
            }
            else
            {
                // Shade all other cells of the same value
                for (var i = 0; i < 36; i++)
                    if (i != ix.Value && NumberGrid[i] == NumberGrid[ix.Value])
                        takens[i][0] = true;

                // Shade all cells in the same row/column that would take the total above 9
                {
                    var row = ix.Value / 6;
                    var rowTotal = Enumerable.Range(0, 6).Select(col => grid[6 * row + col] == 0 ? NumberGrid[6 * row + col] : 0).Sum();
                    for (var col = 0; col < 6; col++)
                        if (grid[6 * row + col] == null && rowTotal + NumberGrid[6 * row + col] > 9)
                            takens[6 * row + col][0] = true;
                }
                {
                    var col = ix.Value % 6;
                    var colTotal = Enumerable.Range(0, 6).Select(row => grid[6 * row + col] == 0 ? NumberGrid[6 * row + col] : 0).Sum();
                    for (var row = 0; row < 6; row++)
                        if (grid[6 * row + col] == null && colTotal + NumberGrid[6 * row + col] > 9)
                            takens[6 * row + col][0] = true;
                }
            }
            return null;
        }
    }
}

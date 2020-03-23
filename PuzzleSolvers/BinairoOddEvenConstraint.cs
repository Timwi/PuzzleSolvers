using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint that mandates that the parity (odd/evenness) of the values must form a valid Binairo.</summary>
    /// <remarks>
    ///     This constraint only works if it covers the entire grid. The grid must be square and have even sidelengths.</remarks>
    public class BinairoOddEvenConstraint : BinairoOddEvenConstraintWithoutUniqueness
    {
        /// <summary>Constructor.</summary>
        public BinairoOddEvenConstraint(int size) : base(size) { }

        /// <summary>Implements the additional Binairo constraint that each row and column must be unique.</summary>
        protected override void AdditionalDeductions(bool[][] takens, int?[] grid, int x, int y, int minValue, int maxValue)
        {
            var numNullsInColumn = Enumerable.Range(0, Size).Count(row => grid[x + Size * row] == null);
            if (numNullsInColumn < 2)
            {
                // We just placed the last or second-last value in this column. 
                // If it’s the last, we need to make sure that no almost-complete column is about to be filled with the same parities.
                // If it’s the second-last, we need to make sure that THIS column isn’t going to be filled with the same parities as another equal column.
                for (var col = 0; col < Size; col++)
                {
                    if (col != x)
                    {
                        var discrepantRow = -1;
                        for (var row = 0; row < Size; row++)
                        {
                            if (grid[(numNullsInColumn == 0 ? col : x) + Size * row] == null && grid[(numNullsInColumn == 0 ? x : col) + Size * row] != null)
                            {
                                if (discrepantRow == -1)
                                    discrepantRow = row;
                                else
                                    goto nextColumn;
                            }
                            else if (grid[col + Size * row] == null || grid[col + Size * row].Value % 2 != grid[x + Size * row].Value % 2)
                                goto nextColumn;
                        }
                        for (var v = 0; v < takens[(numNullsInColumn == 0 ? col : x) + Size * discrepantRow].Length; v++)
                            if ((v + minValue) % 2 == grid[(numNullsInColumn == 0 ? x : col) + Size * discrepantRow].Value % 2)
                                takens[(numNullsInColumn == 0 ? col : x) + Size * discrepantRow][v] = true;
                    }
                    nextColumn:;
                }
            }

            var numNullsInRow = Enumerable.Range(0, Size).Count(col => grid[col + Size * y] == null);
            if (numNullsInRow < 2)
            {
                // See comment above for columns
                for (var row = 0; row < Size; row++)
                {
                    if (row != y)
                    {
                        var discrepantCol = -1;
                        for (var col = 0; col < Size; col++)
                        {
                            if (grid[col + Size * (numNullsInRow == 0 ? row : y)] == null && grid[col + Size * (numNullsInRow == 0 ? y : row)] != null)
                            {
                                if (discrepantCol == -1)
                                    discrepantCol = col;
                                else
                                    goto nextRow;
                            }
                            else if (grid[col + Size * row] == null || grid[col + Size * row].Value % 2 != grid[col + Size * y].Value % 2)
                                goto nextRow;
                        }
                        for (var v = 0; v < takens[discrepantCol + Size * (numNullsInRow == 0 ? row : y)].Length; v++)
                            if ((v + minValue) % 2 == grid[discrepantCol + Size * (numNullsInRow == 0 ? y : row)].Value % 2)
                                takens[discrepantCol + Size * (numNullsInRow == 0 ? row : y)][v] = true;
                    }
                    nextRow:;
                }
            }
        }
    }
}

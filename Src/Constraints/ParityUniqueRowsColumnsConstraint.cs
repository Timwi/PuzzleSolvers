using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement puzzle that mandates that the parities (odd/evenness) of the values
    ///     must form different patterns in every row and column.</summary>
    /// <remarks>
    ///     This constraint only works if it covers the entire grid.</remarks>
    public class ParityUniqueRowsColumnsConstraint : Constraint
    {
        /// <summary>The side length of the square grid.</summary>
        public int SideLength { get; private set; }

        /// <summary>Constructor.</summary>
        public ParityUniqueRowsColumnsConstraint(int sideLength, IEnumerable<int> affectedCells = null) : base(affectedCells)
        {
            if (AffectedCells != null && AffectedCells.Length != sideLength * sideLength)
                throw new ArgumentException("ParityUniqueRowsColumnsConstraint: The number of affected cells must be equal to the square of the side length.");
            SideLength = sideLength;
        }

        /// <summary>Override; see base.</summary>
        public override ConstraintResult Process(SolverState state)
        {
            if (state.LastPlacedCell is int cell)
            {
                var innerCell = AffectedCells == null ? cell : AffectedCells.IndexOf(cell);
                var x = innerCell % SideLength;
                var y = innerCell / SideLength;
                var numUnknownsInColumn = Enumerable.Range(0, SideLength).Count(row => state[coord(x, row)] == null);
                if (numUnknownsInColumn < 2)
                {
                    // We just placed the last or second-last value in this column. 
                    // If it’s the last, we need to make sure that no almost-complete column is about to be filled with the same parities.
                    // If it’s the second-last, we need to make sure that THIS column isn’t going to be filled with the same parities as another equal column.
                    for (var col = 0; col < SideLength; col++)
                    {
                        if (col == x)
                            continue;

                        var discrepantRow = -1;
                        for (var row = 0; row < SideLength; row++)
                        {
                            if (state[coord(numUnknownsInColumn == 0 ? col : x, row)] == null && state[coord(numUnknownsInColumn == 0 ? x : col, row)] != null)
                            {
                                if (discrepantRow == -1)
                                    discrepantRow = row;
                                else
                                    goto nextColumn;
                            }
                            else if (state[coord(col, row)] == null || state[coord(col, row)].Value % 2 != state[coord(x, row)].Value % 2)
                                goto nextColumn;
                        }
                        for (var v = state.MinValue; v <= state.MaxValue; v++)
                            if (v % 2 == state[coord(numUnknownsInColumn == 0 ? x : col, discrepantRow)].Value % 2)
                                state.MarkImpossible(coord(numUnknownsInColumn == 0 ? col : x, discrepantRow), v);
                        nextColumn:;
                    }
                }

                var numUnknownsInRow = Enumerable.Range(0, SideLength).Count(col => state[coord(col, y)] == null);
                if (numUnknownsInRow < 2)
                {
                    // See comment above for columns
                    for (var row = 0; row < SideLength; row++)
                    {
                        if (row == y)
                            continue;

                        var discrepantCol = -1;
                        for (var col = 0; col < SideLength; col++)
                        {
                            if (state[coord(col, numUnknownsInRow == 0 ? row : y)] == null && state[coord(col, numUnknownsInRow == 0 ? y : row)] != null)
                            {
                                if (discrepantCol == -1)
                                    discrepantCol = col;
                                else
                                    goto nextRow;
                            }
                            else if (state[coord(col, row)] == null || state[coord(col, row)].Value % 2 != state[coord(col, y)].Value % 2)
                                goto nextRow;
                        }
                        for (var v = state.MinValue; v <= state.MaxValue; v++)
                            if (v % 2 == state[coord(discrepantCol, numUnknownsInRow == 0 ? y : row)].Value % 2)
                                state.MarkImpossible(coord(discrepantCol, numUnknownsInRow == 0 ? row : y), v);
                        nextRow:;
                    }
                }
            }
            return null;
        }

        private int coord(int col, int row) => AffectedCells == null ? col + SideLength * row : AffectedCells[col + SideLength * row];
    }
}

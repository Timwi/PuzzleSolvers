using System.Collections.Generic;
using RT.Util;
using RT.Util.ExtensionMethods;
using System.Linq;

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
        public ParityUniqueRowsColumnsConstraint(int sideLength) : base(Enumerable.Range(0, sideLength * sideLength))
        {
            SideLength = sideLength;
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(SolverState state)
        {
            if (state.LastPlacedCell is int cell)
            {
                var x = cell % SideLength;
                var y = cell / SideLength;
                var numUnknownsInColumn = Enumerable.Range(0, SideLength).Count(row => state[x + SideLength * row] == null);
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
                            if (state[(numUnknownsInColumn == 0 ? col : x) + SideLength * row] == null && state[(numUnknownsInColumn == 0 ? x : col) + SideLength * row] != null)
                            {
                                if (discrepantRow == -1)
                                    discrepantRow = row;
                                else
                                    goto nextColumn;
                            }
                            else if (state[col + SideLength * row] == null || state[col + SideLength * row].Value % 2 != state[x + SideLength * row].Value % 2)
                                goto nextColumn;
                        }
                        for (var v = state.MinValue; v <= state.MaxValue; v++)
                            if (v % 2 == state[(numUnknownsInColumn == 0 ? x : col) + SideLength * discrepantRow].Value % 2)
                                state.MarkImpossible((numUnknownsInColumn == 0 ? col : x) + SideLength * discrepantRow, v);
                        nextColumn:;
                    }
                }

                var numUnknownsInRow = Enumerable.Range(0, SideLength).Count(col => state[col + SideLength * y] == null);
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
                            if (state[col + SideLength * (numUnknownsInRow == 0 ? row : y)] == null && state[col + SideLength * (numUnknownsInRow == 0 ? y : row)] != null)
                            {
                                if (discrepantCol == -1)
                                    discrepantCol = col;
                                else
                                    goto nextRow;
                            }
                            else if (state[col + SideLength * row] == null || state[col + SideLength * row].Value % 2 != state[col + SideLength * y].Value % 2)
                                goto nextRow;
                        }
                        for (var v = state.MinValue; v <= state.MaxValue; v++)
                            if (v % 2 == state[discrepantCol + SideLength * (numUnknownsInRow == 0 ? y : row)].Value % 2)
                                state.MarkImpossible(discrepantCol + SideLength * (numUnknownsInRow == 0 ? row : y), v);
                        nextRow:;
                    }
                }
            }
            return null;
        }
    }
}

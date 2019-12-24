using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement puzzle (such as a Thermometer Sudoku) where a series of cells must be
    ///     in ascending order.</summary>
    public class LessThanConstraint : Constraint
    {
        /// <summary>
        ///     Contains the region of cells affected by this constraint (the “thermometer”). The order of cells in this array
        ///     is significant as the constraint assumes these to be ordered from smallest to largest value.</summary>
        public new int[] AffectedCells => base.AffectedCells;

        /// <summary>
        ///     Specifies an optional background color to be used when outputting a solution using <see
        ///     cref="Puzzle.SudokuSolutionToConsoleString(int[], int)"/>.</summary>
        public ConsoleColor? BackgroundColor { get; private set; }

        /// <summary>Constructor.</summary>
        public LessThanConstraint(IEnumerable<int> affectedCells, ConsoleColor? backgroundColor = null) : base(affectedCells)
        {
            BackgroundColor = backgroundColor;
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            // At the start, mark cells along the sequence. For example, the second cell can’t be 1, the third can’t be 1 or 2, etc.
            if (ix == null)
            {
                var min = minValue;
                var max = maxValue - AffectedCells.Length + 1;
                for (var q = 0; q < AffectedCells.Length; q++)
                {
                    for (var v = 0; v < takens[AffectedCells[q]].Length; v++)
                        if (v + minValue < min || v + minValue > max)
                            takens[AffectedCells[q]][v] = true;
                    min++;
                    max++;
                }
            }

            // Also make sure that all the values in the grid are considered.
            for (var p = 0; p < AffectedCells.Length; p++)
            {
                // Consider all placed values only if ix == null (performance optimization).
                if ((ix != null && AffectedCells[p] != ix.Value) || grid[AffectedCells[p]] == null)
                    continue;

                var val = grid[AffectedCells[p]].Value;
                for (var q = 0; q < AffectedCells.Length; q++)
                    for (var v = 0; v < takens[AffectedCells[q]].Length; v++)
                        if ((q < p && v > val - p + q) || (q > p && v < val - p + q))
                            takens[AffectedCells[q]][v] = true;
            }
            return null;
        }

        /// <summary>Override; see base;</summary>
        public override ConsoleColor? CellBackgroundColor(int ix) => AffectedCells.Contains(ix) ? BackgroundColor : null;
    }
}

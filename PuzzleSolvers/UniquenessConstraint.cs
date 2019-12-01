using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Describes a constraint that mandates that a region of cells must have different values.</summary>
    public class UniquenessConstraint : Constraint
    {
        /// <summary>The cells that must have different values.</summary>
        public int[] AffectedCells { get; private set; }

        /// <summary>
        ///     An optional background color to be used when outputting a solution through <see
        ///     cref="Puzzle.SudokuSolutionToConsoleString(int[], int)"/>.</summary>
        public ConsoleColor? BackgroundColor { get; private set; }

        /// <summary>Constructor.</summary>
        public UniquenessConstraint(IEnumerable<int> affectedCells, ConsoleColor? backgroundColor = null)
        {
            AffectedCells = affectedCells.ToArray();
            BackgroundColor = backgroundColor;
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix != null)
            {
                if (!AffectedCells.Contains(ix.Value))
                    return null;

                foreach (var cell in AffectedCells)
                    if (cell != ix.Value)
                        takens[cell][grid[ix.Value].Value] = true;
            }
            else
            {
                var values = ix == null ? AffectedCells.Where(cell => grid[cell] != null).Select(cell => grid[cell].Value).ToHashSet() : null;
                if (values.Count > 0)
                    foreach (var cell in AffectedCells)
                        for (var v = 0; v < takens[cell].Length; v++)
                            if (values.Contains(v))
                                takens[cell][v] = true;
            }
            return null;
        }

        /// <summary>Override; see base.</summary>
        public override ConsoleColor? CellBackgroundColor(int ix) => AffectedCells.Contains(ix) ? BackgroundColor : null;
    }
}

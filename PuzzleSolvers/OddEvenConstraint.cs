using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Describes a constraint in which a group of cells can be only all evens or all odds.</summary>
    public sealed class OddEvenConstraint : Constraint
    {
        /// <summary>The group of cells affected by this constraint.</summary>
        public int[] AffectedCells { get; private set; }

        /// <summary>Specifies the specific flavor of this constraint.</summary>
        public OddEvenType Type { get; private set; }

        /// <summary>
        ///     An optional background color to be used when outputting a solution through <see
        ///     cref="Puzzle.SudokuSolutionToConsoleString(int[], int)"/>.</summary>
        public ConsoleColor? BackgroundColor { get; private set; }

        /// <summary>Constructor.</summary>
        public OddEvenConstraint(OddEvenType type, IEnumerable<int> affectedCells, ConsoleColor? backgroundColor = null)
        {
            Type = type;
            AffectedCells = affectedCells.ToArray();
            BackgroundColor = backgroundColor;
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            int req;

            // Do we know the parity yet?
            switch (Type)
            {
                case OddEvenType.AllEven:
                    req = 0;
                    break;

                case OddEvenType.AllOdd:
                    req = 1;
                    break;

                case OddEvenType.AllSame:
                    // If the algorithm placed a value outside of our affected group, we can’t do anything
                    if (ix != null && !AffectedCells.Contains(ix.Value))
                        return null;

                    if (ix == null)
                    {
                        // If no value within our affected group has yet been placed, we also can’t do anything
                        var affIx = AffectedCells.IndexOf(cell => grid[cell] != null);
                        if (affIx == -1)
                            return null;
                        ix = AffectedCells[affIx];
                    }

                    // We have a value — use its parity
                    req = (grid[ix.Value].Value + minValue) % 2;
                    break;

                default:
                    throw new InvalidOperationException(string.Format(@"OddEvenConstraint.Type has unknown value: {0}", Type));
            }

            // Mark all the cells of the wrong parity as taken. After this, we don’t need the constraint anymore.
            foreach (var cell in AffectedCells)
                for (var v = 0; v < takens[cell].Length; v++)
                    if ((v + minValue) % 2 != req)
                        takens[cell][v] = true;
            return Enumerable.Empty<Constraint>();
        }

        /// <summary>Override; see base.</summary>
        public override ConsoleColor? CellBackgroundColor(int ix) => AffectedCells.Contains(ix) ? BackgroundColor : null;
    }

    /// <summary>Describes the specific flavor of an <see cref="OddEvenConstraint"/>.</summary>
    public enum OddEvenType
    {
        /// <summary>All of the affected cells must be even.</summary>
        AllEven,
        /// <summary>All of the affected cells must be odd.</summary>
        AllOdd,
        /// <summary>All of the affected cells must have the same parity, but it is not initially specified which.</summary>
        AllSame
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    public abstract class Constraint
    {
        public abstract void MarkInitialTakens(bool[][] takens, int minValue, int maxValue);
        public abstract IEnumerable<Constraint> MarkTaken(bool[][] takens, int?[] grid, int ix, int val, int minValue, int maxValue);
        public virtual ConsoleColor? CellColor(int ix) => null;

        public virtual ConsoleColor? CellBackgroundColor(int ix) => null;

        public static IEnumerable<Constraint> Givens(int?[] givens)
        {
            if (givens == null)
                throw new ArgumentNullException(nameof(givens));

            for (var i = 0; i < givens.Length; i++)
            {
                if (givens[i] == null)
                    continue;

                if (givens[i] >= 1 && givens[i] <= 9)
                    yield return new GivenConstraint { Location = i, Value = givens[i].Value };
                else
                    throw new InvalidOperationException(@"The given constraints must be between 1–9 or null to signify no given.");
            }
        }

        public static IEnumerable<Constraint> LatinSquare()
        {
            // Rows
            for (var row = 0; row < 9; row++)
                yield return new UniquenessConstraint { AffectedCells = Enumerable.Range(0, 9).Select(col => row * 9 + col).ToArray() };

            // Columns
            for (var col = 0; col < 9; col++)
                yield return new UniquenessConstraint { AffectedCells = Enumerable.Range(0, 9).Select(row => row * 9 + col).ToArray() };
        }

        public static IEnumerable<Constraint> Sudoku()
        {
            foreach (var c in LatinSquare())
                yield return c;

            // 3×3 regions
            for (var x = 0; x < 3; x++)
                for (var y = 0; y < 3; y++)
                    yield return new UniquenessConstraint { AffectedCells = Enumerable.Range(0, 9).Select(i => i % 3 + 3 * x + 9 * (i / 3 + 3 * y)).ToArray() };
        }

        public static IEnumerable<Constraint> KillerCage(int sum, int[] affectedCells, ConsoleColor? backgroundColor = null)
        {
            yield return new SumConstraint { Sum = sum, AffectedCells = affectedCells };
            yield return new UniquenessConstraint { AffectedCells = affectedCells, BackgroundColor = backgroundColor };
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint that mandates that the parity (odd/evenness) of the values cannot form triplets in a
    ///     row/column. This is a subset of the rules of Binairo.</summary>
    /// <remarks>
    ///     This constraint only works if it covers the entire grid. The grid must be square and have even sidelengths.</remarks>
    public class BinairoOddEvenConstraintWithoutUniqueness : Constraint
    {
        /// <summary>All the cells of the grid.</summary>
        public new int[] AffectedCells => base.AffectedCells;

        /// <summary>Specifies the size of the Binairo grid.</summary>
        public int Size { get; private set; }

        /// <summary>Constructor.</summary>
        public BinairoOddEvenConstraintWithoutUniqueness(int size) : base(Enumerable.Range(0, size * size))
        {
            if (size % 2 == 1)
                throw new ArgumentException("The size of a Binairo grid must be even.", nameof(size));
            Size = size;
        }

        /// <summary>Combinations of existing values that necessitate a constraint enforcement</summary>
        private static readonly (int offset, int toEnforce)[] _combinations = new (int offset, int toEnforce)[] { (-2, -1), (-1, -2), (-1, 1), (1, -1), (1, 2), (2, 1) };
        private static readonly int[] _zeroAndOne = new[] { 0, 1 };

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix == null)
                return null;

            var i = ix.Value;
            var x = i % Size;
            var y = i / Size;

            PreventThreeInARow(takens, grid, minValue, i, x, y);
            EnsureEqualNumbersOfEvensAndOdds(takens, grid, minValue, x, y);
            AdditionalDeductions(takens, grid, x, y, minValue, maxValue);
            return null;
        }

        /// <summary>Allows derived classes to perform additional deductions.</summary>
        protected virtual void AdditionalDeductions(bool[][] takens, int?[] grid, int x, int y, int minValue, int maxValue) { }

        private void EnsureEqualNumbersOfEvensAndOdds(bool[][] takens, int?[] grid, int minValue, int x, int y)
        {
            foreach (var parity in _zeroAndOne)
            {
                var numInColumn = Enumerable.Range(0, Size).Count(row => grid[x + Size * row] != null && grid[x + Size * row].Value % 2 == parity);
                if (numInColumn == Size / 2)
                    for (var row = 0; row < Size; row++)
                        for (var v = 0; v < takens[x + Size * row].Length; v++)
                            if ((v + minValue) % 2 == parity)
                                takens[x + Size * row][v] = true;
                var numInRow = Enumerable.Range(0, Size).Count(col => grid[col + Size * y] != null && grid[col + Size * y].Value % 2 == parity);
                if (numInRow == Size / 2)
                    for (var col = 0; col < Size; col++)
                        for (var v = 0; v < takens[col + Size * y].Length; v++)
                            if ((v + minValue) % 2 == parity)
                                takens[col + Size * y][v] = true;
            }
        }

        private void PreventThreeInARow(bool[][] takens, int?[] grid, int minValue, int i, int x, int y)
        {
            foreach (var (offset, toEnforce) in _combinations)
            {
                if (x + offset >= 0 && x + offset < Size && x + toEnforce >= 0 && x + toEnforce < Size && grid[i + offset] != null && grid[i + offset].Value % 2 == grid[i].Value % 2 && grid[i + toEnforce] == null)
                    for (var v = 0; v < takens[i + toEnforce].Length; v++)
                        if ((v + minValue) % 2 == grid[i].Value % 2)
                            takens[i + toEnforce][v] = true;
                if (y + offset >= 0 && y + offset < Size && y + toEnforce >= 0 && y + toEnforce < Size && grid[i + Size * offset] != null && grid[i + Size * offset].Value % 2 == grid[i].Value % 2 && grid[i + Size * toEnforce] == null)
                    for (var v = 0; v < takens[i + Size * toEnforce].Length; v++)
                        if ((v + minValue) % 2 == grid[i].Value % 2)
                            takens[i + Size * toEnforce][v] = true;
            }
        }
    }
}

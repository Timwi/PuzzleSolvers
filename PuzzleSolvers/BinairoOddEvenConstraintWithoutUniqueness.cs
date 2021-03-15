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
        public override IEnumerable<Constraint> MarkTakens(SolverState state)
        {
            var ix = state.LastPlaced;
            if (ix == null)
                return null;

            var i = ix.Value;
            var x = i % Size;
            var y = i / Size;

            PreventThreeInARow(state, i, x, y);
            EnsureEqualNumbersOfEvensAndOdds(state, x, y);
            AdditionalDeductions(state, x, y);
            return null;
        }

        /// <summary>Allows derived classes to perform additional deductions.</summary>
        protected virtual void AdditionalDeductions(SolverState state, int x, int y) { }

        private void EnsureEqualNumbersOfEvensAndOdds(SolverState state, int x, int y)
        {
            foreach (var parity in _zeroAndOne)
            {
                var numInColumn = Enumerable.Range(0, Size).Count(row => state[x + Size * row] != null && state[x + Size * row].Value % 2 == parity);
                if (numInColumn == Size / 2)
                    for (var row = 0; row < Size; row++)
                        for (var v = state.MinValue; v <= state.MaxValue; v++)
                            if (v % 2 == parity)
                                state.MarkImpossible(x + Size * row, v);
                var numInRow = Enumerable.Range(0, Size).Count(col => state[col + Size * y] != null && state[col + Size * y].Value % 2 == parity);
                if (numInRow == Size / 2)
                    for (var col = 0; col < Size; col++)
                        for (var v = state.MinValue; v <= state.MaxValue; v++)
                            if (v % 2 == parity)
                                state.MarkImpossible(col + Size * y, v);
            }
        }

        private void PreventThreeInARow(SolverState state, int i, int x, int y)
        {
            foreach (var (offset, toEnforce) in _combinations)
            {
                if (x + offset >= 0 && x + offset < Size && x + toEnforce >= 0 && x + toEnforce < Size && state[i + offset] != null && state[i + offset].Value % 2 == state[i].Value % 2 && state[i + toEnforce] == null)
                    for (var v = state.MinValue; v <= state.MaxValue; v++)
                        if (v % 2 == state[i].Value % 2)
                            state.MarkImpossible(i + toEnforce, v);
                if (y + offset >= 0 && y + offset < Size && y + toEnforce >= 0 && y + toEnforce < Size && state[i + Size * offset] != null && state[i + Size * offset].Value % 2 == state[i].Value % 2 && state[i + Size * toEnforce] == null)
                    for (var v = state.MinValue; v <= state.MaxValue; v++)
                        if (v % 2 == state[i].Value % 2)
                            state.MarkImpossible(i + Size * toEnforce, v);
            }
        }
    }
}

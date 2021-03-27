using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Encapsulates a 9×9 Sudoku puzzle with Killer cages (sum + uniqueness constraints).</summary>
    public sealed class KillerSudoku : Sudoku
    {
        /// <summary>
        ///     Constructs a Killer Sudoku with cages (regions) that each have a <see cref="SumConstraint"/> and a <see
        ///     cref="UniquenessConstraint"/>.</summary>
        /// <param name="field">
        ///     A string containing letters A, B, etc. identifying the cages (one letter per cell in the grid).</param>
        /// <param name="sums">
        ///     The sums associated with each cage in the same order as the letters.</param>
        public KillerSudoku(string field, params int[] sums) : this(field, sums, 1) { }

        /// <summary>
        ///     Constructs a Killer Sudoku with cages (regions) that each have a <see cref="SumConstraint"/> and a <see
        ///     cref="UniquenessConstraint"/>.</summary>
        /// <param name="field">
        ///     A string containing letters A, B, etc. identifying the cages (one letter per cell in the grid).</param>
        /// <param name="sums">
        ///     The sums associated with each cage in the same order as the letters.</param>
        /// <param name="minValue">
        ///     The minimum value to be used in the grid.</param>
        public KillerSudoku(string field, int[] sums, int minValue) : base(minValue)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));
            if (field.Any(ch => (ch < 'A' || ch > 'Z') && ch != '.'))
                throw new ArgumentException("‘field’ must contain only letters A-Z or periods (.).", nameof(field));
            if (field.Length != 81)
                throw new ArgumentException("‘field’ must have exactly 81 characters. Use the period (.) character to indicate cells not occupied by a Killer cage.", nameof(field));
            if (sums == null)
                throw new ArgumentNullException(nameof(sums));

            var cages = new Dictionary<char, List<int>>();
            for (var cell = 0; cell < field.Length; cell++)
                if (field[cell] != '.')
                    cages.AddSafe(field[cell], cell);

            if (Enumerable.Range(0, sums.Length).Any(i => !cages.ContainsKey((char) ('A' + i))))
                throw new ArgumentException("‘field’ must contain every letter from A up to however many elements are in ‘sums’.", nameof(field));

            foreach (var kvp in cages)
            {
                AddConstraint(new UniquenessConstraint(kvp.Value), background: (ConsoleColor) ((kvp.Key - 'A') % 6 + 1));
                AddConstraint(new SumConstraint(sums[kvp.Key - 'A'], kvp.Value));
            }
        }
    }
}

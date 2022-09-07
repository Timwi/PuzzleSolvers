using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Contains methods to deal with Nonogram puzzles.</summary>
    public class Nonogram : Puzzle
    {
        /// <summary>
        ///     Returns all solutions to a Nonogram puzzle with the specified column and row clues.</summary>
        /// <param name="colClues">
        ///     The column clues. Each integer specifies the length of a consecutive run of set pixels. A <c>null</c> value
        ///     can be used to include a run of any length.</param>
        /// <param name="rowClues">
        ///     The row clues. See <paramref name="colClues"/> for details.</param>
        /// <param name="solverInstructions">
        ///     Instructions to the puzzle solver.</param>
        public static IEnumerable<bool[]> Solve(int?[][] colClues, int?[][] rowClues, SolverInstructions solverInstructions = null) =>
            new Nonogram(colClues, rowClues).Solve(solverInstructions).Select(solution => solution.Select(i => i != 0).ToArray());

        /// <summary>
        ///     Returns a <see cref="Puzzle"/> object that represents a Nonogram puzzle with the specified column and row
        ///     clues.</summary>
        /// <param name="colClues">
        ///     The column clues. Each integer specifies the length of a consecutive run of set pixels. A <c>null</c> value
        ///     can be used to include a run of any length.</param>
        /// <param name="rowClues">
        ///     The row clues. See <paramref name="colClues"/> for details.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if either argument is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown if either argument is a zero-length array.</exception>
        public Nonogram(int?[][] colClues, int?[][] rowClues)
            : base((colClues?.Length ?? 1) * (rowClues?.Length ?? 1), 0, 1)
        {
            if (colClues == null)
                throw new ArgumentNullException(nameof(colClues));
            if (rowClues == null)
                throw new ArgumentNullException(nameof(rowClues));
            if (colClues.Length == 0)
                throw new ArgumentException("Length of column clues array cannot be zero.");
            if (rowClues.Length == 0)
                throw new ArgumentException("Length of row clues array cannot be zero.");

            var ix = 0;
            foreach (var clue in colClues.Concat(rowClues))
            {
                var combinations = new List<int?[]>();
                var numBits = ix < colClues.Length ? rowClues.Length : colClues.Length;
                for (var i = ~(-1 << numBits); i >= 0; i--)
                {
                    var arr = new int?[numBits];
                    for (var bit = 0; bit < numBits; bit++)
                        arr[bit] = (i >> bit) & 1;
                    var clueIx = 0;
                    foreach (var clump in arr.GroupConsecutive())
                        if (clump.Key == 1)
                        {
                            if (clueIx >= clue.Length || (clue[clueIx] != null && clue[clueIx].Value != clump.Count))
                                goto busted;
                            clueIx++;
                        }
                    if (clueIx == clue.Length)
                        combinations.Add(arr);
                    busted:;
                }
                AddConstraint(new CombinationsConstraint(ix < colClues.Length
                    ? Enumerable.Range(0, rowClues.Length).Select(y => ix + colClues.Length * y)
                    : Enumerable.Range(0, colClues.Length).Select(x => x + colClues.Length * (ix - colClues.Length)),
                    combinations));
                ix++;
            }
        }
    }
}

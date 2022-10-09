using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Represents a Skyscraper puzzle.</summary>
    public class SkyscraperPuzzle : Puzzle
    {
        /// <summary>
        ///     Returns all solutions to a Skyscraper puzzle with the specified column and row clues.</summary>
        /// <param name="topClues">
        ///     The column clues along the top edge of the grid, from left to right. Each integer specifies the number of
        ///     visible skyscrapers in each column. A <c>null</c> value can be used to indicate the absence of a clue.</param>
        /// <param name="rightClues">
        ///     The row clues along the right edge of the grid, from top to bottom.</param>
        /// <param name="bottomClues">
        ///     The column clues along the bottom edge of the grid, from left to right.</param>
        /// <param name="leftClues">
        ///     The row clues along the left edge of the grid, from top to bottom.</param>
        /// <param name="solverInstructions">
        ///     Instructions to the puzzle solver.</param>
        public static IEnumerable<int[]> Solve(int?[] topClues, int?[] rightClues, int?[] bottomClues, int?[] leftClues, SolverInstructions solverInstructions = null) =>
            new SkyscraperPuzzle(topClues, rightClues, bottomClues, leftClues).Solve(solverInstructions);

        /// <summary>
        ///     Returns a <see cref="Puzzle"/> object that represents a Nonogram puzzle with the specified column and row
        ///     clues.</summary>
        /// <param name="topClues">
        ///     The column clues along the top edge of the grid, from left to right. Each integer specifies the number of
        ///     visible skyscrapers in each column.</param>
        /// <param name="rightClues">
        ///     The row clues along the right edge of the grid, from top to bottom.</param>
        /// <param name="bottomClues">
        ///     The column clues along the bottom edge of the grid, from left to right.</param>
        /// <param name="leftClues">
        ///     The row clues along the left edge of the grid, from top to bottom.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any argument is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown if any argument is a zero-length array, or any two of the arrays do not match in length.</exception>
        public SkyscraperPuzzle(int[] topClues, int[] rightClues, int[] bottomClues, int[] leftClues)
            : this(
                 topClues?.SelectNullable().ToArray(),
                 rightClues?.SelectNullable().ToArray(),
                 bottomClues?.SelectNullable().ToArray(),
                 leftClues?.SelectNullable().ToArray())
        {
        }

        /// <summary>
        ///     Returns a <see cref="Puzzle"/> object that represents a Nonogram puzzle with the specified column and row
        ///     clues.</summary>
        /// <param name="topClues">
        ///     The column clues along the top edge of the grid, from left to right. Each integer specifies the number of
        ///     visible skyscrapers in each column. A <c>null</c> value can be used to indicate the absence of a clue.</param>
        /// <param name="rightClues">
        ///     The row clues along the right edge of the grid, from top to bottom.</param>
        /// <param name="bottomClues">
        ///     The column clues along the bottom edge of the grid, from left to right.</param>
        /// <param name="leftClues">
        ///     The row clues along the left edge of the grid, from top to bottom.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any argument is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown if any argument is a zero-length array, or any two of the arrays do not match in length.</exception>
        public SkyscraperPuzzle(int?[] topClues, int?[] rightClues, int?[] bottomClues, int?[] leftClues)
            : base((topClues?.Length ?? 1) * (topClues?.Length ?? 1), 1, topClues?.Length ?? 1)
        {
            if (topClues == null)
                throw new ArgumentNullException(nameof(topClues));
            if (rightClues == null)
                throw new ArgumentNullException(nameof(rightClues));
            if (bottomClues == null)
                throw new ArgumentNullException(nameof(bottomClues));
            if (leftClues == null)
                throw new ArgumentNullException(nameof(leftClues));

            if (topClues.Length == 0)
                throw new ArgumentException("Length of clues array cannot be zero.");
            if (rightClues.Length != topClues.Length || bottomClues.Length != topClues.Length || leftClues.Length != topClues.Length)
                throw new ArgumentException($"Lengths of clues arrays must be equal (observed lengths are {topClues.Length}, {rightClues.Length}, {bottomClues.Length}, {leftClues.Length}).");

            var sz = topClues.Length;
            for (var i = 0; i < sz; i++)
            {
                if (topClues[i] is int tc)
                    AddConstraint(new SkyscraperUniquenessConstraint(tc, Enumerable.Range(0, sz).Select(y => i + sz * y)));
                if (rightClues[i] is int rc)
                    AddConstraint(new SkyscraperUniquenessConstraint(rc, Enumerable.Range(0, sz).Select(x => (sz - 1 - x) + sz * i)));
                if (bottomClues[i] is int bc)
                    AddConstraint(new SkyscraperUniquenessConstraint(bc, Enumerable.Range(0, sz).Select(y => i + sz * (sz - 1 - y))));
                if (leftClues[i] is int lc)
                    AddConstraint(new SkyscraperUniquenessConstraint(lc, Enumerable.Range(0, sz).Select(x => x + sz * i)));
                if (topClues[i] == null && bottomClues[i] == null)
                    AddConstraint(new UniquenessConstraint(Enumerable.Range(0, sz).Select(y => i + sz * y)));
                if (leftClues[i] == null && rightClues[i] == null)
                    AddConstraint(new UniquenessConstraint(Enumerable.Range(0, sz).Select(x => x + sz * i)));
            }
        }

        /// <summary>Calculates what the Skyscraper clue would be for a given row of numbers.</summary>
        public static int CalculateSkyscraperClue(IEnumerable<int> values)
        {
            var cl = 0;
            int? prev = null;
            foreach (var value in values)
            {
                if (prev == null || value > prev.Value)
                {
                    cl++;
                    prev = value;
                }
            }
            return cl;
        }
    }
}

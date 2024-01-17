using System;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes an Akari puzzle — a shading-type puzzle in which lightbulbs must be placed in vacant cells in such a way
    ///     that all cells are illuminated by at least one lightbulb, and no two lightbulbs can shine at each other
    ///     horizontally or vertically. Blocks in the grid can block the path of a lightbulb.</summary>
    public class Akari : Puzzle
    {
        /// <summary>
        ///     Instantiates an Akari puzzle based on a character-based description provided as a string.</summary>
        /// <param name="width">
        ///     Width of the Akari puzzle.</param>
        /// <param name="height">
        ///     Height of the Akari puzzle.</param>
        /// <param name="description">
        ///     A string of characters in which a period (<c>.</c>) represents a vacant cell, a hash (<c>#</c>) represents a
        ///     block, and a digit (<c>0</c> through <c>9</c>) represents a block with a constraint on the number of
        ///     lightbulbs orthogonally adjacent to the block.</param>
        public Akari(int width, int height, string description) : this(width, height,

            description == null ? throw new ArgumentNullException(nameof(description)) :
            description.Length != width * height ? throw new ArgumentException($"The length of the ‘{nameof(description)}’ string must be equal to width times height ({width * height}).", nameof(description)) :
            !description.All(".#0123456789".Contains) ? throw new ArgumentException($"The ‘{nameof(description)}’ string must only contain periods (.), hashes (#) and digits (0–9).", nameof(description)) :
             description.Select(ch => ch != '.').ToArray(),

            description.Select(ch => ch >= '0' && ch <= '9' ? (ch - '0').Nullable() : null).ToArray())
        {
        }

        /// <summary>
        ///     Instantiates an Akari puzzle.</summary>
        /// <param name="width">
        ///     Width of the Akari puzzle.</param>
        /// <param name="height">
        ///     Height of the Akari puzzle.</param>
        /// <param name="isBlock">
        ///     Determines which cells are blocks — cells that block the line-of-sight of lightbulbs.</param>
        /// <param name="clues">
        ///     Determines which cells are clues. A clue determines the number of lightbulbs orthogonally adjacent to the
        ///     cell. A clued cell must also be a block (<paramref name="isBlock"/> must be <c>true</c> at this index).</param>
        public Akari(int width, int height, bool[] isBlock, int?[] clues) : base(width * height, 0, 1)
        {
            if (isBlock == null)
                throw new ArgumentNullException(nameof(isBlock));
            if (isBlock.Length != width * height)
                throw new ArgumentException($"The length of the ‘{nameof(isBlock)}’ array must be equal to width times height ({width * height}).", nameof(isBlock));
            if (clues == null)
                throw new ArgumentNullException(nameof(clues));
            if (clues.Length != width * height)
                throw new ArgumentException($"The length of the ‘{nameof(clues)}’ array must be equal to width times height ({width * height}).", nameof(clues));

            // Each cell that is a block cannot be a light bulb
            for (var cell = 0; cell < isBlock.Length; cell++)
                if (isBlock[cell])
                    AddConstraint(new GivenConstraint(cell, 0), ConsoleColor.DarkGray, ConsoleColor.DarkGray);

            // Each cell that is a numerical clue must be satisfied
            for (var cell = 0; cell < clues.Length; cell++)
                if (clues[cell] is int clue)
                {
                    if (!isBlock[cell])
                        throw new ArgumentException($"Cell {cell} has a numerical clue ({clue}) but is not designated a block. Each non-null value in ‘{nameof(clue)}’ must have a true value in ‘{nameof(isBlock)}’.", nameof(clues));
                    AddConstraint(new SumConstraint(clue, PuzzleUtil.Orthogonal(cell, width, height)));
                }

            var horizontalCorridors = Enumerable.Range(0, height)
                .SelectMany(y => Enumerable.Range(width * y, width).GroupConsecutiveBy(cell => !isBlock[cell]).Where(c => c.Key).Select(c => c.ToArray()))
                .ToArray();
            var verticalCorridors = Enumerable.Range(0, width)
                .SelectMany(x => Enumerable.Range(0, height).Select(y => x + width * y).GroupConsecutiveBy(cell => !isBlock[cell]).Where(c => c.Key).Select(c => c.ToArray()))
                .ToArray();

            // Each corridor can only have one bulb in it
            foreach (var corridor in horizontalCorridors.Concat(verticalCorridors))
                AddConstraint(new MaxSumConstraint(1, corridor));

            // Each cell must be illuminated by at least one bulb in the same corridor
            for (var cell = 0; cell < isBlock.Length; cell++)
                if (!isBlock[cell])
                    AddConstraint(new MinSumConstraint(1, horizontalCorridors.Where(h => h.Contains(cell)).Concat(verticalCorridors.Where(v => v.Contains(cell))).SelectMany(c => c).Distinct()));
        }
    }
}

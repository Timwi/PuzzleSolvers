using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;
using RT.Util.ExtensionMethods.Obsolete;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes an Aquapelago puzzle — a shading puzzle in which shaded cells cannot be orthogonally adjacent, unshaded
    ///     cells must all be contiguous, and unshaded cells cannot form a 2×2 area. Each clue determines the number of shaded
    ///     cells that are diagonally connected to the clue (including itself).</summary>
    public class Aquapelago : Puzzle
    {
        /// <summary>The width of the puzzle grid.</summary>
        public int Width { get; private set; }
        /// <summary>The height of the puzzle grid.</summary>
        public int Height { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="width">
        ///     The width of the puzzle grid.</param>
        /// <param name="height">
        ///     The height of the puzzle grid.</param>
        /// <param name="description">
        ///     A string of characters in which a period (<c>.</c>) represents a vacant cell, a hash (<c>#</c>) represents a
        ///     given shaded cell, and a digit (<c>1</c> through <c>9</c>) represents a given shaded cell with a constraint on
        ///     the number of diagonally connected shaded cells.</param>
        public Aquapelago(int width, int height, string description) : base(width * height, 0, 1)
        {
            Width = width;
            Height = height;
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            if (description.Length != width * height)
                throw new ArgumentException($"‘{nameof(description)}’ must have length ‘width’×‘height’.");

            for (var cell = 0; cell < description.Length; cell++)
            {
                if (description[cell] == '#')
                    AddConstraint(new GivenConstraint(cell, 1));
                else if (description[cell] >= '1' && description[cell] <= '9')
                    AddConstraint(new CombinationsConstraint(Enumerable.Range(0, width * height), genAquapelagoRegion([cell], [], description[cell] - '0')));
                else if (description[cell] != '.')
                    throw new ArgumentException($"‘{nameof(description)}’ may only contain ‘.’, ‘#’, or digits 1–9.");
            }

            AddConstraint(new NoAdjacentConstraint(width, height, 1));
            AddConstraint(new ContiguousAreaConstraint(width, height, 0));
            AddConstraint(new No2x2sConstraint(width, height, 0));
        }

        private IEnumerable<int?[]> genAquapelagoRegion(int[] sofar, int[] forbidden, int clue)
        {
            if (sofar.Length == clue)
            {
                var result = new int?[Width * Height];
                foreach (var cell in sofar)
                {
                    result[cell] = 1;
                    foreach (var adj in PuzzleUtil.Adjacent(cell, Width, Height))
                        if (result[adj] == null)
                            result[adj] = 0;
                }
                yield return result;
                yield break;
            }

            var newAvenues = sofar.SelectMany(c => PuzzleUtil.Diagonal(c, Width, Height)).Where(diag => !sofar.Contains(diag) && !forbidden.Contains(diag)).ToHashSet();
            var newForbidden = forbidden;
            foreach (var cell in newAvenues)
            {
                var newSofar = sofar.Append(cell);
                var area = ContiguousAreaConstraint.FindArea(newSofar.Contains(0) ? 1 : 0, Width, Height, v => !newSofar.Contains(v));
                if (newSofar.Length + area.Count < Width * Height)
                    continue;
                foreach (var result in genAquapelagoRegion(newSofar, newForbidden, clue))
                    yield return result;
                newForbidden = newForbidden.Append(cell);
            }
        }
    }
}

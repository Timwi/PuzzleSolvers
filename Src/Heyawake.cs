using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Describes a Heyawake puzzle.</summary>
    public class Heyawake : Puzzle
    {
        /// <summary>Specifies the width of the grid.</summary>
        public int Width { get; private set; }
        /// <summary>Specifies the height of the grid.</summary>
        public int Height { get; private set; }
        /// <summary>Specifies the regions the grid is subdivided into.</summary>
        public int[][] Regions { get; private set; }
        /// <summary>
        ///     Specifies the number of cells to be shaded in each region with the same index in <see cref="Regions"/>, or
        ///     <c>null</c> for a region with no clue.</summary>
        public int?[] Clues { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="width">
        ///     Specifies the width of the grid.</param>
        /// <param name="height">
        ///     Specifies the height of the grid.</param>
        /// <param name="regions">
        ///     Specifies the regions the grid is subdivided into. The regions must be disjoint (have no cells in common) and
        ///     must cover the entire grid.</param>
        /// <param name="clues">
        ///     Specifies the number of cells to be shaded in each region with the same index in <paramref name="regions"/>,
        ///     or <c>null</c> for a region with no clue. The length of this array must match that of <see cref="Regions"/>.</param>
        public Heyawake(int width, int height, int[][] regions, int?[] clues) : base(width * height, 0, 1)
        {
            Width = width;
            Height = height;
            Regions = regions ?? throw new ArgumentNullException(nameof(regions));
            Clues = clues ?? throw new ArgumentNullException(nameof(clues));

            if (regions.Length != clues.Length)
                throw new ArgumentException($"‘{nameof(regions)}’ and ‘{nameof(clues)}’ must have the same length.");
            if (regions.Contains(null))
                throw new ArgumentException($"‘{nameof(regions)}’ cannot contain null.");
            if (!regions.SelectMany(r => r).Order().SequenceEqual(Enumerable.Range(0, width * height)))
                throw new ArgumentException($"The regions specified by ‘{nameof(regions)}’ must cover the entire grid.");

            AddConstraint(new NoAdjacentConstraint(width, height, 1));
            AddConstraint(new ContiguousAreaConstraint(width, height, 0));
            AddConstraint(new LineRuleConstraint(width, height, regions));
            for (var i = 0; i < regions.Length; i++)
            {
                var fore = i % 14 > 8 ? ConsoleColor.Black : ConsoleColor.White;
                var back = (ConsoleColor) (i % 14 + 1);
                if (clues[i] is int clue)
                    AddConstraint(new CombinationsConstraint(regions[i], generateRegion([], width, height, regions[i], clue)), fore, back);
                else
                    AddConstraint(new AlwaysTrueConstraint(regions[i]), fore, back);
            }
        }

        private IEnumerable<int[]> generateRegion(int[] sofar, int width, int height, int[] region, int remainingClue)
        {
            if (sofar.Length == region.Length)
            {
                var rest = region.Where((c, ix) => sofar[ix] == 0).ToArray();
                while (rest.Length > 0)
                {
                    var area = ContiguousAreaConstraint.FindArea(rest[0], width, height, rest.Contains);
                    if (!area.SelectMany(c => PuzzleUtil.Orthogonal(c, width, height).Where(c => !region.Contains(c))).Any())
                        yield break;
                    rest = rest.Except(area).ToArray();
                }

                if (remainingClue == 0)
                    yield return sofar;
                yield break;
            }

            for (var v = 0; v <= 1; v++)
            {
                if (v == 0 && remainingClue >= region.Length - sofar.Length)
                    continue;
                if (v == 1 && (remainingClue == 0 || sofar.Any((s, ix) => s == 1 && region[ix].IsOrthogonal(region[sofar.Length], width, height))))
                    continue;
                foreach (var gen in generateRegion(sofar.Append(v), width, height, region, remainingClue - v))
                    yield return gen;
            }
        }

        private class LineRuleConstraint(int width, int height, int[][] regions) : Constraint(null)
        {
            private readonly bool[] _borderRight = Enumerable.Range(0, width * height).Select(ix => ix % width < width - 1 && regions.IndexOf(r => r.Contains(ix)) != regions.IndexOf(r => r.Contains(ix + 1))).ToArray();
            private readonly bool[] _borderDown = Enumerable.Range(0, width * height).Select(ix => ix / width < height - 1 && regions.IndexOf(r => r.Contains(ix)) != regions.IndexOf(r => r.Contains(ix + width))).ToArray();

            private static ConstraintResult check(int size1, int size2, Func<int, int, int?> getState, Func<int, int, bool> borderAfter, Action<int, int> markImpossible0)
            {
                for (var y = 0; y < size2; y++)
                {
                    int prevX = -1, prevX2 = -1, prevBorders = -1;
                    for (var x = 0; x < size1; x++)
                    {
                        if (getState(x, y) != 0)
                            continue;
                        int x2 = x + 1, numBorders = 0;
                        while (x2 < size1 && getState(x2, y) == 0)
                        {
                            numBorders += borderAfter(x2 - 1, y) ? 1 : 0;
                            x2++;
                        }
                        if (numBorders > 1)
                            return ConstraintResult.Violation;
                        if (numBorders == 1 && x > 0 && borderAfter(x - 1, y))
                            markImpossible0(x - 1, y);
                        if (numBorders == 1 && x2 < size1 && borderAfter(x2 - 1, y))
                            markImpossible0(x2, y);

                        if (prevX != -1 && prevX2 + 1 == x && prevBorders + numBorders + (borderAfter(prevX2 - 1, y) ? 1 : 0) + (borderAfter(prevX2, y) ? 1 : 0) > 1)
                            markImpossible0(prevX2, y);

                        prevX = x;
                        prevX2 = x2;
                        prevBorders = numBorders;
                    }
                }
                return null;
            }

            public override ConstraintResult Process(SolverState state) =>
                check(width, height, (x, y) => state[x + width * y], (x, y) => _borderRight[x + width * y], (x, y) => state.MarkImpossible(x + width * y, 0)) ??
                check(height, width, (y, x) => state[x + width * y], (y, x) => _borderDown[x + width * y], (y, x) => state.MarkImpossible(x + width * y, 0));
        }
    }
}

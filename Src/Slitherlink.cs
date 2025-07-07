using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Provides functions to solve Slitherlink puzzles. This solver does not use <see cref="Puzzle"/>.</summary>
    public static class Slitherlink
    {
        /// <summary>
        ///     Solves a Slitherlink puzzle on a rectangular grid and returns all solutions found.</summary>
        /// <param name="width">
        ///     Width of the grid in squares.</param>
        /// <param name="height">
        ///     Height of the grid in squares.</param>
        /// <param name="clues">
        ///     Clues (number of edges that are part of the loop). May be <c>null</c>, in which case all possible Slitherlinks
        ///     of the specified size are generated.</param>
        /// <param name="randomizer">
        ///     Permits the results to be randomized.</param>
        /// <param name="minNumLoops">
        ///     Minimum number of loops allowed in the solution.</param>
        /// <param name="maxNumLoops">
        ///     Maximum number of loops allowed in the solution.</param>
        /// <returns>
        ///     A boolean array for each solution. The array specifies which cells are inside of the loop.</returns>
        public static IEnumerable<bool[]> Solve(int width, int height, int?[] clues = null, Random randomizer = null, int minNumLoops = 1, int maxNumLoops = 1) =>
            minNumLoops > maxNumLoops
                ? throw new ArgumentException("‘minNumLoops must be less than or equal to ‘maxNumLoops’.", nameof(maxNumLoops))
                : slitherlinkRecurse(width, height, clues, new bool?[width * height], [], 0, 0, randomizer, minNumLoops, maxNumLoops);

        private static IEnumerable<bool[]> slitherlinkRecurse(int w, int h, int?[] clues, bool?[] sofar, List<(int ix1, int ix2)> segments, int closedLoops, int depth, Random randomizer, int minNumLoops, int maxNumLoops)
        {
            void addLine(int ix1, int ix2)
            {
                var e1 = segments.IndexOf(tup => tup.ix1 == ix1 || tup.ix2 == ix1);
                var e2 = segments.IndexOf(tup => tup.ix1 == ix2 || tup.ix2 == ix2);
                if (e1 != -1 && e2 == e1)
                {
                    closedLoops++;
                    segments.RemoveAt(e1);
                }
                else if (e1 != -1 && e2 != -1)
                {
                    var o1 = segments[e1].ix1 == ix1 ? segments[e1].ix2 : segments[e1].ix1;
                    var o2 = segments[e2].ix1 == ix2 ? segments[e2].ix2 : segments[e2].ix1;
                    segments.RemoveAt(Math.Max(e1, e2));
                    segments.RemoveAt(Math.Min(e1, e2));
                    segments.Add((o1, o2));
                }
                else if (e1 != -1)
                    segments[e1] = (ix2, segments[e1].ix1 == ix1 ? segments[e1].ix2 : segments[e1].ix1);
                else if (e2 != -1)
                    segments[e2] = (ix1, segments[e2].ix1 == ix2 ? segments[e2].ix2 : segments[e2].ix1);
                else
                    segments.Add((ix1, ix2));
            }

            void setPixel(int ix, bool v)
            {
                var x = ix % w;
                var y = ix / w;
                sofar[ix] = v;
                if (x > 0 ? (sofar[ix - 1] == !v) : v)
                    addLine(x + (w + 1) * y, x + (w + 1) * (y + 1));
                if (x < w - 1 ? (sofar[ix + 1] == !v) : v)
                    addLine(x + 1 + (w + 1) * y, x + 1 + (w + 1) * (y + 1));
                if (y > 0 ? (sofar[ix - w] == !v) : v)
                    addLine(x + (w + 1) * y, x + 1 + (w + 1) * y);
                if (y < h - 1 ? (sofar[ix + w] == !v) : v)
                    addLine(x + (w + 1) * (y + 1), x + 1 + (w + 1) * (y + 1));
            }

            var firstNullIx = -1;
            for (var i = 0; i < w * h; i++)
            {
                next:
                if (sofar[i] == null)
                {
                    if (firstNullIx == -1)
                        firstNullIx = i;
                    continue;
                }
                if (clues == null || clues[i] == null)
                    continue;
                var x = i % w;
                var y = i / w;
                var v = sofar[i].Value;
                var nSame = 0;
                var nDiff = 0;
                var nNull = 0;
                if (x > 0) { if (sofar[i - 1] == null) nNull++; else if (sofar[i - 1].Value == v) nSame++; else nDiff++; } else if (v) nDiff++; else nSame++;
                if (x < w - 1) { if (sofar[i + 1] == null) nNull++; else if (sofar[i + 1].Value == v) nSame++; else nDiff++; } else if (v) nDiff++; else nSame++;
                if (y > 0) { if (sofar[i - w] == null) nNull++; else if (sofar[i - w].Value == v) nSame++; else nDiff++; } else if (v) nDiff++; else nSame++;
                if (y < h - 1) { if (sofar[i + w] == null) nNull++; else if (sofar[i + w].Value == v) nSame++; else nDiff++; } else if (v) nDiff++; else nSame++;
                if (nDiff > clues[i].Value || nDiff + nNull < clues[i].Value)
                    yield break;
                if (nNull > 0)
                {
                    if (nDiff == clues[i].Value)
                    {
                        if (x > 0 && sofar[i - 1] == null) setPixel(i - 1, v);
                        if (x < w - 1 && sofar[i + 1] == null) setPixel(i + 1, v);
                        if (y > 0 && sofar[i - w] == null) setPixel(i - w, v);
                        if (y < h - 1 && sofar[i + w] == null) setPixel(i + w, v);
                        i -= w + 1;
                        if (i < 0)
                            i = 0;
                        if (firstNullIx >= i)
                            firstNullIx = -1;
                        goto next;
                    }
                    if (nDiff + nNull == clues[i].Value)
                    {
                        if (x > 0 && sofar[i - 1] == null) setPixel(i - 1, !v);
                        if (x < w - 1 && sofar[i + 1] == null) setPixel(i + 1, !v);
                        if (y > 0 && sofar[i - w] == null) setPixel(i - w, !v);
                        if (y < h - 1 && sofar[i + w] == null) setPixel(i + w, !v);
                        i -= w + 1;
                        if (i < 0)
                            i = 0;
                        if (firstNullIx >= i)
                            firstNullIx = -1;
                        goto next;
                    }
                }
            }

            // Reject checkerboards
            for (var i = w + 1; i < w * h; i++)
            {
                var x = i % w;
                var y = i / w;
                if (x == 0 || y == 0)
                    continue;
                if (sofar[i] != null && sofar[i - 1] != null && sofar[i - w] != null && sofar[i - w - 1] != null && sofar[i - w - 1].Value == sofar[i].Value && sofar[i - w].Value != sofar[i].Value && sofar[i - 1].Value != sofar[i].Value)
                    yield break;
            }

            if (firstNullIx == -1 && closedLoops >= minNumLoops && closedLoops <= maxNumLoops && segments.Count == 0)
            {
                yield return sofar.Select(b => b.Value).ToArray();
                yield break;
            }

            if (firstNullIx == -1 || closedLoops > maxNumLoops || (closedLoops >= maxNumLoops && segments.Count > 0))
                yield break;

            var ofs = randomizer != null && randomizer.Next(0, 2) != 0;
            var oldSofar = sofar.ToArray();
            var oldClosedLoops = closedLoops;
            var oldSegments = segments.ToList();
            setPixel(firstNullIx, ofs);
            foreach (var solution in slitherlinkRecurse(w, h, clues, sofar, segments, closedLoops, depth + 1, randomizer, minNumLoops, maxNumLoops))
                yield return solution;
            sofar = oldSofar;
            closedLoops = oldClosedLoops;
            segments = oldSegments;
            setPixel(firstNullIx, !ofs);
            foreach (var solution in slitherlinkRecurse(w, h, clues, sofar, segments, closedLoops, depth + 1, randomizer, minNumLoops, maxNumLoops))
                yield return solution;
        }

        /// <summary>
        ///     Returns an array containing all of the Slitherlink clues for the specified grid.</summary>
        /// <param name="grid">
        ///     A filled grid of pixels.</param>
        /// <param name="width">
        ///     The width of the grid. The height is assumed to be <paramref name="grid"/>.Length − <paramref name="width"/>.</param>
        /// <remarks>
        ///     This method will not check whether the grid is a valid Slitherlink (a single contiguous region with no holes).</remarks>
        public static int[] CalculateSlitherlinkClues(bool[] grid, int width) => grid.Select((v, i) =>
            ((i % width > 0 ? grid[i - 1] != v : v) ? 1 : 0) +
            ((i % width + 1 < width ? grid[i + 1] != v : v) ? 1 : 0) +
            ((i - width >= 0 ? grid[i - width] != v : v) ? 1 : 0) +
            ((i + width < grid.Length ? grid[i + width] != v : v) ? 1 : 0)).ToArray();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Provides functions to solve Slitherlink puzzles. This solver does not use <see cref="Puzzle"/>.</summary>
    public static class Slitherlink
    {
        /// <summary>Solves a Slitherlink puzzle on a rectangular grid and returns all solutions found.</summary>
        /// <param name="width">Width of the grid in squares.</param>
        /// <param name="height">Height of the grid in squares.</param>
        /// <param name="clues">Clues (number of edges that are part of the loop).</param>
        /// <returns>A boolean array for each solution. The array specifies which cells are inside of the loop.</returns>
        public static IEnumerable<bool[]> Solve(int width, int height, int?[] clues) => slitherlinkRecurse(width, height, clues, new bool?[width * height], new List<(int ix1, int ix2)>(), 0, 0);

        private static IEnumerable<bool[]> slitherlinkRecurse(int w, int h, int?[] clues, bool?[] sofar, List<(int ix1, int ix2)> segments, int closedLoops, int depth)
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
                if (clues[i] == null)
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

            if (firstNullIx == -1 && closedLoops == 1 && segments.Count == 0)
            {
                yield return sofar.Select(b => b.Value).ToArray();
                yield break;
            }

            if (firstNullIx == -1 || closedLoops > 1 || (closedLoops > 0 && segments.Count > 0))
                yield break;

            var oldSofar = sofar.ToArray();
            var oldClosedLoops = closedLoops;
            var oldSegments = segments.ToList();
            setPixel(firstNullIx, true);
            foreach (var solution in slitherlinkRecurse(w, h, clues, sofar, segments, closedLoops, depth + 1))
                yield return solution;
            sofar = oldSofar;
            closedLoops = oldClosedLoops;
            segments = oldSegments;
            setPixel(firstNullIx, false);
            foreach (var solution in slitherlinkRecurse(w, h, clues, sofar, segments, closedLoops, depth + 1))
                yield return solution;
        }
    }
}

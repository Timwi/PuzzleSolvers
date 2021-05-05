using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Contains an algorithm for solving Numberlink puzzles. This solver is independent of <see cref="Puzzle"/>.</summary>
    public static class Numberlink
    {
        /// <summary>
        ///     Solves a Numberlink puzzle of the specified size.</summary>
        /// <param name="w">
        ///     Width of the grid.</param>
        /// <param name="grid">
        ///     A string containing the numbers to be linked and <c>'.'</c>s for the available spaces. The height of the grid
        ///     is calculated from the array’s length and <paramref name="w"/>. If you need more than 10 digits (0–9), use
        ///     <see cref="Solve(int, int?[], bool)"/>.</param>
        /// <param name="debugOutput">
        ///     Determines whether some information is output to the console during solve.</param>
        /// <returns>
        ///     Each solution returned is represented as a set of links. Each link is an array of grid locations going from
        ///     one digit to its matching partner.</returns>
        public static IEnumerable<int[][]> Solve(int w, string grid, bool debugOutput = false)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            return Solve(w, grid.Select(ch => ch == '.' ? null : (int?) ch - '0').ToArray(), debugOutput);
        }

        /// <summary>
        ///     Solves a Numberlink puzzle of the specified size.</summary>
        /// <param name="w">
        ///     Width of the grid.</param>
        /// <param name="grid">
        ///     A grid containing the numbers to be linked and <c>null</c>s for the available spaces. The height of the grid
        ///     is calculated from the array’s length and <paramref name="w"/>.</param>
        /// <param name="debugOutput">
        ///     Determines whether some information is output to the console during solve.</param>
        /// <returns>
        ///     Each solution returned is represented as a set of links. Each link is an array of grid locations going from
        ///     one digit to its matching partner.</returns>
        public static IEnumerable<int[][]> Solve(int w, int?[] grid, bool debugOutput = false)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (w == 0 || grid.Length % w != 0)
                throw new ArgumentException("The size of the grid must be divisible by the width.", nameof(grid));
            var h = grid.Length / w;

            var numbers = grid.WhereNotNull().Distinct().ToArray();

            var directions = new (int dx, int dy)[] { (0, -1), (1, 0), (0, 1), (-1, 0) };
            IEnumerable<int[]> recurse(int[] sofar, int num)
            {
                var last = sofar.Last();
                if (sofar.Length > 1 && grid[last] == num)
                {
                    yield return sofar;
                    yield break;
                }

                var lastX = last % w;
                var lastY = last / w;
                foreach (var (dx, dy) in directions)
                {
                    var newX = lastX + dx;
                    var newY = lastY + dy;
                    var newPos = newX + w * newY;
                    if (newX < 0 || newX >= w || newY < 0 || newY >= h || newPos == sofar[0] || (grid[newPos] != null && grid[newPos] != num) || sofar.Contains(newPos))
                        continue;

                    for (var rx = -1; rx <= 0; rx++)
                        if (!(newX + rx < 0 || newX + rx >= w - 1))
                            for (var ry = -1; ry <= 0; ry++)
                                if (!(newY + ry < 0 || newY + ry >= h - 1))
                                {
                                    var nx = newX + rx;
                                    var ny = newY + ry;
                                    var candidates = new[] { nx + w * ny, nx + 1 + w * ny, nx + w * (ny + 1), nx + 1 + w * (ny + 1) };
                                    if (candidates.Count(c => c != newPos && (grid[c] == num || sofar.Contains(c))) >= 3)
                                        goto busted;
                                }

                    foreach (var sol in recurse(sofar.Insert(sofar.Length, newPos), num))
                        yield return sol;

                    busted:;
                }
            }

            var allPaths = new int[numbers.Length][][];
            Enumerable.Range(0, numbers.Length).ParallelForEach(Environment.ProcessorCount, numIx =>
            {
                allPaths[numIx] = recurse(new[] { grid.IndexOf(numbers[numIx]) }, numbers[numIx]).ToArray();
                if (debugOutput)
                    lock (allPaths)
                        Console.WriteLine($"{numbers[numIx]} has {allPaths[numIx].Length} paths");
            });

            IEnumerable<int[][]> solve(int[][][] paths, int[] examined)
            {
                if (examined.Length == paths.Length)
                {
                    if (!paths.All(arr => arr.Length == 1))
                        throw new InvalidOperationException("The Numberlink.Solve() algorithm encountered a bug. Please report it to the author!");
                    yield return paths.Select(arr => arr[0]).ToArray();
                    yield break;
                }

                var pathIx = Enumerable.Range(0, paths.Length).Except(examined).MinElement(ix => paths[ix].Length);

                // Find squares that other paths don’t use
                var unusedSquares = Enumerable.Range(0, w * h).Where(ix => grid[ix] == null).ToHashSet();
                for (var pIx = 0; pIx < paths.Length; pIx++)
                    if (pIx != pathIx)
                        foreach (var path in paths[pIx])
                            foreach (var sq in path)
                                unusedSquares.Remove(sq);

                var candidates = Enumerable.Range(0, paths[pathIx].Length).ParallelSelect(Environment.ProcessorCount, possIx =>
                {
                    var path = paths[pathIx][possIx];
                    if (unusedSquares.Any(sq => !path.Contains(sq)))
                        return null;
                    if (debugOutput && possIx % 25 == 0)
                        lock (allPaths)
                            Console.Write($"{examined.Concat(pathIx).Select(ix => numbers[ix]).JoinString("/")}: {possIx * 100 / (double) paths[pathIx].Length:0.0}%\r");

                    var newPaths = new int[paths.Length][][];
                    for (var ix = 0; ix < paths.Length; ix++)
                    {
                        newPaths[ix] = ix == pathIx ? new[] { path } : paths[ix].Where(p => !p.Intersect(path).Any()).ToArray();
                        if (newPaths[ix].Length == 0)
                            return null;
                    }

                    // Make sure paths don’t cross over
                    if (newPaths.Any(arr => arr.Length == 0))
                        return null;
                    // Make sure all squares are used
                    var test = new HashSet<int>();
                    foreach (var pathPossibilities in newPaths)
                        foreach (var pth in pathPossibilities)
                            test.AddRange(pth);
                    if (test.Count != w * h)
                        return null;

                    return (path, newPaths).Nullable();
                })
                    .WhereNotNull()
                    .ToArray();

                if (debugOutput)
                    Console.WriteLine($"{examined.Concat(pathIx).Select(ix => numbers[ix]).JoinString("/")}: {candidates.Length} candidates");

                foreach (var (path, newPaths) in candidates)
                    foreach (var sol in solve(newPaths, examined.Insert(examined.Length, pathIx)))
                        yield return sol;
            }

            return solve(allPaths, new int[0]);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;
using RT.Util.ExtensionMethods.Obsolete;

namespace PuzzleSolvers
{
    /// <summary>Contains an algorithm for solving Fillomino puzzles. This solver is independent of <see cref="Puzzle"/>.</summary>
    public static class Fillomino
    {
        /// <summary>
        ///     Solves a Fillomino puzzle of the specified size.</summary>
        /// <param name="w">
        ///     Width of the grid.</param>
        /// <param name="grid">
        ///     Givens. The height of the grid is calculated from the array’s length and <paramref name="w"/>.</param>
        /// <param name="debugOutput">
        ///     Outputs information to the console as the algorithm progresses.</param>
        /// <returns>
        ///     Each solution returned is represented as the filled grid. To get a list of the polyominoes instead, use <see
        ///     cref="SolveP(int, int?[], bool)"/>.</returns>
        public static IEnumerable<int[]> Solve(int w, int?[] grid, bool debugOutput = false)
        {
            foreach (var polyominoes in SolveP(w, grid, debugOutput))
            {
                var newGrid = new int[grid.Length];
                foreach (var polyomino in polyominoes)
                    foreach (var cell in polyomino)
                        newGrid[cell] = polyomino.Length;
                yield return newGrid;
            }
        }

        /// <summary>
        ///     Solves a Fillomino puzzle of the specified size.</summary>
        /// <param name="w">
        ///     Width of the grid.</param>
        /// <param name="grid">
        ///     Givens. The height of the grid is calculated from the array’s length and <paramref name="w"/>.</param>
        /// <param name="debugOutput">
        ///     Outputs information to the console as the algorithm progresses.</param>
        /// <returns>
        ///     Each solution returned is represented as a set of polyominoes. Each polyomino is an array of grid locations.
        ///     The size of the polyomino determines the number.</returns>
        public static IEnumerable<int[][]> SolveP(int w, int?[] grid, bool debugOutput = false)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));
            if (w == 0 || grid.Length % w != 0)
                throw new ArgumentException("The size of the grid must be divisible by the width.", nameof(grid));
            var h = grid.Length / w;

            IEnumerable<int> getAdj(int cell)
            {
                if (cell % w != 0)
                    yield return cell - 1;
                if (cell % w != w - 1)
                    yield return cell + 1;
                if (cell / w != 0)
                    yield return cell - w;
                if (cell / w != h - 1)
                    yield return cell + w;
            }

            IEnumerable<int[]> enumeratePolyominoes(int?[] thisGrid, int[] incompletePolyomino, int desiredSize, int[] usedCells)
            {
                if (incompletePolyomino.Length > desiredSize)
                    yield break;
                if (incompletePolyomino.Length == desiredSize)
                {
                    if (!incompletePolyomino.Any(cell => getAdj(cell).Any(adj => !incompletePolyomino.Contains(adj) && thisGrid[adj] == desiredSize)))
                        yield return incompletePolyomino.ToArray();
                    yield break;
                }

                var used = new List<int>(usedCells);
                foreach (var adj in incompletePolyomino.SelectMany(cell => getAdj(cell)).Distinct().Where(cell => !usedCells.Contains(cell) && !incompletePolyomino.Contains(cell) && (thisGrid[cell] == null || thisGrid[cell] == desiredSize)))
                {
                    used.Add(adj);
                    foreach (var poly in enumeratePolyominoes(thisGrid, incompletePolyomino.Insert(incompletePolyomino.Length, adj), desiredSize, used.ToArray()))
                        yield return poly;
                }
            }

            IEnumerable<int[][]> recurseFillomino(int?[] thisGrid, int[][] polysSoFar, List<(int[] cells, int desiredSize, int[][] allPossibilities)> allPolys, bool debugOut)
            {
                shortcut:
                if (polysSoFar.Length > 0)
                {
                    var orderedLast = polysSoFar.Last().Order().ToArray();
                    if (polysSoFar.SkipLast(1).Any(p => p.Order().SequenceEqual(orderedLast)))
                    {
                        foreach (var polyomino in polysSoFar)
                        {
                            Console.WriteLine(polyomino.JoinString(", "));
                            ConsoleUtil.WriteLine(Enumerable.Range(0, w * h).Select(cell => { var poly = polysSoFar.FirstOrDefault(p => p.Contains(cell)); return poly == null ? "??" : (polyomino.Contains(cell) ? "██" : "▒▒").Color((ConsoleColor) (poly.Length)); }).Split(w).Select(row => row.JoinColoredString()).JoinColoredString("\n"));
                            Console.WriteLine();
                        }
                        Debugger.Break();
                    }
                }

                if (polysSoFar.Sum(ar => ar.Length) == w * h)
                {
                    yield return polysSoFar.ToArray();
                    yield break;
                }

                if (allPolys.Count == 0)
                {
                    var firstEmptyCell = thisGrid.IndexOf(ch => ch == null);
                    if (firstEmptyCell == -1)
                    {
                        foreach (var polyomino in polysSoFar)
                        {
                            Console.WriteLine(polyomino.JoinString(", "));
                            ConsoleUtil.WriteLine(Enumerable.Range(0, w * h).Select(cell =>
                            {
                                var poly = polysSoFar.SingleOrDefault(p => p.Contains(cell));
                                return poly == null ? "??" : (polyomino.Contains(cell) ? "██" : "▒▒").Color((ConsoleColor) (poly.Length));
                            }).Split(w).Select(row => row.JoinColoredString()).JoinColoredString("\n"));
                            Console.WriteLine();
                        }
                        Console.WriteLine(polysSoFar.Sum(p => p.Length));
                        Console.WriteLine(w * h);
                        Console.WriteLine();
                    }

                    var maxLength = thisGrid.Count(ch => ch == null);

                    for (var size = 1; size <= maxLength; size++)
                    {
                        var possibilities = enumeratePolyominoes(thisGrid, new[] { firstEmptyCell }, size, new int[0]).ToArray();
                        if (possibilities.Length == 0)
                            continue;
                        var newGrid = thisGrid.ToArray();
                        newGrid[firstEmptyCell] = size;
                        foreach (var solution in recurseFillomino(newGrid, polysSoFar, new List<(int[] cells, int desiredSize, int[][] allPossibilities)> { (new[] { firstEmptyCell }, size, possibilities) }, false))
                            yield return solution;
                    }
                    yield break;
                }

                int[] bestPolyomino = null;
                var bestPolySize = 0;
                int[][] bestPossibleCompletions = null;
                int bestIx = -1;

                for (var polyIx = 0; polyIx < allPolys.Count; polyIx++)
                {
                    var (polyomino, polySize, poss) = allPolys[polyIx];
                    if (poss.Length == 0)
                        yield break;
                    if (poss.Length == 1)
                    {
                        polysSoFar = polysSoFar.Insert(polysSoFar.Length, poss[0]);
                        foreach (var possCell in poss[0])
                            thisGrid[possCell] = polySize;
                        allPolys.RemoveAt(polyIx);
                        allPolys.RemoveAll(poly => poly.cells.Intersect(poss[0]).Any() || (poly.desiredSize == poss[0].Length && poly.cells.SelectMany(c => getAdj(c)).Intersect(poss[0]).Any()));
                        for (var i = 0; i < allPolys.Count; i++)
                        {
                            var (cells, desiredSize, allPossibilities) = allPolys[i];
                            allPolys[i] = (cells, desiredSize, allPossibilities.Where(poly => !poly.Intersect(poss[0]).Any() && (desiredSize != poss[0].Length || !poly.SelectMany(c => getAdj(c)).Intersect(poss[0]).Any())).ToArray());
                            if (allPolys[i].allPossibilities.Length == 0)
                                yield break;
                        }
                        goto shortcut;
                    }
                    if (polyIx == 0 || poss.Length < bestPossibleCompletions.Length)
                    {
                        bestPolyomino = polyomino;
                        bestPolySize = polySize;
                        bestPossibleCompletions = poss;
                        bestIx = polyIx;
                    }
                }

                for (var possIx = 0; possIx < bestPossibleCompletions.Length; possIx++)
                {
                    var poss = bestPossibleCompletions[possIx];
                    var newGrid = thisGrid.ToArray();
                    foreach (var possCell in poss)
                        newGrid[possCell] = bestPolySize;
                    if (debugOut)
                        Console.WriteLine($"{new string(' ', new StackTrace().FrameCount)}Trying ({possIx}/{bestPossibleCompletions.Length}): {bestPolySize} at {poss.JoinString("+")}  \r");
                    foreach (var result in recurseFillomino(newGrid,
                        polysSoFar: polysSoFar.Insert(polysSoFar.Length, poss),
                        allPolys: allPolys
                            .Where((poly, ix) => ix != bestIx && !poly.cells.Intersect(poss).Any() && (poly.desiredSize != bestPolySize || !poly.cells.SelectMany(c => getAdj(c)).Intersect(poss).Any()))
                            .Select(tup => (tup.cells, tup.desiredSize, tup.allPossibilities.Where(poly => !poly.Intersect(poss).Any() && (tup.desiredSize != poss.Length || !poly.SelectMany(c => getAdj(c)).Intersect(poss).Any())).ToArray()))
                            .ToList(),
                        debugOut: debugOut))
                        yield return result;
                    if (debugOut)
                        Console.CursorTop -= 1;
                }
                Console.Write(new string(' ', Console.BufferWidth - 1) + "\r");
            }

            // Find all contiguous areas of equal givens
            var allAreas = new List<(int[] givenCells, int desiredSize)>();
            for (var cell = 0; cell < grid.Length; cell++)
            {
                if (grid[cell] == null || allAreas.Any(p => p.givenCells.Contains(cell)))
                    continue;

                var polyomino = new HashSet<int> { cell };
                while (true)
                {
                    var prevCount = polyomino.Count;
                    polyomino.UnionWith(polyomino.SelectMany(getAdj).Where(c => grid[c] == grid[cell]).ToHashSet());
                    if (polyomino.Count == prevCount)
                        break;
                }
                allAreas.Add((polyomino.ToArray(), grid[cell].Value));
            }

            // Find all possible polyominoes that could be placed
            var allPolyominoes = allAreas
                .ParallelSelect(Environment.ProcessorCount, tup =>
                {
                    var ret = (tup.givenCells, tup.desiredSize, allPossibilities: enumeratePolyominoes(grid, tup.givenCells, tup.desiredSize, new int[0]).ToArray());
                    if (debugOutput)
                        lock (allAreas)
                            Console.WriteLine($"{tup.desiredSize} at {tup.givenCells.JoinString("+")} has {ret.allPossibilities.Length} possibilities");
                    return ret;
                })
                .ToList();

            return recurseFillomino(grid.ToArray(), new int[0][], allPolyominoes, debugOutput);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;
using RT.Util.ExtensionMethods.Obsolete;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint that mandates that the specified <paramref name="values"/> must form a contiguous
    ///     (orthogonally connected) area.</summary>
    /// <param name="width">
    ///     Width of the puzzle grid.</param>
    /// <param name="height">
    ///     Height of the puzzle grid.</param>
    /// <param name="values">
    ///     Set of values that must form a contiguous area.</param>
    public class ContiguousAreaConstraint(int width, int height, params int[] values) : Constraint(null)
    {
        /// <summary>Width of the puzzle grid.</summary>
        public int Width { get; private set; } = width;
        /// <summary>Height of the puzzle grid.</summary>
        public int Height { get; private set; } = height;
        /// <summary>Set of values that must form a contiguous area.</summary>
        public int[] Values { get; private set; } = values ?? throw new ArgumentNullException(nameof(values));

        private static string __debugStr(SolverState state, int width, int height, int[] highlight = null) =>
            Enumerable.Range(0, width * height).Select(i => (highlight != null && highlight.Contains(i) ? "[{0}]" : " {0} ").Fmt(state[i] is int v ? "·■"[v] : '?')).Split(width).Select(row => row.JoinString()).JoinString("\n");

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            if (state.LastPlacedCell is not int cell)
                return null;

            ConstraintResult checkContiguousArea(List<int> startCells)
            {
                while (startCells.Count > 0)
                {
                    var area = FindArea(startCells[0], Width, Height, c => state[c] is int v && Values.Contains(v));
                    var escapes = area.SelectMany(c => PuzzleUtil.Orthogonal(c, Width, Height).Where(c => state[c] == null)).Distinct().Take(2).ToArray();
                    if (escapes.Length == 0)
                        for (var c = 0; c < Width * Height; c++)
                        {
                            if (state[c] == null)
                                state.MarkImpossible(c, Values.Contains);
                            else if (!area.Contains(c) && Values.Contains(state[c].Value))
                                return ConstraintResult.Violation;
                        }
                    if (escapes.Length == 1 && Enumerable.Range(0, state.GridSize).Any(ix => state[ix] is int v && Values.Contains(v) && !area.Contains(ix)))
                        state.MarkImpossible(escapes.First(), v => !Values.Contains(v));
                    startCells.RemoveAll(area.Contains);
                }
                return null;
            }

            return values.Contains(state.LastPlacedValue)
                ? checkContiguousArea([cell])
                : checkContiguousArea(PuzzleUtil.Orthogonal(cell, Width, Height).Where(adj => state[adj] is int v && Values.Contains(v)).ToList());
        }

        /// <summary>
        ///     Provides a helper method to determine if an area is contiguous.</summary>
        /// <param name="cell">
        ///     Starting cell from which to determine a contiguous area.</param>
        /// <param name="width">
        ///     Width of the puzzle grid.</param>
        /// <param name="height">
        ///     Height of the puzzle grid.</param>
        /// <param name="isValue">
        ///     Determines whether a cell contains a value that should be part of the contiguous area.</param>
        /// <returns>
        ///     A hashset containing the contiguous area.</returns>
        public static HashSet<int> FindArea(int cell, int width, int height, Func<int, bool> isValue)
        {
            var area = new HashSet<int> { cell };
            var prevAdded = new HashSet<int> { cell };
            while (true)
            {
                var newToAdd = prevAdded.SelectMany(c => PuzzleUtil.Orthogonal(c, width, height).Where(adj => !area.Contains(adj) && isValue(adj))).ToHashSet();
                if (newToAdd.Count == 0)
                    break;
                prevAdded = newToAdd;
                area.UnionWith(newToAdd);
            }
            return area;
        }
    }
}

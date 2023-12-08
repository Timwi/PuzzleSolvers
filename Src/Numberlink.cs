using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Represents a Numberlink puzzle.</summary>
    public class Numberlink : Puzzle
    {
        /// <summary>Specifies the width of the Numberlink puzzle.</summary>
        public int Width { get; private set; }
        /// <summary>Specifies the height of the Numberlink puzzle.</summary>
        public int Height { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="width">
        ///     Width of the whole puzzle grid.</param>
        /// <param name="height">
        ///     Height of the whole puzzle grid.</param>
        /// <param name="cellGroups">
        ///     Specifies sets of grid coordinates that must be linked in pairs by paths. For a traditional Numberlink in
        ///     which pairs of digits are printed in the grid, each set should consist of two cells with the same digit in
        ///     them. It is permitted to specify larger (even-sized) groups, in which case the solver must determine how they
        ///     link up in pairs.</param>
        public Numberlink(int width, int height, int[][] cellGroups) : base(width * height, 0, 6 + cellGroups.Length)
        {
            if (cellGroups == null)
                throw new ArgumentNullException(nameof(cellGroups));
            if (cellGroups.Any(v => v == null || v.Length % 2 != 0))
                throw new ArgumentException($"‘cellGroups’ must contain only arrays of even lengths.", nameof(cellGroups));

            Width = width;
            Height = height;
            AddConstraint(new PathConstraint(width, height, true));
            AddConstraint(new NumberLinkConstraint(width, height));

            for (var cell = 0; cell < width * height; cell++)
            {
                // The entire grid must be filled
                var cellGroup = cellGroups.IndexOf(cg => cg.Contains(cell));
                if (cellGroup == -1)
                    AddConstraint(new OneCellLambdaConstraint(cell, v => v >= 1 && v <= 6));
                else
                    AddConstraint(new GivenConstraint(cell, 7 + cellGroup), ConsoleColor.White, (ConsoleColor) (cellGroup % 14 + 1));

                // Path can’t run off the edge of the grid
                if (cell / width == 0)
                    AddConstraint(new OneCellLambdaConstraint(cell, v => v >= 7 || (Path.ToBits[v] & 1) == 0));
                if (cell % width == width - 1)
                    AddConstraint(new OneCellLambdaConstraint(cell, v => v >= 7 || (Path.ToBits[v] & 2) == 0));
                if (cell / width == width - 1)
                    AddConstraint(new OneCellLambdaConstraint(cell, v => v >= 7 || (Path.ToBits[v] & 4) == 0));
                if (cell % width == 0)
                    AddConstraint(new OneCellLambdaConstraint(cell, v => v >= 7 || (Path.ToBits[v] & 8) == 0));
            }

            foreach (var group in cellGroups)
                foreach (var cell in group)
                    AddConstraint(new OneConnectorConstraint(width, height, cell));
        }

        /// <summary>
        ///     Adds a constraint to the Numberlink puzzle that prohibits paths from taking U-turns, that is, two 90° turns in
        ///     immediate succession. (However, two 90° turns making an S-curve are still permitted.)</summary>
        public void AddNoUTurnConstraint()
        {
            AddConstraint(new NoUTurnConstraint(Width, Height));
        }

        private class NoUTurnConstraint(int width, int height) : Constraint(null)
        {
            public override ConstraintResult Process(SolverState state)
            {
                if (state.LastPlacedCell is not int cell || state.LastPlacedValue >= 7)
                    return null;
                var x = cell % width;
                var y = cell / width;
                if (x > 0 && (Path.ToBits[state.LastPlacedValue] & (1 << 3)) != 0)
                    state.MarkImpossible(cell - 1, Path.ToBits.IndexOf(Path.ToBits[state.LastPlacedValue] ^ 10));
                if (x < width - 1 && (Path.ToBits[state.LastPlacedValue] & (1 << 1)) != 0)
                    state.MarkImpossible(cell + 1, Path.ToBits.IndexOf(Path.ToBits[state.LastPlacedValue] ^ 10));
                if (y > 0 && (Path.ToBits[state.LastPlacedValue] & (1 << 0)) != 0)
                    state.MarkImpossible(cell - width, Path.ToBits.IndexOf(Path.ToBits[state.LastPlacedValue] ^ 5));
                if (y < height - 1 && (Path.ToBits[state.LastPlacedValue] & (1 << 2)) != 0)
                    state.MarkImpossible(cell + width, Path.ToBits.IndexOf(Path.ToBits[state.LastPlacedValue] ^ 5));
                return null;
            }
        }

        private class OneConnectorConstraint(int width, int height, int cell) : Constraint(PuzzleUtil.Orthogonal(cell, width, height))
        {
            public override ConstraintResult Process(SolverState state)
            {
                List<int> already = [], possible = [];
                foreach (var ac in AffectedCells)
                {
                    var dir = ac % width == cell % width ? ac / width == cell / width - 1 ? 2 : 0 : ac % width == cell % width - 1 ? 1 : 3;
                    if (state[ac] is int v && v < 7 && (Path.ToBits[v] & (1 << dir)) != 0)
                        already.Add(ac);
                    else if (state[ac] == null && state.Possible(ac).Any(v => v < 7 && (Path.ToBits[v] & (1 << dir)) != 0))
                        possible.Add(ac);
                }
                if (already.Count > 1)
                    return ConstraintResult.Violation;
                else if (already.Count == 1)
                {
                    foreach (var ac in possible)
                    {
                        var dir = ac % width == cell % width ? ac / width == cell / width - 1 ? 2 : 0 : ac % width == cell % width - 1 ? 1 : 3;
                        state.MarkImpossible(ac, v => v < 7 && (Path.ToBits[v] & (1 << dir)) != 0);
                    }
                    return ConstraintResult.Remove;
                }

                if (possible.Count == 0)
                    return ConstraintResult.Violation;
                else if (possible.Count == 1)
                {
                    var ac = possible[0];
                    var dir = ac % width == cell % width ? ac / width == cell / width - 1 ? 2 : 0 : ac % width == cell % width - 1 ? 1 : 3;
                    state.MarkImpossible(ac, v => v >= 7 || (Path.ToBits[v] & (1 << dir)) == 0);
                }

                return null;
            }
        }

        private class NumberLinkConstraint(int width, int height) : Constraint(null)
        {
            private (int group, bool loop)? GetCellGroupConnectedTo(int cell, int ignoreDir, SolverState state)
            {
                var x = cell % width;
                var y = cell / width;
                var dir = ignoreDir;
                while (true)
                {
                    var newDir = Enumerable.Range(0, 4).First(d => d != dir && (Path.ToBits[state[x + width * y].Value] & (1 << d)) != 0);
                    x += PuzzleUtil.Dxs[newDir];
                    y += PuzzleUtil.Dys[newDir];
                    if (state[x + width * y] is not int v)
                        return null;
                    if (v >= 7)
                        return (v - 7, false);

                    if (x + width * y == cell)
                        // We’ve created a closed loop
                        return (default, true);

                    dir = (newDir + 2) % 4;
                }
            }

            public override ConstraintResult Process(SolverState state)
            {
                if (state.LastPlacedCell is not int cell)
                    return null;
                var v = state.LastPlacedValue;
                if (v >= 7)
                {
                    // We placed a cell from a cell group
                    var x = cell % width;
                    var y = cell / width;
                    var left = x > 0 && state[cell - 1] is int cv1 && cv1 < 7 && (Path.ToBits[cv1] & (1 << 1)) != 0;
                    var right = x < width - 1 && state[cell + 1] is int cv2 && cv2 < 7 && (Path.ToBits[cv2] & (1 << 3)) != 0;
                    var up = y > 0 && state[cell - width] is int cv3 && cv3 < 7 && (Path.ToBits[cv3] & (1 << 2)) != 0;
                    var down = y < height - 1 && state[cell + width] is int cv4 && cv4 < 7 && (Path.ToBits[cv4] & (1 << 0)) != 0;
                    // Make sure that it doesn’t have more than one connected path
                    if ((left ? 1 : 0) + (right ? 1 : 0) + (up ? 1 : 0) + (down ? 1 : 0) > 1)
                        return ConstraintResult.Violation;
                    // Make sure that it doesn’t already link up to another group
                    if (left && GetCellGroupConnectedTo(cell - 1, 1, state) is (int group1, bool loop1) && (loop1 || group1 != v - 7))
                        return ConstraintResult.Violation;
                    if (right && GetCellGroupConnectedTo(cell + 1, 3, state) is (int group2, bool loop2) && (loop2 || group2 != v - 7))
                        return ConstraintResult.Violation;
                    if (up && GetCellGroupConnectedTo(cell - width, 2, state) is (int group3, bool loop3) && (loop3 || group3 != v - 7))
                        return ConstraintResult.Violation;
                    if (down && GetCellGroupConnectedTo(cell + width, 0, state) is (int group4, bool loop4) && (loop4 || group4 != v - 7))
                        return ConstraintResult.Violation;
                }
                else if (v > 0)
                {
                    // We placed a piece of a path
                    // Make sure that this doesn’t connect cells from different groups
                    var connectedTo = -1;
                    for (var dir = 0; dir < 4; dir++)
                        if ((Path.ToBits[v] & (1 << dir)) != 0 && GetCellGroupConnectedTo(cell, dir, state) is (int group, bool loop))
                        {
                            if (loop)
                                return ConstraintResult.Violation;
                            if (connectedTo == -1)
                                connectedTo = group;
                            else if (connectedTo != group)
                                return ConstraintResult.Violation;
                        }
                }
                return null;
            }
        }
    }
}

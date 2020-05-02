using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Encapsulates a puzzle.</summary>
    public sealed class Puzzle
    {
        /// <summary>
        ///     The number of cells to be filled in this puzzle.</summary>
        /// <remarks>
        ///     Note that for puzzles in which the solver must draw lines across gridlines, the “cells” are actually the
        ///     gridlines, not the squares of the grid. If a puzzle requires both gridlines as well as squares of the grid to
        ///     be filled, these must be considered separate cells, so this value must be the sum of all of them.</remarks>
        public int Size { get; private set; }

        /// <summary>
        ///     The minimum value to be placed in a cell.</summary>
        /// <remarks>
        ///     This is really only useful for number-placement puzzles in which the numerical values can be affected by
        ///     constraints (e.g. sums). For other puzzles, use integer values that represent the abstract values to be filled
        ///     in.</remarks>
        public int MinValue { get; private set; }

        /// <summary>
        ///     The maximum value to be placed in a cell.</summary>
        /// <remarks>
        ///     In conjunction with <see cref="MinValue"/>, this defines how many possible values can be in a cell.</remarks>
        public int MaxValue { get; private set; }

        /// <summary>Returns the list of constraints used by this puzzle.</summary>
        public List<Constraint> Constraints { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="size">
        ///     The number of cells in this puzzle. See <see cref="Size"/> for more information.</param>
        /// <param name="minValue">
        ///     See <see cref="MinValue"/>.</param>
        /// <param name="maxValue">
        ///     See <see cref="MaxValue"/>.</param>
        /// <remarks>
        ///     When using this constructor, be sure to populate <see cref="Constraints"/> before running <see
        ///     cref="Solve(int?, Random)"/>.</remarks>
        /// <seealso cref="Puzzle(int, int, int, IEnumerable{Constraint}[])"/>
        /// <seealso cref="Puzzle(int, int, int, IEnumerable{Constraint})"/>
        public Puzzle(int size, int minValue, int maxValue)
        {
            Size = size;
            MinValue = minValue;
            MaxValue = maxValue;
            Constraints = new List<Constraint>();
        }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="size">
        ///     The number of cells in this puzzle. See <see cref="Size"/> for more information.</param>
        /// <param name="minValue">
        ///     See <see cref="MinValue"/>.</param>
        /// <param name="maxValue">
        ///     See <see cref="MaxValue"/>.</param>
        /// <param name="constraints">
        ///     A series of constraints for this puzzle.</param>
        /// <seealso cref="Puzzle(int, int, int, IEnumerable{Constraint}[])"/>
        /// <seealso cref="Puzzle(int, int, int, IEnumerable{Constraint})"/>
        public Puzzle(int size, int minValue, int maxValue, params Constraint[] constraints)
        {
            Size = size;
            MinValue = minValue;
            MaxValue = maxValue;
            Constraints = constraints.ToList();
        }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="size">
        ///     The number of cells in this puzzle. See <see cref="Size"/> for more information.</param>
        /// <param name="minValue">
        ///     See <see cref="MinValue"/>.</param>
        /// <param name="maxValue">
        ///     See <see cref="MaxValue"/>.</param>
        /// <param name="constraints">
        ///     A series of constraints for this puzzle.</param>
        /// <seealso cref="Puzzle(int, int, int, IEnumerable{Constraint}[])"/>
        /// <seealso cref="Puzzle(int, int, int, Constraint[])"/>
        public Puzzle(int size, int minValue, int maxValue, IEnumerable<Constraint> constraints)
        {
            Size = size;
            MinValue = minValue;
            MaxValue = maxValue;
            Constraints = constraints.ToList();
        }

        /// <summary>
        ///     Convenience constructor to allow the use of several helper methods such as <see cref="Constraint.Sudoku"/>,
        ///     <see cref="Constraint.Givens(int?[])"/>, etc. The value <c>null</c> is also allowed and will simply be
        ///     skipped.</summary>
        /// <seealso cref="Puzzle(int, int, int, IEnumerable{Constraint})"/>
        /// <seealso cref="Puzzle(int, int, int, Constraint[])"/>
        public Puzzle(int size, int minValue, int maxValue, params IEnumerable<Constraint>[] constraints)
        {
            Size = size;
            MinValue = minValue;
            MaxValue = maxValue;
            Constraints = constraints.Where(cs => cs != null).SelectMany(x => x.Where(c => c != null)).ToList();
        }

        /// <summary>
        ///     Converts a Sudoku solution to a <see cref="ConsoleColoredString"/> that includes the coloring offered by some
        ///     constraints.</summary>
        /// <param name="solution">
        ///     The solution to be colored.</param>
        /// <param name="width">
        ///     The width of the puzzle grid. For a standard Sudoku, this is 9.</param>
        public ConsoleColoredString SudokuSolutionToConsoleString(int?[] solution, int width = 9)
        {
            ConsoleColor? findColor(int ix) => Constraints.Where(c => c.AffectedCells.Contains(ix)).Aggregate((ConsoleColor?) null, (prev, c) => prev ?? c.CellColor);
            ConsoleColor? findBackgroundColor(int ix) => Constraints.Where(c => c.AffectedCells.Contains(ix)).Aggregate((ConsoleColor?) null, (prev, c) => prev ?? c.CellBackgroundColor);
            return solution.Split(width).Select((chunk, row) => chunk.Select((val, col) => ((val == null ? "?" : val.Value.ToString()) + " ").Color(val == null ? ConsoleColor.DarkGray : findColor(col + width * row), findBackgroundColor(col + width * row))).JoinColoredString()).JoinColoredString("\n");
        }

        /// <summary>
        ///     Converts a Sudoku solution to a <see cref="ConsoleColoredString"/> that includes the coloring offered by some
        ///     constraints.</summary>
        /// <param name="solution">
        ///     The solution to be colored.</param>
        /// <param name="width">
        ///     The width of the puzzle grid. For a standard Sudoku, this is 9.</param>
        public ConsoleColoredString SudokuSolutionToConsoleString(int[] solution, int width = 9) => SudokuSolutionToConsoleString(solution.Select(val => val.Nullable()).ToArray(), width);



        // Implementation of the solver

        private int _numVals;

        /// <summary>Returns a lazy sequence containing the solutions for this puzzle.</summary>
        public IEnumerable<int[]> Solve(int? showDebugOutput = null, Random randomizer = null)
        {
            _numVals = MaxValue - MinValue + 1;
            var cells = new int?[Size];
            var takens = new bool[Size][];
            for (var i = 0; i < Size; i++)
                takens[i] = new bool[_numVals];

            var newConstraints = new List<Constraint>();
            foreach (var constraint in Constraints)
            {
                var cs = constraint.MarkTakens(takens, cells, null, MinValue, MaxValue);
                if (cs == null)
                    newConstraints.Add(constraint);
                else
                    newConstraints.AddRange(cs);
            }

            var numConstraintsPerCell = new int[Size];
            foreach (var constraint in newConstraints)
                foreach (var cell in constraint.AffectedCells ?? Enumerable.Range(0, Size))
                    numConstraintsPerCell[cell]++;

            return solve(cells, takens, newConstraints, numConstraintsPerCell, showDebugOutput, randomizer).Select(solution => solution.Select(val => val + MinValue).ToArray());
        }

        private IEnumerable<int[]> solve(int?[] filledInValues, bool[][] takens, List<Constraint> constraints, int[] numConstraintsPerCell, int? showDebugOutput, Random randomizer, int recursionDepth = 0)
        {
            var fewestPossibleValues = int.MaxValue;
            var ix = -1;
            for (var cell = 0; cell < Size; cell++)
            {
                if (filledInValues[cell] != null)
                    continue;
                var count = 0;
                for (var v = 0; v < takens[cell].Length; v++)
                    if (!takens[cell][v])
                        count++;
                if (count == 0)
                    yield break;
                count -= numConstraintsPerCell[cell];
                if (count < fewestPossibleValues)
                {
                    ix = cell;
                    fewestPossibleValues = count;
                }
            }

            if (ix == -1)
            {
                yield return filledInValues.Select(val => val.Value).ToArray();
                yield break;
            }

            var startAt = randomizer != null ? randomizer.Next(0, takens[ix].Length) : 0;

            for (var tVal = 0; tVal < takens[ix].Length; tVal++)
            {
                var val = (tVal + startAt) % takens[ix].Length;
                if (takens[ix][val])
                    continue;

                if (showDebugOutput != null && recursionDepth < showDebugOutput.Value)
                {
                    Console.CursorLeft = 0;
                    Console.CursorTop = recursionDepth;
                    ConsoleUtil.Write($"Cell {ix}: " + Enumerable.Range(0, takens[ix].Length).Select(v => (v + MinValue).ToString().Color(
                        takens[ix][v] ? ConsoleColor.DarkBlue : v == val ? ConsoleColor.Yellow : ConsoleColor.DarkCyan,
                        v == val ? ConsoleColor.DarkGreen : ConsoleColor.Black)).JoinColoredString(" "));
                }

                // Attempt to put the value into this cell
                filledInValues[ix] = val;
                var takensCopy = takens.Select(arr => arr.ToArray()).ToArray();

                // If placing this value modifies any of the constraints, we need to take a copy of the list of constraints,
                // but for performance reasons we want to keep the original list if none of the constraints changed
                List<Constraint> constraintsCopy = null;

                for (var i = 0; i < constraints.Count; i++)
                {
                    var constraint = constraints[i];
                    var newConstraints = constraint.MarkTakens(takensCopy, filledInValues, ix, MinValue, MaxValue);
                    if (newConstraints != null)
                    {
                        // A constraint changed. That means we definitely need a new array of constraints for the recursive call.
                        if (constraintsCopy == null)
                            constraintsCopy = new List<Constraint>(constraints.Take(i));
                        var constraintIx = constraintsCopy.Count;
                        constraintsCopy.AddRange(newConstraints);

                        // Tell the new constraints to update on the basis of all of the values that are already placed.
                        // Note that if these constraints now return even more constraints, these will not be correctly processed.
                        // At present, this is considered part of the contract, so violations will throw an exception.
                        // It may be debatable whether this needs to be supported.
                        for (var cIx = constraintIx; cIx < constraintsCopy.Count; cIx++)
                        {
                            var yetMoreConstraints = constraintsCopy[cIx].MarkTakens(takensCopy, filledInValues, null, MinValue, MaxValue);
                            if (yetMoreConstraints != null)
                                throw new NotImplementedException(string.Format(
                                    @"While entering a {0} in cell {1}, a {2} returned a {3}. When calling MarkTakens on this new constraint, it returned yet further constraints. This scenario is not currently supported by the algorithm. The constraints returned from MarkTaken() must not themselves return further constraints when MarkTaken() is called on them for values already placed in the grid.",
                                    val + MinValue, ix, constraint.GetType().Name, constraintsCopy[cIx].GetType().Name));
                        }
                    }
                    else if (constraintsCopy != null)
                        constraintsCopy.Add(constraint);
                }

                foreach (var solution in solve(filledInValues, takensCopy, constraintsCopy ?? constraints, numConstraintsPerCell, showDebugOutput, randomizer, recursionDepth + 1))
                    yield return solution;
            }
            filledInValues[ix] = null;

            if (showDebugOutput != null && recursionDepth < showDebugOutput.Value)
            {
                Console.CursorLeft = 0;
                Console.CursorTop = recursionDepth;
                Console.Write(new string(' ', Console.BufferWidth - 1));
            }
        }
    }
}

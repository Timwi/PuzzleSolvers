using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a puzzle.</summary>
    /// <param name="size">
    ///     The number of cells in this puzzle. See <see cref="GridSize"/> for more information.</param>
    /// <param name="minValue">
    ///     See <see cref="MinValue"/>.</param>
    /// <param name="maxValue">
    ///     See <see cref="MaxValue"/>.</param>
    /// <remarks>
    ///     When using this constructor, be sure to populate <see cref="Constraints"/> before running <see
    ///     cref="Solve(SolverInstructions)"/>.</remarks>
    public class Puzzle(int size, int minValue, int maxValue)
    {
        /// <summary>
        ///     The number of cells to be filled in this puzzle.</summary>
        /// <remarks>
        ///     Note that for puzzles in which the solver must draw lines across gridlines, the “cells” are actually the
        ///     gridlines, not the squares of the grid. If a puzzle requires both gridlines as well as squares of the grid to
        ///     be filled, these must be considered separate cells, so this value must be the sum of all of them.</remarks>
        public int GridSize { get; private set; } = size;

        /// <summary>
        ///     The minimum value to be placed in a cell.</summary>
        /// <remarks>
        ///     This is really only useful for number-placement puzzles in which the numerical values can be affected by
        ///     constraints (e.g. sums). For other puzzles, use integer values that represent the abstract values to be filled
        ///     in.</remarks>
        public int MinValue { get; private set; } = minValue;

        /// <summary>
        ///     The maximum value to be placed in a cell.</summary>
        /// <remarks>
        ///     In conjunction with <see cref="MinValue"/>, this defines how many possible values can be in a cell.</remarks>
        public int MaxValue { get; private set; } = maxValue;

        /// <summary>Returns the list of constraints used by this puzzle.</summary>
        public List<Constraint> Constraints { get; private set; } = [];

        /// <summary>Contains colors for use by <see cref="SolutionToConsole(int[], Func{int, string}, int)"/>.</summary>
        public Dictionary<Constraint, (ConsoleColor? foreground, ConsoleColor? background)> ConstraintColors { get; private set; } = [];

        /// <summary>
        ///     Converts a partial puzzle solution to a <see cref="ConsoleColoredString"/> that includes the coloring offered
        ///     by some constraints.</summary>
        /// <param name="solution">
        ///     The partial solution to be colored.</param>
        /// <param name="width">
        ///     The width of the puzzle grid. For a standard Sudoku, this is 9.</param>
        /// <param name="getName">
        ///     An optional function to stringify values differently. The function receives the cell value and its index
        ///     within the puzzle solution. Default is to represent them as integers.</param>
        public ConsoleColoredString SolutionToConsole(int?[] solution, Func<int?, int, string> getName = null, int width = 9)
        {
            var digits = solution.Select((v, ix) => getName == null ? v.ToString().Length.ClipMin(2) : getName(v, ix).Length).Max();
            return solution.Select((val, ix) =>
            {
                var firstConstraint = Constraints.FirstOrDefault(c => c.AffectedCells != null && c.AffectedCells.Contains(ix) && ConstraintColors.ContainsKey(c));
                var (foreground, background) = firstConstraint == null ? default : ConstraintColors.Get(firstConstraint, default);
                return ((getName != null ? getName(val, ix) : val == null ? "?" : val.Value.ToString()).PadLeft(digits)).Color(val == null ? ConsoleColor.DarkGray : foreground, background);
            }).Split(width).Select(row => row.JoinColoredString()).JoinColoredString("\n");
        }

        /// <summary>
        ///     Converts a partial puzzle solution to a <see cref="ConsoleColoredString"/> that includes the coloring offered
        ///     by some constraints.</summary>
        /// <param name="solution">
        ///     The partial solution to be colored.</param>
        /// <param name="width">
        ///     The width of the puzzle grid. For a standard Sudoku, this is 9.</param>
        /// <param name="getName">
        ///     An optional function to stringify values differently. Default is to represent them as integers.</param>
        public ConsoleColoredString SolutionToConsole(int?[] solution, Func<int?, string> getName, int width = 9) =>
            SolutionToConsole(solution, getName == null ? null : (v, ix) => getName(v), width);

        /// <summary>
        ///     Converts a puzzle solution to a <see cref="ConsoleColoredString"/> that includes the coloring offered by some
        ///     constraints.</summary>
        /// <param name="solution">
        ///     The solution to be colored.</param>
        /// <param name="width">
        ///     The width of the puzzle grid. For a standard Sudoku, this is 9.</param>
        /// <param name="getName">
        ///     An optional function to stringify values differently. The function receives the cell value and its index
        ///     within the puzzle solution. Default is to represent them as integers.</param>
        public ConsoleColoredString SolutionToConsole(int[] solution, Func<int, int, string> getName = null, int width = 9) =>
            SolutionToConsole(solution.Select(val => val.Nullable()).ToArray(), getName == null ? null : (v, ix) => getName(v.Value, ix), width);

        /// <summary>
        ///     Converts a puzzle solution to a <see cref="ConsoleColoredString"/> that includes the coloring offered by some
        ///     constraints.</summary>
        /// <param name="solution">
        ///     The solution to be colored.</param>
        /// <param name="width">
        ///     The width of the puzzle grid. For a standard Sudoku, this is 9.</param>
        /// <param name="getName">
        ///     An optional function to stringify values differently. Default is to represent them as integers.</param>
        public ConsoleColoredString SolutionToConsole(int[] solution, Func<int, string> getName, int width = 9) =>
            SolutionToConsole(solution.Select(val => val.Nullable()).ToArray(), getName == null ? null : (v, ix) => getName(v.Value), width);

        /// <summary>Adds the specified <paramref name="constraint"/> to the <see cref="Constraints"/> list.</summary>
        public Puzzle AddConstraint(Constraint constraint, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (constraint == null)
                throw new ArgumentNullException(nameof(constraint));
            Constraints.Add(constraint);
            if (foreground != null || background != null)
                ConstraintColors[constraint] = (foreground, background);
            return this;
        }

        /// <summary>
        ///     Adds the specified <paramref name="constraints"/> to the <see cref="Constraints"/> list.</summary>
        /// <param name="constraints">
        ///     The constraints to be added.</param>
        /// <remarks>
        ///     The constraints are automatically colored using 7 console colors in a cyclic sequence, starting from DarkBlue.</remarks>
        public Puzzle AddConstraints(params Constraint[] constraints) => AddConstraints(constraints, avoidColors: false);

        /// <summary>
        ///     Adds the specified <paramref name="constraints"/> to the <see cref="Constraints"/> list.</summary>
        /// <param name="constraints">
        ///     The constraints to be added.</param>
        /// <param name="avoidColors">
        ///     If <c>false</c> (the default), the constraints are automatically colored using 7 console colors in a cyclic
        ///     sequence, starting from DarkBlue. Use <c>true</c> to avoid adding the colors.</param>
        public Puzzle AddConstraints(IEnumerable<Constraint> constraints, bool avoidColors = false)
        {
            if (constraints == null)
                throw new ArgumentNullException(nameof(constraints));
            if (constraints.Contains(null))
                throw new ArgumentException("‘constraints’ cannot contain a null value.", nameof(constraints));
            var background = ConsoleColor.DarkBlue;
            foreach (var constraint in constraints)
            {
                AddConstraint(constraint, avoidColors ? null : ConsoleColor.White, avoidColors ? null : background);
                background = (ConsoleColor) (((int) background % 7) + 1);
            }
            return this;
        }

        /// <summary>
        ///     Adds a range of <see cref="GivenConstraint"/>s from the specified array of tuples.</summary>
        /// <param name="givens">
        ///     An array of tuples containing cell indexes and given values.</param>
        public Puzzle AddGivens(params (int cell, int value)[] givens)
        {
            if (givens == null)
                throw new ArgumentNullException(nameof(givens));
            foreach (var (cell, value) in givens)
            {
                if (cell < 0 || cell >= GridSize)
                    throw new ArgumentOutOfRangeException("‘cell’ cannot be negative and must be less than the size given by GridSize.", nameof(givens));
                if (value < MinValue || value > MaxValue)
                    throw new ArgumentOutOfRangeException("‘value’ cannot be outside the range given by MinValue and MaxValue.", nameof(givens));
            }
            foreach (var (cell, value) in givens)
                AddConstraint(new GivenConstraint(cell, value));
            return this;
        }

        /// <summary>
        ///     Adds a range of <see cref="GivenConstraint"/>s from the specified collection of tuples.</summary>
        /// <param name="givens">
        ///     A collection of tuples containing cell indexes and given values.</param>
        /// <param name="foreground">
        ///     Color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string}, int)"/>.</param>
        /// <param name="background">
        ///     Background color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string},
        ///     int)"/>.</param>
        public Puzzle AddGivens(IEnumerable<(int cell, int value)> givens, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (givens == null)
                throw new ArgumentNullException(nameof(givens));
            foreach (var (cell, value) in givens)
            {
                if (cell < 0 || cell >= GridSize)
                    throw new ArgumentOutOfRangeException("‘cell’ cannot be negative and must be less than the size given by GridSize.", nameof(givens));
                if (value < MinValue || value > MaxValue)
                    throw new ArgumentOutOfRangeException("‘value’ cannot be outside the range given by MinValue and MaxValue.", nameof(givens));
            }
            foreach (var (cell, value) in givens)
                AddConstraint(new GivenConstraint(cell, value), foreground, background);
            return this;
        }

        /// <summary>
        ///     Adds a range of <see cref="GivenConstraint"/>s from the specified string representation.</summary>
        /// <param name="givens">
        ///     A string such as <c>"3...5...8.9..7.5.....8.41...2.7.....5...28..47.....6...6....8....2...9.1.1.9.5..."</c>.
        ///     Each digit is a given value, while periods (<c>.</c>) indicate no given for that cell.</param>
        /// <param name="foreground">
        ///     Color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string}, int)"/>.</param>
        /// <param name="background">
        ///     Background color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string},
        ///     int)"/>.</param>
        public Puzzle AddGivens(string givens, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (givens == null)
                throw new ArgumentNullException(nameof(givens));
            if (givens.Length > GridSize)
                throw new ArgumentException("The length of ‘givens’ cannot be greater than the size of the puzzle.", nameof(givens));
            if (!givens.All(ch => ch == '.' || (ch >= '0' && ch <= '9')))
                throw new ArgumentException("‘givens’ must contain only digits 0–9 and periods (.) for cells with no given.", "givens");
            for (var i = 0; i < givens.Length; i++)
                if (givens[i] != '.')
                    AddConstraint(new GivenConstraint(i, givens[i] - '0'), foreground, background);
            return this;
        }

        /// <summary>
        ///     Adds a range of <see cref="GivenConstraint"/>s from the specified array representation.</summary>
        /// <param name="givens">
        ///     An array containing integers and <c>null</c> values. Each non-<c>null</c> value translates to a given for the
        ///     cell in the same position.</param>
        /// <param name="foreground">
        ///     Color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string}, int)"/>.</param>
        /// <param name="background">
        ///     Background color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string},
        ///     int)"/>.</param>
        public Puzzle AddGivens(int?[] givens, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (givens == null)
                throw new ArgumentNullException(nameof(givens));
            if (givens.Length > GridSize)
                throw new ArgumentException("The length of ‘givens’ cannot be greater than the size of the puzzle.", nameof(givens));
            if (!givens.All(val => val == null || (val >= MinValue && val <= MaxValue)))
                throw new ArgumentException("‘givens’ must contain only values within the range given by MinValue and MaxValue.", "givens");
            for (var i = 0; i < givens.Length; i++)
                if (givens[i] != null)
                    AddConstraint(new GivenConstraint(i, givens[i].Value), foreground, background);
            return this;
        }

        /// <summary>
        ///     Adds a cage (region) for a Killer Sudoku. This is just a <see cref="SumConstraint"/> and a <see
        ///     cref="UniquenessConstraint"/> for the same region.</summary>
        /// <param name="sum">
        ///     The desired sum for the cage.</param>
        /// <param name="affectedCells">
        ///     The set of cells contained in this cage.</param>
        /// <param name="foreground">
        ///     Color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string}, int)"/>.</param>
        /// <param name="background">
        ///     Background color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string},
        ///     int)"/>.</param>
        /// <returns>
        ///     A collection containing the two required constraints.</returns>
        public Puzzle AddKillerCage(int sum, IEnumerable<int> affectedCells, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (affectedCells == null)
                throw new ArgumentNullException(nameof(affectedCells));
            AddConstraint(new SumConstraint(sum, affectedCells), foreground, background);
            AddConstraint(new UniquenessConstraint(affectedCells), foreground, background);
            return this;
        }

        /// <summary>
        ///     Adds a cage (region) for a Killer Sudoku. This is just a <see cref="SumConstraint"/> and a <see
        ///     cref="UniquenessConstraint"/> for the same region.</summary>
        /// <param name="sum">
        ///     The desired sum for the cage.</param>
        /// <param name="affectedCells">
        ///     A string representation of the set of cells contained in this cage, in the format understood by <see
        ///     cref="Constraint.TranslateCoordinates(string, int)"/>.</param>
        /// <param name="gridWidth">
        ///     The width of the puzzle grid.</param>
        /// <param name="foreground">
        ///     Color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string}, int)"/>.</param>
        /// <param name="background">
        ///     Background color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string},
        ///     int)"/>.</param>
        /// <returns>
        ///     A collection containing the two required constraints.</returns>
        public Puzzle AddKillerCage(int sum, string affectedCells, int gridWidth = 9, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (affectedCells == null)
                throw new ArgumentNullException(nameof(affectedCells));
            var affectedCellsTranslated = Constraint.TranslateCoordinates(affectedCells, gridWidth);
            AddConstraint(new SumConstraint(sum, affectedCellsTranslated), foreground, background);
            AddConstraint(new UniquenessConstraint(affectedCellsTranslated), foreground, background);
            return this;
        }


        // Implementation of the solver

        private int _numVals;

        /// <summary>Returns a lazy sequence containing the solutions for this puzzle.</summary>
        public IEnumerable<int[]> Solve(SolverInstructions solverInstructions = null)
        {
            if (solverInstructions != null && solverInstructions.IntendedSolution == null && solverInstructions.ExamineConstraint != null)
                throw new InvalidOperationException(@"solverInstructions.ExamineConstraints cannot be specified when solverInstructions.IntendedSolution is null.");
            if (solverInstructions != null && solverInstructions.IntendedSolution != null && solverInstructions.IntendedSolution.Length != GridSize)
                throw new InvalidOperationException(@"solverInstructions.IntendedSolution must have the same length as the size of the puzzle.");

            _numVals = MaxValue - MinValue + 1;
            var grid = new int?[GridSize];
            var takens = new bool[GridSize][];
            for (var i = 0; i < GridSize; i++)
                takens[i] = new bool[_numVals];

            var constraintsUse = Constraints;
            var state = new SolverStateImpl { Takens = takens, Grid = grid, GridSizeVal = GridSize, LastPlacedIx = null, MinVal = MinValue, MaxVal = MaxValue };
            reevaluate:
            List<Constraint> newConstraints = null;
            for (var cIx = 0; cIx < constraintsUse.Count; cIx++)
            {
                var constraint = constraintsUse[cIx];
                state.CurrentConstraint = constraint;
                var cs = constraint.Process(state);
                if (cs is ConstraintReplace repl)
                {
                    newConstraints ??= constraintsUse.Take(cIx).ToList();
                    newConstraints.AddRange(repl.NewConstraints);
                }
                else
                    newConstraints?.Add(constraint);
            }
            if (newConstraints != null)
            {
                constraintsUse = newConstraints;
                goto reevaluate;
            }

            var cellPriorities = Enumerable.Range(0, GridSize).Select(cell =>
            {
                var applicableConstraints = constraintsUse.Where(c => c.AffectedCells == null || c.AffectedCells.Contains(cell)).ToArray();
                return (cell, priority: applicableConstraints.OfType<CombinationsConstraint>().MinOrDefault(c => c.Combinations.Length, 0) - applicableConstraints.Length);
            }).ToArray();
            Array.Sort(cellPriorities, (a, b) =>
            {
                if (solverInstructions != null && solverInstructions.CellPriority != null)
                {
                    var p1 = Array.IndexOf(solverInstructions.CellPriority, a.cell);
                    var p2 = Array.IndexOf(solverInstructions.CellPriority, b.cell);
                    if (p1 != -1 && p2 != -1)
                        return p1 < p2 ? -1 : 1;
                    if (p1 != -1)
                        return -1;
                    if (p2 != -1)
                        return 1;
                }
                return a.priority < b.priority ? -1 : a.priority > b.priority ? 1 : 0;
            });
            var cellPriority = cellPriorities.Select(tup => tup.cell).ToArray();

            if (solverInstructions?.BulkLoggingFile != null)
                lock (solverInstructions?.LockObject ?? _fallbackLockObject)
                    File.WriteAllText(solverInstructions.BulkLoggingFile, "");

            return solve(grid, takens, constraintsUse, cellPriority, solverInstructions).Select(solution => solution.Select(val => val + MinValue).ToArray());
        }

        private readonly object _fallbackLockObject = new();

        private IEnumerable<int[]> solve(int?[] grid, bool[][] takens, List<Constraint> constraints, int[] cellPriority, SolverInstructions instr, int recursionDepth = 0)
        {
            var fewestPossibleValues = int.MaxValue;
            var fewestCombinations = int.MaxValue;
            var ix = -1;
            var startIx = instr?.Randomizer?.Next(0, cellPriority.Length) ?? 0;
            for (var cpIx = 0; cpIx < cellPriority.Length; cpIx++)
            {
                var cell = cellPriority[(cpIx + startIx) % cellPriority.Length];
                if (grid[cell] != null)
                    continue;
                var count = 0;
                for (var v = 0; v < takens[cell].Length; v++)
                    if (!takens[cell][v])
                        count++;
                if (count == 0)
                    yield break;

                if (count > fewestPossibleValues)
                    continue;
                var numComb = constraints.Where(c => c.NumCombinations != null && (c.AffectedCells == null || c.AffectedCells.Contains(cell))).MinOrNull(c => c.NumCombinations.Value);
                if (count < fewestPossibleValues || (numComb != null && numComb.Value < fewestCombinations))
                {
                    ix = cell;
                    fewestPossibleValues = count;
                    fewestCombinations = numComb != null ? numComb.Value : count < fewestPossibleValues ? int.MaxValue : fewestCombinations;
                    if (count == 1)
                        goto immediate;
                }
            }

            if (ix == -1)
            {
                var solutionArr = grid.Select(val => val.Value).ToArray();
                if (instr?.BulkLoggingFile != null)
                    lock (instr?.LockObject ?? _fallbackLockObject)
                        File.AppendAllText(instr.BulkLoggingFile, $"{new string(' ', recursionDepth)}Solution found: {solutionArr.JoinString()}\n");
                yield return solutionArr;
                yield break;
            }

            immediate:
            var startAt = instr?.Randomizer?.Next(0, takens[ix].Length) ?? instr?.ValuePriority ?? 0;
            var state = new SolverStateImpl { Grid = grid, GridSizeVal = GridSize, MinVal = MinValue, MaxVal = MaxValue, Takens = takens };
            var showContinuousProgress = fewestPossibleValues > 1 && instr != null && instr.ShowContinuousProgress != null && recursionDepth < instr.ShowContinuousProgress.Value;
            var showContinuousProgressLineLen = 0;

            for (var tVal = 0; tVal < takens[ix].Length; tVal++)
            {
                var val = (tVal + startAt) % takens[ix].Length;
                if (takens[ix][val])
                    continue;

                if (instr?.BulkLoggingFile != null)
                    lock (instr?.LockObject ?? _fallbackLockObject)
                        File.AppendAllText(instr.BulkLoggingFile, $"{new string(' ', recursionDepth)}Cell {ix} trying {val}\n");

                if (showContinuousProgress)
                {
                    var output = new ConsoleColoredString($"Cell {(instr.GetCellName == null ? ix.ToString().PadLeft((takens.Length - 1).ToString().Length) : instr.GetCellName(ix))}: " + Enumerable.Range(0, takens[ix].Length).Select(i => (i + startAt) % takens[ix].Length).Where(v => !instr.ShowContinuousProgressShortened || !takens[ix][v]).Select(v => (instr.GetValueName?.Invoke(v) ?? (v + MinValue).ToString()).Color(
                        takens[ix][v] ? ConsoleColor.DarkBlue : v == val ? ConsoleColor.Yellow : ConsoleColor.DarkCyan,
                        v == val ? ConsoleColor.DarkGreen : ConsoleColor.Black)).JoinColoredString(" "));
                    showContinuousProgressLineLen = Math.Max(showContinuousProgressLineLen, output.Length);
                    lock (instr?.LockObject ?? _fallbackLockObject)
                    {
                        Console.CursorLeft = instr.ShowContinuousProgressConsoleLeft ?? 0;
                        Console.CursorTop = recursionDepth + (instr.ShowContinuousProgressConsoleTop ?? 0);
                        ConsoleUtil.Write(output);
                    }
                }

                // Attempt to put the value into this cell
                grid[ix] = val;
                state.LastPlacedIx = ix;
                state.Takens = takens.Select(arr => arr.ToArray()).ToArray();

                List<Constraint> constraintsUse = constraints;
                List<Constraint> mustReevaluate = null;

                reevaluate:
                List<Constraint> newConstraintsUse = null;
                List<Constraint> newMustReevaluate = null;

                for (var i = 0; i < constraintsUse.Count; i++)
                {
                    var constraint = constraintsUse[i];
                    state.CurrentConstraint = constraint;

                    ConstraintResult processResult = null;
                    if ((state.LastPlacedIx != null && (constraint.AffectedCells == null || constraint.AffectedCells.Contains(ix))) || (mustReevaluate != null && mustReevaluate.Contains(constraint)))
                    {
                        var doExamine = instr != null && (instr.ExamineConstraint == null || instr.ExamineConstraint(constraint));
                        var wasIntendedSolutionPossible = doExamine && intendedSolutionPossible(instr, state);
                        var takensDebugCopy = wasIntendedSolutionPossible ? state.Takens.Select(b => (bool[]) b.Clone()).ToArray() : null;

                        // CALL THE CONSTRAINT
                        processResult = constraint.Process(state);
                        var isViolation = processResult is ConstraintViolation;

                        // If the intended solution was previously possible but not anymore, output the requested debug information
                        if (wasIntendedSolutionPossible && (isViolation || !intendedSolutionPossible(instr, state)))
                            lock (instr?.LockObject ?? _fallbackLockObject)
                            {
                                if (isViolation)
                                    ConsoleUtil.WriteLine("Constraint {0/Magenta} considered this state a violation at recursion depth {1}:".Color(ConsoleColor.White).Fmt(constraint.GetType().FullName, recursionDepth));
                                else
                                    ConsoleUtil.WriteLine("Constraint {0/Magenta} removed the intended solution at recursion depth {1}:".Color(ConsoleColor.White).Fmt(constraint.GetType().FullName, recursionDepth));
                                var numDigits = instr.GetCellName == null ? (GridSize - 1).ToString().Length : Enumerable.Range(0, GridSize).Max(c => instr.GetCellName(c).Length);
                                for (var cell = 0; cell < GridSize; cell++)
                                {
                                    string valueId(int v) => instr.GetValueName != null ? instr.GetValueName(v + MinValue) : (v + MinValue).ToString();
                                    var cellLine = Enumerable.Range(0, MaxValue - MinValue + 1)
                                        .Select(v => valueId(v).Color(takensDebugCopy[cell][v] != state.Takens[cell][v] ? ConsoleColor.Red : state.Takens[cell][v] ? ConsoleColor.DarkGray : ConsoleColor.Yellow))
                                        .JoinColoredString(" ");
                                    var cellStr = (instr.GetCellName != null ? instr.GetCellName(cell) : cell.ToString()).PadLeft(numDigits, ' ') + ". ";
                                    ConsoleUtil.WriteLine($"{cellStr.Color(cell == ix ? ConsoleColor.Cyan : ConsoleColor.DarkCyan)}{cellLine}   {(grid[cell] == null ? "?".Color(ConsoleColor.DarkGreen) : valueId(grid[cell].Value).Color(ConsoleColor.Green))}", null);
                                }
                                Console.WriteLine();
                            }

                        if (isViolation)
                            goto digitBusted;
                    }

                    // This code MUST be kept outside of the “if” above. This is because its “else” clause needs to run even if the above “if” didn’t.
                    if (processResult is ConstraintReplace repl)
                    {
                        // A constraint changed. That means we definitely need a new list of constraints for the recursive call.
                        if (newConstraintsUse == null)
                        {
                            newConstraintsUse = constraintsUse.Take(i).ToList();
                            newMustReevaluate ??= [];
                        }
                        var cIx = newConstraintsUse.Count;
                        newConstraintsUse.AddRange(repl.NewConstraints);
                        newMustReevaluate.AddRange(newConstraintsUse.Skip(cIx));
                    }
                    else
                        newConstraintsUse?.Add(constraint);
                }

                // Check if any reevaluatable constraints affect a cell that changed
                if (state.TakensChanged != null)
                    foreach (var constraint in constraintsUse)
                        if (constraint.CanReevaluate && (constraint.AffectedCells == null
                            ? state.TakensChanged.Any(tup => tup.changed && tup.initiator != constraint)
                            : constraint.AffectedCells.Any(c => state.TakensChanged[c].changed && state.TakensChanged[c].initiator != constraint)))
                        {
                            newMustReevaluate ??= [];
                            newMustReevaluate.Add(constraint);
                        }

                if (newConstraintsUse != null || newMustReevaluate != null)
                {
                    constraintsUse = newConstraintsUse ?? constraintsUse;
                    mustReevaluate = newMustReevaluate;
                    state.LastPlacedIx = null;
                    state.ClearTakensChanged();
                    goto reevaluate;
                }

                // Allow this list to be garbage-collected
                mustReevaluate = null;

                foreach (var solution in solve(grid, state.Takens, constraintsUse, cellPriority, instr, fewestPossibleValues == 1 ? recursionDepth : recursionDepth + 1))
                    yield return solution;

                digitBusted:;
            }
            grid[ix] = null;

            if (showContinuousProgress)
                lock (instr?.LockObject ?? _fallbackLockObject)
                {
                    Console.CursorLeft = instr.ShowContinuousProgressConsoleLeft ?? 0;
                    Console.CursorTop = recursionDepth + (instr.ShowContinuousProgressConsoleTop ?? 0);
                    Console.WriteLine(new string(' ', showContinuousProgressLineLen));
                }
        }

        bool intendedSolutionPossible(SolverInstructions instr, SolverStateImpl state) =>
            instr != null && instr.IntendedSolution != null &&
            instr.IntendedSolution.All((v, cell) => state.Grid[cell] == v - MinValue || (state.Grid[cell] == null && !state.Takens[cell][v - MinValue]));

        enum __debugSvgLabels { Numbers, Letters, Loop, Shading }

        void __debug_generateSvg(SolverStateImpl state, int recursionDepth = 0, int[] intendedSolution = null, IEnumerable<int> highlightIxs = null, int width = 9, int height = 9, int values = 9, int wrap = 3, __debugSvgLabels labels = __debugSvgLabels.Numbers)
        {
            string cnv(int val) =>
                labels == __debugSvgLabels.Shading && val.IsBetween(0, 1) ? val == 0 ? "·" : "■" :
                labels == __debugSvgLabels.Loop && val >= 0 && val <= 6 ? Path.ToChar[val].ToString() :
                labels == __debugSvgLabels.Letters ? ((char) ('A' + val)).ToString() : (val + state.MinVal).ToString();
            File.WriteAllText(@"D:\temp\temp.svg", $@"
                <svg viewBox='-.1 -.1 {width + 1.2} {width + .2}' xmlns='http://www.w3.org/2000/svg' text-anchor='middle' font-family='Work Sans'>
                    {Enumerable.Range(0, width * height).Select(cell => $@"
                        <rect x='{cell % width}' y='{cell / width}' width='1' height='1' stroke='black' stroke-width='{(highlightIxs != null && highlightIxs.Contains(cell) ? .05 : .01)}' fill='{(intendedSolution != null && (state.Grid[cell] != null ? (state.Grid[cell].Value != intendedSolution[cell] - state.MinVal) : (state.Takens[cell][intendedSolution[cell] - state.MinVal])) ? "rgba(255, 0, 0, .1)" : state[cell] != null ? "rgba(0, 192, 0, .1)" : "none")}' />
                        {(state.Grid[cell] != null
                            ? $"<text x='{cell % width + .5}' y='{cell / width + .8}' font-size='.8'>{cnv(state.Grid[cell].Value)}</text>"
                            : Enumerable.Range(0, values).Where(v => !state.Takens[cell][v]).Select(v => $"<text x='{cell % width + 1d / (wrap + 1) * (1 + v % wrap)}' y='{cell / width + 1d / (wrap + 1) * (1 + v / wrap) + .1}' font-size='.2'>{cnv(v)}</text>").JoinString()
                        )}
                    ").JoinString()}
                    <text text-anchor='left' x='{width + .1}' y='1' font-size='.1'>{recursionDepth}</text>
                </svg>
            ");
        }

        string __debug_string(SolverStateImpl state, int width, string chars = null) =>
            Enumerable.Range(0, state.GridSize).Select(ix => state[ix] == null ? "?" : chars == null ? state[ix].Value.ToString() : chars[state[ix].Value].ToString())
                .Split(width).Select(row => row.JoinString(" ")).JoinString("\n");

        sealed class SolverStateImpl : SolverState
        {
            internal int GridSizeVal;
            internal int?[] Grid;
            internal bool[][] Takens;
            internal int? LastPlacedIx;
            internal int MinVal;
            internal int MaxVal;
            internal (bool changed, Constraint initiator)[] TakensChanged;
            internal Constraint CurrentConstraint;

            internal void ClearTakensChanged()
            {
                if (TakensChanged == null)
                    TakensChanged = new (bool changed, Constraint initiator)[GridSizeVal];
                else
                    for (var i = 0; i < GridSizeVal; i++)
                        TakensChanged[i] = default;
            }

            public override void MarkImpossible(int cell, int value)
            {
                if (Grid[cell] != null)
                    return;
                if (!Takens[cell][value - MinVal])
                {
                    Takens[cell][value - MinVal] = true;
                    TakensChanged ??= new (bool changed, Constraint initiator)[GridSize];
                    if (!TakensChanged[cell].changed)
                        TakensChanged[cell] = (true, CurrentConstraint);
                    else if (TakensChanged[cell].initiator != CurrentConstraint)
                        TakensChanged[cell] = (true, null);
                }
            }

            public override void MarkImpossible(int cell, Func<int, bool> isImpossible)
            {
                if (Grid[cell] != null)
                    return;
                for (var value = MinVal; value <= MaxVal; value++)
                    if (!Takens[cell][value - MinVal] && isImpossible(value))
                        MarkImpossible(cell, value);
            }

            public override void MustBe(int cell, int value)
            {
                if (Grid[cell] != null)
                {
                    if (Grid[cell].Value + MinVal != value)
                        throw new InvalidOperationException("A constraint attempted to set a cell to a value that is already set to a different value.");
                    return;
                }
                for (var otherValue = MinVal; otherValue <= MaxVal; otherValue++)
                    if (otherValue != value)
                        MarkImpossible(cell, otherValue);
            }

            public override bool AllSame<T>(int cell, Func<int, T> selector, out T result)
            {
                if (Grid[cell] != null)
                {
                    result = selector(Grid[cell].Value + MinVal);
                    return true;
                }
                T tmpResult = default;
                var found = false;
                for (var value = MinVal; value <= MaxVal; value++)
                    if (!Takens[cell][value - MinVal])
                    {
                        var mapped = selector(value);
                        if (!found)
                        {
                            tmpResult = mapped;
                            found = true;
                        }
                        else if (!mapped.Equals(tmpResult))
                        {
                            result = default;
                            return false;
                        }
                    }
                result = tmpResult;
                return found;
            }

            public override int? this[int cell] => Grid[cell] == null ? null : Grid[cell].Value + MinVal;
            public override bool IsImpossible(int cell, int value) => value < MinVal || value > MaxVal || (Grid[cell] != null && Grid[cell].Value != value - MinVal) || Takens[cell][value - MinVal];
            public override int? LastPlacedCell => LastPlacedIx;
            public override int MinValue => MinVal;
            public override int MaxValue => MaxVal;
            public override int GridSize => GridSizeVal;
        }
    }
}

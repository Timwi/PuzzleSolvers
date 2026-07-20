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

        /// <summary>Contains colors for use by <see cref="SolutionToConsole(int[], Func{int, string}, int?)"/>.</summary>
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
        public virtual ConsoleColoredString SolutionToConsole(int?[] solution, Func<int?, int, string> getName = null, int? width = null)
        {
            var rWidth = width ?? 9;
            getName ??= (v, ix) => v == null ? "?" : v.Value.ToString();
            var digits = solution.Select((v, ix) => getName(v, ix).Length).Max().ClipMin(2);
            return solution.Select((val, ix) =>
            {
                var firstConstraint = Constraints.FirstOrDefault(c => c.AffectedCells != null && c.AffectedCells.Contains(ix) && ConstraintColors.ContainsKey(c));
                var (foreground, background) = firstConstraint == null ? default : ConstraintColors.Get(firstConstraint, default);
                return getName(val, ix).PadLeft(digits).Color(val == null ? ConsoleColor.DarkGray : foreground, background);
            }).Split(rWidth).Select(row => row.JoinColoredString()).JoinColoredString("\n");
        }

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
        public ConsoleColoredString SolutionToConsole(int[] solution, Func<int, int, string> getName = null, int? width = null) =>
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
        public ConsoleColoredString SolutionToConsole(int[] solution, Func<int, string> getName, int? width = null) =>
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
        ///     Color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string}, int?)"/>.</param>
        /// <param name="background">
        ///     Background color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string},
        ///     int?)"/>.</param>
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
        ///     Color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string}, int?)"/>.</param>
        /// <param name="background">
        ///     Background color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string},
        ///     int?)"/>.</param>
        public Puzzle AddGivens(string givens, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (givens == null)
                throw new ArgumentNullException(nameof(givens));
            if (givens.Length > GridSize)
                throw new ArgumentException("The length of ‘givens’ cannot be greater than the size of the puzzle.", nameof(givens));
            if (!givens.All(ch => ch is '.' or >= '0' and <= '9'))
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
        ///     Color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string}, int?)"/>.</param>
        /// <param name="background">
        ///     Background color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string},
        ///     int?)"/>.</param>
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
        ///     Color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string}, int?)"/>.</param>
        /// <param name="background">
        ///     Background color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string},
        ///     int?)"/>.</param>
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
        ///     Color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string}, int?)"/>.</param>
        /// <param name="background">
        ///     Background color to use when outputting a solution with <see cref="SolutionToConsole(int[], Func{int, string},
        ///     int?)"/>.</param>
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

        /// <summary>Returns a lazy sequence containing the solutions for this puzzle.</summary>
        public IEnumerable<int[]> Solve(SolverInstructions solverInstructions = null)
        {
            if (solverInstructions != null && solverInstructions.IntendedSolution == null && solverInstructions.ExamineConstraint != null)
                throw new InvalidOperationException(@"solverInstructions.ExamineConstraints cannot be specified when solverInstructions.IntendedSolution is null.");
            if (solverInstructions != null && solverInstructions.IntendedSolution != null && solverInstructions.IntendedSolution.Length != GridSize)
                throw new InvalidOperationException(@"solverInstructions.IntendedSolution must have the same length as the size of the puzzle.");

            var cellPriority = (solverInstructions?.CellPriority is { } givenPrio ? givenPrio.Concat(Enumerable.Range(0, GridSize).Except(givenPrio)) : Enumerable.Range(0, GridSize)).ToArray();
            if (MaxValue - MinValue + 1 <= 64)
            {
                var fullMask = MaxValue - MinValue + 1 == 64 ? ulong.MaxValue : (1UL << (MaxValue - MinValue + 1)) - 1;
                return solve(cellPriority, solverInstructions, new SolverStateImplEfficient(this, new int?[GridSize], Ut.NewArray(GridSize, _ => fullMask), Constraints));
            }
            else
                return solve(cellPriority, solverInstructions, new SolverStateImplInefficient(this, new int?[GridSize], Ut.NewArray(GridSize, _ => Enumerable.Range(MinValue, MaxValue - MinValue + 1).ToList()), Constraints));
        }

        private readonly object _fallbackLockObject = new();

        private IEnumerable<int[]> solve(int[] cellPriority, SolverInstructions instr, SolverStateImplBase state)
        {
            var stack = new Stack<SolverStateImplBase>();
            var firstIteration = true;

            newIteration:
            List<Constraint> mustReevaluate = null;

            //** PART 1: Allow all the constraints to restrict the possible values in their cells
            reevaluate:
            List<Constraint> newConstraintsUse = null;
            List<Constraint> newMustReevaluate = null;

            for (var i = 0; i < state.Constraints.Count; i++)
            {
                var constraint = state.Constraints[i];
                state.CurrentConstraint = constraint;

                ConstraintResult processResult = null;
                if (firstIteration || (state.LastPlacedIx != null && (constraint.AffectedCells == null || constraint.AffectedCells.Contains(state.LastPlacedIx.Value))) || (mustReevaluate != null && mustReevaluate.Contains(constraint)))
                {
                    var doExamine = instr != null && (instr.ExamineConstraint == null || instr.ExamineConstraint(constraint));
                    state.SetPrevAvailableIf(doExamine && instr?.IntendedSolution is { } intSol && state.IntendedSolutionPossible(intSol));

                    // CALL THE CONSTRAINT
                    processResult = constraint.Process(state);
                    var isViolation = processResult is ConstraintViolation;

                    // If the intended solution was previously possible but not anymore, output the requested debug information
                    if (state.WasIntendedSolutionPossible() && (isViolation || !state.IntendedSolutionPossible(instr.IntendedSolution)))
                        lock (instr?.LockObject ?? _fallbackLockObject)
                            (instr?.ProgressVisualizer ?? new ProgressVisualizer(0)).VisualizeIntendedSolutionBug(state);

                    if (isViolation)
                        goto violation;
                }

                // This code MUST be kept outside of the “if” above. This is because its “else” clause needs to run even if the above “if” didn’t.
                if (processResult is ConstraintReplace { NewConstraints: { } newConstraints })
                {
                    // A constraint changed. That means we definitely need a new list of constraints for the recursive call.
                    if (newConstraintsUse == null)
                    {
                        newConstraintsUse = state.Constraints.Take(i).ToList();
                        newMustReevaluate ??= [];
                    }
                    var cIx = newConstraintsUse.Count;
                    newConstraintsUse.AddRange(newConstraints);
                    newMustReevaluate.AddRange(newConstraintsUse.Skip(cIx));
                }
                else
                    newConstraintsUse?.Add(constraint);
            }

            firstIteration = false;

            // Check if any reevaluatable constraints affect a cell that changed
            if (state.AvailablesChanged != null)
                foreach (var constraint in state.Constraints)
                    if (constraint.CanReevaluate)
                    {
                        if (constraint.AffectedCells == null)
                        {
                            foreach (var (changed, initiator) in state.AvailablesChanged)
                                if (changed && initiator != constraint)
                                    goto hasChanged;
                        }
                        else
                        {
                            foreach (var cell in constraint.AffectedCells)
                                if (state.AvailablesChanged[cell] is { } tup && tup.changed && tup.initiator != constraint)
                                    goto hasChanged;
                        }

                        continue;
                        hasChanged:
                        (newMustReevaluate ??= []).Add(constraint);
                    }

            if (newConstraintsUse != null || newMustReevaluate != null)
            {
                state.Constraints = newConstraintsUse ?? state.Constraints;
                mustReevaluate = newMustReevaluate;
                state.LastPlacedIx = null;
                state.ClearAvailablesChanged();
                goto reevaluate;
            }

            // Clean up a bit for the benefit of the garbage collector
            state.CurrentConstraint = null;
            state.AvailablesChanged = null;


            //** PART 2: Find the next cell where we will perform trial and error

            var curCell = -1;

            switch (instr?.CellOrderStrategy ?? CellOrderStrategy.Default)
            {
                default:
                {
                    var fewestPossibleValues = int.MaxValue;
                    for (var cIx = 0; cIx < cellPriority.Length; cIx++)
                    {
                        var cell = cellPriority[cIx];
                        if (state.Grid[cell] != null)
                            continue;
                        var count = state.AvailableCount(cell);
                        if (count == 0)
                            goto violation;
                        if (count < fewestPossibleValues)
                        {
                            curCell = cell;
                            fewestPossibleValues = count;
                            if (count == 1)
                            {
                                state.IsSingularValue = true;
                                goto immediate;
                            }
                        }
                    }
                    break;
                }

                case CellOrderStrategy.GivenOrder:
                {
                    for (var cell = 0; cell < GridSize; cell++)
                        if (state.Grid[cell] == null)
                        {
                            if (curCell < 0)
                                curCell = cell;
                            var count = state.AvailableCount(cell);
                            if (count == 0)
                                goto violation;
                            if (count == 1)
                            {
                                curCell = cell;
                                state.IsSingularValue = true;
                                goto immediate;
                            }
                        }
                    break;
                }
            }

            if (curCell == -1)
            {
                // Grid is fully filled in — return it as a solution
                var solutionArr = state.Grid.Select(val => val.Value).ToArray();
                if (instr?.BulkLoggingFile != null)
                    lock (instr?.LockObject ?? _fallbackLockObject)
                        File.AppendAllText(instr.BulkLoggingFile, $"{new string(' ', state.RecursionDepth)}Solution found: {solutionArr.JoinString()}\n");
                yield return solutionArr;
                goto violation;
            }

            immediate:
            state.LastPlacedIx = curCell;
            state.VisualizingProgress = !state.IsSingularValue && instr?.ProgressVisualizer?.IsActive(state.RecursionDepth) == true;

            // Loop to go through possible values for the current cell
            state.SetCandidates(curCell, instr?.Randomizer, instr?.ValuePriority);
            nextCandidate:
            curCell = state.LastPlacedIx.Value;
            if (!state.GetNextCandidate(out var val))
            {
                state.Grid[curCell] = null;
                goto violation;
            }

            if (instr?.BulkLoggingFile != null)
                lock (instr?.LockObject ?? _fallbackLockObject)
                    File.AppendAllText(instr.BulkLoggingFile, $"{new string(' ', state.RecursionDepth)}Cell {curCell} trying {val}\n");

            // Attempt to put the value into this cell
            state.Grid[curCell] = val;

            if (state.VisualizingProgress)
                state.ProgressVisualizationObject = instr.ProgressVisualizer.VisualizeProgress(state);

            stack.Push(state);
            var newState = state.CloneForNextIteration(curCell);
            newState.LastPlacedIx = curCell;
            newState.RecursionDepth = state.IsSingularValue ? state.RecursionDepth : state.RecursionDepth + 1;
            state = newState;
            goto newIteration;

            violation:
            if (state.VisualizingProgress)
                instr.ProgressVisualizer.EraseProgress(state);
            if (stack.Count == 0)
                yield break;
            state = stack.Pop();
            goto nextCandidate;
        }

        private enum __debugSvgLabels
        { Numbers, Letters, Loop, Shading }

        private void __debug_generateSvg(SolverStateImplBase state, int recursionDepth = 0, int[] intendedSolution = null, IEnumerable<int> highlightIxs = null, int width = 9, int height = 9, int values = 9, int wrap = 3, __debugSvgLabels labels = __debugSvgLabels.Numbers)
        {
            string cnv(int val) =>
                labels == __debugSvgLabels.Shading && val.IsBetween(0, 1) ? val == 0 ? "·" : "■" :
                labels == __debugSvgLabels.Loop && val >= 0 && val <= 6 ? Path.ToChar[val].ToString() :
                labels == __debugSvgLabels.Letters ? ((char) ('A' + val)).ToString() : (val + state.MinValue).ToString();
            File.WriteAllText(@"D:\temp\temp.svg", $@"
                <svg viewBox='-.1 -.1 {width + 1.2} {width + .2}' xmlns='http://www.w3.org/2000/svg' text-anchor='middle' font-family='Work Sans'>
                    {Enumerable.Range(0, width * height).Select(cell => $@"
                        <rect x='{cell % width}' y='{cell / width}' width='1' height='1' stroke='black' stroke-width='{(highlightIxs != null && highlightIxs.Contains(cell) ? .05 : .01)}' fill='{(intendedSolution != null && (state.Grid[cell] != null ? (state.Grid[cell].Value != intendedSolution[cell]) : !state.IsImpossible(cell, intendedSolution[cell])) ? "rgba(255, 0, 0, .1)" : state[cell] != null ? "rgba(0, 192, 0, .1)" : "none")}' />
                        {(state.Grid[cell] != null
                            ? $"<text x='{cell % width + .5}' y='{cell / width + .8}' font-size='.8'>{cnv(state.Grid[cell].Value)}</text>"
                            : Enumerable.Range(0, values).Where(v => !state.IsImpossible(cell, v)).Select(v => $"<text x='{cell % width + 1d / (wrap + 1) * (1 + v % wrap)}' y='{cell / width + 1d / (wrap + 1) * (1 + v / wrap) + .1}' font-size='.2'>{cnv(v)}</text>").JoinString()
                        )}
                    ").JoinString()}
                    <text text-anchor='left' x='{width + .1}' y='1' font-size='.1'>{recursionDepth}</text>
                </svg>
            ");
        }

        private string __debug_string(SolverStateImplBase state, int width, string chars = null) =>
            Enumerable.Range(0, state.GridSize).Select(ix => state[ix] == null ? "?" : chars == null ? state[ix].Value.ToString() : chars[state[ix].Value].ToString())
                .Split(width).Select(row => row.JoinString(" ")).JoinString("\n");
    }
}

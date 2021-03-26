using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RT.Util;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Describes a puzzle.</summary>
    public class Puzzle
    {
        /// <summary>
        ///     The number of cells to be filled in this puzzle.</summary>
        /// <remarks>
        ///     Note that for puzzles in which the solver must draw lines across gridlines, the “cells” are actually the
        ///     gridlines, not the squares of the grid. If a puzzle requires both gridlines as well as squares of the grid to
        ///     be filled, these must be considered separate cells, so this value must be the sum of all of them.</remarks>
        public int GridSize { get; private set; }

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

        /// <summary>Contains colors for use by <see cref="SolutionToConsole(int?[], int)"/>.</summary>
        public Dictionary<Constraint, (ConsoleColor? foreground, ConsoleColor? background)> ConstraintColors { get; private set; } = new Dictionary<Constraint, (ConsoleColor? foreground, ConsoleColor? background)>();

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="size">
        ///     The number of cells in this puzzle. See <see cref="GridSize"/> for more information.</param>
        /// <param name="minValue">
        ///     See <see cref="MinValue"/>.</param>
        /// <param name="maxValue">
        ///     See <see cref="MaxValue"/>.</param>
        /// <remarks>
        ///     When using this constructor, be sure to populate <see cref="Constraints"/> before running <see
        ///     cref="Solve(SolverInstructions)"/>.</remarks>
        public Puzzle(int size, int minValue, int maxValue)
        {
            GridSize = size;
            MinValue = minValue;
            MaxValue = maxValue;
            Constraints = new List<Constraint>();
        }

        /// <summary>
        ///     Converts a Sudoku solution to a <see cref="ConsoleColoredString"/> that includes the coloring offered by some
        ///     constraints.</summary>
        /// <param name="solution">
        ///     The solution to be colored.</param>
        /// <param name="width">
        ///     The width of the puzzle grid. For a standard Sudoku, this is 9.</param>
        public ConsoleColoredString SolutionToConsole(int?[] solution, int width = 9)
        {
            var digits = solution.Max().ToString().Length + 1;
            return solution.Split(width).Select((chunk, row) => chunk.Select((val, col) =>
            {
                var firstConstraint = Constraints.FirstOrDefault(c => c.AffectedCells != null && c.AffectedCells.Contains(col + width * row) && ConstraintColors.ContainsKey(c));
                var (foreground, background) = firstConstraint == null ? default : ConstraintColors.Get(firstConstraint, default);
                return ((val == null ? "?" : val.Value.ToString()).PadLeft(digits)).Color(val == null ? ConsoleColor.DarkGray : foreground, background);
            })
                .JoinColoredString()).JoinColoredString("\n");
        }

        /// <summary>
        ///     Converts a Sudoku solution to a <see cref="ConsoleColoredString"/> that includes the coloring offered by some
        ///     constraints.</summary>
        /// <param name="solution">
        ///     The solution to be colored.</param>
        /// <param name="width">
        ///     The width of the puzzle grid. For a standard Sudoku, this is 9.</param>
        public ConsoleColoredString SolutionToConsole(int[] solution, int width = 9) => SolutionToConsole(solution.Select(val => val.Nullable()).ToArray(), width);

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
                AddConstraint(constraint, null, avoidColors ? (ConsoleColor?) null : background);
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
        ///     Color to use when outputting a solution with <see cref="SolutionToConsole(int?[], int)"/>.</param>
        /// <param name="background">
        ///     Background color to use when outputting a solution with <see cref="SolutionToConsole(int?[], int)"/>.</param>
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
        ///     Color to use when outputting a solution with <see cref="SolutionToConsole(int?[], int)"/>.</param>
        /// <param name="background">
        ///     Background color to use when outputting a solution with <see cref="SolutionToConsole(int?[], int)"/>.</param>
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
        ///     Color to use when outputting a solution with <see cref="SolutionToConsole(int?[], int)"/>.</param>
        /// <param name="background">
        ///     Background color to use when outputting a solution with <see cref="SolutionToConsole(int?[], int)"/>.</param>
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
        ///     Color to use when outputting a solution with <see cref="SolutionToConsole(int?[], int)"/>.</param>
        /// <param name="background">
        ///     Background color to use when outputting a solution with <see cref="SolutionToConsole(int?[], int)"/>.</param>
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
        ///     Color to use when outputting a solution with <see cref="SolutionToConsole(int?[], int)"/>.</param>
        /// <param name="background">
        ///     Background color to use when outputting a solution with <see cref="SolutionToConsole(int?[], int)"/>.</param>
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
            if (solverInstructions != null && solverInstructions.UseLetters && GridSize > 26)
                throw new InvalidOperationException(@"solverInstructions.UseLetters cannot be true when there are more than 26 cells in the puzzle.");

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
                var cs = constraint.MarkTakens(state);
                if (cs != null)
                {
                    if (newConstraints == null)
                        newConstraints = new List<Constraint>(constraintsUse.Take(cIx));
                    newConstraints.AddRange(cs);
                }
                else if (newConstraints != null)
                    newConstraints.Add(constraint);
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

            return solve(grid, takens, constraintsUse, cellPriority, solverInstructions).Select(solution => solution.Select(val => val + MinValue).ToArray());
        }

        private IEnumerable<int[]> solve(int?[] grid, bool[][] takens, List<Constraint> constraints, int[] cellPriority, SolverInstructions instr, int recursionDepth = 0)
        {
            var fewestPossibleValues = int.MaxValue;
            var ix = -1;
            foreach (var cell in cellPriority)
            {
                if (grid[cell] != null)
                    continue;
                var count = 0;
                for (var v = 0; v < takens[cell].Length; v++)
                    if (!takens[cell][v])
                        count++;
                if (count == 0)
                    yield break;
                if (count == 1)
                {
                    ix = cell;
                    goto immediate;
                }
                if (count < fewestPossibleValues)
                {
                    ix = cell;
                    fewestPossibleValues = count;
                }
            }

            if (ix == -1)
            {
                yield return grid.Select(val => val.Value).ToArray();
                yield break;
            }

            immediate:
            var startAt = instr?.Randomizer?.Next(0, takens[ix].Length) ?? 0;
            var state = new SolverStateImpl { Grid = grid, GridSizeVal = GridSize, MinVal = MinValue, MaxVal = MaxValue, Takens = takens };

            for (var tVal = 0; tVal < takens[ix].Length; tVal++)
            {
                var val = (tVal + startAt) % takens[ix].Length;
                if (takens[ix][val])
                    continue;

                if (instr != null && instr.ShowContinuousProgress != null && recursionDepth < instr.ShowContinuousProgress.Value)
                {
                    Console.CursorLeft = 0;
                    Console.CursorTop = recursionDepth + (instr.ShowContinuousProgressConsoleTop ?? 0);
                    ConsoleUtil.Write($"Cell {ix}: " + Enumerable.Range(0, takens[ix].Length).Where(v => !instr.ShowContinuousProgressShortened || !takens[ix][v]).Select(v => (v + MinValue).ToString().Color(
                        takens[ix][v] ? ConsoleColor.DarkBlue : v == val ? ConsoleColor.Yellow : ConsoleColor.DarkCyan,
                        v == val ? ConsoleColor.DarkGreen : ConsoleColor.Black)).JoinColoredString(" "));
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

                try
                {
                    for (var i = 0; i < constraintsUse.Count; i++)
                    {
                        var constraint = constraintsUse[i];
                        state.CurrentConstraint = constraint;

                        IEnumerable<Constraint> replaceConstraints = null;
                        if ((state.LastPlacedIx != null && (constraint.AffectedCells == null || constraint.AffectedCells.Contains(ix))) || (mustReevaluate != null && mustReevaluate.Contains(constraint)))
                        {
                            var doExamine = instr != null && (instr.ExamineConstraint == null || instr.ExamineConstraint(constraint));
                            var wasIntendedSolutionPossible = doExamine && intendedSolutionPossible(instr, state);
                            var takensDebugCopy = wasIntendedSolutionPossible ? state.Takens.Select(b => (bool[]) b.Clone()).ToArray() : null;

                            // CALL THE CONSTRAINT
                            replaceConstraints = constraint.MarkTakens(state);

                            // If the intended solution was previously possible but not anymore, output the requested debug information
                            if (wasIntendedSolutionPossible && !intendedSolutionPossible(instr, state))
                            {
                                ConsoleUtil.WriteLine("Constraint {0/Magenta} removed the intended solution:".Color(ConsoleColor.White).Fmt(constraint.GetType().FullName));
                                var numDigits = (GridSize - 1).ToString().Length;
                                for (var cell = 0; cell < GridSize; cell++)
                                {
                                    string valueId(int v) => instr.UseLetters ? ((char) ('A' + v)).ToString() : (v + MinValue).ToString();
                                    var cellLine = Enumerable.Range(0, MaxValue - MinValue + 1)
                                        .Select(v => valueId(v).Color(takensDebugCopy[cell][v] != state.Takens[cell][v] ? ConsoleColor.Red : state.Takens[cell][v] ? ConsoleColor.DarkGray : ConsoleColor.Yellow))
                                        .JoinColoredString(" ");
                                    var cellStr = cell.ToString().PadLeft(numDigits, ' ') + ". ";
                                    ConsoleUtil.WriteLine($"{cellStr.Color(cell == ix ? ConsoleColor.Cyan : ConsoleColor.DarkCyan)}{cellLine}   {(grid[cell] == null ? "?".Color(ConsoleColor.DarkGreen) : valueId(grid[cell].Value).Color(ConsoleColor.Green))}", null);
                                }
                                Console.WriteLine();
                            }
                        }

                        if (replaceConstraints != null)
                        {
                            // A constraint changed. That means we definitely need a new list of constraints for the recursive call.
                            if (newConstraintsUse == null)
                            {
                                newConstraintsUse = new List<Constraint>(constraintsUse.Take(i));
                                if (newMustReevaluate == null)
                                    newMustReevaluate = new List<Constraint>();
                            }
                            var cIx = newConstraintsUse.Count;
                            newConstraintsUse.AddRange(replaceConstraints);
                            newMustReevaluate.AddRange(newConstraintsUse.Skip(cIx));
                        }
                        else if (newConstraintsUse != null)
                            newConstraintsUse.Add(constraint);
                    }

                    // Check if any reevaluatable constraints affect a cell that changed
                    if (state.TakensChanged != null)
                        foreach (var constraint in constraintsUse)
                            if (constraint.CanReevaluate && (constraint.AffectedCells == null
                                ? state.TakensChanged.Any(tup => tup.changed && tup.initiator != constraint)
                                : constraint.AffectedCells.Any(c => state.TakensChanged[c].changed && state.TakensChanged[c].initiator != constraint)))
                            {
                                if (newMustReevaluate == null)
                                    newMustReevaluate = new List<Constraint>();
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
                }
                catch (ConstraintViolationException)
                {
                    goto digitBusted;
                }
                foreach (var solution in solve(grid, state.Takens, constraintsUse, cellPriority, instr, recursionDepth + 1))
                    yield return solution;

                digitBusted:;
            }
            grid[ix] = null;

            if (instr != null && instr.ShowContinuousProgress != null && recursionDepth < instr.ShowContinuousProgress.Value)
            {
                Console.CursorLeft = 0;
                Console.CursorTop = recursionDepth + (instr.ShowContinuousProgressConsoleTop ?? 0);
                Console.Write(new string(' ', Console.BufferWidth - 1));
            }
        }

        bool intendedSolutionPossible(SolverInstructions instr, SolverStateImpl state) =>
            instr != null && instr.IntendedSolution != null &&
            instr.IntendedSolution.All((v, cell) => state.Grid[cell] == v - MinValue || (state.Grid[cell] == null && !state.Takens[cell][v - MinValue]));

        void __debug_generateSvg(SolverStateImpl state, int[] intendedSolution = null, IEnumerable<int> highlightIxs = null)
        {
            File.WriteAllText(@"D:\temp\temp.svg", $@"
                <svg viewBox='-.1 -.1 9.2 9.2' xmlns='http://www.w3.org/2000/svg' text-anchor='middle' font-family='Work Sans'>
                    {Enumerable.Range(0, 81).Select(cell => $@"
                        <rect x='{cell % 9}' y='{cell / 9}' width='1' height='1' stroke='black' stroke-width='{(highlightIxs != null && highlightIxs.Contains(cell) ? .05 : .01)}' fill='{(intendedSolution != null && (state.Grid[cell] != null ? (state.Grid[cell].Value != intendedSolution[cell] - state.MinVal) : (state.Takens[cell][intendedSolution[cell] - state.MinVal])) ? "rgba(255, 0, 0, .1)" : "none")}' />
                        {(state.Grid[cell] != null
                            ? $"<text x='{cell % 9 + .5}' y='{cell / 9 + .8}' font-size='.8'>{state.Grid[cell] + state.MinVal}</text>"
                            : Enumerable.Range(0, 9).Where(v => !state.Takens[cell][v]).Select(v => $"<text x='{cell % 9 + .25 * (1 + v % 3)}' y='{cell / 9 + .25 * (1 + v / 3) + .1}' font-size='.3'>{v + state.MinVal}</text>").JoinString()
                        )}
                    ").JoinString()}
                </svg>
            ");
        }

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
                    if (TakensChanged == null)
                        TakensChanged = new (bool changed, Constraint initiator)[GridSize];
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
                        throw new ConstraintViolationException();
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

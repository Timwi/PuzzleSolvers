using System;
using System.Collections.Generic;
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
            if (solverInstructions != null && (solverInstructions.ExamineConstraints == null) != (solverInstructions.IntendedSolution == null))
                throw new InvalidOperationException(@"solverInstructions.ExamineConstraints and solverInstructions.IntendedSolution must both be null or both be non-null.");
            if (solverInstructions != null && solverInstructions.ExamineConstraints != null && solverInstructions.ExamineConstraints.Length == 0)
                throw new InvalidOperationException(@"solverInstructions.ExamineConstraints cannot be an empty array.");
            if (solverInstructions != null && solverInstructions.IntendedSolution != null && solverInstructions.IntendedSolution.Length != GridSize)
                throw new InvalidOperationException(@"solverInstructions.IntendedSolution must have the same length as the size of the puzzle.");
            if (solverInstructions != null && solverInstructions.UseLetters && GridSize > 26)
                throw new InvalidOperationException(@"solverInstructions.UseLetters cannot be true when there are more than 26 cells in the puzzle.");

            _numVals = MaxValue - MinValue + 1;
            var cells = new int?[GridSize];
            var takens = new bool[GridSize][];
            for (var i = 0; i < GridSize; i++)
                takens[i] = new bool[_numVals];

            var constraintsUse = Constraints;
            reevaluate:
            List<Constraint> newConstraints = null;
            for (var cIx = 0; cIx < constraintsUse.Count; cIx++)
            {
                var constraint = constraintsUse[cIx];
                var cs = constraint.MarkTakens(takens, cells, null, MinValue, MaxValue);
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

            return solve(cells, takens, constraintsUse, cellPriority, solverInstructions).Select(solution => solution.Select(val => val + MinValue).ToArray());
        }

        private IEnumerable<int[]> solve(int?[] filledInValues, bool[][] takens, List<Constraint> constraints, int[] cellPriority, SolverInstructions instr, int recursionDepth = 0)
        {
            var fewestPossibleValues = int.MaxValue;
            var ix = -1;
            foreach (var cell in cellPriority)
            {
                if (filledInValues[cell] != null)
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
                //count -= numConstraintsPerCell[cell];
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

            immediate:
            var startAt = instr?.Randomizer?.Next(0, takens[ix].Length) ?? 0;

            for (var tVal = 0; tVal < takens[ix].Length; tVal++)
            {
                var val = (tVal + startAt) % takens[ix].Length;
                if (takens[ix][val])
                    continue;

                if (instr != null && instr.ShowContinuousProgress != null && recursionDepth < instr.ShowContinuousProgress.Value)
                {
                    Console.CursorLeft = 0;
                    Console.CursorTop = recursionDepth;
                    ConsoleUtil.Write($"Cell {ix}: " + Enumerable.Range(0, takens[ix].Length).Where(v => !instr.ShowContinuousProgressShortened || !takens[ix][v]).Select(v => (v + MinValue).ToString().Color(
                        takens[ix][v] ? ConsoleColor.DarkBlue : v == val ? ConsoleColor.Yellow : ConsoleColor.DarkCyan,
                        v == val ? ConsoleColor.DarkGreen : ConsoleColor.Black)).JoinColoredString(" "));
                }

                // Attempt to put the value into this cell
                filledInValues[ix] = val;
                var takensCopy = takens.Select(arr => arr.ToArray()).ToArray();
                var specificCell = true;
                var constraintsUse = constraints;
                reevaluate:

                // If placing this value modifies any of the constraints, we need to take a copy of the list of constraints,
                // but for performance reasons we want to keep the original list if none of the constraints changed
                List<Constraint> constraintsCopy = null;

                for (var i = 0; i < constraintsUse.Count; i++)
                {
                    var constraint = constraintsUse[i];
                    var doExamine = instr != null && instr.ExamineConstraints != null && instr.ExamineConstraints.Contains(constraint);
                    var intendedSolutionPossible = doExamine && instr.IntendedSolution.Select((v, cell) => filledInValues[cell] == v - MinValue || (filledInValues[cell] == null && !takensCopy[cell][v - MinValue])).All(b => b);
                    var takensDebugCopy = intendedSolutionPossible ? takensCopy.Select(b => (bool[]) b.Clone()).ToArray() : null;

                    // CALL THE CONSTRAINT
                    var newConstraints = specificCell && constraint.AffectedCells != null && !constraint.AffectedCells.Contains(ix) ? null : constraint.MarkTakens(takensCopy, filledInValues, specificCell ? null : ix, MinValue, MaxValue);

                    // If the intended solution was previously possible but not anymore, output the requested debug information
                    if (intendedSolutionPossible && !instr.IntendedSolution.Select((v, cell) => filledInValues[cell] == v - MinValue || (filledInValues[cell] == null && !takensCopy[cell][v - MinValue])).All(b => b))
                    {
                        ConsoleUtil.WriteLine("Constraint {0/Magenta} {1/DarkMagenta} removed the intended solution:"
                            .Color(ConsoleColor.White).Fmt(Array.IndexOf(instr.ExamineConstraints, constraint), $@"({constraint.GetType().FullName})"));
                        var numDigits = (GridSize - 1).ToString().Length;
                        for (var cell = 0; cell < GridSize; cell++)
                        {
                            string valueId(int v) => instr.UseLetters ? ((char) ('A' + v)).ToString() : (v + MinValue).ToString();
                            var cellLine = Enumerable.Range(0, MaxValue - MinValue + 1)
                                .Select(v => valueId(v).Color(takensDebugCopy[cell][v] != takensCopy[cell][v] ? ConsoleColor.Red : takensCopy[cell][v] ? ConsoleColor.DarkGray : ConsoleColor.Yellow))
                                .JoinColoredString(" ");
                            var cellStr = cell.ToString().PadLeft(numDigits, ' ') + ". ";
                            ConsoleUtil.WriteLine($"{cellStr.Color(cell == ix ? ConsoleColor.Cyan : ConsoleColor.DarkCyan)}{cellLine}   {(filledInValues[cell] == null ? "?".Color(ConsoleColor.DarkGreen) : valueId(filledInValues[cell].Value).Color(ConsoleColor.Green))}", null);
                        }
                        Console.WriteLine();
                    }

                    if (newConstraints != null)
                    {
                        // A constraint changed. That means we definitely need a new array of constraints for the recursive call.
                        if (constraintsCopy == null)
                            constraintsCopy = new List<Constraint>(constraintsUse.Take(i));
                        constraintsCopy.AddRange(newConstraints);
                    }
                    else if (constraintsCopy != null)
                        constraintsCopy.Add(constraint);
                }

                if (constraintsCopy != null)
                {
                    constraintsUse = constraintsCopy;
                    specificCell = false;
                    goto reevaluate;
                }

                foreach (var solution in solve(filledInValues, takensCopy, constraintsUse, cellPriority, instr, recursionDepth + 1))
                    yield return solution;
            }
            filledInValues[ix] = null;

            if (instr != null && instr.ShowContinuousProgress != null && recursionDepth < instr.ShowContinuousProgress.Value)
            {
                Console.CursorLeft = 0;
                Console.CursorTop = recursionDepth;
                Console.Write(new string(' ', Console.BufferWidth - 1));
            }
        }
    }
}

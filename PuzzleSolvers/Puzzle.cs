using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    public class Puzzle
    {
        public int Size;
        public int MinValue;
        public int MaxValue;
        public List<Constraint> Constraints = new List<Constraint>();

        public static int?[] TranslateGivens(string givens)
        {
            if (givens == null)
                throw new ArgumentNullException(nameof(givens));
            if (givens.Length != 81 || !givens.All(ch => ch == '.' || (ch >= '1' && ch <= '9')))
                throw new ArgumentException("‘givens’ must have length 81 and contain only digits 1–9 and periods (.) for no given.", "givens");
            return givens.Select(ch => ch == '.' ? (int?) null : ch - '0').ToArray();
        }

        public ConsoleColoredString SudokuSolutionToConsoleString(int[] solution) =>
            solution.Split(9).Select((chunk, row) => chunk.Select((val, col) => (val + " ").Color(findColor(col + 9 * row), findBackgroundColor(col + 9 * row))).JoinColoredString()).JoinColoredString("\n");

        private ConsoleColor? findColor(int ix) => Constraints.Aggregate((ConsoleColor?) null, (prev, c) => prev ?? c.CellColor(ix));
        private ConsoleColor? findBackgroundColor(int ix) => Constraints.Aggregate((ConsoleColor?) null, (prev, c) => prev ?? c.CellBackgroundColor(ix));

        public static Puzzle Sudoku(int?[] givens = null)
        {
            if (givens != null && (givens.Length != 81 || !givens.All(val => val == null || (val.Value >= 1 && val.Value <= 9))))
                throw new ArgumentException("‘givens’ must be null or have length 81 and contain only digits 1–9 and nulls for no given.", "givens");

            var puzzle = new Puzzle { Size = 81, MinValue = 1, MaxValue = 9 };
            puzzle.Constraints.AddRange(Constraint.Sudoku());
            if (givens != null)
                puzzle.Constraints.AddRange(Enumerable.Range(0, 81).Where(i => givens[i] != null).Select(i => new GivenConstraint { Location = i, Value = givens[i].Value }));
            return puzzle;
        }

        public static int[] TranslateCoordinates(string str, int gridWidth = 9)
        {
            var cells = new List<int>();
            foreach (var part in str.Split(','))
            {
                var m = Regex.Match(part, @"^\s*(?<col>[A-I](\s*-\s*(?<colr>[A-I]))?)\s*(?<row>[1-9](\s*-\s*(?<rowr>[1-9]))?)\s*$");
                if (!m.Success)
                    throw new ArgumentException(string.Format(@"The region “{0}” is not in a valid format. Expected a column letter (or a range of columns, e.g. A-D) followed by a row digit (or a range of rows, e.g. 1-4).", part), nameof(str));
                var col = m.Groups["col"].Value[0] - 'A';
                var colr = m.Groups["colr"].Success ? m.Groups["colr"].Value[0] - 'A' : col;
                var row = m.Groups["row"].Value[0] - '1';
                var rowr = m.Groups["rowr"].Success ? m.Groups["rowr"].Value[0] - '1' : row;
                for (var c = col; c <= col; c++)
                    for (var r = row; r <= row; r++)
                        cells.Add(c + gridWidth * r);
            }
            return cells.ToArray();
        }

        public static Puzzle JigsawSudoku(int?[] givens, params int[][] regions)
        {
            if (givens == null)
                throw new ArgumentNullException(nameof(givens));
            if (givens.Length != 81 || !givens.All(val => val == null || (val.Value >= 1 && val.Value <= 9)))
                throw new ArgumentException("‘givens’ must have length 81 and contain only digits 1–9 and nulls for no given.", "givens");

            ConsoleColor convertToColor(int number)
            {
                number++;
                if (number >= 7)
                    number++;
                return (ConsoleColor) number;
            }

            var puzzle = new Puzzle { Size = 81, MinValue = 1, MaxValue = 9 };
            var backgroundColors = new ConsoleColor?[81];

            foreach (var region in regions)
            {
                if (region.Length != 9)
                    throw new ArgumentException(string.Format(@"A region ({0}) does not cover exactly 9 cells.", region.JoinString(", ")), nameof(regions));
                puzzle.Constraints.Add(new UniquenessConstraint { AffectedCells = region, BackgroundColor = convertToColor(puzzle.Constraints.Count) });
            }
            puzzle.Constraints.AddRange(Constraint.LatinSquare());
            puzzle.Constraints.AddRange(Constraint.Givens(givens));
            return puzzle;
        }

        public static Puzzle NoTouchSudoku(int?[] givens)
        {
            var sudoku = Sudoku(givens);
            sudoku.Constraints.Add(new NoTouchConstraint { GridWidth = 9, GridHeight = 9 });
            return sudoku;
        }

        public static Puzzle ThermometerSudoku(params int[][] thermometers) => ThermometerSudoku(null, thermometers);
        public static Puzzle ThermometerSudoku(int?[] givens, params int[][] thermometers)
        {
            var sudoku = Sudoku(givens);
            for (var i = 0; i < thermometers.Length; i++)
                sudoku.Constraints.Add(new LessThanConstraint { AffectedCells = thermometers[i], BackgroundColor = (ConsoleColor) (i + 1) });
            return sudoku;
        }

        /// <summary>
        /// Returns a Sudoku surrounded by numbers. Every number indicates the sum of the first few cells visible from that location. Between <paramref name="minLength"/>
        /// and <paramref name="maxLength"/> cells may be included in that sum.
        /// </summary>
        /// <param name="givens">The numbers pre-filled inside the grid, or <c>null</c> if no givens are provided.</param>
        /// <param name="minLength">The minimum number of cells included in the sum in each clue.</param>
        /// <param name="maxLength">The maximum number of cells included in the sum in each clue.</param>
        /// <param name="cluesClockwiseFromTopLeft">The clues outside the grid, given in clockwise order from the top of the left column.</param>
        public static Puzzle FrameSudoku(int?[] givens, int minLength, int maxLength, params int[] cluesClockwiseFromTopLeft)
        {
            if (cluesClockwiseFromTopLeft == null)
                throw new ArgumentNullException(nameof(cluesClockwiseFromTopLeft));
            if (cluesClockwiseFromTopLeft.Length != 36)
                throw new ArgumentException("‘cluesClockwiseFromTopLeft’ should provide exactly 36 clues (9 per side of the grid).", nameof(cluesClockwiseFromTopLeft));

            var sudoku = Sudoku(givens);
            for (var col = 0; col < 9; col++)
            {
                sudoku.Constraints.Add(new SumAlternativeConstraint { Sum = cluesClockwiseFromTopLeft[col], AffectedCellGroups = Enumerable.Range(minLength, maxLength - minLength + 1).Select(len => Enumerable.Range(0, len).Select(row => 9 * row + col).ToArray()).ToArray() });
                sudoku.Constraints.Add(new SumAlternativeConstraint { Sum = cluesClockwiseFromTopLeft[8 - col + 18], AffectedCellGroups = Enumerable.Range(minLength, maxLength - minLength + 1).Select(len => Enumerable.Range(0, len).Select(row => 9 * (8 - row) + col).ToArray()).ToArray() });
            }
            for (var row = 0; row < 9; row++)
            {
                sudoku.Constraints.Add(new SumAlternativeConstraint { Sum = cluesClockwiseFromTopLeft[row + 9], AffectedCellGroups = Enumerable.Range(minLength, maxLength - minLength + 1).Select(len => Enumerable.Range(0, len).Select(col => 9 * row + (8 - col)).ToArray()).ToArray() });
                sudoku.Constraints.Add(new SumAlternativeConstraint { Sum = cluesClockwiseFromTopLeft[8 - row + 27], AffectedCellGroups = Enumerable.Range(minLength, maxLength - minLength + 1).Select(len => Enumerable.Range(0, len).Select(col => 9 * row + col).ToArray()).ToArray() });
            }
            return sudoku;
        }



        // Implementation of the solver

        private int _numVals;

        public IEnumerable<int[]> Solve()
        {
            _numVals = MaxValue - MinValue + 1;
            var cells = new int?[Size];
            var takens = new bool[Size][];
            for (var i = 0; i < Size; i++)
                takens[i] = new bool[_numVals];

            foreach (var constraint in Constraints)
                constraint.MarkInitialTakens(takens, MinValue, MaxValue);

            return solve(cells, takens, Constraints).Select(solution => solution.Select(val => val + MinValue).ToArray());
        }

        private IEnumerable<int[]> solve(int?[] filledInValues, bool[][] takens, List<Constraint> constraints)
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

            for (var val = 0; val < takens[ix].Length; val++)
            {
                if (takens[ix][val])
                    continue;

                // Attempt to put the value into this cell
                filledInValues[ix] = val;
                var takenCopy = takens.Select(arr => arr.ToArray()).ToArray();

                // If placing this value modifies any of the constraints, we need to take a copy of the list of constraints,
                // but for performance reasons we want to keep the original list if none of the constraints changed
                List<Constraint> constraintsCopy = null;

                for (var i = 0; i < Constraints.Count; i++)
                {
                    var constraint = Constraints[i];
                    var newConstraints = constraint.MarkTaken(takenCopy, filledInValues, ix, val, MinValue, MaxValue);
                    if (newConstraints != null)
                    {
                        // A constraint changed. That means we definitely need a new array of constraints for the recursive call.
                        if (constraintsCopy == null)
                        {
                            constraintsCopy = new List<Constraint>();
                            constraintsCopy.AddRange(constraints.Take(i));
                        }
                        constraintsCopy.AddRange(newConstraints);
                    }
                    else if (constraintsCopy != null)
                        constraintsCopy.Add(constraint);
                }

                foreach (var solution in solve(filledInValues, takenCopy, constraintsCopy ?? constraints))
                    yield return solution;
            }
            filledInValues[ix] = null;
        }
    }
}

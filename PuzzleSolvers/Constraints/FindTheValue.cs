using System.Collections.Generic;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement puzzle that indicates that whatever value the specified <see
    ///     cref="Cell"/> contains, the cell that many steps away must contain <see cref="Value"/>.</summary>
    /// <example>
    ///     For example, if <see cref="Cell"/> is the top-left corner and the constraint covers the top row, then <see
    ///     cref="Cell"/> can only contain a 3 if the fourth cell (3 cells to the right of the top-left corner) contains <see
    ///     cref="Value"/>.</example>
    public class FindTheValueConstraint : Constraint
    {
        /// <summary>The “focus” cell that indicates how far <see cref="Value"/> is.</summary>
        public int Cell { get; private set; }
        /// <summary>The value that must be in the cell a number of steps in <see cref="Direction"/> from <see cref="Cell"/>.</summary>
        public int Value { get; private set; }

        /// <summary>Constructor.</summary>
        public FindTheValueConstraint(int cell, int value, IEnumerable<int> affectedCells) : base(affectedCells)
        {
            Cell = cell;
            Value = value;
        }

        /// <summary>
        ///     Convenience constructor to generate a <see cref="FindTheValueConstraint"/> given a grid position, size, and a
        ///     direction.</summary>
        public FindTheValueConstraint(int cell, int gridWidth, int gridHeight, CellDirection direction, int value) : base(findAffectedCells(cell, gridWidth, gridHeight, direction))
        {
            Cell = cell;
            Value = value;
        }

        private static IEnumerable<int> findAffectedCells(int cell, int gridWidth, int gridHeight, CellDirection direction)
        {
            var x = cell % 9;
            var y = cell / 9;
            while (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            {
                yield return x + 9 * y;
                x += direction switch { CellDirection.Left => -1, CellDirection.Right => 1, _ => 0 };
                y += direction switch { CellDirection.Up => -1, CellDirection.Down => 1, _ => 0 };
            }
        }

        /// <summary>Override; see base.</summary>
        public override bool CanReevaluate => true;

        /// <summary>Override; see base.</summary>
        public override ConstraintResult Process(SolverState state)
        {
            state.MarkImpossible(AffectedCells[0], value => value < 0 || value >= AffectedCells.Length || state.IsImpossible(AffectedCells[value], Value));

            if (state.LastPlacedCell == AffectedCells[0])
                // The focus cell has been set, therefore place the value in the correct position
                state.MustBe(AffectedCells[state.LastPlacedValue], Value);

            return null;
        }

        /// <summary>Used to indicate directions in <see cref="FindTheValueConstraint(int, int, int, CellDirection, int)"/>.</summary>
        public enum CellDirection { Up, Right, Down, Left }
    }
}

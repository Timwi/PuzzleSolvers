using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement puzzle that requires a cell’s value to indicate the position of
    ///     another value within a specified run of cells.</summary>
    /// <remarks>
    ///     Examples of this include row/column indexing Sudoku. In a column indexing Sudoku, a digit on an indexing cell in
    ///     column X indicates the column in which X is placed in its row. For example, if R3C1 is a column indexing cell, a 4
    ///     in it would indicate that R3C4 is 1. In this example, the set of affected cells would be the whole row; <see
    ///     cref="Position"/> would be 0 (the column number, but counting from 0), and <see cref="Value"/> would be 1 (the
    ///     value to be placed).</remarks>
    /// <param name="affectedCells">
    ///     The set of cells affected by this constraint (the “run”).</param>
    /// <param name="position">
    ///     The index of the cell within <paramref name="affectedCells"/> to which the constraint applies.</param>
    /// <param name="value">
    ///     The value that must be assigned to the cell at the specified position for the constraint to be satisfied.</param>
    /// <param name="offset">
    ///     The discrepancy between the 0-based indexing used by <paramref name="affectedCells"/> and the value to be placed
    ///     in the targeted cell. In a typical 9×9 indexing Sudoku, the value to be placed is 1-based, so the offset would be
    ///     1.</param>
    public class IndexingConstraint(IEnumerable<int> affectedCells, int position, int value, int offset = 1) : Constraint(affectedCells)
    {
        /// <summary>The index of the cell within <see cref="Constraint.AffectedCells"/> to which the constraint applies.</summary>
        public int Position { get; private set; } = position;
        /// <summary>The value that must be assigned to the cell at the specified position for the constraint to be satisfied.</summary>
        public int Value { get; private set; } = value;
        /// <summary>
        ///     The discrepancy between the 0-based indexing used by <see cref="Constraint.AffectedCells"/> and the value to
        ///     be placed in the targeted cell. In a typical 9×9 indexing Sudoku, the value to be placed is 1-based, so the
        ///     offset would be 1.</summary>
        public int Offset { get; private set; } = offset;

        /// <inheritdoc/>
        public override int? NumCombinations => AffectedCells.Length;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state) => new ConstraintReplace(new CombinationsConstraint(AffectedCells, Enumerable.Range(0, AffectedCells.Length).Select(ix =>
        {
            if (ix == Position && Value != ix + Offset)
                return null;
            var combination = new int?[AffectedCells.Length];
            combination[ix] = Value;
            combination[Position] = ix + Offset;
            return combination;
        }).Where(c => c != null)));
    }
}

using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement grid puzzle where no adjacent cells can have numerically consecutive
    ///     values.</summary>
    public class NoConsecutiveConstraint : Constraint
    {
        /// <summary>
        ///     If not <c>null</c>, the constraint is limited to these values in the grid. This must include all affected
        ///     consecutive digits; for example, if this contains 2 and 3, then 1 and 2 can still be adjacent. If this
        ///     contains a single digit, the constraint is entirely ineffectual.</summary>
        public int[] AffectedValues { get; private set; }
        /// <summary>
        ///     Optionally specifies a limited set of cells on which the no-consecutive constraint is enforced.</summary>
        /// <remarks>
        ///     Note that if <see cref="EnforcedCellsOnly"/> is <c>false</c>, this differs from <see
        ///     cref="Constraint.AffectedCells"/> as that will contain all enforced cells plus those that are adjacent to
        ///     them.</remarks>
        public int[] EnforcedCells { get; private set; }
        /// <summary>
        ///     If <c>true</c>, only adjacent cells that are both within <see cref="EnforcedCells"/> are enforced. Otherwise,
        ///     cells outside of <see cref="EnforcedCells"/> that are adjacent to a cell in <see cref="EnforcedCells"/> are
        ///     also enforced.</summary>
        public bool EnforcedCellsOnly { get; private set; }
        /// <summary>The width of the grid this constraint applies to.</summary>
        public int GridWidth { get; private set; }
        /// <summary>The height of the grid this constraint applies to.</summary>
        public int GridHeight { get; private set; }
        /// <summary>
        ///     If <c>true</c>, the constraint also applies to cells diagonally adjacent to one another. If <c>false</c>, only
        ///     orthogonally adjacent cells are affected.</summary>
        public bool IncludeDiagonals { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="gridWidth">
        ///     See <see cref="GridWidth"/>.</param>
        /// <param name="gridHeight">
        ///     See <see cref="GridHeight"/>.</param>
        /// <param name="includeDiagonals">
        ///     See <see cref="IncludeDiagonals"/>.</param>
        /// <param name="affectedValues">
        ///     See <see cref="AffectedValues"/>.</param>
        /// <param name="enforcedCells">
        ///     See <see cref="EnforcedCells"/>. If <c>null</c>, the default is to enforce the entire grid.</param>
        /// <param name="enforcedCellsOnly">
        ///     See <see cref="EnforcedCellsOnly"/>.</param>
        public NoConsecutiveConstraint(int gridWidth, int gridHeight, bool includeDiagonals, int[] affectedValues = null, IEnumerable<int> enforcedCells = null, bool enforcedCellsOnly = false)
            : base(null)
        {
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            IncludeDiagonals = includeDiagonals;
            AffectedValues = affectedValues;
            EnforcedCells = enforcedCells?.ToArray();
            EnforcedCellsOnly = enforcedCellsOnly;
            AffectedCells = EnforcedCells == null ? null : EnforcedCellsOnly ? EnforcedCells : EnforcedCells.SelectMany(cell => AdjacentCells(cell, gridWidth, gridHeight, includeDiagonals)).Distinct().ToArray();
        }

        /// <summary>
        ///     Returns the set of cells adjacent to the specified cell.</summary>
        /// <param name="cell">
        ///     The cell whose neighbors to examine.</param>
        /// <param name="gridWidth">
        ///     The width of the grid.</param>
        /// <param name="gridHeight">
        ///     The height of the grid.</param>
        /// <param name="includeDiagonals">
        ///     <c>true</c> to include cells that are diagonally adjacent as well.</param>
        public static IEnumerable<int> AdjacentCells(int cell, int gridWidth, int gridHeight, bool includeDiagonals)
        {
            var x = cell % gridWidth;
            var y = cell / gridWidth;
            for (var yy = y - 1; yy <= y + 1; yy++)
                if (yy >= 0 && yy < gridHeight)
                    for (var xx = x - 1; xx <= x + 1; xx++)
                        if (xx >= 0 && xx < gridWidth && (xx != x || yy != y) && (includeDiagonals || xx == x || yy == y))
                            yield return xx + gridWidth * yy;
        }

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            for (var cellIx = 0; cellIx < (state.LastPlacedCell != null ? 1 : AffectedCells != null ? AffectedCells.Length : state.GridSize); cellIx++)
            {
                var cell = state.LastPlacedCell ?? (AffectedCells != null ? AffectedCells[cellIx] : cellIx);
                if (state[cell] == null || (AffectedValues != null && !AffectedValues.Contains(state[cell].Value)))
                    continue;

                foreach (var relatedCell in AdjacentCells(cell, GridWidth, GridHeight, IncludeDiagonals))
                    if (EnforcedCells == null || (EnforcedCellsOnly
                            ? (EnforcedCells.Contains(cell) && EnforcedCells.Contains(relatedCell))
                            : (EnforcedCells.Contains(cell) || EnforcedCells.Contains(relatedCell))))
                    {
                        if (state[cell].Value > state.MinValue && (AffectedValues == null || AffectedValues.Contains(state[cell].Value - 1)))
                            state.MarkImpossible(relatedCell, state[cell].Value - 1);
                        if (state[cell].Value < state.MaxValue && (AffectedValues == null || AffectedValues.Contains(state[cell].Value + 1)))
                            state.MarkImpossible(relatedCell, state[cell].Value + 1);
                    }
            }
            return null;
        }
    }
}

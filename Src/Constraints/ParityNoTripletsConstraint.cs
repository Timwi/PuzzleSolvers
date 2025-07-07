using System.Collections.Generic;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement puzzle that mandates that the parity (odd/evenness) of the values
    ///     cannot form triplets in a row/column.</summary>
    /// <remarks>
    ///     This constraint enforces a single row or column. For a full Binairo puzzle, you will need instances of this
    ///     constraint for every row and every column.</remarks>
    public class ParityNoTripletsConstraint(IEnumerable<int> affectedCells) : Constraint(affectedCells)
    {
        /// <summary>Combinations of existing values that necessitate a constraint enforcement.</summary>
        private static readonly (int offset, int toEnforce)[] _combinations = [(-2, -1), (-1, -2), (-1, 1), (1, -1), (1, 2), (2, 1)];

        /// <inheritdoc/>
        public override bool CanReevaluate => true;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            for (var ix = 0; ix < AffectedCells.Length; ix++)
            {
                foreach (var (offset, toEnforce) in _combinations)
                {
                    if (ix + offset < 0 || ix + offset >= AffectedCells.Length)
                        continue;
                    if (ix + toEnforce < 0 || ix + toEnforce >= AffectedCells.Length)
                        continue;

                    if (state[AffectedCells[ix + toEnforce]] == null &&
                        state.AllSame(AffectedCells[ix + offset], value => value % 2, out var parity1) &&
                        state.AllSame(AffectedCells[ix], value => value % 2, out var parity2) &&
                        parity1 == parity2)
                        state.MarkImpossible(AffectedCells[ix + toEnforce], value => value % 2 == parity1);
                }
            }
            return null;
        }
    }
}

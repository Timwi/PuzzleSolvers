using System.Collections.Generic;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement puzzle that mandates that the parity (odd/evenness) of the values
    ///     cannot form triplets in a row/column.</summary>
    /// <remarks>
    ///     This constraint enforces a single row or column. For a full Binairo puzzle, you will need instances of this
    ///     constraint for every row and every column.</remarks>
    public class ParityNoTripletsConstraint : Constraint
    {
        /// <summary>Constructor.</summary>
        public ParityNoTripletsConstraint(IEnumerable<int> affectedCells) : base(affectedCells) { }

        /// <summary>Combinations of existing values that necessitate a constraint enforcement.</summary>
        private static readonly (int offset, int toEnforce)[] _combinations = new (int offset, int toEnforce)[] { (-2, -1), (-1, -2), (-1, 1), (1, -1), (1, 2), (2, 1) };

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(SolverState state)
        {
            if (state.LastPlacedCell is int lastPlacedCell)
            {
                var ix = AffectedCells.IndexOf(lastPlacedCell);
                foreach (var (offset, toEnforce) in _combinations)
                {
                    if (ix + offset < 0 || ix + offset >= AffectedCells.Length)
                        continue;
                    if (ix + toEnforce < 0 || ix + toEnforce >= AffectedCells.Length)
                        continue;
                    if (state[AffectedCells[ix + offset]] == null || state[AffectedCells[ix + offset]].Value % 2 != state[lastPlacedCell].Value % 2 || state[AffectedCells[ix + toEnforce]] != null)
                        continue;
                    for (var v = state.MinValue; v <= state.MaxValue; v++)
                        if (v % 2 == state[lastPlacedCell].Value % 2)
                            state.MarkImpossible(AffectedCells[ix + toEnforce], v);
                }
            }
            return null;
        }
    }
}

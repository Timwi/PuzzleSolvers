using System.Collections.Generic;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers.Constraints.Exotic
{
    public sealed class LudokuTrackConstraint : Constraint
    {
        public LudokuTrackConstraint(IEnumerable<int> affectedCells) : base(affectedCells)
        {
        }

        public override ConstraintResult Process(SolverState state)
        {
            if (state.LastPlacedCell != null)
            {
                var i = AffectedCells.IndexOf(state.LastPlacedCell.Value);
                var v = state.LastPlacedValue;
                state.MarkImpossible(AffectedCells[((i + v) % AffectedCells.Length + AffectedCells.Length) % AffectedCells.Length], v);
                state.MarkImpossible(AffectedCells[((i - v) % AffectedCells.Length + AffectedCells.Length) % AffectedCells.Length], v);
            }
            else
            {
                for (var i = 0; i < AffectedCells.Length; i++)
                    state.MarkImpossible(AffectedCells[i], v => (i + v) % AffectedCells.Length == i);
            }
            return null;
        }
    }
}

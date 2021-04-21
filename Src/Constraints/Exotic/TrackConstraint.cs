using System.Collections.Generic;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers.Constraints.Exotic
{
    /// <summary>
    ///     Describes a constraint in a number-placement puzzle. Two cells that are n steps from one another along the track
    ///     can’t both have the number n. The track is assumed to be a closed loop.</summary>
    public sealed class TrackConstraint : Constraint
    {
        /// <summary>
        ///     Constructor.</summary>
        /// <param name="affectedCells">
        ///     Describes the track. The order of the cells is significant, and the track is assumed to loop back on itself.</param>
        public TrackConstraint(IEnumerable<int> affectedCells) : base(affectedCells)
        {
        }

        /// <summary>Override; see base.</summary>
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

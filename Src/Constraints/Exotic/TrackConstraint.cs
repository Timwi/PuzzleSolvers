using System.Collections.Generic;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers.Exotic;

/// <summary>
///     Describes a constraint as used in Ludoku by Brawlbox
///     (https://logic-masters.de/Raetselportal/Raetsel/zeigen.php?id=000CX2). Two cells that are n steps from one another
///     along the track can’t both have the number n. The track is assumed to be a closed loop.</summary>
/// <param name="affectedCells">
///     Describes the track. The order of the cells is significant, and the track is assumed to loop back on itself.</param>
public sealed class TrackConstraint(IEnumerable<int> affectedCells) : Constraint(affectedCells)
{
    /// <inheritdoc/>
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

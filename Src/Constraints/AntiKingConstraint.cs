using System.Collections.Generic;

namespace PuzzleSolvers;

/// <summary>
///     Describes a constraint in a number placement puzzle where no adjacent cells (including diagonals) can have the same
///     value.</summary>
public class AntiKingConstraint : AntiChessConstraint
{
    /// <summary>
    ///     Constructor.</summary>
    /// <param name="gridWidth">
    ///     See <see cref="AntiChessConstraint.GridWidth"/>.</param>
    /// <param name="gridHeight">
    ///     See <see cref="AntiChessConstraint.GridHeight"/>.</param>
    /// <param name="affectedValues">
    ///     See <see cref="AntiChessConstraint.AffectedValues"/>.</param>
    /// <param name="enforcedCells">
    ///     See <see cref="AntiChessConstraint.EnforcedCells"/>. If <c>null</c>, the default is to enforce the entire grid.</param>
    public AntiKingConstraint(int gridWidth, int gridHeight, int[] affectedValues = null, IEnumerable<int> enforcedCells = null)
        : base(gridWidth, gridHeight, affectedValues, enforcedCells) { }

    /// <summary>Returns all cells reachable from the specified cell by a king’s move in chess.</summary>
    public static IEnumerable<int> KingsMoves(int cell, int gridWidth, int gridHeight)
    {
        for (var dx = -1; dx <= 1; dx++)
            if (cell % gridWidth + dx >= 0 && cell % gridWidth + dx < gridWidth)
                for (var dy = -1; dy <= 1; dy++)
                    if ((dx != 0 || dy != 0) && cell / gridWidth + dy >= 0 && cell / gridWidth + dy < gridHeight)
                        yield return cell + dx + gridWidth * dy;
    }

    /// <inheritdoc/>
    protected override IEnumerable<int> getRelatedCells(int cell) => KingsMoves(cell, GridWidth, GridHeight);
}

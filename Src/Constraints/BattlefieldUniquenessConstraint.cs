using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers;

/// <summary>
///     Describes a “battlefield” constraint: the first and last number in the region represent the sizes of two armies, who
///     march inward; the clue specifies the sum of the digits that are sandwiched between the armies or that are within the
///     armies’ overlap. This constraint implies a uniqueness constraint.</summary>
/// <remarks>
///     Warning: This constraint is very memory-intensive. It is implemented as a <see cref="CombinationsConstraint"/> with
///     all of the possible number combinations for the specified set of cells. Avoid using this on oversized puzzles. (At
///     time of writing, this is only feasible for up to 11 cells, which uses about 2 GB of RAM for each constraint.)</remarks>
/// <param name="clue">
///     The sum of the digits sandwiched or overlapped.</param>
/// <param name="affectedCells">
///     The set of cells affected by this constraint. This is usually a row or column in a grid, but it can be any subset of
///     grid points.</param>
/// <param name="minValue">
///     The minimum value of numbers in the grid for this puzzle.</param>
/// <param name="maxValue">
///     The maximum value of numbers in the grid for this puzzle.</param>
public class BattlefieldUniquenessConstraint(int clue, IEnumerable<int> affectedCells, int minValue = 1, int maxValue = 9) : PermutationUniquenessConstraint(affectedCells, GenerateCombinations(minValue, maxValue, clue, affectedCells.Count()))
{
    /// <summary>The sum of the digits sandwiched or overlapped.</summary>
    public int Clue { get; private set; } = clue;

    private static readonly Dictionary<(int minValue, int maxValue, int clue, int numAffectedCells), int[][]> _cache = [];

    /// <summary>
    ///     Generates (and caches) all possible combinations of digits that satisfy a given Battlefield clue.</summary>
    /// <param name="minValue">
    ///     Minimum value a cell can have.</param>
    /// <param name="maxValue">
    ///     Maximum value a cell can have.</param>
    /// <param name="clue">
    ///     Battlefield clue to satisfy.</param>
    /// <param name="numAffectedCells">
    ///     Number of cells involved in the region.</param>
    public static int[][] GenerateCombinations(int minValue, int maxValue, int clue, int numAffectedCells)
    {
        lock (_cache)
        {
            if (!_cache.TryGetValue((minValue, maxValue, clue, numAffectedCells), out var result))
            {
                result = GetCachedPermutations(minValue, maxValue, numAffectedCells).Where(p => CalculateBattlefieldClue(p) == clue).ToArray();
                _cache[(minValue, maxValue, clue, numAffectedCells)] = result;
            }
            return result;
        }
    }

    /// <summary>Calculates what the Battlefield clue would be for a given row of numbers.</summary>
    public static int CalculateBattlefieldClue(int[] values)
    {
        var left = values[0];
        var right = values[values.Length - 1];
        var sum = 0;
        if (values.Length - left - right >= 0)
            for (var cIx = left; cIx < values.Length - right; cIx++)
                sum += values[cIx];
        else
            for (var cIx = values.Length - right; cIx < left; cIx++)
                sum += values[cIx];
        return sum;
    }
}

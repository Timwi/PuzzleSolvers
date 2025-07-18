using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes an “X-sum” constraint: the first X numbers must sum up to a specified total, where X is the first of
    ///     those digits. This constraint implies a uniqueness constraint.</summary>
    /// <remarks>
    ///     Warning: This constraint is very memory-intensive. It is implemented as a <see cref="CombinationsConstraint"/>
    ///     with all of the possible number combinations for the specified set of cells. Avoid using this on oversized
    ///     puzzles.</remarks>
    /// <param name="sum">
    ///     The total that the affected numbers must sum up to.</param>
    /// <param name="affectedCells">
    ///     The set of cells affected by this constraint. This is usually a row or column in a grid, but it can be any subset
    ///     of grid points.</param>
    /// <param name="minValue">
    ///     The minimum value of numbers in the grid for this puzzle.</param>
    /// <param name="maxValue">
    ///     The maximum value of numbers in the grid for this puzzle.</param>
    public class XSumUniquenessConstraint(int sum, IEnumerable<int> affectedCells, int minValue = 1, int maxValue = 9) : PermutationUniquenessConstraint(affectedCells, generateCombinations(minValue, maxValue, sum, affectedCells.Count()))
    {
        /// <summary>The total that the affected numbers must sum up to.</summary>
        public int Sum { get; private set; } = sum;

        private static int[][] generateCombinations(int minValue, int maxValue, int sum, int numAffectedCells) =>
            GetCachedPermutations(minValue, maxValue, numAffectedCells).Where(p => p.Take(p[0]).Sum() == sum).ToArray();
    }
}

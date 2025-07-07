using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a “killer cage”: the numbers within the region must be unique and sum up to a specified total.</summary>
    /// <remarks>
    ///     Warning: This constraint is very memory-intensive. It is implemented as a <see cref="CombinationsConstraint"/>
    ///     with all of the possible number combinations for the specified set of cells. Avoid using this on oversized
    ///     puzzles.</remarks>
    /// <param name="sum">
    ///     The total that the cells must sum up to.</param>
    /// <param name="affectedCells">
    ///     The set of cells affected by this constraint (the “cage”).</param>
    /// <param name="minValue">
    ///     The minimum value of numbers in the grid for this puzzle.</param>
    /// <param name="maxValue">
    ///     The maximum value of numbers in the grid for this puzzle.</param>
    public class SumUniquenessConstraint(int sum, IEnumerable<int> affectedCells, int minValue = 1, int maxValue = 9) : CombinationsConstraint(affectedCells, GenerateCombinations(minValue, maxValue, sum, affectedCells.Count()))
    {
        /// <summary>The total that the cells must sum up to.</summary>
        public int Sum { get; private set; } = sum;

        private static readonly Dictionary<(int minValue, int maxValue, int sum, int numAffectedCells), int[][]> _cache = [];

        /// <summary>
        ///     Generates all combinations of <paramref name="num"/> unique values between <paramref name="minValue"/> and
        ///     <paramref name="maxValue"/> that sum up to <paramref name="sum"/>.</summary>
        public static int[][] GenerateCombinations(int minValue, int maxValue, int sum, int num, int[] forbiddenValues = null)
        {
            lock (_cache)
            {
                if (_cache.TryGetValue((minValue, maxValue, sum, num), out var result))
                    return result;

                IEnumerable<int[]> findCombinations(int[] sofar, int remainingSum)
                {
                    if (remainingSum == 0 && sofar.Length == num)
                    {
                        yield return sofar;
                        yield break;
                    }
                    if (sofar.Length >= num)
                        yield break;
                    for (var v = minValue; v <= maxValue; v++)
                        if (remainingSum - v >= 0 && !sofar.Contains(v) && (forbiddenValues == null || !forbiddenValues.Contains(v)))
                            foreach (var s in findCombinations(sofar.Insert(sofar.Length, v), remainingSum - v))
                                yield return s;
                }

                result = findCombinations([], sum).ToArray();
                _cache[(minValue, maxValue, sum, num)] = result;
                return result;
            }
        }
    }
}

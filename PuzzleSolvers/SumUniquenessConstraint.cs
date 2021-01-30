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
    public class SumUniquenessConstraint : CombinationsConstraint
    {
        /// <summary>The total that the cells must sum up to.</summary>
        public int Sum { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="sum">
        ///     The total that the cells must sum up to.</param>
        /// <param name="affectedCells">
        ///     The set of cells affected by this constraint (the “cage”).</param>
        /// <param name="minValue">
        ///     The minimum value of numbers in the grid for this puzzle.</param>
        /// <param name="maxValue">
        ///     The maximum value of numbers in the grid for this puzzle.</param>
        public SumUniquenessConstraint(int sum, IEnumerable<int> affectedCells, int minValue = 1, int maxValue = 9)
            : base(affectedCells, generateCombinations(minValue, maxValue, sum, affectedCells.Count()))
        {
            Sum = sum;
        }

        private static readonly Dictionary<(int minValue, int maxValue, int sum, int numAffectedCells), int[][]> _cache = new Dictionary<(int minValue, int maxValue, int sum, int numAffectedCells), int[][]>();

        private static int[][] generateCombinations(int minValue, int maxValue, int sum, int numAffectedCells)
        {
            lock (_cache)
            {
                if (_cache.TryGetValue((minValue, maxValue, sum, numAffectedCells), out var result))
                    return result;

                IEnumerable<int[]> findCombinations(int[] sofar, int remainingSum)
                {
                    if (remainingSum == 0 && sofar.Length == numAffectedCells)
                    {
                        yield return sofar;
                        yield break;
                    }
                    if (sofar.Length >= numAffectedCells)
                        yield break;
                    for (var v = minValue; v <= maxValue; v++)
                        if (remainingSum - v >= 0 && !sofar.Contains(v))
                            foreach (var s in findCombinations(sofar.Insert(sofar.Length, v), remainingSum - v))
                                yield return s;
                }

                result = findCombinations(new int[0], sum).ToArray();
                _cache[(minValue, maxValue, sum, numAffectedCells)] = result;
                return result;
            }
        }
    }
}

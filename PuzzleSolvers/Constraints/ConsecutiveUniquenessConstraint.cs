using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a “Renban cage”: the numbers within the region must be unique and form a set of consecutive integers
    ///     (though not necessarily in order).</summary>
    /// <remarks>
    ///     Warning: This constraint is very memory-intensive. It is implemented as a <see cref="CombinationsConstraint"/>
    ///     with all of the possible number combinations for the specified set of cells. Avoid using this on oversized
    ///     puzzles.</remarks>
    public class ConsecutiveUniquenessConstraint : CombinationsConstraint
    {
        /// <summary>
        ///     Constructor.</summary>
        /// <param name="affectedCells">
        ///     The set of cells affected by this constraint (the “cage”).</param>
        /// <param name="minValue">
        ///     The minimum value of numbers in the grid for this puzzle.</param>
        /// <param name="maxValue">
        ///     The maximum value of numbers in the grid for this puzzle.</param>
        public ConsecutiveUniquenessConstraint(IEnumerable<int> affectedCells, int minValue = 1, int maxValue = 9)
            : base(affectedCells, generateCombinations(minValue, maxValue, affectedCells.Count())) { }

        private static readonly Dictionary<(int minValue, int maxValue, int numAffectedCells), int[][]> _cache = new Dictionary<(int minValue, int maxValue, int numAffectedCells), int[][]>();

        private static int[][] generateCombinations(int minValue, int maxValue, int numAffectedCells)
        {
            lock (_cache)
            {
                if (_cache.TryGetValue((minValue, maxValue, numAffectedCells), out var result))
                    return result;

                IEnumerable<int[]> findCombinations(int[] sofar)
                {
                    if (sofar.Length == numAffectedCells && sofar.Count(v => !sofar.Contains(v + 1)) == 1)
                    {
                        yield return sofar;
                        yield break;
                    }
                    if (sofar.Length >= numAffectedCells)
                        yield break;
                    for (var v = minValue; v <= maxValue; v++)
                        if (!sofar.Contains(v) && (sofar.Length == 0 || (v >= sofar.Min() - (numAffectedCells - sofar.Length) && v <= sofar.Max() + (numAffectedCells - sofar.Length))))
                            foreach (var s in findCombinations(sofar.Insert(sofar.Length, v)))
                                yield return s;
                }

                result = findCombinations(new int[0]).ToArray();
                _cache[(minValue, maxValue, numAffectedCells)] = result;
                return result;
            }
        }
    }
}

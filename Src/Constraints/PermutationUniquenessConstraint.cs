using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers;

/// <summary>Describes a <see cref="CombinationsConstraint"/> that also implies a <see cref="UniquenessConstraint"/>.</summary>
public class PermutationUniquenessConstraint : CombinationsConstraint
{
    /// <summary>Constructor.</summary>
    protected PermutationUniquenessConstraint(IEnumerable<int> affectedCells, IEnumerable<int[]> combinations) : base(affectedCells, combinations) { }

    /// <summary>
    ///     Constructs a <see cref="PermutationUniquenessConstraint"/> by generating all permutations of numbers between
    ///     <paramref name="minValue"/> and <paramref name="maxValue"/> that satisfy the given <paramref name="predicate"/>.</summary>
    /// <param name="affectedCells">
    ///     The cells affected by this constraint.</param>
    /// <param name="minValue">
    ///     The minimum value a cell can have.</param>
    /// <param name="maxValue">
    ///     The maximum value a cell can have.</param>
    /// <param name="predicate">
    ///     A function defining what permutations are acceptable.</param>
    public PermutationUniquenessConstraint(IEnumerable<int> affectedCells, int minValue, int maxValue, Func<int[], bool> predicate)
        : base(affectedCells, GetCachedPermutations(minValue, maxValue, affectedCells.Count()).Where(predicate))
    {
    }

    private static readonly Dictionary<(int minValue, int maxValue, int numAffectedCells), int[][]> _cachePermutations = [];

    /// <summary>
    ///     Helper method to generate all permutations of numbers within a specified number of affected cells. The array
    ///     generated by this is cached.</summary>
    protected static int[][] GetCachedPermutations(int minValue, int maxValue, int numAffectedCells)
    {
        lock (_cachePermutations)
        {
            if (!_cachePermutations.TryGetValue((minValue, maxValue, numAffectedCells), out var permutations))
            {
                IEnumerable<int[]> getPermutations(int[] sofar, int ix)
                {
                    if (ix == numAffectedCells)
                    {
                        yield return sofar.ToArray();
                        yield break;
                    }

                    for (var i = minValue; i <= maxValue; i++)
                        if (!sofar.Take(ix).Contains(i))
                        {
                            sofar[ix] = i;
                            foreach (var item in getPermutations(sofar, ix + 1))
                                yield return item;
                        }
                }

                permutations = getPermutations(new int[numAffectedCells], 0).ToArray();
                _cachePermutations[(minValue, maxValue, numAffectedCells)] = permutations;
            }
            return permutations;
        }
    }
}

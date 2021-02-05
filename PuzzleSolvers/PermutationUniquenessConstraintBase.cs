﻿using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    public abstract class PermutationUniquenessConstraintBase : CombinationsConstraint
    {
        protected PermutationUniquenessConstraintBase(IEnumerable<int> affectedCells, IEnumerable<int[]> combinations) : base(affectedCells, combinations) { }

        private static readonly Dictionary<(int minValue, int maxValue, int numAffectedCells), int[][]> _cachePermutations = new Dictionary<(int minValue, int maxValue, int numAffectedCells), int[][]>();

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
}

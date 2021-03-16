using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a “sandwich wraparound” constraint: the numbers that follow a specific number and precede a specific
    ///     other number, wrapping around the grid if necessary, must sum up to a specified total. This constraint implies a
    ///     uniqueness constraint.</summary>
    /// <remarks>
    ///     Warning: This constraint is very memory-intensive. It is implemented as a <see cref="CombinationsConstraint"/>
    ///     with all of the possible number combinations for the specified set of cells. Avoid using this on oversized
    ///     puzzles.</remarks>
    public class SandwichWraparoundUniquenessConstraint : CombinationsConstraint
    {
        /// <summary>The number that forms the beginning of the sandwich.</summary>
        public int Value1 { get; private set; }
        /// <summary>The number that forms the end of the sandwich.</summary>
        public int Value2 { get; private set; }
        /// <summary>The total that the sandwiched numbers must sum up to.</summary>
        public int Sum { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="value1">
        ///     The first number forms the left edge of the sandwich.</param>
        /// <param name="value2">
        ///     The second number that forms the right edge of the sandwich.</param>
        /// <param name="sum">
        ///     The total that the sandwiched numbers must sum up to.</param>
        /// <param name="affectedCells">
        ///     The set of cells affected by this constraint. This is usually a row or column in a grid, but it can be any
        ///     subset of grid points.</param>
        /// <param name="minValue">
        ///     The minimum value of numbers in the grid for this puzzle.</param>
        /// <param name="maxValue">
        ///     The maximum value of numbers in the grid for this puzzle.</param>
        public SandwichWraparoundUniquenessConstraint(int value1, int value2, int sum, IEnumerable<int> affectedCells, int minValue = 1, int maxValue = 9)
            : base(affectedCells, generateCombinations(minValue, maxValue, value1, value2, sum, affectedCells.Count()))
        {
            Value1 = value1;
            Value2 = value2;
            Sum = sum;
        }

        private static int?[][] generateCombinations(int minValue, int maxValue, int value1, int value2, int sum, int numAffectedCells)
        {
            // Function to find all possible sandwiches
            IEnumerable<int?[]> findSandwiches(int?[] sofar, int ix, int remainingSum)
            {
                if (remainingSum == 0)
                {
                    yield return sofar.Subarray(0, ix);
                    yield break;
                }
                if (ix >= numAffectedCells)
                    yield break;
                for (var v = minValue; v <= maxValue; v++)
                    if (v != value1 && v != value2 && remainingSum - v >= 0 && !sofar.Take(ix).Contains(v))
                    {
                        sofar[ix] = v;
                        foreach (var s in findSandwiches(sofar, ix + 1, remainingSum - v))
                            yield return s;
                    }
            }

            return findSandwiches(new int?[numAffectedCells], 0, sum).SelectMany(sandwich =>
                Enumerable.Range(0, numAffectedCells)
                    .Select(ix =>
                    {
                        var arr = new int?[numAffectedCells];
                        arr[ix] = value1;
                        for (var i = 0; i < sandwich.Length; i++)
                            arr[(ix + 1 + i) % arr.Length] = sandwich[i];
                        arr[(ix + 1 + sandwich.Length) % arr.Length] = value2;
                        return arr;
                    }))
                    .ToArray();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a “sandwich” constraint: the numbers sandwiched between two specific numbers must sum up to a specified
    ///     total. This constraint implies a uniqueness constraint. The two specified numbers can be in any order.</summary>
    /// <remarks>
    ///     Warning: This constraint is very memory-intensive. It is implemented as a <see cref="CombinationsConstraint"/>
    ///     with all of the possible number combinations for the specified set of cells. Avoid using this on oversized
    ///     puzzles.</remarks>
    public class SandwichUniquenessConstraint : CombinationsConstraint
    {
        /// <summary>One of the numbers that form the edges of the sandwich (the other is <see cref="Value2"/>).</summary>
        public int Value1 { get; private set; }
        /// <summary>One of the numbers that form the edges of the sandwich (the other is <see cref="Value1"/>).</summary>
        public int Value2 { get; private set; }
        /// <summary>The total that the sandwiched numbers must sum up to.</summary>
        public int Sum { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="value1">
        ///     One of the numbers that form the edges of the sandwich.</param>
        /// <param name="value2">
        ///     The other number that forms the edge of the sandwich.</param>
        /// <param name="sum">
        ///     The total that the sandwiched numbers must sum up to.</param>
        /// <param name="affectedCells">
        ///     The set of cells affected by this constraint. This is usually a row or column in a grid, but it can be any
        ///     subset of grid points.</param>
        /// <param name="minValue">
        ///     The minimum value of numbers in the grid for this puzzle.</param>
        /// <param name="maxValue">
        ///     The maximum value of numbers in the grid for this puzzle.</param>
        public SandwichUniquenessConstraint(int value1, int value2, int sum, IEnumerable<int> affectedCells, int minValue = 1, int maxValue = 9)
            : base(affectedCells, GenerateCombinations(minValue, maxValue, value1, value2, sum, affectedCells.Count()))
        {
            Value1 = value1;
            Value2 = value2;
            Sum = sum;
        }

        /// <summary>
        ///     Returns a collection containing all combinations of <paramref name="numAffectedCells"/> cells of values
        ///     between <paramref name="minValue"/> and <paramref name="maxValue"/> in which the sum of the digits sandwiched
        ///     between <paramref name="value1"/> and <paramref name="value2"/> is <paramref name="sum"/>.</summary>
        public static int?[][] GenerateCombinations(int minValue, int maxValue, int value1, int value2, int sum, int numAffectedCells)
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
                Enumerable.Range(1, numAffectedCells - sandwich.Length - 1)
                    // value1 first
                    .Select(ix => { var arr = new int?[numAffectedCells]; Array.Copy(sandwich, 0, arr, ix, sandwich.Length); arr[ix - 1] = value1; arr[ix + sandwich.Length] = value2; return arr; })
                    .Concat(Enumerable.Range(1, numAffectedCells - sandwich.Length - 1)
                        // value2 first
                        .Select(ix => { var arr = new int?[numAffectedCells]; Array.Copy(sandwich, 0, arr, ix, sandwich.Length); arr[ix - 1] = value2; arr[ix + sandwich.Length] = value1; return arr; })))
                    .ToArray();
        }
    }
}

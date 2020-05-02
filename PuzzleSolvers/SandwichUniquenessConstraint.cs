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
        /// <param name="color">
        ///     See <see cref="Constraint.CellColor"/>.</param>
        /// <param name="backgroundColor">
        ///     See <see cref="Constraint.CellBackgroundColor"/>.</param>
        public SandwichUniquenessConstraint(int value1, int value2, int sum, IEnumerable<int> affectedCells, int minValue = 1, int maxValue = 9, ConsoleColor? color = null, ConsoleColor? backgroundColor = null)
            : base(affectedCells, generateCombinations(minValue, maxValue, value1, value2, sum, affectedCells.ToArray()), color, backgroundColor)
        {
            Value1 = value1;
            Value2 = value2;
            Sum = sum;
        }

        private static int[][] generateCombinations(int minValue, int maxValue, int value1, int value2, int sum, int[] affectedCells)
        {
            // Function to find all possible sandwiches
            IEnumerable<int[]> findSandwiches(int[] sofar, int remainingSum)
            {
                if (remainingSum == 0)
                {
                    yield return sofar;
                    yield break;
                }
                if (sofar.Length >= affectedCells.Length)
                    yield break;
                for (var v = minValue; v <= maxValue; v++)
                    if (v != value1 && v != value2 && remainingSum - v >= 0 && !sofar.Contains(v))
                        foreach (var s in findSandwiches(sofar.Insert(sofar.Length, v), remainingSum - v))
                            yield return s;
            }

            return findSandwiches(new int[0], sum).SelectMany(sandwich =>
            {
                // Function to find all combinations of numbers outside of the sandwich (“outies”)
                IEnumerable<int[]> findOuties(int[] sofar, int remainingCells)
                {
                    if (remainingCells == 0)
                    {
                        foreach (var v1first in new[] { false, true })
                            for (var chunk = 0; chunk <= sofar.Length; chunk++)
                            {
                                var fullResult = new int[affectedCells.Length];
                                Array.Copy(sofar, 0, fullResult, 0, chunk);
                                fullResult[chunk] = v1first ? value1 : value2;
                                Array.Copy(sandwich, 0, fullResult, chunk + 1, sandwich.Length);
                                fullResult[chunk + 1 + sandwich.Length] = v1first ? value2 : value1;
                                Array.Copy(sofar, chunk, fullResult, chunk + sandwich.Length + 2, sofar.Length - chunk);
                                yield return fullResult;
                            }
                        yield break;
                    }
                    for (var v = minValue; v <= maxValue; v++)
                        if (v != value1 && v != value2 && !sandwich.Contains(v) && !sofar.Contains(v))
                            foreach (var s in findOuties(sofar.Insert(sofar.Length, v), remainingCells - 1))
                                yield return s;
                }
                return findOuties(new int[0], affectedCells.Length - sandwich.Length - 2);
            }).ToArray();
        }
    }
}

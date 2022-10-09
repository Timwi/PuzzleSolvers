using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers.Exotic
{
    /// <summary>
    ///     Describes a “sandwich” constraint that does not imply uniqueness of all digits. The digits <see cref="Digit1"/>
    ///     and <see cref="Digit2"/> must occur exactly once each (in any order) and form the edges of the sandwich. The
    ///     numbers sandwiched between them must sum up to <see cref="Sum"/>.</summary>
    /// <remarks>
    ///     Warning: This constraint is very memory-intensive. It is implemented as a <see cref="CombinationsConstraint"/>
    ///     with all of the possible number combinations for the specified set of cells. Avoid using this on oversized
    ///     puzzles.</remarks>
    public class LittleSandwichConstraint : Constraint
    {
        /// <summary>One of the numbers that form the edges of the sandwich (the other is <see cref="Digit2"/>).</summary>
        public int Digit1 { get; private set; }
        /// <summary>One of the numbers that form the edges of the sandwich (the other is <see cref="Digit1"/>).</summary>
        public int Digit2 { get; private set; }
        /// <summary>The total that the sandwiched numbers must sum up to.</summary>
        public int Sum { get; private set; }

        private CombinationsConstraint _subconstraint;

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="affectedCells">
        ///     The set of cells affected by this constraint.</param>
        /// <param name="sum">
        ///     The total that the sandwiched numbers must sum up to.</param>
        /// <param name="digit1">
        ///     One of the numbers that form the edges of the sandwich.</param>
        /// <param name="digit2">
        ///     The other number that forms the edge of the sandwich.</param>
        /// <param name="minValue">
        ///     The minimum value of numbers in the grid for this puzzle.</param>
        /// <param name="maxValue">
        ///     The maximum value of numbers in the grid for this puzzle.</param>
        public LittleSandwichConstraint(IEnumerable<int> affectedCells, int sum, int digit1 = 1, int digit2 = 9, int minValue = 1, int maxValue = 9)
            : base(affectedCells)
        {
            Sum = sum;
            Digit1 = digit1;
            Digit2 = digit2;
            _subconstraint = new CombinationsConstraint(AffectedCells, makeCombinations(affectedCells.Count(), sum, digit1, digit2, minValue, maxValue));
        }

        private LittleSandwichConstraint(IEnumerable<int> affectedCells) : base(affectedCells) { }

        private static IEnumerable<int?[]> makeCombinations(int numCells, int sum, int digit1, int digit2, int minValue, int maxValue)
        {
            if (numCells < 0)
                throw new ArgumentException("‘numCells’ cannot be negative.", nameof(numCells));
            if (sum < 0)
                throw new ArgumentException("‘sum’ cannot be negative.", nameof(sum));
            if (maxValue < minValue)
                throw new ArgumentException("‘maxValue’ must be greater than or equal to ‘minValue’.", nameof(maxValue));

            var allowedValues = Enumerable.Range(minValue, maxValue - minValue + 1).Where(i => i != digit1 && i != digit2).ToArray();
            var min = allowedValues.First();
            var max = allowedValues.Last();

            var combinations = new List<int?[]>();
            var maxLen = (sum + min - 1) / min;
            for (var subLen = (sum + max - 1) / max; subLen <= maxLen; subLen++)
                foreach (var combination in sumCombinations(allowedValues, subLen, sum))
                    for (var startIndex = 0; startIndex + subLen + 2 <= numCells; startIndex++)
                    {
                        // Digit1, then meat, then Digit2
                        var arr = new int?[numCells];
                        arr[startIndex] = digit1;
                        for (var i = 0; i < subLen; i++)
                            arr[startIndex + 1 + i] = combination[i];
                        arr[startIndex + 1 + subLen] = digit2;
                        combinations.Add(arr);

                        // Digit2, then meat, then Digit1
                        arr = (int?[]) arr.Clone();
                        arr[startIndex] = digit2;
                        arr[startIndex + 1 + subLen] = digit1;
                        combinations.Add(arr);
                    }
            return combinations;
        }

        private static IEnumerable<int[]> sumCombinations(int[] allowedValues, int numDigits, int targetSum)
        {
            IEnumerable<int[]> recurse(int[] sofar, int ix, int partialSum)
            {
                if (ix == numDigits && partialSum == targetSum)
                {
                    yield return sofar.ToArray();
                    yield break;
                }

                var rest = numDigits - ix - 1;
                var restMin = rest * allowedValues[0];
                var restMax = rest * allowedValues[allowedValues.Length - 1];

                foreach (var v in allowedValues)
                    if ((targetSum - partialSum - v).IsBetween(restMin, restMax))
                    {
                        sofar[ix] = v;
                        foreach (var s in recurse(sofar, ix + 1, partialSum + v))
                            yield return s;
                    }
            }
            return recurse(new int[numDigits], 0, 0);
        }

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            // Disallow duplicates of Digit1 and Digit2
            if (state.LastPlacedCell != null)
            {
                if (state.LastPlacedValue == Digit1)
                    foreach (var cell in AffectedCells)
                        if (cell != state.LastPlacedCell.Value)
                            state.MarkImpossible(cell, Digit1);

                if (state.LastPlacedValue == Digit2)
                    foreach (var cell in AffectedCells)
                        if (cell != state.LastPlacedCell.Value)
                            state.MarkImpossible(cell, Digit2);
            }

            var subresult = _subconstraint?.Process(state);
            if (subresult is ConstraintReplace repl)
            {
                var c = repl.NewConstraints.Count();
                if (c > 1)
                    throw new InvalidOperationException("LittleSandwichConstraint expected 0 or 1 subconstraints.");
                return new ConstraintReplace(new LittleSandwichConstraint(AffectedCells)
                {
                    _subconstraint = (CombinationsConstraint) repl.NewConstraints.FirstOrDefault(),
                    Digit1 = Digit1,
                    Digit2 = Digit2,
                    Sum = Sum
                });
            }
            else if (subresult is ConstraintViolation)
                return subresult;
            return null;
        }

        /// <inheritdoc/>
        public override string ToString() => _subconstraint == null ? "Combinations done" : $"{_subconstraint.Combinations.Length} combinations";
    }
}

using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a “skyscraper” constraint: the numbers in the grid represent the height of a skyscraper; taller
    ///     skyscrapers occlude smaller ones behind them; and the clue specifies how many skyscrapers are visible from the
    ///     direction of the clue. This constraint implies a uniqueness constraint.</summary>
    /// <remarks>
    ///     Warning: This constraint is very memory-intensive. It is implemented as a <see cref="CombinationsConstraint"/>
    ///     with all of the possible number combinations for the specified set of cells. Avoid using this on oversized
    ///     puzzles. (At time of writing, this is only feasible for up to 11 cells, which uses about 2 GB of RAM for each
    ///     constraint.)</remarks>
    public class SkyscraperUniquenessConstraint : PermutationUniquenessConstraintBase
    {
        /// <summary>The number of skyscrapers visible.</summary>
        public int Clue { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="clue">
        ///     The number of skyscrapers visible.</param>
        /// <param name="affectedCells">
        ///     The set of cells affected by this constraint. This is usually a row or column in a grid, but it can be any
        ///     subset of grid points.</param>
        /// <param name="minValue">
        ///     The minimum value of numbers in the grid for this puzzle.</param>
        /// <param name="maxValue">
        ///     The maximum value of numbers in the grid for this puzzle.</param>
        public SkyscraperUniquenessConstraint(int clue, IEnumerable<int> affectedCells, int minValue = 1, int maxValue = 9)
            : base(affectedCells, generateCombinations(minValue, maxValue, clue, affectedCells.Count()))
        {
            Clue = clue;
        }

        private static readonly Dictionary<(int minValue, int maxValue, int clue, int numAffectedCells), int[][]> _cache = new Dictionary<(int minValue, int maxValue, int clue, int numAffectedCells), int[][]>();

        private static int[][] generateCombinations(int minValue, int maxValue, int clue, int numAffectedCells)
        {
            lock (_cache)
            {
                if (!_cache.TryGetValue((minValue, maxValue, clue, numAffectedCells), out var result))
                {
                    result = GetCachedPermutations(minValue, maxValue, numAffectedCells).Where(p => CalculateSkyscraperClue(p) == clue).ToArray();
                    _cache[(minValue, maxValue, clue, numAffectedCells)] = result;
                }
                return result;
            }
        }

        /// <summary>Calculates what the Skyscraper clue would be for a given row of numbers.</summary>
        public static int CalculateSkyscraperClue(IEnumerable<int> values)
        {
            var cl = 0;
            int? prev = null;
            foreach (var value in values)
            {
                if (prev == null || value > prev.Value)
                {
                    cl++;
                    prev = value;
                }
            }
            return cl;
        }
    }
}

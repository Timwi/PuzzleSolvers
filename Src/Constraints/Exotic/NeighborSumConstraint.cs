using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers.Exotic
{
    /// <summary>
    ///     Describes a constraint as used in Neighbor Sum Sudoku by Timwi
    ///     (https://logic-masters.de/Raetselportal/Raetsel/zeigen.php?id=0005NV): Each number outside the grid gives the sum
    ///     of all orthogonal neighbors of one cell in the corresponding row or column. Where multiple numbers are given, the
    ///     sums must be in the same order as the clues.</summary>
    /// <remarks>
    ///     This is implemented as a <see cref="CombinationsConstraint"/>, which may make it very memory-intensive on
    ///     oversized puzzles.</remarks>
    /// <param name="isCol">
    ///     If <c>true</c>, the constraint applies to a column; otherwise a row.</param>
    /// <param name="rowCol">
    ///     The row or column (depending on <paramref name="isCol"/>).</param>
    /// <param name="clue">
    ///     The set of neighbor sums. Currently only 1 or 2 numbers are supported.</param>
    /// <param name="furtherRestrictions">
    ///     Pass in a <see cref="UniquenessConstraint"/> collection to restrict the number of combinations generated. This can
    ///     yield profound improvements in the speed of finding solutions. For example, you can construct a <see
    ///     cref="Sudoku"/> instance, then construct this <see cref="NeighborSumConstraint"/> by passing in
    ///     <c>sudoku.Constraints.OfType&lt;UniquenessConstraint&gt;()</c>.</param>
    /// <param name="gridWidth">
    ///     The width of the grid (default: 9).</param>
    /// <param name="gridHeight">
    ///     The height of the grid (default: 9).</param>
    /// <param name="minValue">
    ///     Specifies the smallest value used in this puzzle.</param>
    /// <param name="maxValue">
    ///     Specifies the largest value used in this puzzle.</param>
    public class NeighborSumConstraint(bool isCol, int rowCol, int[] clue, IEnumerable<UniquenessConstraint> furtherRestrictions = null, int gridWidth = 9, int gridHeight = 9, int minValue = 1, int maxValue = 9) : CombinationsConstraint(Enumerable.Range(0, gridWidth * gridHeight), generateCombinations(isCol, rowCol, clue, furtherRestrictions, gridWidth, gridHeight, minValue, maxValue))
    {
        /// <summary>Specifies whether the constraint applies to a column (<c>true</c>) or row (<c>false</c>).</summary>
        public bool IsCol { get; private set; } = isCol;
        /// <summary>Specifies the affected row or column (depending on <see cref="IsCol"/>).</summary>
        public int RowCol { get; private set; } = rowCol;
        /// <summary>Specifies the set of neighbor sums. Currently only 1 or 2 numbers are supported.</summary>
        public int[] Clue { get; private set; } = clue ?? throw new ArgumentNullException(nameof(clue));

        private static IEnumerable<int?[]> generateCombinations(bool isCol, int rowCol, int[] clue, IEnumerable<UniquenessConstraint> furtherRestrictions, int gridWidth, int gridHeight, int minValue, int maxValue)
        {
            var combinations = new List<int?[]>();
            var affectedCenterCells = Enumerable.Range(0, isCol ? gridHeight : gridWidth).Select(ix => isCol ? rowCol + gridWidth * ix : ix + gridWidth * rowCol).ToArray();

            bool combinationAcceptable(int?[] combination)
            {
                if (furtherRestrictions == null)
                    return true;
                foreach (var uniq in furtherRestrictions)
                {
                    for (var i = 0; i < uniq.AffectedCells.Length; i++)
                        if (combination[uniq.AffectedCells[i]] != null)
                            for (var j = i + 1; j < uniq.AffectedCells.Length; j++)
                                if (combination[uniq.AffectedCells[j]] == combination[uniq.AffectedCells[i]])
                                    return false;
                }
                return true;
            }

            switch (clue.Length)
            {
                case 0:
                    return [];

                case 1:
                {
                    for (var i1 = 0; i1 < affectedCenterCells.Length; i1++)
                    {
                        var comb = new int?[gridWidth * gridHeight];
                        var neigh1 = PuzzleUtil.Orthogonal(affectedCenterCells[i1], gridWidth, gridHeight).ToArray();
                        foreach (var numbers in PuzzleUtil.Combinations(minValue, maxValue, neigh1.Length, allowDuplicates: true).Where(c => c.Sum() == clue[0]))
                        {
                            for (var i = 0; i < neigh1.Length; i++)
                                comb[neigh1[i]] = numbers[i];
                            if (combinationAcceptable(comb))
                                combinations.Add((int?[]) comb.Clone());
                        }
                    }
                    break;
                }

                case 2:
                {
                    for (var i1 = 0; i1 < affectedCenterCells.Length; i1++)
                    {
                        var comb = new int?[gridWidth * gridHeight];
                        var neigh1 = PuzzleUtil.Orthogonal(affectedCenterCells[i1], gridWidth, gridHeight).ToArray();
                        foreach (var numbers in PuzzleUtil.Combinations(minValue, maxValue, neigh1.Length, allowDuplicates: true).Where(c => c.Sum() == clue[0]))
                        {
                            for (var i = 0; i < neigh1.Length; i++)
                                comb[neigh1[i]] = numbers[i];
                            if (!combinationAcceptable(comb))
                                continue;

                            for (var i2 = i1 + 1; i2 < affectedCenterCells.Length; i2++)
                            {
                                var neigh2 = PuzzleUtil.Orthogonal(affectedCenterCells[i2], gridWidth, gridHeight).ToArray();
                                foreach (var numbers2 in PuzzleUtil.Combinations(minValue, maxValue, neigh2.Length, allowDuplicates: true).Where(c => c.Sum() == clue[1]))
                                {
                                    var comb2 = (int?[]) comb.Clone();
                                    for (var i = 0; i < neigh2.Length; i++)
                                    {
                                        if (comb2[neigh2[i]] != null && comb2[neigh2[i]] != numbers2[i])
                                            goto busted2;
                                        comb2[neigh2[i]] = numbers2[i];
                                    }
                                    if (combinationAcceptable(comb2))
                                        combinations.Add(comb2);
                                }
                                busted2:;
                            }
                        }
                    }
                    break;
                }

                default:
                    throw new NotImplementedException("NeighborSumConstraint currently only supports up to 2 numbers per clue.");
            }

            return combinations;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in which a specified set of cells must have one of a specified set of exact value
    ///     combinations.</summary>
    public class CombinationsConstraint : Constraint
    {
        /// <summary>The set of combinations allowed for the specified set of cells.</summary>
        public int[][] Combinations { get; private set; }

        /// <summary>Constructor.</summary>
        public CombinationsConstraint(IEnumerable<int> affectedCells, IEnumerable<int[]> combinations, ConsoleColor? color = null, ConsoleColor? backgroundColor = null) : base(affectedCells, color, backgroundColor)
        {
            Combinations = (combinations as int[][]) ?? combinations.ToArray();
        }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            if (ix != null && !AffectedCells.Contains(ix.Value))
                return null;

            var newCombinations = Combinations
                .Where(cmb => Enumerable.Range(0, AffectedCells.Length).All(i => grid[AffectedCells[i]] == null ? !takens[AffectedCells[i]][cmb[i] - minValue] : (grid[AffectedCells[i]].Value == cmb[i] - minValue)))
                .ToArray();
            for (var i = 0; i < AffectedCells.Length; i++)
                if (grid[AffectedCells[i]] == null)
                    for (var v = 0; v < takens[AffectedCells[i]].Length; v++)
                        if (!newCombinations.Any(cmb => cmb[i] == v + minValue))
                            takens[AffectedCells[i]][v] = true;

            return ix == null ? null : new[] { new CombinationsConstraint(AffectedCells, newCombinations, CellColor, CellBackgroundColor) };
        }
    }
}

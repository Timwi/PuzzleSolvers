using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement puzzle (such as Sudoku) where several regions must have the same sum,
    ///     but the sum is not given.</summary>
    public sealed class EqualSumsConstraint : Constraint
    {
        /// <summary>Contains the regions affected by this constraint. Each region is an array of cell indices.</summary>
        public int[][] Regions { get; private set; }

        /// <summary>Constructor.</summary>
        public EqualSumsConstraint(params IEnumerable<int>[] regions) { Regions = regions.Select(r => r.ToArray()).ToArray(); }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            for (var i = 0; i < Regions.Length; i++)
            {
                if (Regions[i].All(cell => grid[cell] != null))
                {
                    var sum = Regions[i].Sum(cell => grid[cell].Value + minValue);
                    return Regions.Select(region => new SumConstraint(sum, region));
                }
            }
            return null;
        }
    }
}

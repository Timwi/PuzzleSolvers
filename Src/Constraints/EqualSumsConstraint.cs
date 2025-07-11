using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement puzzle (such as Sudoku) where several regions must have the same sum,
    ///     but the sum is not given.</summary>
    /// <param name="regions">
    ///     Specifies the regions affected by this constraint. Each region is an array of cell indices.</param>
    public class EqualSumsConstraint(params IEnumerable<int>[] regions) : Constraint(regions.SelectMany(r => r).Distinct())
    {
        /// <summary>Contains the regions affected by this constraint. Each region is an array of cell indices.</summary>
        public int[][] Regions { get; private set; } = regions.Select(r => r.ToArray()).ToArray();

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            for (var i = 0; i < Regions.Length; i++)
            {
                if (Regions[i].All(cell => state[cell] != null))
                {
                    var sum = Regions[i].Sum(cell => state[cell].Value);
                    return Regions.Select(region => new SumConstraint(sum, region)).ToArray();
                }
            }
            return null;
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    public class EqualSumsConstraint : Constraint
    {
        public int[][] Regions;

        public override void MarkInitialTakens(bool[][] takens, int minValue, int maxValue) { }

        public override IEnumerable<Constraint> MarkTaken(bool[][] takens, int?[] grid, int ix, int val, int minValue, int maxValue)
        {
            for (var i = 0; i < Regions.Length; i++)
            {
                if (Regions[i].All(cell => grid[cell] != null))
                {
                    var sum = Regions[i].Sum(cell => grid[cell].Value + minValue);
                    var newConstraints = new List<Constraint>();

                    for (var j = 0; j < Regions.Length; j++)
                    {
                        var region = Regions[j];
                        var newConstraint = new SumConstraint { Sum = sum, AffectedCells = region };
                        foreach (var cell in region)
                            if (grid[cell] != null)
                                newConstraint.MarkTaken(takens, grid, cell, grid[cell].Value, minValue, maxValue);
                        newConstraints.Add(newConstraint);
                    }
                    return newConstraints;
                }
            }
            return null;
        }
    }
}

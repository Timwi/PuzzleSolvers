using System;
using System.Collections.Generic;
using System.Text;

namespace PuzzleSolvers
{
    public class NoTouchConstraint : Constraint
    {
        public int GridWidth;
        public int GridHeight;

        public override void MarkInitialTakens(bool[][] takens, int minValue, int maxValue) { }

        public override IEnumerable<Constraint> MarkTaken(bool[][] takens, int?[] grid, int ix, int val, int minValue, int maxValue)
        {
            var x = ix % GridWidth;
            var y = ix / GridWidth;

            for (var dx = -1; dx <= 1; dx++)
                if (x + dx >= 0 && x + dx < GridWidth)
                    for (var dy = -1; dy <= 1; dy++)
                        if (y + dy >= 0 && y + dy < GridHeight)
                            takens[(x + dx) + 9 * (y + dy)][val] = true;

            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuzzleSolvers
{
    public class UniquenessConstraint : Constraint
    {
        public int[] AffectedCells;
        public ConsoleColor? BackgroundColor;

        public override void MarkInitialTakens(bool[][] takens, int minValue, int maxValue) { }

        public override IEnumerable<Constraint> MarkTaken(bool[][] takens, int?[] grid, int ix, int val, int minValue, int maxValue)
        {
            if (!AffectedCells.Contains(ix))
                return null;

            foreach (var cell in AffectedCells)
                if (cell != ix)
                    takens[cell][val] = true;

            return null;
        }

        public override ConsoleColor? CellBackgroundColor(int ix) => AffectedCells.Contains(ix) ? BackgroundColor : null;
    }
}

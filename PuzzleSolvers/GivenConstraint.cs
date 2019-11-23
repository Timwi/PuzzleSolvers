using System;
using System.Collections.Generic;
using System.Text;

namespace PuzzleSolvers
{
    public class GivenConstraint : Constraint
    {
        public int Location;
        public int Value;

        public override void MarkInitialTakens(bool[][] takens, int minValue, int maxValue)
        {
            for (var i = 0; i < takens[Location].Length; i++)
                if (i + minValue != Value)
                    takens[Location][i] = true;
        }

        public override IEnumerable<Constraint> MarkTaken(bool[][] takens, int?[] grid, int ix, int val, int minValue, int maxValue) => null;

        public override ConsoleColor? CellColor(int ix) => ix == Location ? (ConsoleColor?) ConsoleColor.White : null;
    }
}

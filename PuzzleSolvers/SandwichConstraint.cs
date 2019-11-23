using System;
using RT.Util.ExtensionMethods;
using System.Collections.Generic;

namespace PuzzleSolvers
{
    public class SandwichConstraint : Constraint
    {
        public SandwichConstraint()
        {
            throw new NotImplementedException(@"The implementation for this constraint MAY have a bug, as no Sandwich Sudoku has been successfully solved by this algorithm yet.");
        }

        // These are the values that the sum is sandwiched between
        public int Value1, Value2;
        public int Sum;
        public int[] AffectedCells;

        private int _minCellsNeeded, _maxCellsNeeded;

        public override void MarkInitialTakens(bool[][] takens, int minValue, int maxValue)
        {
            var maxInnerValue = maxValue;
            while (maxInnerValue == Value1 || maxInnerValue == Value2)
                maxInnerValue--;

            var minInnerValue = minValue;
            while (minInnerValue == Value1 || minInnerValue == Value2)
                minInnerValue++;

            _minCellsNeeded = (Sum + maxInnerValue - 1) / maxInnerValue;
            _maxCellsNeeded = Sum / minInnerValue;

            for (var i = AffectedCells.Length - 2 - _minCellsNeeded; i <= _minCellsNeeded + 1; i++)
            {
                takens[AffectedCells[i]][Value1 - minValue] = true;
                takens[AffectedCells[i]][Value2 - minValue] = true;
            }
        }

        public override IEnumerable<Constraint> MarkTaken(bool[][] takens, int?[] grid, int ix, int val, int minValue, int maxValue)
        {
            var isValue1 = (val + minValue == Value1);

            if (!isValue1 && val + minValue != Value2)
                return null;

            var p = Array.IndexOf(AffectedCells, ix);
            if (p == -1)
                return null;

            var otherVal = (isValue1 ? Value2 : Value1) - minValue;

            var p2 = AffectedCells.IndexOf(cell => grid[cell] == otherVal);
            if (p2 != -1)
            {
                // Both sandwich values have been placed, so replace this constraint with a SumConstraint
                var newAffectedCells = (p < p2) ? AffectedCells.Subarray(p + 1, p2 - p - 1) : AffectedCells.Subarray(p2 + 1, p - p2 - 1);
                var sumConstraint = new SumConstraint { Sum = Sum, AffectedCells = newAffectedCells };
                for (var cell = 0; cell < newAffectedCells.Length; cell++)
                    if (grid[cell] != null)
                        sumConstraint.MarkTaken(takens, grid, cell, grid[cell].Value, minValue, maxValue);
                return new[] { sumConstraint };
            }

            for (var i = 0; i < AffectedCells.Length; i++)
            {
                var d = Math.Abs(p - i);
                if (d <= _minCellsNeeded || d >= _maxCellsNeeded + 2)
                    takens[AffectedCells[i]][otherVal] = true;
            }
            return null;
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in which a specific region of cells must sum up to the value in another cell. In a variant
    ///     Sudoku, this is often represented as an arrow coming out of a circle, and hence, the variant is known as “Arrow
    ///     Sudoku”.</summary>
    public class IndirectSumConstraint : Constraint
    {
        /// <summary>The cell that must contain the desired sum.</summary>
        public int SumCell { get; private set; }

        /// <summary>The set of cells that must sum up to the value in <see cref="SumCell"/>.</summary>
        public int[] Region { get; private set; }

        /// <summary>Constructor.</summary>
        public IndirectSumConstraint(int sumCell, IEnumerable<int> region) : base(region.Concat(new[] { sumCell }))
        {
            SumCell = sumCell;
            Region = region.ToArray();
        }

        /// <summary>Override; see base.</summary>
        public override bool CanReevaluate => true;

        /// <summary>Override; see base.</summary>
        public override ConstraintResult Process(SolverState state)
        {
            var minPossibleTarget = state.MinPossible(SumCell);
            var maxPossibleTarget = state.MaxPossible(SumCell);
            var minPossibleSum = Region.Sum(state.MinPossible);
            var maxPossibleSum = Region.Sum(state.MaxPossible);

            // Constrain the sum cell
            if (state[SumCell] == null)
                state.MarkImpossible(SumCell, value => value < minPossibleSum || value > maxPossibleSum);

            // Constrain the operand cells
            for (var ix = 0; ix < Region.Length; ix++)
            {
                var cell = Region[ix];
                var minOther = minPossibleSum - state.MinPossible(cell);
                var maxOther = maxPossibleSum - state.MaxPossible(cell);
                state.MarkImpossible(cell, value => minOther + value > maxPossibleTarget || maxOther + value < minPossibleTarget);
            }

            return null;
        }
    }
}

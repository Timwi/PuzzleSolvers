using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement puzzle (such as a Thermometer Sudoku) where a series of cells must be
    ///     in ascending order.</summary>
    public class LessThanConstraint : Constraint
    {
        /// <summary>
        ///     Contains the region of cells affected by this constraint (the “thermometer”). The order of cells in this array
        ///     is significant as the constraint assumes these to be ordered from smallest to largest value.</summary>
        public new int[] AffectedCells => base.AffectedCells;

        /// <summary>Constructor.</summary>
        public LessThanConstraint(IEnumerable<int> affectedCells) : base(affectedCells) { }

        /// <summary>
        ///     Generates multiple <see cref="LessThanConstraint"/> from a semicolon-separated list of coordinate regions
        ///     represented in the format understood by <see cref="Constraint.TranslateCoordinates(string, int)"/>.</summary>
        /// <param name="representation">
        ///     The string representation of the set of regions.</param>
        /// <param name="gridWidth">
        ///     The width of the grid. If not specified, the default value is 9.</param>
        public static IEnumerable<LessThanConstraint> FromString(string representation, int gridWidth = 9)
        {
            if (representation == null)
                throw new ArgumentNullException(nameof(representation));
            foreach (var piece in representation.Split(';'))
                yield return new LessThanConstraint(TranslateCoordinates(piece, gridWidth));
        }

        /// <inheritdoc/>
        public override bool CanReevaluate => true;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            var min = state.MinPossible(AffectedCells[0]) + 1;
            for (var i = 1; i < AffectedCells.Length; i++)
            {
                state.MarkImpossible(AffectedCells[i], value => value < min);
                min = state.MinPossible(AffectedCells[i]) + 1;
            }
            var max = state.MaxPossible(AffectedCells[AffectedCells.Length - 1]) - 1;
            for (var i = AffectedCells.Length - 2; i >= 0; i--)
            {
                state.MarkImpossible(AffectedCells[i], value => value > max);
                max = state.MaxPossible(AffectedCells[i]) - 1;
            }
            return null;
        }
    }
}

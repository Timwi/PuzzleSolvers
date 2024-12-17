using System;
using System.Collections.Generic;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Describes a constraint in a number-placement puzzle (such as a Thermometer Sudoku) where a series of cells must be
    ///     in ascending order.</summary>
    /// <param name="affectedCells">
    ///     The cells where, in order, each must be less than the next; in other words, the values must be in ascending order.</param>
    /// <param name="minDifference">
    ///     Specifies the minimum difference between adjacent cells. The default is 1. This can be set to 0 to achieve a “slow
    ///     thermometer”.</param>
    public class LessThanConstraint(IEnumerable<int> affectedCells, int minDifference = 1) : Constraint(affectedCells)
    {
        /// <summary>
        ///     Contains the region of cells affected by this constraint (the “thermometer”). The order of cells in this array
        ///     is significant as the constraint assumes these to be ordered from smallest to largest value.</summary>
        public new int[] AffectedCells => base.AffectedCells;

        /// <summary>Specifies the minimum difference between adjacent cells.</summary>
        public int MinDifference { get; private set; } = minDifference;

        /// <summary>
        ///     Generates multiple <see cref="LessThanConstraint"/> from a semicolon-separated list of coordinate regions
        ///     represented in the format understood by <see cref="Constraint.TranslateCoordinates(string, int)"/>.</summary>
        /// <param name="representation">
        ///     The string representation of the set of regions.</param>
        /// <param name="gridWidth">
        ///     The width of the grid. If not specified, the default value is 9.</param>
        /// <param name="minDifference">
        ///     Specifies the minimum difference between adjacent cells. The default is 1. This can be set to 0 to achieve a
        ///     “slow thermometer”.</param>
        public static IEnumerable<LessThanConstraint> FromString(string representation, int gridWidth = 9, int minDifference = 1)
        {
            if (representation == null)
                throw new ArgumentNullException(nameof(representation));
            foreach (var piece in representation.Split(';'))
                yield return new LessThanConstraint(TranslateCoordinates(piece, gridWidth), minDifference);
        }

        /// <inheritdoc/>
        public override bool CanReevaluate => true;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            var min = state.MinPossible(AffectedCells[0]) + MinDifference;
            for (var i = 1; i < AffectedCells.Length; i++)
            {
                state.MarkImpossible(AffectedCells[i], value => value < min);
                min = state.MinPossible(AffectedCells[i]) + MinDifference;
            }
            var max = state.MaxPossible(AffectedCells[AffectedCells.Length - 1]) - MinDifference;
            for (var i = AffectedCells.Length - 2; i >= 0; i--)
            {
                state.MarkImpossible(AffectedCells[i], value => value > max);
                max = state.MaxPossible(AffectedCells[i]) - MinDifference;
            }
            return null;
        }
    }
}

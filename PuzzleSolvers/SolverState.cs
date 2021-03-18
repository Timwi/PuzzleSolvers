using System;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Encapsulates information given to <see cref="Constraint"/> implementations (in a call to <see
    ///     cref="Constraint.MarkTakens(SolverState)"/>) during the puzzle solving algorithm.</summary>
    public abstract class SolverState
    {
        /// <summary>
        ///     Exposes read-only access to the incomplete solution at the current point during the algorithm.</summary>
        /// <remarks>
        ///     In order to communicate that a cell must have a specific value, mark all other possible values on that cell as
        ///     impossible using <see cref="MustBe(int, int)"/>.</remarks>
        public abstract int? this[int cell] { get; }

        /// <summary>
        ///     Marks a specific <paramref name="value"/> in a specific <paramref name="cell"/> as no longer possible.</summary>
        /// <param name="cell">
        ///     The cell to modify. This is a 0-based index and must be less than <see cref="Puzzle.GridSize"/>.</param>
        /// <param name="value">
        ///     The value that is no longer possible. This must be between <see cref="MinValue"/> and <see cref="MaxValue"/>
        ///     (inclusive).</param>
        public abstract void MarkImpossible(int cell, int value);

        /// <summary>
        ///     Marks values in a specific <paramref name="cell"/> as no longer possible that satisfy the given predicate
        ///     <paramref name="isImpossible"/>.</summary>
        /// <param name="cell">
        ///     The cell to modify. This is a 0-based index and must be less than <see cref="Puzzle.GridSize"/>.</param>
        /// <param name="isImpossible">
        ///     A delegate that returns <c>true</c> for each value that is no longer possible.</param>
        public abstract void MarkImpossible(int cell, Func<int, bool> isImpossible);

        /// <summary>
        ///     Marks a specific <paramref name="cell"/> as having to contain a specific <paramref name="value"/>.</summary>
        /// <param name="cell">
        ///     The cell to modify. This is a 0-based index and must be less than <see cref="Puzzle.GridSize"/>.</param>
        /// <param name="value">
        ///     The value that the cell must have. This must be between <see cref="MinValue"/> and <see cref="MaxValue"/>
        ///     (inclusive).</param>
        public abstract void MustBe(int cell, int value);

        /// <summary>
        ///     Determines if a specific <paramref name="value"/> in a specific <paramref name="cell"/> has already been
        ///     marked impossible or is out of range.</summary>
        public abstract bool IsImpossible(int cell, int value);

        /// <summary>
        ///     Determines if all values that are still possible in the specified <paramref name="cell"/> have the same value
        ///     when projected through the specified <paramref name="predicate"/>. For example, this can be used to determine
        ///     if all the values still possible are the same parity, or all primes/non-primes, etc.</summary>
        /// <param name="cell">
        ///     The cell to examine</param>
        /// <param name="predicate">
        ///     The projection function to run each value through.</param>
        /// <param name="result">
        ///     Receives the projected value that all still possible values satisfy.</param>
        public abstract bool AllSame<T>(int cell, Func<int, T> predicate, out T result) where T : IEquatable<T>;

        /// <summary>Returns the smallest value that is still possible within the specified <paramref name="cell"/>.</summary>
        public int MinPossible(int cell)
        {
            if (this[cell] is int already)
                return already;
            for (var value = MinValue; value <= MaxValue; value++)
                if (!IsImpossible(cell, value))
                    return value;
            return MinValue;
        }

        /// <summary>Returns the largest value that is still possible within the specified <paramref name="cell"/>.</summary>
        public int MaxPossible(int cell)
        {
            if (this[cell] is int already)
                return already;
            for (var value = MaxValue; value >= MinValue; value--)
                if (!IsImpossible(cell, value))
                    return value;
            return MinValue;
        }

        /// <summary>
        ///     If not <c>null</c>, <see cref="Constraint.MarkTakens(SolverState)"/> was called immediately after placing a
        ///     value in the cell given by this index. (This value may be tentative; if the algorithm finds it to be
        ///     impossible, it will backtrack.) The implementation may examine deductions from just that one value. If
        ///     <c>null</c>, the method may have been called (a) at the very start of the algorithm before placing any values;
        ///     (b) after this constraint was returned from another constraint’s <see
        ///     cref="Constraint.MarkTakens(SolverState)"/> call to replace it; or (c) if the constraint has <see
        ///     cref="Constraint.CanReevaluate"/> and one of its affected cells has changed. In these cases, implementations
        ///     should examine the whole set of affected cells for possible deductions.</summary>
        public abstract int? LastPlacedCell { get; }

        /// <summary>
        ///     Returns the value stored in the cell given by <see cref="LastPlacedCell"/> (see there for details). Throws an
        ///     exception if <see cref="LastPlacedCell"/> is <c>null</c>.</summary>
        public int LastPlacedValue => this[LastPlacedCell.Value].Value;

        /// <summary>The minimum value that squares can have in this puzzle. For standard Sudoku, this is 1.</summary>
        public abstract int MinValue { get; }

        /// <summary>The maximum value that squares can have in this puzzle. For standard Sudoku, this is 9.</summary>
        public abstract int MaxValue { get; }

        /// <summary>Returns the size of the puzzle being solved.</summary>
        public abstract int GridSize { get; }
    }
}

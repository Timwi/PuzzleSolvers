namespace PuzzleSolvers
{
    /// <summary>
    ///     Exposes information about the puzzle solver state while invoking methods on an implementation of <see
    ///     cref="IProgressVisualizer"/>.</summary>
    public interface IProgressVisualizerData
    {
        /// <summary>Returns the size of the puzzle (number of cells).</summary>
        public int GridSize { get; }
        /// <summary>Minimum value for the cells in the puzzle.</summary>
        public int MinValue { get; }
        /// <summary>Maximum value for the cells in the puzzle.</summary>
        public int MaxValue { get; }
        /// <summary>Current recursion depth.</summary>
        public int Depth { get; }
        /// <summary>
        ///     Returns the value of a particular cell in the puzzle, or <c>null</c> if that cell is not currently under
        ///     examination.</summary>
        /// <param name="cell">
        ///     The index of a cell in the puzzle.</param>
        public int? GetValue(int cell);
        /// <summary>
        ///     Determines whether a specific value has been ruled out for a specific cell within the current recursion
        ///     context.</summary>
        /// <param name="cell">
        ///     The index of a cell in the puzzle.</param>
        /// <param name="value">
        ///     The value.</param>
        public bool IsTaken(int cell, int value);
        /// <summary>
        ///     This method is only called if <see cref="SolverInstructions.IntendedSolution"/> was specified and the solver
        ///     encountered a constraint that ruled out the intended solution. Determines whether a specific value was already
        ///     ruled out for a specific cell within the current recursion context before the problematic constraint was
        ///     processed.</summary>
        /// <param name="cell">
        ///     The index of a cell in the puzzle.</param>
        /// <param name="value">
        ///     The value.</param>
        public bool WasTaken(int cell, int value);
    }
}

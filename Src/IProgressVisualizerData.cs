using System.Collections.Generic;

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
        ///     The current cell under examination.</summary>
        /// <remarks>
        ///     During <see cref="IProgressVisualizer.VisualizeIntendedSolutionBug(IProgressVisualizerData)"/>, this may be
        ///     <c>null</c>. During <see cref="IProgressVisualizer.VisualizeProgress(IProgressVisualizerData)"/>, this is
        ///     guaranteed non-<c>null</c>.</remarks>
        public int? CurrentCell { get; }

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
        public bool IsImpossible(int cell, int value);
        /// <summary>
        ///     This method is only called if <see cref="SolverInstructions.IntendedSolution"/> was specified and the solver
        ///     encountered a constraint that ruled out the intended solution. Determines whether a specific value was already
        ///     ruled out for a specific cell within the current recursion context before the problematic constraint was
        ///     processed.</summary>
        /// <param name="cell">
        ///     The index of a cell in the puzzle.</param>
        /// <param name="value">
        ///     The value.</param>
        public bool WasImpossible(int cell, int value);

        /// <summary>
        ///     Returns the set of candidate values currently under consideration, in the order in which they are considered.</summary>
        public IEnumerable<int> Candidates { get; }
        /// <summary>
        ///     During calls to <see cref="IProgressVisualizer.VisualizeProgress(IProgressVisualizerData)"/>, when examining a
        ///     new cell, this is <c>null</c>. When examining a different value in the same cell, or during calls to <see
        ///     cref="IProgressVisualizer.EraseProgress(IProgressVisualizerData)"/>, contains the object previously returned
        ///     by <see cref="IProgressVisualizer.VisualizeProgress(IProgressVisualizerData)"/>.</summary>
        public object ProgressVisualizationObject { get; }
    }
}

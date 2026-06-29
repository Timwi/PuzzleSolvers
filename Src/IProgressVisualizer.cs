namespace PuzzleSolvers
{
    /// <summary>
    ///     Provides a way to visualize progress during the puzzle solving process, e.g. by showing which cells are under
    ///     consideration, which possibilities are still to be examined, etc.</summary>
    public interface IProgressVisualizer
    {
        /// <summary>
        ///     Determines whether the visualizer should be invoked at the specified recursion depth.</summary>
        /// <param name="recursionDepth">
        ///     The depth of recursion the solver is currently at.</param>
        bool IsActive(int recursionDepth);
        /// <summary>
        ///     This method is only called if <see cref="SolverInstructions.IntendedSolution"/> was specified and the solver
        ///     encountered a constraint that ruled out the intended solution. Visualizes the state of the solver at this
        ///     point in order to show the information necessary to debug the constraint.</summary>
        /// <param name="data">
        ///     An <see cref="IProgressVisualizerData"/> object containing information about the puzzle and solver state.</param>
        void VisualizeIntendedSolutionBug(IProgressVisualizerData data);
        /// <summary>
        ///     Visualizes the progress during the puzzle solving process.</summary>
        /// <param name="data">
        ///     An <see cref="IProgressVisualizerData"/> object containing information about the puzzle and solver state.</param>
        /// <returns>
        ///     An arbitrary object. During the next invocation of <see cref="VisualizeProgress(IProgressVisualizerData)"/>
        ///     for the same cell, this object can be retrieved from <see
        ///     cref="IProgressVisualizerData.ProgressVisualizationObject"/>.</returns>
        object VisualizeProgress(IProgressVisualizerData data);
        /// <summary>
        ///     Removes the visualization of the progress for the current cell when the solver has exhausted all possibilities
        ///     for that cell and prepares to backtrack.</summary>
        /// <param name="data">
        ///     An <see cref="IProgressVisualizerData"/> object containing information about the puzzle and solver state.</param>
        void EraseProgress(IProgressVisualizerData data);
    }
}

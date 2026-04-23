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
        /// <param name="curCell">
        ///     The current cell under examination at which the bug occurred.</param>
        void VisualizeIntendedSolutionBug(IProgressVisualizerData data, int curCell);
        /// <summary>
        ///     Visualizes the progress during the puzzle solving process.</summary>
        /// <param name="data">
        ///     An <see cref="IProgressVisualizerData"/> object containing information about the puzzle and solver state.</param>
        /// <param name="curCell">
        ///     The current cell under examination.</param>
        /// <param name="curValue">
        ///     The current value being placed into the cell.</param>
        /// <param name="startAt">
        ///     This is <c>0</c> (zero) unless <see cref="SolverInstructions.Randomizer"/> is specified, in which case this
        ///     indicates at which value the solver starts to examine possible values for the cell.</param>
        /// <param name="prev">
        ///     When examining a new cell, this is <c>null</c>. When examining a different value in the same cell, the object
        ///     previously returned by <see cref="VisualizeProgress(IProgressVisualizerData, int, int, int, object)"/> is
        ///     passed in.</param>
        /// <returns>
        ///     An object to be passed to the next invocation of <see cref="VisualizeProgress(IProgressVisualizerData, int,
        ///     int, int, object)"/> or <see cref="EraseProgress(IProgressVisualizerData, int, object)"/> for the same cell.</returns>
        object VisualizeProgress(IProgressVisualizerData data, int curCell, int curValue, int startAt, object prev);
        /// <summary>
        ///     Removes the visualization of the progress for the current cell when the solver has exhausted all possibilities
        ///     for that cell and prepares to backtrack.</summary>
        /// <param name="data">
        ///     An <see cref="IProgressVisualizerData"/> object containing information about the puzzle and solver state.</param>
        /// <param name="curCell">
        ///     The cell no longer under examination.</param>
        /// <param name="prev">
        ///     The object last returned by <see cref="VisualizeProgress(IProgressVisualizerData, int, int, int, object)"/>
        ///     for this cell.</param>
        void EraseProgress(IProgressVisualizerData data, int curCell, object prev);
    }
}

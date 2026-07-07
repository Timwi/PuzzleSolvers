namespace PuzzleSolvers
{
    /// <summary>Describes a strategy for deciding in which order to consider cells in a puzzle for trial and error.</summary>
    public enum CellOrderStrategy
    {
        /// <summary>
        ///     The next cell is one with the fewest number of possible values. Ties are broken using <see
        ///     cref="SolverInstructions.CellPriority"/>, or puzzle order if not specified.</summary>
        Default,

        /// <summary>
        ///     The next cell is given by <see cref="SolverInstructions.CellPriority"/>, or the puzzle order if not specified,
        ///     regardless of how many possible values it has.</summary>
        GivenOrder
    }
}

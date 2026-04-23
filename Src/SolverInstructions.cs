using System;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Use this to instruct the puzzle solver (<see cref="Puzzle.Solve(SolverInstructions)"/>) to perform certain types
    ///     of analysis for the debugging of constraint implementations.</summary>
    public class SolverInstructions
    {
        /// <summary>
        ///     Uses a specific RNG to randomize the solver. When “solving” a puzzle with multiple solutions, this allows the
        ///     solver to pick one at random.</summary>
        public Random Randomizer;
        /// <summary>
        ///     Specifies a tentative priority list of cells to consider first during solve. This is not strictly adhered to
        ///     and only provides a small guidance for some puzzles.</summary>
        public int[] CellPriority;
        /// <summary>
        ///     Specifies a cell value to prioritize when filling the grid. This has no effect if <see cref="Randomizer"/> is
        ///     specified.</summary>
        public int? ValuePriority;

        // ** The following are all for debugging only ** //

        /// <summary>
        ///     Displays progress during the puzzle solve process. <see cref="PuzzleSolvers.ProgressVisualizer"/> is provided
        ///     as a default implementation.</summary>
        public IProgressVisualizer ProgressVisualizer;
        /// <summary>
        ///     When this is not <c>null</c>, the solver determines at which point during the solve process one of the
        ///     constraints eliminates the intended solution. This is for debugging constraints that rule out solutions when
        ///     they shouldn’t.</summary>
        public int[] IntendedSolution;
        /// <summary>
        ///     When this and <see cref="IntendedSolution"/> are not <c>null</c>, limits the set of constraints to examine.
        ///     This may be necessary for speed reasons.</summary>
        public Func<Constraint, bool> ExamineConstraint;
        /// <summary>
        ///     If not <c>null</c>, the solver outputs extremely verbose information to this file describing the solving
        ///     process.</summary>
        public string BulkLoggingFile;
        /// <summary>
        ///     If not <c>null</c>, the solver will obtain a lock on this object while outputting information to the (<see
        ///     cref="BulkLoggingFile"/>).</summary>
        public object LockObject;
    }
}

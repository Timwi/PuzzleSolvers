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

        /// <summary>Shows the first n cells of the solver’s recursive process on the console.</summary>
        public int? ShowContinuousProgress;
        /// <summary>Leaves a number of rows at the top of the console window above the debug display.</summary>
        public int? ShowContinuousProgressConsoleTop;
        /// <summary>Leaves a number of columns on the left of the console window beside the debug display.</summary>
        public int? ShowContinuousProgressConsoleLeft;
        /// <summary>
        ///     Only show candidate values for each cell; useful when cells can have many possible values but the majority of
        ///     them are not applicable most of the time.</summary>
        public bool ShowContinuousProgressShortened;
        /// <summary>
        ///     When this is not <c>null</c>, the solver determines at which point during the solve process one of these
        ///     constraints eliminates the intended solution. This is for debugging constraints that rule out solutions when
        ///     they shouldn’t.</summary>
        public int[] IntendedSolution;
        /// <summary>
        ///     When this and <see cref="IntendedSolution"/> are not <c>null</c>, limits the set of constraints to examine.
        ///     This may be necessary for speed reasons.</summary>
        public Func<Constraint, bool> ExamineConstraint;
        /// <summary>
        ///     Only applies when <see cref="IntendedSolution"/> is not <c>null</c>. Specifies how the debug output should
        ///     identify the cells in a puzzle (locations where digits are entered). When not specified, the cells are
        ///     numbered from 0.</summary>
        public Func<int, string> GetCellName;
        /// <summary>
        ///     Only applies when <see cref="IntendedSolution"/> is not <c>null</c>. Specifies how the debug output should
        ///     identify the values in a puzzle (the digits that are being entered into the cells). When not specified, the
        ///     values are numbered from <see cref="Puzzle.MinValue"/>.</summary>
        public Func<int, string> GetValueName;
        /// <summary>
        ///     If not <c>null</c>, the solver outputs extremely verbose information to this file describing the solving
        ///     process.</summary>
        public string BulkLoggingFile;
        /// <summary>
        ///     If not <c>null</c>, the solver will obtain a lock on this object while outputting information to the console
        ///     (<see cref="ShowContinuousProgress"/>) or a file (<see cref="BulkLoggingFile"/>).</summary>
        public object LockObject;
    }
}

using System;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Signals to the puzzle solver algorithm that a constraint has been violated. USE THIS VERY SPARINGLY. Prefer to use
    ///     <see cref="SolverState.MarkImpossible(int, int)"/> to rule out possibilities before the algorithm fills them in.</summary>
    public sealed class ConstraintViolatedException : Exception
    {
    }
}

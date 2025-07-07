namespace PuzzleSolvers;

/// <summary>
///     Describes a constraint that has no effect on a puzzle, but can be used for coloring the solution.</summary>
/// <param name="region">
///     A region of cells used for coloring.</param>
/// <remarks>
///     <para>
///         The following example demonstrates how to use this constraint to color a region in a puzzle:</para>
///     <code>
///         puzzle.AddConstraint(new AlwaysTrueConstraint(region), ConsoleColor.White, ConsoleColor.DarkBlue);</code>
///     <para>
///         Once the puzzle is solved using <see cref="Puzzle.Solve(SolverInstructions)"/>, the solution can be visualized as
///         follows:</para>
///     <code>
///         ConsoleUtil.WriteLine(puzzle.SolutionToConsole(solution, width: width));</code>
///     <para>
///         The resulting output will have the cells within the region specified by <c>region</c> colored with a dark-blue
///         background color.</para></remarks>
public class AlwaysTrueConstraint(int[] region) : Constraint(region)
{
    /// <summary>A region of cells used for coloring.</summary>
    public int[] Region { get; private set; } = region;
    /// <inheritdoc/>
    public override ConstraintResult Process(SolverState state) => ConstraintResult.Remove;
}

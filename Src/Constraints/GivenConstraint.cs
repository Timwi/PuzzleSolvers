namespace PuzzleSolvers
{
    /// <summary>Describes a given in a puzzle (a value already pre-filled at the start).</summary>
    public class GivenConstraint(int cell, int value) : Constraint([cell])
    {
        /// <summary>The index of the cell that contains this given.</summary>
        public int Cell { get; private set; } = cell;
        /// <summary>The value of the given.</summary>
        public int Value { get; private set; } = value;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            state.MustBe(Cell, Value);
            return ConstraintResult.Remove;
        }

        /// <inheritdoc/>
        public override string ToString() => $"Given: {Cell} is {Value}";
    }
}

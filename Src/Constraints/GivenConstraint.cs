namespace PuzzleSolvers
{
    /// <summary>Describes a given in a puzzle (a value already pre-filled at the start).</summary>
    public class GivenConstraint : Constraint
    {
        /// <summary>The index of the cell that contains this given.</summary>
        public int Cell { get; private set; }
        /// <summary>The value of the given.</summary>
        public int Value { get; private set; }

        /// <summary>Constructor.</summary>
        public GivenConstraint(int cell, int value) : base(new[] { cell })
        {
            Cell = cell;
            Value = value;
        }

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

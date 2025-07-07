using System;

namespace PuzzleSolvers;

/// <summary>Constrains a single cell to values that satisfy a lambda expression.</summary>
public class OneCellLambdaConstraint : Constraint
{
    /// <summary>A function that determines whether a value is valid in the relevant cell.</summary>
    public Func<int, bool> IsValid { get; private set; }

    /// <summary>Constructor.</summary>
    public OneCellLambdaConstraint(int affectedCell, Func<int, bool> isValid) : base(new[] { affectedCell })
    {
        IsValid = isValid;
    }

    /// <inheritdoc/>
    public override ConstraintResult Process(SolverState state)
    {
        state.MarkImpossible(AffectedCells[0], value => !IsValid(value));
        return ConstraintResult.Remove;
    }
}

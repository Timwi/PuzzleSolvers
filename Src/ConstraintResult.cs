using System;
using System.Collections.Generic;

namespace PuzzleSolvers
{
    /// <summary>Base class for classes describing the result of a call to <see cref="Constraint.Process(SolverState)"/>.</summary>
    public abstract class ConstraintResult
    {
        /// <summary>Converts from a <see cref="Constraint"/> array to a <see cref="ConstraintReplace"/>.</summary>
        public static implicit operator ConstraintResult(Constraint[] seq) => seq == null ? null : new ConstraintReplace(seq);
        /// <summary>Converts from a <see cref="Constraint"/> list to a <see cref="ConstraintReplace"/>.</summary>
        public static implicit operator ConstraintResult(List<Constraint> seq) => seq == null ? null : new ConstraintReplace(seq);
        /// <summary>Converts from a <see cref="Constraint"/> to a <see cref="ConstraintReplace"/>.</summary>
        public static implicit operator ConstraintResult(Constraint seq) => seq == null ? null : new ConstraintReplace(seq);

        /// <summary>Easy access to a <see cref="ConstraintReplace"/> that removes a constraint.</summary>
        public static readonly ConstraintResult Remove = new ConstraintReplace(new Constraint[0]);
        /// <summary>Easy access to a <see cref="ConstraintViolation"/>.</summary>
        public static readonly ConstraintResult Violation = new ConstraintViolation();
    }

    /// <summary>
    ///     Return this from a call to <see cref="Constraint.Process(SolverState)"/> to indicate that this constraint should
    ///     be replaced with a given set of other constraints (which can be empty to remove this constraint).</summary>
    public class ConstraintReplace : ConstraintResult
    {
        /// <summary>The new constraints to replace the called constraint with.</summary>
        public IEnumerable<Constraint> NewConstraints { get; private set; }

        /// <summary>Constructor.</summary>
        public ConstraintReplace(IEnumerable<Constraint> newConstraints)
        {
            NewConstraints = newConstraints ?? throw new ArgumentNullException(nameof(newConstraints));
        }

        /// <summary>Constructor.</summary>
        public ConstraintReplace(params Constraint[] newConstraints)
        {
            NewConstraints = newConstraints ?? throw new ArgumentNullException(nameof(newConstraints));
        }
    }

    /// <summary>
    ///     Return this from a call to <see cref="Constraint.Process(SolverState)"/> to indicate that the constraint is
    ///     already violated.</summary>
    /// <remarks>
    ///     It is advisable to use this sparingly. Ideally, <see cref="Constraint.Process(SolverState)"/> should rule out
    ///     possibilities before the algorithm places them. Only use this in cases where the violation is a subtle result of
    ///     interaction between multiple constraints.</remarks>
    public class ConstraintViolation : ConstraintResult { }
}

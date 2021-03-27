using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Describes a constraint in which a group of cells can be only all evens or all odds.</summary>
    public sealed class OddEvenConstraint : Constraint
    {
        /// <summary>Specifies the specific flavor of this constraint.</summary>
        public OddEvenType Type { get; private set; }

        /// <summary>Constructor.</summary>
        public OddEvenConstraint(OddEvenType type, IEnumerable<int> affectedCells) : base(affectedCells)
        {
            Type = type;
        }

        /// <summary>Override; see base.</summary>
        public override ConstraintResult Process(SolverState state)
        {
            int req;

            // Do we know the parity yet?
            switch (Type)
            {
                case OddEvenType.AllEven:
                    req = 0;
                    break;

                case OddEvenType.AllOdd:
                    req = 1;
                    break;

                case OddEvenType.AllSame:
                    // Check if any of the affected cells knows its parity
                    for (var ix = 0; ix < AffectedCells.Length; ix++)
                    {
                        var cell = AffectedCells[ix];
                        if (state[cell] != null)
                        {
                            req = state[cell].Value % 2;
                            goto found;
                        }

                        int? parity = null;
                        for (var value = state.MinValue; value <= state.MaxValue; value++)
                            if (!state.IsImpossible(cell, value))
                            {
                                if (parity == null)
                                    parity = value % 2;
                                else if (parity.Value != value % 2)
                                    goto busted;
                            }
                        if (parity != null)
                        {
                            req = parity.Value;
                            goto found;
                        }

                        busted:;
                    }
                    return null;

                default:
                    throw new InvalidOperationException(string.Format(@"OddEvenConstraint.Type has unknown value: {0}", Type));
            }

            found:
            // Mark all the cells of the wrong parity as taken. After this, we don’t need the constraint anymore.
            foreach (var cell in AffectedCells)
                state.MarkImpossible(cell, value => value % 2 != req);
            return ConstraintResult.Remove;
        }
    }

    /// <summary>Describes the specific flavor of an <see cref="OddEvenConstraint"/>.</summary>
    public enum OddEvenType
    {
        /// <summary>All of the affected cells must be even.</summary>
        AllEven,
        /// <summary>All of the affected cells must be odd.</summary>
        AllOdd,
        /// <summary>All of the affected cells must have the same parity, but it is not initially specified which.</summary>
        AllSame
    }
}

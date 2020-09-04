using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>Describes a given in a puzzle (a value already pre-filled at the start).</summary>
    public sealed class GivenConstraint : Constraint
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

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            for (var i = 0; i < takens[Cell].Length; i++)
                if (i + minValue != Value)
                    takens[Cell][i] = true;
            return Enumerable.Empty<Constraint>();
        }
    }
}

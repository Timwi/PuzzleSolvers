using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>Describes a given in a puzzle (a value already pre-filled at the start).</summary>
    public sealed class GivenConstraint : Constraint
    {
        /// <summary>The index of the cell that contains this given.</summary>
        public int Location { get; private set; }
        /// <summary>The value of the given.</summary>
        public int Value { get; private set; }

        /// <summary>Constructor.</summary>
        public GivenConstraint(int location, int value, ConsoleColor? color = null, ConsoleColor? backgroundColor = null) : base(new[] { location }, color, backgroundColor) { Location = location; Value = value; }

        /// <summary>Override; see base.</summary>
        public override IEnumerable<Constraint> MarkTakens(bool[][] takens, int?[] grid, int? ix, int minValue, int maxValue)
        {
            for (var i = 0; i < takens[Location].Length; i++)
                if (i + minValue != Value)
                    takens[Location][i] = true;
            return Enumerable.Empty<Constraint>();
        }
    }
}

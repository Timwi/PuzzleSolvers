using System;
using System.Collections.Generic;
using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>Describes a Binairo (Binary Puzzle, Tohu-wa-Vohu) puzzle.</summary>
    public class Binairo : Puzzle
    {
        /// <summary>
        ///     Constructor.</summary>
        /// <param name="sideLength">
        ///     The length of one side of the (square) grid.</param>
        public Binairo(int sideLength) : base(sideLength * sideLength, 0, 1)
        {
            AddConstraints(GetConstraints(sideLength));
        }

        /// <summary>
        /// Returns the constraints for a Binairo puzzle.
        /// </summary>
        /// <param name="sideLength">The length of one side of the square grid. This must be an even number.</param>
        public static IEnumerable<Constraint> GetConstraints(int sideLength)
        {
            if (sideLength % 2 != 0)
                throw new ArgumentException("Side length for Binairo constraints must be even.", nameof(sideLength));
            for (var i = 0; i < sideLength; i++)
            {
                yield return new ParityNoTripletsConstraint(Enumerable.Range(0, sideLength).Select(row => i + sideLength * row));
                yield return new ParityEvennessConstraint(Enumerable.Range(0, sideLength).Select(row => i + sideLength * row));
                yield return new ParityNoTripletsConstraint(Enumerable.Range(0, sideLength).Select(col => col + sideLength * i));
                yield return new ParityEvennessConstraint(Enumerable.Range(0, sideLength).Select(col => col + sideLength * i));
            }
            yield return new ParityUniqueRowsColumnsConstraint(sideLength);
        }
    }
}

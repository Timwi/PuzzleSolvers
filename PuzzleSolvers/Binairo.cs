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
            for (var i = 0; i < sideLength; i++)
            {
                Constraints.Add(new ParityNoTripletsConstraint(Enumerable.Range(0, sideLength).Select(row => i + sideLength * row)));
                Constraints.Add(new ParityEvennessConstraint(Enumerable.Range(0, sideLength).Select(row => i + sideLength * row)));
                Constraints.Add(new ParityNoTripletsConstraint(Enumerable.Range(0, sideLength).Select(col => col + sideLength * i)));
                Constraints.Add(new ParityEvennessConstraint(Enumerable.Range(0, sideLength).Select(col => col + sideLength * i)));
            }
            Constraints.Add(new ParityUniqueRowsColumnsConstraint(sideLength));
        }
    }
}

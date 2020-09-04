using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Describes a standard 9×9 Sudoku puzzle.</summary>
    public class Sudoku : LatinSquare
    {
        /// <summary>Constructor.</summary>
        public Sudoku(int minValue = 1) : base(9, minValue)
        {
            for (var r = 0; r < 9; r++)
                Constraints.Add(new UniquenessConstraint(Enumerable.Range(0, 9).Select(i => i % 3 + 3 * (r % 3) + 9 * (i / 3 + 3 * (r / 3)))));
        }
    }
}

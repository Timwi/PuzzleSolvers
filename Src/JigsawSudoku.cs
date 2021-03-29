using System;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Describes a Sudoku puzzle with irregular regions.</summary>
    public class JigsawSudoku : LatinSquare
    {
        /// <summary>
        ///     Constructor.</summary>
        /// <param name="regions">
        ///     Describes the irregular regions of the Sudoku. This must completely cover the grid and have no overlaps.</param>
        public JigsawSudoku(params int[][] regions) : this(regions, 9, 1) { }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="regions">
        ///     Describes the irregular regions of the Sudoku. This must completely cover the grid and have no overlaps.</param>
        /// <param name="sideLength">
        ///     Width and height of the Sudoku grid.</param>
        /// <param name="minValue">
        ///     Minimum value in the grid.</param>
        public JigsawSudoku(int[][] regions, int sideLength = 9, int minValue = 1) : base(sideLength, minValue)
        {
            if (regions == null)
                throw new ArgumentNullException(nameof(regions));
            if (regions.Contains(null))
                throw new ArgumentException("‘regions’ cannot contain a null value.", nameof(regions));
            if (regions.Length != sideLength || regions.Any(r => r.Length != sideLength))
                throw new ArgumentException("The number of regions in a Jigsaw Sudoku, and the size of each region, must equal the side length of the grid.", nameof(regions));
            for (var r = 0; r < regions.Length; r++)
                AddConstraint(new UniquenessConstraint(regions[r]), null, (ConsoleColor) (r % 6 + 1));
        }

        /// <summary>
        ///     Constructs a Jigsaw Sudoku from a string representation of the regions.</summary>
        /// <param name="stringifiedRegions">
        ///     An array of strings such as <c>"A-C1,A-B2,A3"</c> that describe the regions as coordinates in the same format
        ///     as <see cref="Constraint.TranslateCoordinates(string, int)"/>.</param>
        /// <param name="sideLength">
        ///     Width and height of the Sudoku grid.</param>
        /// <param name="minValue">
        ///     Minimum value in the grid.</param>
        public JigsawSudoku(string[] stringifiedRegions, int sideLength = 9, int minValue = 1) : base(sideLength, minValue)
        {
            if (stringifiedRegions == null)
                throw new ArgumentNullException(nameof(stringifiedRegions));
            if (stringifiedRegions.Length != sideLength)
                throw new ArgumentException("The number of regions in a Jigsaw Sudoku, and the size of each region, must equal the side length of the grid.", nameof(stringifiedRegions));
            if (stringifiedRegions.Contains(null))
                throw new ArgumentException("‘stringifiedRegions’ cannot contain a null value.", nameof(stringifiedRegions));
            var regions = stringifiedRegions.Select(r => Constraint.TranslateCoordinates(r).ToArray()).ToArray();
            if (regions.Any(r => r.Length != sideLength))
                throw new ArgumentException("The number of regions in a Jigsaw Sudoku, and the size of each region, must equal the side length of the grid.", nameof(stringifiedRegions));
            for (var r = 0; r < regions.Length; r++)
                AddConstraint(new UniquenessConstraint(regions[r]), null, (ConsoleColor) (r % 7 + 1));
        }

        /// <summary>
        ///     Constructs a Jigsaw Sudoku from a string representation of the regions.</summary>
        /// <param name="stringifiedRegions">
        ///     A string such as <c>"A-C1,A-B2,A3;D-E1,C-D2,B-C3;F1-5,E2;D-E3-4,C-D5;E5,B-F6;A-C4,A-B5,A6"</c> that describes
        ///     the regions as coordinates in the same format as <see cref="Constraint.TranslateCoordinates(string, int)"/>.
        ///     This example is for a 6×6 grid.</param>
        /// <param name="sideLength">
        ///     Width and height of the Sudoku grid.</param>
        /// <param name="minValue">
        ///     Minimum value in the grid.</param>
        public JigsawSudoku(string stringifiedRegions, int sideLength = 9, int minValue = 1) : base(sideLength, minValue)
        {
            if (stringifiedRegions == null)
                throw new ArgumentNullException(nameof(stringifiedRegions));
            var regionsStr = stringifiedRegions.Split(';');
            if (regionsStr.Length != sideLength)
                throw new ArgumentException("The number of regions in a Jigsaw Sudoku, and the size of each region, must equal the side length of the grid.", nameof(stringifiedRegions));
            var regions = regionsStr.Select(r => Constraint.TranslateCoordinates(r).ToArray()).ToArray();
            if (regions.Any(r => r.Length != sideLength))
                throw new ArgumentException("The number of regions in a Jigsaw Sudoku, and the size of each region, must equal the side length of the grid.", nameof(stringifiedRegions));
            for (var r = 0; r < regions.Length; r++)
                AddConstraint(new UniquenessConstraint(regions[r]), null, (ConsoleColor) (r % 7 + 1));
        }
    }
}

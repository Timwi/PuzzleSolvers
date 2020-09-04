﻿using System.Linq;

namespace PuzzleSolvers
{
    /// <summary>Describes a standard 9×9 Sudoku puzzle.</summary>
    public class LatinSquare : Puzzle
    {
        public LatinSquare(int sideLength, int minValue) : base(sideLength * sideLength, minValue, minValue + sideLength - 1)
        {
            for (var row = 0; row < sideLength; row++)
                Constraints.Add(new UniquenessConstraint(Enumerable.Range(0, sideLength).Select(i => sideLength * row + i)));
            for (var col = 0; col < sideLength; col++)
                Constraints.Add(new UniquenessConstraint(Enumerable.Range(0, sideLength).Select(i => sideLength * i + col)));
        }
    }
}

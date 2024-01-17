using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Represents a Masyu puzzle, also known as Pearl.</summary>
    public class Masyu : Puzzle
    {
        /// <summary>Specifies the width of the Masyu puzzle.</summary>
        public int Width { get; private set; }
        /// <summary>Specifies the height of the Masyu puzzle.</summary>
        public int Height { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="width">
        ///     Width of the whole puzzle grid.</param>
        /// <param name="height">
        ///     Height of the whole puzzle grid.</param>
        public Masyu(int width, int height) : base(width * height, 0, 6)
        {
            Width = width;
            Height = height;
            AddConstraint(new SingleLoopConstraint(width, height));

            // Path can’t run off the edge of the grid
            for (var i = 0; i < width * height; i++)
            {
                if (i / width == 0)
                    AddConstraint(new OneCellLambdaConstraint(i, v => (Path.ToBits[v] & 1) == 0));
                if (i % width == width - 1)
                    AddConstraint(new OneCellLambdaConstraint(i, v => (Path.ToBits[v] & 2) == 0));
                if (i / width == width - 1)
                    AddConstraint(new OneCellLambdaConstraint(i, v => (Path.ToBits[v] & 4) == 0));
                if (i % width == 0)
                    AddConstraint(new OneCellLambdaConstraint(i, v => (Path.ToBits[v] & 8) == 0));
            }
        }

        /// <summary>
        ///     Adds a black pearl constraint to the puzzle.</summary>
        /// <param name="x">
        ///     Specifies the column within the grid containing the new pearl.</param>
        /// <param name="y">
        ///     Specifies the row within the grid containing the new pearl.</param>
        public void AddBlackPearl(int x, int y)
        {
            var combinations = new List<int?[]>();
            if (x >= 2 && y >= 2)
                combinations.Add([0b0101, 0b1010, 0b1001, null, null]);
            if (x <= Width - 3 && y >= 2)
                combinations.Add([0b0101, null, 0b0011, 0b1010, null]);
            if (x >= 2 && y <= Height - 3)
                combinations.Add([null, 0b1010, 0b1100, null, 0b0101]);
            if (x <= Width - 3 && y <= Height - 3)
                combinations.Add([null, null, 0b0110, 0b1010, 0b0101]);
            var ix = x + Width * y;
            var affectedCells = new[] { ix - Width, ix - 1, ix, ix + 1, ix + Width };
            var keepIxs = new[] { y >= 2, x >= 2, true, x <= Width - 3, y <= Height - 3 }.SelectIndexWhere(b => b).ToArray();
            AddConstraint(new CombinationsConstraint(
                affectedCells: keepIxs.Select(ix => affectedCells[ix]).ToArray(),
                combinations: combinations.Where(c => Enumerable.Range(0, c.Length).All(i => c[i] == null || keepIxs.Contains(i))).Select(comb => keepIxs.Select(ix => comb[ix].NullOr(c => Path.ToBits.IndexOf(c))).ToArray()).ToArray()));
        }
        /// <summary>
        ///     Adds a black pearl constraint to the puzzle.</summary>
        /// <param name="cell">
        ///     Specifies the cell within the grid containing the new pearl.</param>
        public void AddBlackPearl(int cell) => AddBlackPearl(cell % Width, cell / Width);

        /// <summary>
        ///     Adds a white pearl constraint to the puzzle.</summary>
        /// <param name="x">
        ///     Specifies the column within the grid containing the new pearl.</param>
        /// <param name="y">
        ///     Specifies the row within the grid containing the new pearl.</param>
        public void AddWhitePearl(int x, int y)
        {
            var combinations = new List<int?[]>();
            if (x >= 1 && x <= Width - 2)
            {
                combinations.Add([null, null, 0b1010, 0b1100, null]);
                combinations.Add([null, null, 0b1010, 0b1001, null]);
                combinations.Add([null, 0b0011, 0b1010, null, null]);
                combinations.Add([null, 0b0110, 0b1010, null, null]);
            }
            if (y >= 1 && y <= Height - 2)
            {
                combinations.Add([null, null, 0b0101, null, 0b1001]);
                combinations.Add([null, null, 0b0101, null, 0b0011]);
                combinations.Add([0b0110, null, 0b0101, null, null]);
                combinations.Add([0b1100, null, 0b0101, null, null]);
            }
            var ix = x + Width * y;
            var affectedCells = new[] { ix - Width, ix - 1, ix, ix + 1, ix + Width };
            var keepIxs = new[] { y >= 1, x >= 1, true, x <= Width - 2, y <= Height - 2 }.SelectIndexWhere(b => b).ToArray();
            AddConstraint(new CombinationsConstraint(
                affectedCells: keepIxs.Select(ix => affectedCells[ix]).ToArray(),
                combinations: combinations.Where(c => Enumerable.Range(0, c.Length).All(i => c[i] == null || keepIxs.Contains(i))).Select(comb => keepIxs.Select(ix => comb[ix].NullOr(c => Path.ToBits.IndexOf(c))).ToArray()).ToArray()));
        }
        /// <summary>
        ///     Adds a white pearl constraint to the puzzle.</summary>
        /// <param name="cell">
        ///     Specifies the cell within the grid containing the new pearl.</param>
        public void AddWhitePearl(int cell) => AddWhitePearl(cell % Width, cell / Width);
    }
}

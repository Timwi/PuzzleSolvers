using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>Represents a Yajilin puzzle.</summary>
    public class Yajilin : Puzzle
    {
        /// <summary>Represents a block (shaded cell) in a Yajilin solution.</summary>
        public const int Block = 7;

        /// <summary>
        ///     Encapsulates information about a single Yajilin clue.</summary>
        /// <param name="x">
        ///     X-coordinate of the cell containing this clue.</param>
        /// <param name="y">
        ///     Y-coordinate of the cell containing this clue.</param>
        /// <param name="number">
        ///     The number in the clue, which identifies the number of <see cref="Block"/> values (shaded cells) in the
        ///     direction of <paramref name="direction"/>.</param>
        /// <param name="direction">
        ///     The direction in which this clue is pointing.</param>
        public struct Clue(int x, int y, int number, Direction direction)
        {
            /// <summary>X-coordinate of the cell containing this clue.</summary>
            public int X { get; private set; } = x;
            /// <summary>Y-coordinate of the cell containing this clue.</summary>
            public int Y { get; private set; } = y;
            /// <summary>
            ///     The number in the clue, which identifies the number of <see cref="Block"/> values (shaded cells) in the
            ///     direction of <see cref="Direction"/>.</summary>
            public int Number { get; private set; } = number;
            /// <summary>The direction in which this clue is pointing.</summary>
            public Direction Direction { get; private set; } = direction;
        }

        /// <summary>Specifies the width of the grid.</summary>
        public int Width { get; private set; }
        /// <summary>Specifies the height of the grid.</summary>
        public int Height { get; private set; }
        /// <summary>Allows read-only access to the clues in this puzzle.</summary>
        public IEnumerable<Clue> Clues => _clues.AsReadOnly();

        private readonly Clue[] _clues;

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="width">
        ///     Specifies the width of the grid.</param>
        /// <param name="height">
        ///     Specifies the height of the grid.</param>
        /// <param name="clues">
        ///     Specifies the Yajilin clues in this puzzle. See <see cref="Clue"/> for details.</param>
        public Yajilin(int width, int height, params Clue[] clues) : this(width, height, (IEnumerable<Clue>) clues) { }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="width">
        ///     Specifies the width of the grid.</param>
        /// <param name="height">
        ///     Specifies the height of the grid.</param>
        /// <param name="clues">
        ///     Specifies the Yajilin clues in this puzzle. See <see cref="Clue"/> for details.</param>
        public Yajilin(int width, int height, IEnumerable<Clue> clues) : base(width * height, 0, 7)
        {
            _clues = clues.ToArray();
            Width = width;
            Height = height;

            AddConstraint(new SingleLoopConstraint(width, height));
            AddConstraint(new NoAdjacentConstraint(width, height, Block));

            var isClue = new bool[width * height];
            foreach (var clue in clues)
            {
                if (clue.X < 0 || clue.X >= width || clue.Y < 0 || clue.Y >= height)
                    throw new InvalidOperationException($"A Yajilin clue cannot be placed outside of the grid.");
                isClue[clue.X + width * clue.Y] = true;
            }

            for (var i = 0; i < width * height; i++)
                AddConstraint(new OneCellLambdaConstraint(i, isClue[i] ? (v => v == 0) : (v => v > 0)), isClue[i] ? ConsoleColor.White : null, isClue[i] ? ConsoleColor.DarkGray : null);

            foreach (var clue in clues)
            {
                var x = clue.X;
                var y = clue.Y;
                var number = clue.Number;
                var direction = clue.Direction;

                bool adj((int x, int y) one, (int x, int y) other) => Math.Abs(one.x - other.x) + Math.Abs(one.y - other.y) == 1;

                List<(int x, int y)> getCells(int startX, int startY, Direction dir)
                {
                    var dx = dir switch { Direction.Right => 1, Direction.Left => -1, _ => 0 };
                    var dy = dir switch { Direction.Down => 1, Direction.Up => -1, _ => 0 };
                    int x = startX + dx, y = startY + dy;
                    var cells = new List<(int x, int y)>();
                    while (x.IsBetween(0, Width - 1) && y.IsBetween(0, Height - 1))
                    {
                        if (!isClue[x + Width * y])
                            cells.Add((x, y));
                        x += dx;
                        y += dy;
                    }
                    return cells;
                }

                var cells = getCells(x, y, direction);
                var dir = (int) direction;

                if (cells.Count == 0)
                {
                    if (number > 0)
                        throw new ArgumentException("You cannot place a non-zero Yajilin clue pointing right at the edge of the grid.");
                    continue;
                }

                IEnumerable<int?[]> getCombinations(int?[] sofar, int remainingBlocks)
                {
                    if (sofar.Length == cells.Count)
                    {
                        yield return sofar;
                        yield break;
                    }

                    // No block
                    if (cells.Count - sofar.Length > 2 * remainingBlocks - 1)
                        foreach (var result in getCombinations(sofar.Append(null), remainingBlocks))
                            yield return result;

                    // Yes block
                    if (remainingBlocks > 0 && (sofar.Length == 0 || !adj(cells[sofar.Length - 1], cells[sofar.Length]) || sofar[sofar.Length - 1] != Block))
                        foreach (var result in getCombinations(sofar.Append(Block), remainingBlocks - 1))
                            yield return result;
                }

                AddConstraint(new CombinationsConstraint(cells.Select(c => c.x + Width * c.y), getCombinations([], number)));
                AddConstraint(new MaximumCountConstraint(cells.Select(c => c.x + Width * c.y), v => v == Block, number));
            }
        }
    }
}

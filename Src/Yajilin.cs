using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    public class Yajilin : Puzzle
    {
        public const int Block = 7;

        public struct Clue(int x, int y, int number, Direction direction)
        {
            public int X { get; private set; } = x;
            public int Y { get; private set; } = y;
            public int Number { get; private set; } = number;
            public Direction Direction { get; private set; } = direction;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public IEnumerable<Clue> Clues => _clues.AsReadOnly();

        private readonly Clue[] _clues;

        public Yajilin(int width, int height, params Clue[] clues) : this(width, height, (IEnumerable<Clue>) clues) { }

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
                        throw new ArgumentException($"You cannot place a non-zero Yajilin clue pointing right at the edge of the grid.");
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

                Console.WriteLine((clue.X + width * clue.Y).AsCoordinate(width));
                AddConstraint(new CombinationsConstraint(cells.Select(c => c.x + Width * c.y), getCombinations([], number)));
                AddConstraint(new MaximumBlockCountConstraint(cells.Select(c => c.x + Width * c.y), number));
            }
        }

        private class MaximumBlockCountConstraint(IEnumerable<int> cells, int number) : Constraint(cells)
        {
            public int Number { get; private set; } = number;

            public override ConstraintResult Process(SolverState state)
            {
                var countAlready = AffectedCells.Count(c => state[c] == Block);
                if (countAlready > Number)
                    return ConstraintResult.Violation;
                if (countAlready == Number)
                {
                    for (var ix = 0; ix < AffectedCells.Length; ix++)
                        state.MarkImpossible(AffectedCells[ix], Block);
                    return ConstraintResult.Remove;
                }
                return null;
            }
        }
    }
}

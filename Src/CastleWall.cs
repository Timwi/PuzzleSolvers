using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    public class CastleWall : Puzzle
    {
        public struct Clue(int x, int y, (int number, Direction direction)? directionalClue, bool? isInside)
        {
            public int X { get; private set; } = x;
            public int Y { get; private set; } = y;
            public (int number, Direction direction)? DirectionalClue { get; private set; } = directionalClue;
            public bool? IsInside { get; private set; } = isInside;

            public Clue(int x, int y) : this(x, y, null, null) { }
            public Clue(int x, int y, int number, Direction direction) : this(x, y, (number, direction), null) { }
            public Clue(int x, int y, bool isInside) : this(x, y, null, isInside) { }
            public Clue(int x, int y, int number, Direction direction, bool isInside) : this(x, y, (number, direction), isInside) { }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public IEnumerable<int> CluedCells => _cluedCells.Select(clue => clue.X + Width * clue.Y);

        private List<Clue> _cluedCells = new List<Clue>();

        public CastleWall(int width, int height) : base(width * height, 0, 6)
        {
            AddConstraints(new SingleLoopConstraint(width, height));
            Width = width;
            Height = height;
        }

        public void AddClue(Clue clue) => AddClues(clue);

        public void AddClues(IEnumerable<Clue> clues) => AddClues(clues.ToArray());

        public void AddClues(params Clue[] clues)
        {
            var newConstraints = new List<(Constraint constraint, ConsoleColor? fore, ConsoleColor? back)>();

            foreach (var clue in clues)
            {
                var x = clue.X;
                var y = clue.Y;
                var number = clue.DirectionalClue?.number;
                var direction = clue.DirectionalClue?.direction;
                var isInside = clue.IsInside;

                bool adj((int x, int y) one, (int x, int y) other) => Math.Abs(one.x - other.x) + Math.Abs(one.y - other.y) == 1;

                (int dir, List<(int x, int y)> cells) getCells(int startX, int startY, Direction dir)
                {
                    var dx = dir switch { Direction.Right => 1, Direction.Left => -1, _ => 0 };
                    var dy = dir switch { Direction.Down => 1, Direction.Up => -1, _ => 0 };
                    int x = startX + dx, y = startY + dy;
                    var cells = new List<(int x, int y)>();
                    while (x.IsBetween(0, Width - 1) && y.IsBetween(0, Height - 1))
                    {
                        if (!_cluedCells.Any(c => c.X == x && c.Y == y) && !clues.Any(c => c.X == x && c.Y == y))
                            cells.Add((x, y));
                        x += dx;
                        y += dy;
                    }
                    return ((int) dir, cells);
                }

                (int dir, List<(int x, int y)> cells) = direction == null
                    ? EnumStrong.GetValues<Direction>().Select(dir => getCells(x, y, dir)).MinElement(tup => tup.cells.Count)
                    : getCells(x, y, direction.Value);

                if (cells.Count == 0 && isInside == true)
                    throw new ArgumentException($"You cannot place a CastleWall clue with ‘IsInside’ = true at the edge of the grid, or next to other clues at the edge of the grid.");

                newConstraints.Add((new GivenConstraint(x + Width * y, 0), isInside == true ? ConsoleColor.Black : ConsoleColor.White, isInside switch { true => ConsoleColor.White, false => ConsoleColor.DarkGray, null => ConsoleColor.DarkGreen }));
                if (cells.Count == 0 || (number == null && isInside == null))
                    continue;

                IEnumerable<int[]> getCombinations(int[] sofar, int nSofar)
                {
                    if (sofar.Length == cells.Count)
                    {
                        bool haveLeft = false, haveRight = false, isOdd = false;
                        var n = 0;
                        if (number != null || isInside != null)
                            for (var i = 0; i < sofar.Length; i++)
                            {
                                n += (Path.ToBits[sofar[i]] & (1 << dir)) != 0 ? 1 : 0;
                                n += (Path.ToBits[sofar[i]] & (1 << (dir ^ 2))) != 0 ? 1 : 0;
                                bool left = (Path.ToBits[sofar[i]] & (1 << ((dir + 3) % 4))) != 0, right = (Path.ToBits[sofar[i]] & (1 << ((dir + 1) % 4))) != 0;
                                if (left && right)
                                    isOdd = !isOdd;
                                else if (left)
                                {
                                    if (haveLeft) { haveLeft = false; }
                                    else if (haveRight) { haveRight = false; isOdd = !isOdd; }
                                    else { haveLeft = true; }
                                }
                                else if (right)
                                {
                                    if (haveRight) { haveRight = false; }
                                    else if (haveLeft) { haveLeft = false; isOdd = !isOdd; }
                                    else { haveRight = true; }
                                }
                            }
                        if ((number == null || n == 2 * number.Value) && (isInside == null || isOdd == isInside.Value))
                            yield return sofar;
                        yield break;
                    }

                    for (var v = 0; v < Path.ToBits.Length; v++)
                    {
                        var bits = Path.ToBits[v];

                        // Paths at the start of a run can’t point “down”
                        if ((sofar.Length == 0 || !adj(cells[sofar.Length - 1], cells[sofar.Length])) && (bits & (1 << (dir ^ 2))) != 0)
                            continue;
                        // Paths within a run must join up
                        if (sofar.Length > 0 && adj(cells[sofar.Length - 1], cells[sofar.Length]) && ((bits & (1 << (dir ^ 2))) != 0) != ((Path.ToBits[sofar[sofar.Length - 1]] & (1 << dir)) != 0))
                            continue;
                        // Paths at the end of a run can’t point “up”
                        if ((sofar.Length == cells.Count - 1 || !adj(cells[sofar.Length], cells[sofar.Length + 1])) && (bits & (1 << dir)) != 0)
                            continue;

                        // Make sure that the required number is still attainable
                        var newN = nSofar
                            + ((bits & (1 << dir)) != 0 ? 1 : 0)
                            + ((bits & (1 << (dir ^ 2))) != 0 ? 1 : 0);
                        if (number != null && (newN > 2 * number || newN + 2 * (cells.Count - sofar.Length) - 1 < 2 * number))
                            continue;

                        foreach (var result in getCombinations(sofar.Append(v), newN))
                            yield return result;
                    }
                }
                newConstraints.Add((new CombinationsConstraint(cells.Select(c => c.x + Width * c.y), getCombinations([], 0)), null, null));
            }

            foreach (var (constraint, fore, back) in newConstraints)
                AddConstraint(constraint, fore, back);
            _cluedCells.AddRange(clues);
        }
    }
}

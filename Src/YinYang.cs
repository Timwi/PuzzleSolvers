using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    public class YinYang : Puzzle
    {
        public YinYang(int width, int height, bool?[] givens) : base(width * height, 0, 1)
        {
            if (givens == null)
                throw new ArgumentNullException(nameof(givens));
            if (givens.Length != width * height)
                throw new ArgumentException($"The length of ‘givens’ must equal width times height ({width * height}).", nameof(givens));

            AddConstraint(new ContiguousAreaConstraint(width, height, [0]));
            AddConstraint(new ContiguousAreaConstraint(width, height, [1]));
            AddConstraint(new No2x2sConstraint(width, height, [0]));
            AddConstraint(new No2x2sConstraint(width, height, [1]));

            AddConstraint(new YinYangEdgeConstraint(width, height));
            AddConstraint(new YinYangCheckerboardConstraint(width, height));
        }

        public YinYang(int width, int height, string description) : this(width, height,
            description == null ? throw new ArgumentNullException(nameof(description)) :
            description.Length != width * height ? throw new ArgumentException($"The length of ‘desccription’ must equal width times height ({width * height}).", nameof(description)) :
            !description.All(".kw".Contains) ? throw new ArgumentException($"The ‘description’ string must only contain periods (.), k’s for Black and w’s for White.", nameof(description)) :
            description.Select(ch => ch == '.' ? null : (ch != 'k').Nullable()).ToArray())
        {
        }

        // Implements the common deduction on the edge of the grid
        private class YinYangEdgeConstraint(int width, int height) : Constraint(null)
        {
            private readonly int[] _cells = Enumerable.Range(0, width - 1)
                .Concat(Enumerable.Range(0, height - 1).Select(y => width - 1 + width * y))
                .Concat(Enumerable.Range(0, width - 1).Select(x => width - 1 - x + width * (height - 1)))
                .Concat(Enumerable.Range(0, height - 1).Select(y => width * (height - 1 - y)))
                .ToArray();

            public override ConstraintResult Process(SolverState state)
            {
                var segments = new List<(int sIx, int sVal, int eIx, int eVal)>();
                var startIx = _cells.IndexOf(c => state[c] != null);
                if (startIx == -1)
                    return null;
                var curIx = startIx;
                var curVal = state[_cells[curIx]].Value;
                var anyBlack = curVal == 0;
                var anyWhite = curVal == 1;
                for (var tIx = 0; tIx < _cells.Length; tIx++)
                {
                    var ix = (tIx + startIx + 1) % _cells.Length;
                    if (state[_cells[ix]] is int val)
                    {
                        if ((ix - curIx + _cells.Length) % _cells.Length > 1 && curVal == val)
                            segments.Add((curIx, curVal, ix, val));
                        curIx = ix;
                        curVal = val;
                        anyBlack |= curVal == 0;
                        anyWhite |= curVal == 1;
                    }
                }
                if (anyWhite && anyBlack)
                    foreach (var (sIx, sVal, eIx, eVal) in segments)
                        for (var i = (sIx + 1) % _cells.Length; i != eIx; i = (i + 1) % _cells.Length)
                            state.MustBe(_cells[i], sVal);
                return null;
            }
        }

        // Implements the common checkerboard deduction
        private class YinYangCheckerboardConstraint(int width, int height) : Constraint(null)
        {
            public override ConstraintResult Process(SolverState state)
            {
                for (var topLeft = 0; topLeft < width * height; topLeft++)
                    if (topLeft % width < width - 1 && topLeft / width < height - 1)
                    {
                        var cells = new[] { topLeft, topLeft + 1, topLeft + width, topLeft + width + 1 };
                        if (state[cells[0]] is int r1 && state[cells[3]] == r1)
                        {
                            if (state[cells[1]] == (r1 ^ 1))
                                state.MustBe(cells[2], r1);
                            else if (state[cells[2]] == (r1 ^ 1))
                                state.MustBe(cells[1], r1);
                        }
                        else if (state[cells[1]] is int r2 && state[cells[2]] == r2)
                        {
                            if (state[cells[0]] == (r2 ^ 1))
                                state.MustBe(cells[3], r2);
                            else if (state[cells[3]] == (r2 ^ 1))
                                state.MustBe(cells[0], r2);
                        }
                    }
                return null;
            }
        }
    }
}

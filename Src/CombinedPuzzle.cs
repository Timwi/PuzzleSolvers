using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;

namespace PuzzleSolvers
{
    public class CombinedPuzzle : Puzzle
    {
        public CombinedPuzzle(IEnumerable<Puzzle> subpuzzles) : this(subpuzzles?.ToArray()) { }
        public CombinedPuzzle(params Puzzle[] subpuzzles) : base(
            (subpuzzles ?? throw new ArgumentNullException(nameof(subpuzzles))).Sum(p => p.GridSize),
            subpuzzles.Min(p => p.MinValue),
            subpuzzles.Max(p => p.MaxValue))
        {
            if (subpuzzles.Length == 0)
                throw new ArgumentException($"‘{nameof(subpuzzles)}’ cannot be an empty array.", nameof(subpuzzles));

            for (int startCell = 0, pzIx = 0; pzIx < subpuzzles.Length; startCell += subpuzzles[pzIx].GridSize, pzIx++)
            {
                var subpuzzle = subpuzzles[pzIx];
                if (subpuzzle.MinValue > MinValue)
                    for (var cell = 0; cell < subpuzzles[pzIx].GridSize; cell++)
                        AddConstraint(new OneCellLambdaConstraint(cell + startCell, v => v >= subpuzzle.MinValue));
                if (subpuzzle.MaxValue < MaxValue)
                    for (var cell = 0; cell < subpuzzles[pzIx].GridSize; cell++)
                        AddConstraint(new OneCellLambdaConstraint(cell + startCell, v => v <= subpuzzle.MaxValue));

                foreach (var constraint in subpuzzle.Constraints)
                    AddConstraint(new TransferConstraint(constraint, startCell, subpuzzle.MinValue, subpuzzle.MaxValue, subpuzzle.GridSize));
            }
        }

        private class TransferConstraint(Constraint baseConstraint, int cellOffset, int minValue, int maxValue, int gridSize)
            : Constraint((baseConstraint ?? throw new ArgumentNullException(nameof(baseConstraint))).AffectedCells.NullOr(a => a.Select(c => c + cellOffset)) ?? Enumerable.Range(cellOffset, gridSize))
        {
            private class SolverStateImpl(SolverState baseState, int cellOffset, int minValue, int maxValue, int gridSize) : SolverState
            {
                public override int? this[int cell] => baseState[cell + cellOffset];
                public override void MarkImpossible(int cell, int value) => baseState.MarkImpossible(cell + cellOffset, value);
                public override void MarkImpossible(int cell, Func<int, bool> isImpossible) => baseState.MarkImpossible(cell + cellOffset, isImpossible);
                public override void MustBe(int cell, int value) => baseState.MustBe(cell + cellOffset, value);
                public override bool IsImpossible(int cell, int value) => baseState.IsImpossible(cell + cellOffset, value);
                public override bool AllSame<T>(int cell, Func<int, T> predicate, out T result) => baseState.AllSame(cell + cellOffset, predicate, out result);
                public override int? LastPlacedCell => baseState.LastPlacedCell is int lpc && lpc >= cellOffset && lpc - cellOffset < gridSize ? (lpc - cellOffset).Nullable() : null;
                public override int MinValue => minValue;
                public override int MaxValue => maxValue;
                public override int GridSize => gridSize;
            }

            public override bool CanReevaluate => baseConstraint.CanReevaluate;
            public override int? NumCombinations => baseConstraint.NumCombinations;

            public override ConstraintResult Process(SolverState state)
            {
                var result = baseConstraint.Process(new SolverStateImpl(state, cellOffset, minValue, maxValue, gridSize));
                return result is ConstraintReplace cr
                    ? new ConstraintReplace(cr.NewConstraints.Select(c => new TransferConstraint(c, cellOffset, minValue, maxValue, gridSize)))
                    : result;
            }
        }
    }
}

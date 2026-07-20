using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Internal class that keeps track of the state of the solver at each iteration of the recursive algorithm. The class
    ///     implements both <see cref="SolverState"/> (so it can be passed to implementations of <see cref="Constraint"/>) and
    ///     <see cref="IProgressVisualizerData"/> (so it can be passed to implementations of <see
    ///     cref="IProgressVisualizer"/>). Besides those, it also contains information internal to the recursive algorithm in
    ///     <see cref="Puzzle.solve(int[], SolverInstructions, SolverStateImplBase)"/>.</summary>
    internal abstract class SolverStateImplBase : SolverState, IProgressVisualizerData
    {
        // Basic information
        internal Puzzle Puzzle;
        internal int?[] Grid;                  // reference to the same array in all recursive iterations (does not get copied)
        internal int? LastPlacedIx;            // during Phase 1: the cell that last had a value placed, or null if we are re-evaluating constraints; during Phase 2: the cell we are recursing over
        internal List<Constraint> Constraints; // the list of constraints can change because constraints can have themselves replaced with new constraints any time during the process
        internal int RecursionDepth;           // not counting levels of recursion in which a cell has only one possible value
        internal bool IsSingularValue;         // remembers whether there was a cell has only one possible value

        // During Phase 1 (evaluating constraints), keeps tracks of which cells changed so that we know which other constraints to re-evaluate.
        // This is modified in MarkCellChanged and read out in the main solve algorithm.
        internal (bool changed, Constraint initiator)[] AvailablesChanged;
        internal Constraint CurrentConstraint;

        internal bool VisualizingProgress;           // remembers the result of ProgressVisualizer.IsActive()
        internal object ProgressVisualizationObject; // remembers the result of ProgressVisualizer.VisualizeProgress(), which needs to be passed into it in the next iteration and into ProgressVisualizer.EraseProgress()

        internal void ClearAvailablesChanged()
        {
            if (AvailablesChanged != null)
                for (var i = 0; i < Puzzle.GridSize; i++)
                    AvailablesChanged[i] = default;
        }

        protected void MarkCellChanged(int cell)
        {
            AvailablesChanged ??= new (bool changed, Constraint initiator)[GridSize];
            if (!AvailablesChanged[cell].changed)
                AvailablesChanged[cell] = (true, CurrentConstraint);
            else if (AvailablesChanged[cell].initiator != CurrentConstraint)
                AvailablesChanged[cell] = (true, null);
        }

        protected abstract void mustBe(int cell, int value);
        public override void MustBe(int cell, int value)
        {
            if (Grid[cell] != null)
            {
                if (Grid[cell].Value != value)
                    throw new InvalidOperationException("A constraint attempted to set a cell to a value that is already set to a different value.");
                return;
            }
            mustBe(cell, value);
        }

        public override bool AllSame<T>(int cell, Func<int, T> selector, out T result)
        {
            if (Grid[cell] != null)
            {
                result = selector(Grid[cell].Value);
                return true;
            }
            T tmpResult = default;
            var found = false;
            for (var value = Puzzle.MinValue; value <= Puzzle.MaxValue; value++)
                if (!IsImpossible(cell, value))
                {
                    var mapped = selector(value);
                    if (!found)
                    {
                        tmpResult = mapped;
                        found = true;
                    }
                    else if (!mapped.Equals(tmpResult))
                    {
                        result = default;
                        return false;
                    }
                }
            result = tmpResult;
            return found;
        }

        public override int? this[int cell] => Grid[cell];
        public override int? LastPlacedCell => LastPlacedIx;
        public override int MinValue => Puzzle.MinValue;
        public override int MaxValue => Puzzle.MaxValue;
        public override int GridSize => Puzzle.GridSize;

        internal abstract bool IntendedSolutionPossible(int[] intendedSolution);
        internal abstract object CopyAvailables();
        internal abstract void SetPrevAvailableIf(bool v);
        internal abstract bool WasIntendedSolutionPossible();
        internal abstract SolverStateImplBase CloneForNextIteration(int curCell);
        internal abstract int AvailableCount(int cell);

        protected List<int> _candidates;
        private int _candidateIx;
        internal abstract void SetCandidates(int curCell, Random randomizer, int? valuePriority);
        internal bool GetNextCandidate(out int val)
        {
            if (_candidateIx >= _candidates.Count)
            {
                val = -1;
                return false;
            }
            else
            {
                val = _candidates[_candidateIx++];
                return true;
            }
        }

        int? IProgressVisualizerData.GetValue(int cell) => Grid[cell];
        protected abstract bool wasImpossible(int cell, int value);
        bool IProgressVisualizerData.WasImpossible(int cell, int value) => wasImpossible(cell, value);

        IEnumerable<int> IProgressVisualizerData.Candidates => _candidates;
        int IProgressVisualizerData.Depth => RecursionDepth;
        int? IProgressVisualizerData.CurrentCell => LastPlacedIx;
        object IProgressVisualizerData.ProgressVisualizationObject => ProgressVisualizationObject;
    }

    /// <summary>
    ///     The vast majority of puzzles will use this implementation. It uses a <c>ulong[]</c> to keep track of available
    ///     values in every cell, making it very efficient to access and modify by bit manipulation. Conversely, it can only
    ///     deal with a range of up to 64 values. Any puzzle that uses more values will use <see
    ///     cref="SolverStateImplInefficient"/> instead.</summary>
    internal sealed class SolverStateImplEfficient : SolverStateImplBase
    {
        internal ulong[] Available;

        public SolverStateImplEfficient(Puzzle puzzle, int?[] grid, ulong[] available, List<Constraint> constraints)
        {
            Puzzle = puzzle;
            Grid = grid;
            Available = available;
            Constraints = constraints;
        }

        public override void MarkImpossible(int cell, int value)
        {
            if (Grid[cell] != null)
                return;
            var bitIx = value - Puzzle.MinValue;
            if (bitIx is < 0 or >= 64)
                return;
            var bit = 1UL << bitIx;
            if ((Available[cell] & bit) != 0)
            {
                Available[cell] &= ~bit;
                MarkCellChanged(cell);
            }
        }

        public override void MarkImpossible(int cell, Func<int, bool> isImpossible)
        {
            if (Grid[cell] != null)
                return;
            var any = false;
            for (var value = Puzzle.MinValue; value <= Puzzle.MaxValue; value++)
            {
                var bit = 1UL << (value - Puzzle.MinValue);
                if ((Available[cell] & bit) != 0 && isImpossible(value))
                {
                    Available[cell] &= ~bit;
                    any = true;
                }
            }
            if (any)
                MarkCellChanged(cell);
        }

        protected override void mustBe(int cell, int value)
        {
            var bit = 1UL << (value - Puzzle.MinValue);
            if (Available[cell] != 0 && Available[cell] != bit)
            {
                Available[cell] &= bit;
                MarkCellChanged(cell);
            }
        }

        public override bool IsImpossible(int cell, int value) => value < Puzzle.MinValue || value > Puzzle.MaxValue || (Grid[cell] != null ? Grid[cell].Value != value : (Available[cell] & (1UL << (value - Puzzle.MinValue))) == 0);

        internal override bool IntendedSolutionPossible(int[] intendedSolution) =>
            intendedSolution.All((v, cell) => Grid[cell] == v || (Grid[cell] == null && (Available[cell] & (1UL << (v - Puzzle.MinValue))) != 0));

        internal override object CopyAvailables() => Available.ToArray();

        private ulong[] _prevAvailable;
        internal override void SetPrevAvailableIf(bool v) => _prevAvailable = v ? Available.ToArray() : null;
        internal override bool WasIntendedSolutionPossible() => _prevAvailable != null;
        protected override bool wasImpossible(int cell, int value) => (_prevAvailable[cell] & (1UL << (value - Puzzle.MinValue))) == 0;

        internal override SolverStateImplBase CloneForNextIteration(int curCell) => new SolverStateImplEfficient(Puzzle, Grid, Available.ToArray(), Constraints);

        internal override int AvailableCount(int cell)
        {
            var v = Available[cell];
            v -= (v >> 1) & 0x5555555555555555UL;
            v = (v & 0x3333333333333333UL) + ((v >> 2) & 0x3333333333333333UL);
            v = (v + (v >> 4)) & 0x0f0f0f0f0f0f0f0fUL;
            return (int) ((v * 0x0101010101010101UL) >> 56);
        }

        internal override void SetCandidates(int curCell, Random randomizer, int? valuePriority)
        {
            _candidates = [];
            var av = Available[LastPlacedIx.Value];
            var num = Puzzle.MaxValue - Puzzle.MinValue + 1;
            for (var j = 0; j < num; j++)
            {
                var i = valuePriority is { } vp ? (j + vp) % num : j;
                if ((av & (1UL << i)) != 0)
                    _candidates.Add(i + Puzzle.MinValue);
            }
            if (randomizer != null)
                _candidates.Shuffle(randomizer);
        }
    }

    /// <summary>
    ///     In contrast to <see cref="SolverStateImplEfficient"/>, uses a <c>List&lt;int&gt;[]</c> to keep track of available
    ///     values in every cell. Querying, updating and copying these lists is less efficient than bit manipulation. This
    ///     implementation is only used for puzzles where the range of possible values per cell spans more than 64 possible
    ///     values.</summary>
    internal sealed class SolverStateImplInefficient : SolverStateImplBase
    {
        internal List<int>[] Available;

        public SolverStateImplInefficient(Puzzle puzzle, int?[] grid, List<int>[] available, List<Constraint> constraints)
        {
            Puzzle = puzzle;
            Grid = grid;
            Available = available;
            Constraints = constraints;
        }

        private void MarkImpossibleImpl(int cell, int valueIx)
        {
            var list = Available[cell];
            if (valueIx < list.Count - 1)
                list[valueIx] = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            MarkCellChanged(cell);
        }

        public override void MarkImpossible(int cell, int value)
        {
            if (Grid[cell] != null)
                return;
            if (Available[cell].IndexOf(value) is { } valueIx and not -1)
                MarkImpossibleImpl(cell, valueIx);
        }

        public override void MarkImpossible(int cell, Func<int, bool> isImpossible)
        {
            if (Grid[cell] != null)
                return;
            var valueIx = 0;
            while (valueIx < Available[cell].Count)
            {
                if (isImpossible(Available[cell][valueIx]))
                    MarkImpossibleImpl(cell, valueIx);
                else
                    valueIx++;
            }
        }

        protected override void mustBe(int cell, int value)
        {
            if (Available[cell].Count != 1 || Available[cell][0] != value)
            {
                var wasThere = Available[cell].Contains(value);
                Available[cell].Clear();
                if (wasThere)
                    Available[cell].Add(value);
                MarkCellChanged(cell);
            }
        }

        public override bool IsImpossible(int cell, int value) => value < Puzzle.MinValue || value > Puzzle.MaxValue || (Grid[cell] != null ? Grid[cell].Value != value : !Available[cell].Contains(value));

        internal override bool IntendedSolutionPossible(int[] intendedSolution) =>
            intendedSolution.All((v, cell) => Grid[cell] == v || (Grid[cell] == null && Available[cell].Contains(v)));

        internal override object CopyAvailables() => Available.Select(b => b.ToList()).ToArray();

        private List<int>[] _prevAvailable;
        internal override void SetPrevAvailableIf(bool v) => _prevAvailable = v ? Available.Select(av => av.ToList()).ToArray() : null;
        internal override bool WasIntendedSolutionPossible() => _prevAvailable != null;
        protected override bool wasImpossible(int cell, int value) => !_prevAvailable[cell].Contains(value);

        internal override SolverStateImplBase CloneForNextIteration(int curCell) =>
            new SolverStateImplInefficient(Puzzle, Grid, Ut.NewArray(GridSize, ix => Grid[ix] == null ? Available[ix].ToList() : null), Constraints);
        internal override int AvailableCount(int cell) => Available[cell].Count;

        internal override void SetCandidates(int curCell, Random randomizer, int? valuePriority)
        {
            _candidates = Available[LastPlacedIx.Value];
            if (randomizer == null && valuePriority is { } _startAt)
                _candidates = Available[LastPlacedIx.Value].Skip(_startAt).Concat(Available[LastPlacedIx.Value].Take(_startAt)).ToList();
            else
            {
                _candidates = _candidates.ToList();
                _candidates.Shuffle(randomizer);
            }
        }
    }
}

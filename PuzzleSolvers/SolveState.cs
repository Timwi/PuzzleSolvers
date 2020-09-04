using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    sealed class SolveState : ISolveState
    {
        public int GridSize { get; private set; }
        public int MinValue { get; private set; }
        public int MaxValue { get; private set; }
        public HashSet<int> _changedCells { get; private set; } = new HashSet<int>();
        internal readonly List<int>[] PossibleValues;
        private List<Constraint2> _constraints;

        // Clone constructor
        private SolveState(int gridSize, int minValue, int maxValue, List<Constraint2> constraints, List<int>[] possibleValues, int changingCell, int val)
        {
            GridSize = gridSize;
            MinValue = minValue;
            MaxValue = maxValue;
            _constraints = constraints.ToList();
            PossibleValues = possibleValues.Select((lst, cell) => cell == changingCell ? new List<int> { val } : lst.ToList()).ToArray();
        }

        public SolveState(int gridSize, int minValue, int maxValue, IEnumerable<Constraint2> constraints)
        {
            GridSize = gridSize;
            MinValue = minValue;
            MaxValue = maxValue;
            _constraints = constraints.ToList();
            PossibleValues = Ut.NewArray(gridSize, _ => new List<int>(Enumerable.Range(minValue, maxValue - minValue + 1)));
        }

        public bool IsPossible(int cell, int value) => PossibleValues[cell].Contains(value);
        public int MinPossible(int cell) => PossibleValues[cell].Min();
        public int MaxPossible(int cell) => PossibleValues[cell].Max();
        public IEnumerable<int> GetPossibilities(int cell) => PossibleValues[cell].ToArray();
        public int? GetOnlyPossibility(int cell) => PossibleValues[cell].Count == 1 ? (int?) PossibleValues[cell][0] : null;
        public bool SetOnly(int cell, int value) => SetImpossible(cell, v => v != value);
        public bool SetImpossible(int cell, int value)
        {
            if (!PossibleValues[cell].Remove(value))
                return false;
            if (PossibleValues[cell].Count == 0)
                throw new ImpossibleStateException();
            _changedCells.Add(cell);
            return true;
        }
        public bool SetImpossible(int cell, Predicate<int> predicate)
        {
            var prevCount = PossibleValues[cell].Count;
            PossibleValues[cell].RemoveAll(predicate);
            if (PossibleValues[cell].Count == prevCount)
                return false;
            if (PossibleValues[cell].Count == 0)
                throw new ImpossibleStateException();
            _changedCells.Add(cell);
            return true;
        }

        public SolveState Clone(int changingCell, int val) => new SolveState(GridSize, MinValue, MaxValue, _constraints, PossibleValues, changingCell, val);

        public void ProcessConstraints(int[] affectedCells)
        {
            tryAgain:
            List<Constraint2> allNewConstraints = null;
            for (var i = 0; i < _constraints.Count; i++)
            {
                if (affectedCells != null && _constraints[i].AffectedCells != null)
                {
                    // Avoiding LINQ here for performance reasons
                    for (var j = 0; j < affectedCells.Length; j++)
                        if (_constraints[i].AffectedCells.Contains(affectedCells[j]))
                            goto mustProcess;
                    continue;
                }

                mustProcess:
                var newConstraints = _constraints[i].EvaluateAndReplace(this);

                if (newConstraints != null)
                {
                    // The set of constraints has changed
                    if (allNewConstraints == null)
                    {
                        allNewConstraints = new List<Constraint2>(_constraints.Count);
                        for (var j = 0; j < i; j++)
                            allNewConstraints.Add(_constraints[j]);
                    }
                    allNewConstraints.AddRange(newConstraints);
                }
                else if (allNewConstraints != null)
                    allNewConstraints.Add(_constraints[i]);
            }

            // Avoiding LINQ here for performance reasons
            foreach (var cell in _changedCells)
                if (PossibleValues[cell].Count == 0)
                    return;

            if (allNewConstraints != null || _changedCells.Count > 0)
            {
                affectedCells = _changedCells.ToArray();
                _changedCells.Clear();
                _constraints = allNewConstraints ?? _constraints;
                goto tryAgain;
            }
        }
    }
}

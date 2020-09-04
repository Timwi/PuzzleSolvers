using System;
using System.Collections.Generic;
using System.Text;

namespace PuzzleSolvers
{
    public interface ISolveState
    {
        int GridSize { get; }
        int MinValue { get; }
        int MaxValue { get; }

        bool IsPossible(int cell, int value);
        int MinPossible(int cell);
        int MaxPossible(int cell);
        IEnumerable<int> GetPossibilities(int cell);
        int? GetOnlyPossibility(int cell);
        bool SetOnly(int cell, int value);
        bool SetImpossible(int cell, int value);
        bool SetImpossible(int cell, Predicate<int> predicate);
    }
}

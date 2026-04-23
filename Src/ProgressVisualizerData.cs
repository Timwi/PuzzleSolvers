namespace PuzzleSolvers
{
    internal class ProgressVisualizerData(int gridSize, int minValue, int maxValue, int depth, int?[] grid, bool[][] takens) : IProgressVisualizerData
    {
        public int GridSize => gridSize;
        public int MinValue => minValue;
        public int MaxValue => maxValue;
        public int Depth => depth;
        public int? GetValue(int cell) => grid[cell];
        public bool IsTaken(int cell, int value) => takens[cell][value - minValue];
        internal bool[][] prevTakens = null;
        public bool WasTaken(int cell, int value) => prevTakens[cell][value - minValue];
    }
}

using System;
using System.Linq;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace PuzzleSolvers
{
    /// <summary>
    ///     Provides a default implementation for <see cref="IProgressVisualizer"/> which displays, on the console, one row of
    ///     information for each cell currently under consideration, up to a specified recursion depth, highlighting for each
    ///     cell the values under consideration, thus giving a somewhat informative idea of the amount of progress made in the
    ///     puzzle.</summary>
    /// <param name="depth">
    ///     The maximum amount of recursion depth to visualize (number of rows output to the console).</param>
    public class ProgressVisualizer(int depth) : IProgressVisualizer
    {
        /// <summary>Implements <see cref="IProgressVisualizer.IsActive(int)"/>.</summary>
        public bool IsActive(int recursionDepth) => recursionDepth < depth;

        /// <summary>
        ///     Specifies how the visualization should identify the cells in a puzzle (locations where digits are entered).
        ///     When not specified, the cells are numbered from 0.</summary>
        public Func<int, string> GetCellName = null;
        /// <summary>
        ///     Specifies how the visualization should identify the values in a puzzle (the digits that are being entered into
        ///     the cells). When not specified, the values are numbered from <see cref="Puzzle.MinValue"/>.</summary>
        public Func<int, string> GetValueName = null;

        /// <summary>Leaves a number of rows at the top of the console window above the debug display.</summary>
        public int ConsoleTop = 0;
        /// <summary>Leaves a number of columns on the left of the console window beside the debug display.</summary>
        public int ConsoleLeft = 0;

        /// <summary>
        ///     If <see cref="LockObject"/> is <c>null</c>, a lock is obtained on this default object while outputting to the
        ///     console.</summary>
        public static readonly object DefaultLockObject = new();

        /// <summary>If not <c>null</c>, obtains a lock on this object while outputting information to the console.</summary>
        public object LockObject = null;

        /// <summary>Implements <see cref="IProgressVisualizer.VisualizeIntendedSolutionBug(IProgressVisualizerData)"/>.</summary>
        void IProgressVisualizer.VisualizeIntendedSolutionBug(IProgressVisualizerData data)
        {
            lock (LockObject ?? DefaultLockObject)
            {
                var numDigits = GetCellName == null ? (data.GridSize - 1).ToString().Length : Enumerable.Range(0, data.GridSize).Max(c => GetCellName(c).Length);
                for (var cell = 0; cell < data.GridSize; cell++)
                {
                    string valueId(int v) => GetValueName != null ? GetValueName(v) : v.ToString();
                    var cellLine = Enumerable.Range(data.MinValue, data.MaxValue - data.MinValue + 1)
                        .Select(v => valueId(v).Color(data.WasImpossible(cell, v) != data.IsImpossible(cell, v) ? ConsoleColor.Red : data.IsImpossible(cell, v) ? ConsoleColor.DarkGray : ConsoleColor.Yellow))
                        .JoinColoredString(" ");
                    var cellStr = (GetCellName != null ? GetCellName(cell) : cell.ToString()).PadLeft(numDigits, ' ') + ". ";
                    ConsoleUtil.WriteLine($"{cellStr.Color(cell == data.CurrentCell ? ConsoleColor.Cyan : ConsoleColor.DarkCyan)}{cellLine}   {(data.GetValue(cell) is { } value ? valueId(value).Color(ConsoleColor.Green) : "?".Color(ConsoleColor.DarkGreen))}", null);
                }
                Console.WriteLine();
            }
        }

        /// <summary>Implements <see cref="IProgressVisualizer.VisualizeProgress(IProgressVisualizerData)"/>.</summary>
        object IProgressVisualizer.VisualizeProgress(IProgressVisualizerData data)
        {
            var curValue = data.GetValue(data.CurrentCell.Value);
            var cellName = GetCellName == null ? data.CurrentCell.Value.ToString().PadLeft((data.GridSize - 1).ToString().Length) : GetCellName(data.CurrentCell.Value);
            var numValues = data.MaxValue - data.MinValue + 1;
            var output = new ConsoleColoredString($"Cell {cellName}: " + data.Candidates
                .Select(v => (GetValueName?.Invoke(v) ?? v.ToString()).Color(
                    v == curValue ? ConsoleColor.Yellow : ConsoleColor.DarkCyan,
                    v == curValue ? ConsoleColor.DarkGreen : ConsoleColor.Black)).JoinColoredString(" "));
            lock (LockObject ?? DefaultLockObject)
            {
                Console.CursorLeft = ConsoleLeft;
                Console.CursorTop = ConsoleTop + data.Depth;
                ConsoleUtil.Write(output);
            }
            return Math.Max(data.ProgressVisualizationObject is int prevLineLen ? prevLineLen : 0, output.Length);
        }

        /// <summary>Implements <see cref="IProgressVisualizer.EraseProgress(IProgressVisualizerData)"/>.</summary>
        void IProgressVisualizer.EraseProgress(IProgressVisualizerData data)
        {
            if (data.ProgressVisualizationObject is int lineLen)
                lock (LockObject ?? DefaultLockObject)
                {
                    Console.CursorLeft = ConsoleLeft;
                    Console.CursorTop = ConsoleTop + data.Depth;
                    Console.WriteLine(new string(' ', lineLen));
                }
        }
    }
}

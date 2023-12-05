namespace PuzzleSolvers
{
    /// <summary>
    ///     Contains some constants and values to help interpret cell values in path-genre puzzles (e.g., <see
    ///     cref="Masyu"/>).</summary>
    public static class Path
    {
        /// <summary>
        ///     Maps cell values (0–6) to an integer whose bit pattern represents which edges of the cell are crossed by the
        ///     path (1 = up; 2 = right; 4 = down; 8 = left).</summary>
        public static readonly int[] ToBits = [0, 3, 5, 6, 9, 10, 12];
        /// <summary>Provides a single-character visualization of a cell value.</summary>
        public static readonly string ToChar = "·└│┌┘─┐";
        /// <summary>Provides a two-character visualization of a cell value.</summary>
        public static readonly string[] ToStr = ["· ", "└─", "│ ", "┌─", "┘ ", "──", "┐ "];

        /// <summary>Identifies an empty cell (contains no part of the path).</summary>
        public const int Empty = 0;
        /// <summary>Identifies a piece of the path in the shape of an L.</summary>
        public const int UpRight = 1;
        /// <summary>Identifies a vertical piece of the path.</summary>
        public const int UpDown = 2;
        /// <summary>Identifies a piece of the path in the shape of a Γ.</summary>
        public const int RightDown = 3;
        /// <summary>Identifies a piece of the path in the shape of a mirrored L.</summary>
        public const int UpLeft = 4;
        /// <summary>Identifies a horizontal piece of the path.</summary>
        public const int RightLeft = 5;
        /// <summary>Identifies a piece of the path in the shape of a mirrored Γ.</summary>
        public const int DownLeft = 6;
    }
}

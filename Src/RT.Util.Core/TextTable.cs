using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace RT.Util.Text
{
    /// <summary>Produces a table in a fixed-width character environment.</summary>
    public sealed class TextTable
    {
        /// <summary>Gets or sets the number of characters to space each column apart from the next.</summary>
        public int ColumnSpacing { get; set; }
        /// <summary>Gets or sets the number of characters to space each row apart from the next.</summary>
        public int RowSpacing { get; set; }
        /// <summary>
        ///     Gets or sets the maximum width of the table, including all column spacing. If <see cref="UseFullWidth"/> is
        ///     false, the table may be narrower. If this is null, the table width depends on which method is used to generate
        ///     it.</summary>
        public int? MaxWidth { get; set; }
        /// <summary>
        ///     Gets or sets a value indicating whether horizontal rules are rendered between rows. The horizontal rules are
        ///     rendered only if <see cref="RowSpacing"/> is greater than zero.</summary>
        public bool HorizontalRules { get; set; }
        /// <summary>
        ///     Gets or sets a value indicating whether vertical rules are rendered between columns. The vertical rules are
        ///     rendered only if <see cref="ColumnSpacing"/> is greater than zero.</summary>
        public bool VerticalRules { get; set; }
        /// <summary>
        ///     Gets or sets a value indicating the number of rows from the top that are considered table headers. The only
        ///     effect of this is that the horizontal rule (if any) after the header rows is rendered using '=' characters
        ///     instead of '-'.</summary>
        public int HeaderRows { get; set; }
        /// <summary>
        ///     If true, the table will be expanded to fill the <see cref="MaxWidth"/>. If false, the table will fill the
        ///     whole width only if any cells need to be word-wrapped.</summary>
        public bool UseFullWidth { get; set; }
        /// <summary>
        ///     Specifies the default alignment to use for cells where the alignment is not explicitly set. Default is <see
        ///     cref="HorizontalTextAlignment.Left"/>.</summary>
        public HorizontalTextAlignment DefaultAlignment { get; set; }
        /// <summary>
        ///     Gets or sets a value indicating the number of spaces to add left of the table. This does not count towards the
        ///     <see cref="MaxWidth"/>.</summary>
        public int LeftMargin { get; set; }

        /// <summary>
        ///     Places the specified content into the cell at the specified co-ordinates.</summary>
        /// <param name="col">
        ///     Column where to place the content.</param>
        /// <param name="row">
        ///     Row where to place the content.</param>
        /// <param name="content">
        ///     The content to place.</param>
        /// <param name="colSpan">
        ///     The number of columns to span.</param>
        /// <param name="rowSpan">
        ///     The number of rows to span.</param>
        /// <param name="noWrap">
        ///     If true, indicates that this cell should not be automatically word-wrapped except at explicit newlines in
        ///     <paramref name="content"/>. The cell is word-wrapped only if doing so is necessary to fit all no-wrap cells
        ///     into the table's total width. If false, the cell is automatically word-wrapped to optimise the table's layout.</param>
        /// <param name="alignment">
        ///     How to align the contents within the cell, or null to use <see cref="DefaultAlignment"/>.</param>
        /// <param name="background">
        ///     Specifies a background color for the whole cell.</param>
        public void SetCell(int col, int row, string content, int colSpan = 1, int rowSpan = 1, bool noWrap = false, HorizontalTextAlignment? alignment = null, ConsoleColor? background = null)
        {
            setCell(col, row, content, colSpan, rowSpan, noWrap, alignment, background);
        }

        /// <summary>
        ///     Places the specified content into the cell at the specified co-ordinates.</summary>
        /// <param name="col">
        ///     Column where to place the content.</param>
        /// <param name="row">
        ///     Row where to place the content.</param>
        /// <param name="content">
        ///     The content to place.</param>
        /// <param name="colSpan">
        ///     The number of columns to span.</param>
        /// <param name="rowSpan">
        ///     The number of rows to span.</param>
        /// <param name="noWrap">
        ///     If true, indicates that this cell should not be automatically word-wrapped except at explicit newlines in
        ///     <paramref name="content"/>. The cell is word-wrapped only if doing so is necessary to fit all no-wrap cells
        ///     into the table's total width. If false, the cell is automatically word-wrapped to optimise the table's layout.</param>
        /// <param name="alignment">
        ///     How to align the contents within the cell, or null to use <see cref="DefaultAlignment"/>.</param>
        /// <param name="background">
        ///     Specifies a background color for the whole cell, including its empty space. Characters with background colors
        ///     in the input string take precedence for those characters only.</param>
        public void SetCell(int col, int row, ConsoleColoredString content, int colSpan = 1, int rowSpan = 1, bool noWrap = false, HorizontalTextAlignment? alignment = null, ConsoleColor? background = null)
        {
            setCell(col, row, content, colSpan, rowSpan, noWrap, alignment, background);
        }

        private void setCell(int col, int row, object content, int colSpan, int rowSpan, bool noWrap, HorizontalTextAlignment? alignment, ConsoleColor? background = null)
        {
            if (col < 0)
                throw new ArgumentOutOfRangeException(nameof(col), col, @"""col"" cannot be negative.");
            if (row < 0)
                throw new ArgumentOutOfRangeException(nameof(row), row, @"""row"" cannot be negative.");
            if (colSpan < 1)
                throw new ArgumentOutOfRangeException(nameof(colSpan), colSpan, @"""colSpan"" cannot be less than 1.");
            if (rowSpan < 1)
                throw new ArgumentOutOfRangeException(nameof(rowSpan), rowSpan, @"""rowSpan"" cannot be less than 1.");
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            // Complain if setting this cell would overlap with another cell due to its colspan or rowspan
            if (row >= _cells.Count || col >= _cells[row].Count || _cells[row][col] == null || _cells[row][col] is surrogateCell)
            {
                for (int x = 0; x < colSpan; x++)
                    for (int y = 0; y < rowSpan; y++)
                        if (row + y < _cells.Count && col + x < _cells[row + y].Count && _cells[row + y][col + x] is surrogateCell)
                        {
                            var sur = (surrogateCell) _cells[row][col];
                            var real = (trueCell) _cells[sur.RealRow][sur.RealCol];
                            throw new InvalidOperationException(@"The cell at column {0}, row {1} is already occupied because the cell at column {2}, row {3} has colspan {4} and rowspan {5}.".Fmt(col, row, sur.RealCol, sur.RealRow, real.ColSpan, real.RowSpan));
                        }
            }

            ensureCell(col, row);

            // If the cell contains a true cell, remove it with all its surrogates
            if (_cells[row][col] is trueCell)
            {
                var tr = (trueCell) _cells[row][col];
                for (int x = 0; x < tr.ColSpan; x++)
                    for (int y = 0; y < tr.RowSpan; y++)
                        _cells[row + y][col + x] = null;
            }

            // Insert the cell in the right place
            _cells[row][col] = new trueCell
            {
                ColSpan = colSpan,
                RowSpan = rowSpan,
                Value = content,
                NoWrap = noWrap,
                Alignment = alignment,
                Background = background
            };

            // For cells with span, insert the appropriate surrogate cells.
            for (int x = 0; x < colSpan; x++)
                for (int y = x == 0 ? 1 : 0; y < rowSpan; y++)
                {
                    ensureCell(col + x, row + y);
                    _cells[row + y][col + x] = new surrogateCell { RealCol = col, RealRow = row };
                }
        }

        /// <summary>
        ///     Generates the table.</summary>
        /// <returns>
        ///     The complete rendered table as a single string.</returns>
        public override string ToString()
        {
            var result = new StringBuilder();
            toString(MaxWidth, s => result.Append(s), s => result.Append(s.ToString()));
            return result.ToString();
        }

        /// <summary>
        ///     Generates the table.</summary>
        /// <returns>
        ///     The complete rendered table as a single <see cref="ConsoleColoredString"/>.</returns>
        public ConsoleColoredString ToColoredString()
        {
            var result = new List<ConsoleColoredString>();
            toString(MaxWidth, s => result.Add(s), s => result.Add(s));
            return new ConsoleColoredString(result.ToArray());
        }

        /// <summary>Outputs the entire table to the console.</summary>
        public void WriteToConsole()
        {
            toString(MaxWidth ?? ConsoleUtil.WrapToWidth(), s => Console.Write(s), s => ConsoleUtil.Write(s));
        }

        private void toString(int? maxWidth, Action<string> outputString, Action<ConsoleColoredString> outputColoredString)
        {
            int rows = _cells.Count;
            if (rows == 0)
                return;
            int cols = _cells.Max(row => row.Count);

            // Create a lookup array which, for each column, and for each possible value of colspan, tells you which cells in that column have this colspan and end in this column
            var cellsByColspan = new SortedDictionary<int, List<int>>[cols];
            for (var col = 0; col < cols; col++)
            {
                var cellsInThisColumn = new SortedDictionary<int, List<int>>();
                for (int row = 0; row < rows; row++)
                {
                    if (col >= _cells[row].Count)
                        continue;
                    var cel = _cells[row][col];
                    if (cel == null)
                        continue;
                    if (cel is surrogateCell && ((surrogateCell) cel).RealRow != row)
                        continue;
                    int realCol = cel is surrogateCell ? ((surrogateCell) cel).RealCol : col;
                    var realCell = (trueCell) _cells[row][realCol];
                    if (realCol + realCell.ColSpan - 1 != col)
                        continue;
                    cellsInThisColumn.AddSafe(realCell.ColSpan, row);
                }
                cellsByColspan[col] = cellsInThisColumn;
            }

            // Find out the width that each column would have if the text wasn't wrapped.
            // If this fits into the total width, then we want each column to be at least this wide.
            var columnWidths = generateColumnWidths(cols, cellsByColspan, c => Math.Max(1, c.LongestParagraph()));
            var unwrapped = true;

            // If the table is now too wide, use the length of the longest word, or longest paragraph if nowrap
            if (maxWidth != null && columnWidths.Sum() > maxWidth - (cols - 1) * ColumnSpacing)
            {
                columnWidths = generateColumnWidths(cols, cellsByColspan, c => Math.Max(1, c.MinWidth()));
                unwrapped = false;
            }

            // If the table is still too wide, use the length of the longest paragraph if nowrap, otherwise 0
            if (maxWidth != null && columnWidths.Sum() > maxWidth - (cols - 1) * ColumnSpacing)
                columnWidths = generateColumnWidths(cols, cellsByColspan, c => c.NoWrap ? Math.Max(1, c.LongestParagraph()) : 1);

            // If the table is still too wide, we will have to wrap like crazy.
            if (maxWidth != null && columnWidths.Sum() > maxWidth - (cols - 1) * ColumnSpacing)
            {
                columnWidths = new int[cols];
                for (int i = 0; i < cols; i++) columnWidths[i] = 1;
            }

            // If the table is STILL too wide, all bets are off.
            if (maxWidth != null && columnWidths.Sum() > maxWidth - (cols - 1) * ColumnSpacing)
                throw new InvalidOperationException(@"The specified table width is too narrow. It is not possible to fit the {0} columns and the column spacing of {1} per column into a total width of {2} characters.".Fmt(cols, ColumnSpacing, maxWidth));

            // If we have any extra width to spare...
            var missingTotalWidth = maxWidth == null ? 0 : maxWidth - columnWidths.Sum() - (cols - 1) * ColumnSpacing;
            if (missingTotalWidth > 0 && (UseFullWidth || !unwrapped))
            {
                // Use the length of the longest paragraph in each column to calculate a proportion by which to enlarge each column
                var widthProportionByCol = new int[cols];
                for (var col = 0; col < cols; col++)
                    foreach (var kvp in cellsByColspan[col])
                        distributeEvenly(
                            widthProportionByCol,
                            col,
                            kvp.Key,
                            kvp.Value.Max(row => ((trueCell) _cells[row][col - kvp.Key + 1]).LongestParagraph()) - widthProportionByCol.Skip(col - kvp.Key + 1).Take(kvp.Key).Sum() - (unwrapped ? 0 : columnWidths.Skip(col - kvp.Key + 1).Take(kvp.Key).Sum())
                        );
                var widthProportionTotal = widthProportionByCol.Sum();

                // Adjust the width of the columns according to the calculated proportions so that they fill the missing width.
                // We do this in two steps. Step one: enlarge the column widths by the integer part of the calculated portion (round down).
                // After this the width remaining will be smaller than the number of columns, so each column is missing at most 1 character.
                var widthRemaining = missingTotalWidth;
                var fractionalParts = new double[cols];
                for (int col = 0; col < cols; col++)
                {
                    var widthToAdd = (double) (widthProportionByCol[col] * missingTotalWidth) / widthProportionTotal;
                    var integerPart = (int) widthToAdd;
                    columnWidths[col] += integerPart;
                    fractionalParts[col] = widthToAdd - integerPart;
                    widthRemaining -= integerPart;
                }

                // Step two: enlarge a few more columns by 1 character so that we reach the desired width.
                // The columns with the largest fractional parts here are the furthest from what we ideally want, so we favour those.
                foreach (var elem in fractionalParts.Select((frac, col) => new { Value = frac, Col = col }).OrderByDescending(e => e.Value))
                {
                    if (widthRemaining < 1) break;
                    columnWidths[elem.Col]++;
                    widthRemaining--;
                }
            }

            // Word-wrap all the contents of all the cells
            trueCell truCel;
            foreach (var row in _cells)
                for (int col = 0; col < row.Count; col++)
                    if ((truCel = row[col] as trueCell) != null)
                        truCel.Wordwrap(columnWidths.Skip(col).Take(truCel.ColSpan).Sum() + (truCel.ColSpan - 1) * ColumnSpacing);

            // Calculate the string index for each column
            var strIndexByCol = new int[cols + 1];
            for (var i = 0; i < cols; i++)
                strIndexByCol[i + 1] = strIndexByCol[i] + columnWidths[i] + ColumnSpacing;
            var realWidth = strIndexByCol[cols] - ColumnSpacing;

            // Make sure we don't render rules if we can't
            bool verticalRules = VerticalRules && ColumnSpacing > 0;
            bool horizontalRules = HorizontalRules && RowSpacing > 0;

            // If we do render vertical rules, where should it be (at which string offset, counted backwards from the end of the column spacing)
            var vertRuleOffset = (ColumnSpacing + 1) / 2;

            // Finally, render the entire output
            List<ConsoleColoredString> currentLine = null;
            for (int row = 0; row < rows; row++)
            {
                var rowList = _cells[row];
                var extraRows = RowSpacing + 1;
                var isFirstIteration = true;
                bool anyMoreContentInThisRow;
                do
                {
                    ConsoleColoredString previousLine = currentLine == null ? null : new ConsoleColoredString(currentLine.ToArray());
                    currentLine = new List<ConsoleColoredString>();
                    anyMoreContentInThisRow = false;
                    for (int col = 0; col < cols; col++)
                    {
                        var cel = col < rowList.Count ? rowList[col] : null;

                        // For cells with colspan, consider only the first cell they're spanning and skip the rest
                        if (cel is surrogateCell && ((surrogateCell) cel).RealCol != col)
                            continue;

                        // If the cell has rowspan, what row did this cell start in?
                        var valueRow = cel is surrogateCell ? ((surrogateCell) cel).RealRow : row;

                        // Retrieve the data for the cell
                        var realCell = col < _cells[valueRow].Count ? (trueCell) _cells[valueRow][col] : null;
                        var colspan = realCell == null ? 1 : realCell.ColSpan;
                        var rowspan = realCell == null ? 1 : realCell.RowSpan;
                        var rowBackground = row >= _rowBackgrounds.Count ? null : _rowBackgrounds[row];

                        // Does this cell end in this row?
                        var isLastRow = valueRow + rowspan - 1 == row;

                        // If we are inside the cell, render one line of the contents of the cell
                        if (realCell != null && realCell.WordwrappedValue.Length > realCell.WordwrappedIndex)
                        {
                            var align = realCell.Alignment ?? DefaultAlignment;
                            var curLineLength = currentLine.Sum(c => c.Length);
                            var cellBackground = realCell.Background ?? rowBackground;
                            if (strIndexByCol[col] > curLineLength)
                                currentLine.Add(new string(' ', strIndexByCol[col] - curLineLength).Color(null, cellBackground));
                            object textRaw = realCell.WordwrappedValue[realCell.WordwrappedIndex];
                            ConsoleColoredString text = textRaw is ConsoleColoredString ? (ConsoleColoredString) textRaw : (string) textRaw;  // implicit conversion to ConsoleColoredString
                            if (align == HorizontalTextAlignment.Center)
                                currentLine.Add(new string(' ', (strIndexByCol[col + colspan] - strIndexByCol[col] - ColumnSpacing - text.Length) / 2).Color(null, cellBackground));
                            else if (align == HorizontalTextAlignment.Right)
                                currentLine.Add(new string(' ', strIndexByCol[col + colspan] - strIndexByCol[col] - ColumnSpacing - text.Length).Color(null, cellBackground));
                            if (cellBackground == null)
                                currentLine.Add(text);
                            else
                            {
                                currentLine.Add(text.ColorBackgroundWhereNull(cellBackground.Value));
                                if (align == HorizontalTextAlignment.Center)
                                    currentLine.Add(new string(' ', (strIndexByCol[col + colspan] - strIndexByCol[col] - ColumnSpacing - text.Length + 1) / 2).Color(null, cellBackground));
                                else if (align == HorizontalTextAlignment.Left)
                                    currentLine.Add(new string(' ', strIndexByCol[col + colspan] - strIndexByCol[col] - ColumnSpacing - text.Length).Color(null, cellBackground));
                            }
                            realCell.WordwrappedIndex++;
                        }

                        // If we are at the end of a row, render horizontal rules
                        var horizRuleStart = col > 0 ? strIndexByCol[col] - vertRuleOffset + 1 : 0;
                        var horizRuleEnd = (col + colspan < cols) ? strIndexByCol[col + colspan] - vertRuleOffset + (verticalRules ? 0 : 1) : realWidth;
                        var renderingHorizontalRules = horizontalRules && isLastRow && extraRows == 1;
                        if (renderingHorizontalRules)
                        {
                            currentLine.Add(new string(' ', horizRuleStart - currentLine.Sum(c => c.Length)));
                            currentLine.Add(new string((row == HeaderRows - 1) ? '=' : '-', horizRuleEnd - horizRuleStart));
                        }
                        else
                        {
                            var subtract = (col + colspan == cols ? ColumnSpacing : vertRuleOffset) + currentLine.Sum(c => c.Length);
                            currentLine.Add(new string(' ', strIndexByCol[col + colspan] - subtract).Color(null, (realCell == null ? null : realCell.Background) ?? rowBackground));
                        }

                        // If we are at the beginning of a row, render the horizontal rules for the row above by modifying the previous line.
                        // We want to do this because it may have an unwanted vertical rule if this is a cell with colspan and there are
                        // multiple cells with smaller colspans above it.
                        if (isFirstIteration && horizontalRules && row > 0 && cel is trueCell)
                            previousLine = new ConsoleColoredString(previousLine.Substring(0, horizRuleStart), new string((row == HeaderRows) ? '=' : '-', horizRuleEnd - horizRuleStart), previousLine.Substring(horizRuleEnd));

                        // Render vertical rules
                        if (verticalRules && (col + colspan < cols))
                            currentLine.Add((new string(' ', strIndexByCol[col + colspan] - vertRuleOffset - currentLine.Sum(c => c.Length)) + "|").Color(null, renderingHorizontalRules ? null : rowBackground));

                        // Does this cell still contain any more content that needs to be output before this row can be finished?
                        anyMoreContentInThisRow = anyMoreContentInThisRow || (realCell != null && isLastRow && realCell.WordwrappedValue.Length > realCell.WordwrappedIndex);
                    }

                    if (previousLine != null)
                    {
                        if (LeftMargin > 0)
                            outputString(new string(' ', LeftMargin));
                        outputColoredString(previousLine);
                        outputString(Environment.NewLine);
                    }

                    isFirstIteration = false;

                    // If none of the cells in this row contain any more content, start counting down the row spacing
                    if (!anyMoreContentInThisRow)
                        extraRows--;
                }
                while (anyMoreContentInThisRow || (extraRows > 0 && row < rows - 1));
            }

            // Output the last line
            if (LeftMargin > 0)
                outputString(new string(' ', LeftMargin));
            outputColoredString(new ConsoleColoredString(currentLine.ToArray()));
            outputString(Environment.NewLine);
        }

        private abstract class cell { }
        private sealed class surrogateCell : cell
        {
            public int RealRow, RealCol;
            public override string ToString() { return "{" + RealCol + ", " + RealRow + "}"; }
        }
        private sealed class trueCell : cell
        {
            public object Value;                               // either string or ConsoleColoredString
            public object[] WordwrappedValue;    // either string[] or ConsoleColoredString[]
            public int WordwrappedIndex, ColSpan, RowSpan;
            public bool NoWrap;
            public HorizontalTextAlignment? Alignment; // if null, use TextTable.DefaultAlignment
            public ConsoleColor? Background;
            public override string ToString() { return Value.ToString(); }

            private int? _cachedLongestWord = null;
            private int? _cachedLongestPara = null;
            public int LongestWord()
            {
                return getLongest(' ', false, ref _cachedLongestWord);
            }
            public int LongestParagraph()
            {
                return getLongest('\n', true, ref _cachedLongestPara);
            }
            public int MinWidth()
            {
                return NoWrap ? LongestParagraph() : LongestWord();
            }
            private int getLongest(char sep, bool para, ref int? cache)
            {
                if (cache == null)
                    cache = Value.ToString().Split(sep).Max(s => s.Length);
                return cache.Value;
            }

            public void Wordwrap(int wrapWidth)
            {
                if (Value is string)
                    WordwrappedValue = ((string) Value).WordWrap(wrapWidth).Cast<object>().ToArray();
                else if (Value is ConsoleColoredString)
                    WordwrappedValue = ((ConsoleColoredString) Value).WordWrap(wrapWidth).Cast<object>().ToArray();
                else
                    throw new InvalidOperationException("h287y2");
                WordwrappedIndex = 0;
            }
        }

        private List<List<cell>> _cells = new List<List<cell>>();
        private List<ConsoleColor?> _rowBackgrounds = new List<ConsoleColor?>();

        /// <summary>
        ///     Sets a background color for an entire row within the table, including the vertical rules if <see
        ///     cref="VerticalRules"/> is <c>true</c>. Background colors for individual cells take precedence within the
        ///     bounds of that cell. Background colors in the input string take precendence for those characters.</summary>
        /// <param name="row">
        ///     The index of the row. If during rendering the table turns out to have fewer rows, the background color is
        ///     ignored.</param>
        /// <param name="backgroundColor">
        ///     The background color to set the row to, or <c>null</c> to reset the color.</param>
        public void SetRowBackground(int row, ConsoleColor? backgroundColor)
        {
            if (row < 0)
                throw new ArgumentOutOfRangeException(nameof(row), row, @"""row"" cannot be negative.");
            while (row >= _rowBackgrounds.Count)
                _rowBackgrounds.Add(null);
            _rowBackgrounds[row] = backgroundColor;
        }

        private void ensureCell(int col, int row)
        {
            while (row >= _cells.Count)
                _cells.Add(new List<cell>());
            while (col >= _cells[row].Count)
                _cells[row].Add(null);
        }

        // Distributes 'width' evenly over the columns from 'col' - 'colspan' + 1 to 'col'.
        private void distributeEvenly(int[] colWidths, int col, int colSpan, int width)
        {
            if (width <= 0) return;
            var each = width / colSpan;
            for (var i = 0; i < colSpan; i++)
                colWidths[col - i] += each;
            var gap = width - (colSpan * each);
            for (var i = 0; i < gap; i++)
                colWidths[col - (i * colSpan / gap)]++;
        }

        private int[] generateColumnWidths(int cols, SortedDictionary<int, List<int>>[] cellsByColspan, Func<trueCell, int> getMinWidth)
        {
            var columnWidths = new int[cols];
            for (int col = 0; col < cols; col++)
                foreach (var kvp in cellsByColspan[col])
                    distributeEvenly(
                        columnWidths,
                        col,
                        kvp.Key,
                        kvp.Value.Select(row => (trueCell) _cells[row][col - kvp.Key + 1]).Max(getMinWidth) - columnWidths.Skip(col - kvp.Key + 1).Take(kvp.Key).Sum() - (kvp.Key - 1) * ColumnSpacing
                    );
            return columnWidths;
        }

        /// <summary>Removes columns that contain only empty cells. Subsequent columns are moved to the left accordingly.</summary>
        public void RemoveEmptyColumns()
        {
            int cols = _cells.Max(row => row.Count);
            int col = 0;
            while (col < cols)
            {
                bool columnEmpty = true;
                foreach (var row in _cells)
                {
                    if (row.Count <= col)
                        continue;
                    var cel = row[col] as trueCell;
                    if (cel != null && cel.ColSpan == 1 && cel.Value != null && (
                        (cel.Value is string && ((string) cel.Value).Length > 0) ||
                        (cel.Value is ConsoleColoredString && ((ConsoleColoredString) cel.Value).Length > 0)))
                    {
                        columnEmpty = false;
                        break;
                    }
                }
                if (!columnEmpty)
                {
                    col++;
                    continue;
                }

                for (int row = 0; row < _cells.Count; row++)
                {
                    if (_cells[row].Count <= col)
                        continue;
                    var cel = _cells[row][col];
                    if (cel != null && cel is surrogateCell && ((surrogateCell) cel).RealRow == row)
                    {
                        ((trueCell) _cells[row][((surrogateCell) cel).RealCol]).ColSpan--;
                        _cells[row].RemoveAt(col);
                    }
                    else if (cel != null && cel is trueCell && ((trueCell) cel).ColSpan > 1)
                    {
                        ((trueCell) cel).ColSpan--;
                        _cells[row].RemoveAt(col + 1);
                    }
                    else
                    {
                        _cells[row].RemoveAt(col);
                    }
                    for (int c = col + 1; c < _cells[row].Count; c++)
                    {
                        var cl = _cells[row][c] as surrogateCell;
                        if (cl != null && cl.RealCol > col)
                            cl.RealCol--;
                    }
                }
                cols--;
            }
        }

        /// <summary>
        ///     Adds a new row to the end of the table, using default cell settings for each cell.</summary>
        /// <param name="values">
        ///     Values to add, one for each column.</param>
        public void AddRow(params string[] values)
        {
            var row = _cells.Count;
            for (int col = 0; col < values.Length; col++)
                SetCell(col, row, values[col]);
        }

        /// <summary>
        ///     Adds a new row to the end of the table, using default cell settings for each cell.</summary>
        /// <param name="values">
        ///     Values to add, one for each column.</param>
        public void AddRow(params ConsoleColoredString[] values)
        {
            var row = _cells.Count;
            for (int col = 0; col < values.Length; col++)
                SetCell(col, row, values[col]);
        }
    }
}

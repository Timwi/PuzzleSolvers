using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using RT.Util.ExtensionMethods;
using RT.Util.Text;

namespace RT.Util.Consoles
{
    /// <summary>Console-related utility functions.</summary>
    public static class ConsoleUtil
    {
        /// <summary>Represents the state of a console output stream.</summary>
        public enum ConsoleState
        {
            /// <summary>This output stream is not available - eg when the program is not a console program.</summary>
            Unavailable,
            /// <summary>This output stream is printed on the console.</summary>
            Console,
            /// <summary>This output stream has been redirected - perhaps to a file or a pipe.</summary>
            Redirected,
        }

        private static bool _consoleInfoInitialised = false;
        private static ConsoleState _stdOutState;
        private static ConsoleState _stdErrState;

        /// <summary>
        ///     Determines the state of the standard output stream. The first call determines the state and caches it;
        ///     subsequent calls return the cached value.</summary>
        public static ConsoleState StdOutState()
        {
            return _stdOutState;
        }

        /// <summary>
        ///     Determines the state of the standard error stream. The first call determines the state and caches it;
        ///     subsequent calls return the cached value.</summary>
        public static ConsoleState StdErrState()
        {
            return _stdErrState;
        }

        /// <summary>
        ///     Returns the maximum line width that console applications should use to correctly word-wrap their output. If
        ///     the output is redirected to a file, this will return an arbitrary but sensible value, otherwise the value
        ///     reflects the width of the console buffer.</summary>
        public static int WrapToWidth()
        {
            if (StdOutState() == ConsoleState.Redirected || StdErrState() == ConsoleState.Redirected)
                return 120;
            try { return Console.BufferWidth - 1; }
            catch { return 120; }
        }

        /// <summary>
        ///     Outputs the specified message to the console window, treating newlines as paragraph breaks. All paragraphs are
        ///     word-wrapped to fit in the console buffer, or to a sensible width if redirected to a file. Each paragraph is
        ///     indented by the number of spaces at the start of the corresponding line.</summary>
        /// <param name="message">
        ///     The message to output.</param>
        /// <param name="hangingIndent">
        ///     Specifies a number of spaces by which the message is indented in all but the first line of each paragraph.</param>
        public static void WriteParagraphs(string message, int hangingIndent = 0)
        {
            // Special case: if message is empty, WordWrap would output nothing
            if (message.Length == 0)
            {
                Console.WriteLine();
                return;
            }
            int width;
            try
            {
                width = WrapToWidth();
            }
            catch
            {
                Console.WriteLine(message);
                return;
            }
            foreach (var line in message.WordWrap(width, hangingIndent))
                Console.WriteLine(line);
        }

        /// <summary>
        ///     Outputs the specified coloured message, marked up using EggsML, to the console window, treating newlines as
        ///     paragraph breaks. All paragraphs are word-wrapped to fit in the console buffer, or to a sensible width if
        ///     redirected to a file. Each paragraph is indented by the number of spaces at the start of the corresponding
        ///     line.</summary>
        /// <param name="message">
        ///     The message to output.</param>
        /// <param name="hangingIndent">
        ///     Specifies a number of spaces by which the message is indented in all but the first line of each paragraph.</param>
        /// <remarks>
        ///     See <see cref="EggsNode.ToConsoleColoredStringWordWrap"/> for the colour syntax.</remarks>
        public static void WriteParagraphs(EggsNode message, int hangingIndent = 0)
        {
            int width;
            try
            {
                width = WrapToWidth();
            }
            catch
            {
                // Fall back to non-word-wrapping
                WriteLine(ConsoleColoredString.FromEggsNode(message));
                return;
            }
            bool any = false;
            foreach (var line in message.ToConsoleColoredStringWordWrap(width, hangingIndent))
            {
                WriteLine(line);
                any = true;
            }

            // Special case: if the input is empty, output an empty line
            if (!any)
                Console.WriteLine();
        }

        /// <summary>
        ///     Outputs the specified message to the console window, treating newlines as paragraph breaks. All paragraphs are
        ///     word-wrapped to fit in the console buffer, or to a sensible width if redirected to a file. Each paragraph is
        ///     indented by the number of spaces at the start of the corresponding line.</summary>
        /// <param name="message">
        ///     The message to output.</param>
        /// <param name="hangingIndent">
        ///     Specifies a number of spaces by which the message is indented in all but the first line of each paragraph.</param>
        public static void WriteParagraphs(ConsoleColoredString message, int hangingIndent = 0)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            int width;
            try
            {
                width = WrapToWidth();
            }
            catch
            {
                ConsoleUtil.WriteLine(message);
                return;
            }
            foreach (var line in message.WordWrap(width, hangingIndent))
                ConsoleUtil.WriteLine(line);
        }

        /// <summary>Writes the specified <see cref="ConsoleColoredString"/> to the console.</summary>
        public static void Write(ConsoleColoredString value, bool stdErr = false)
        {
            if (value != null)
                value.writeTo(stdErr ? Console.Error : Console.Out);
        }

        /// <summary>
        ///     Writes the specified <see cref="ConsoleColoredString"/> followed by a newline to the console.</summary>
        /// <param name="value">
        ///     The string to print to the console.</param>
        /// <param name="stdErr">
        ///     <c>true</c> to print to Standard Error instead of Standard Output.</param>
        /// <param name="align">
        ///     Horizontal alignment of the string within the remaining space of the current line. If the string does not fit,
        ///     it will be printed as if left-aligned.</param>
        public static void WriteLine(ConsoleColoredString value, bool stdErr = false, HorizontalTextAlignment align = HorizontalTextAlignment.Left)
        {
            var output = stdErr ? Console.Error : Console.Out;
            if (value != null)
            {
                var cursorLeft = 0;
                try { cursorLeft = Console.CursorLeft; }
                catch { }
                var width = WrapToWidth() - cursorLeft;
                if (align == HorizontalTextAlignment.Center && width > value.Length)
                    output.Write(new string(' ', (width - value.Length) / 2));
                else if (align == HorizontalTextAlignment.Right && width > value.Length)
                    output.Write(new string(' ', width - value.Length));
                value.writeTo(output);
            }
            output.WriteLine();
        }

        /// <summary>
        ///     Writes the specified or current stack trace to the console in pretty colors.</summary>
        /// <param name="stackTraceLines">
        ///     The stack trace. Each string in this collection is expected to be one line of the stack trace. If <c>null</c>,
        ///     defaults to the current stack trace.</param>
        public static void WriteStackTrace(IEnumerable<string> stackTraceLines = null)
        {
            if (stackTraceLines == null)
                stackTraceLines = Environment.StackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Skip(3);
            foreach (var traceLine in stackTraceLines)
            {
                var m = Regex.Match(traceLine, @"^\s*at ([\w\.]+\.)([\w`<>]+)\.([\w\[\],<>]+)(\(.*\))( in (.:\\.*\\)([^\\]+\.cs):line (\d+))?\s*$");
                if (m.Success)
                    ConsoleUtil.WriteParagraphs("    - ".Color(ConsoleColor.DarkGreen) +
                        m.Groups[1].Value.Color(ConsoleColor.DarkGray) +
                        m.Groups[2].Value.Color(ConsoleColor.Cyan) + ".".Color(ConsoleColor.DarkGray) +
                        m.Groups[3].Value.Color(ConsoleColor.White) +
                        m.Groups[4].Value.Color(ConsoleColor.Green) +
                        (m.Groups[5].Length > 0 ?
                            " in ".Color(ConsoleColor.DarkGray) +
                            m.Groups[6].Value.Color(ConsoleColor.DarkYellow) + m.Groups[7].Value.Color(ConsoleColor.Yellow) + " line ".Color(ConsoleColor.DarkMagenta) +
                            m.Groups[8].Value.Color(ConsoleColor.Magenta)
                        : ""), 8
                    );
                else
                {
                    m = Regex.Match(traceLine, @"^\s*at (.*?)\s*$");
                    if (m.Success)
                        ConsoleUtil.WriteParagraphs("    - ".Color(ConsoleColor.DarkGreen) + m.Groups[1].Value.Color(ConsoleColor.DarkGray), 8);
                    else
                        ConsoleUtil.WriteParagraphs(traceLine, 8);
                }
            }
        }
    }
}

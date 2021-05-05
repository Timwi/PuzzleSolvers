using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.Consoles;

namespace RT.Util.ExtensionMethods
{
    /// <summary>Console-related extension methods.</summary>
#if EXPORT_UTIL
    public
#endif
    static class ConsoleExtensions
    {
        /// <summary>Formats a string in a way compatible with <see cref="string.Format(string, object[])"/>.</summary>
        public static string Fmt(this string formatString, params object[] args)
        {
            return Fmt(formatString, null, args);
        }

        /// <summary>Formats a string in a way compatible with <see cref="string.Format(string, object[])"/>.</summary>
        public static string Fmt(this string formatString, IFormatProvider provider, params object[] args)
        {
            if (formatString == null)
                throw new ArgumentNullException(nameof(formatString));
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            return formatString.Color(null).FmtEnumerableInternal(ConsoleColoredString.FormatBehavior.Stringify, provider, args).JoinString();
        }

        /// <summary>
        ///     Formats the specified objects into the format string. The result is an enumerable collection which enumerates
        ///     parts of the format string interspersed with the arguments as appropriate.</summary>
        public static IEnumerable<object> FmtEnumerable(this string formatString, params object[] args)
        {
            return FmtEnumerable(formatString, null, args);
        }

        /// <summary>
        ///     Formats the specified objects into the format string. The result is an enumerable collection which enumerates
        ///     parts of the format string interspersed with the arguments as appropriate.</summary>
        public static IEnumerable<object> FmtEnumerable(this string formatString, IFormatProvider provider, params object[] args)
        {
            if (formatString == null)
                throw new ArgumentNullException(nameof(formatString));
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            return formatString.Color(null).FmtEnumerableInternal(0, provider, args);
        }

        /// <summary>
        ///     Word-wraps the current <see cref="ConsoleColoredString"/> to a specified width. Supports UNIX-style newlines
        ///     and indented paragraphs.</summary>
        /// <remarks>
        ///     <para>
        ///         The supplied text will be split into "paragraphs" at the newline characters. Every paragraph will begin on
        ///         a new line in the word-wrapped output, indented by the same number of spaces as in the input. All
        ///         subsequent lines belonging to that paragraph will also be indented by the same amount.</para>
        ///     <para>
        ///         All multiple contiguous spaces will be replaced with a single space (except for the indentation).</para></remarks>
        /// <param name="text">
        ///     Text to be word-wrapped.</param>
        /// <param name="maxWidth">
        ///     The maximum number of characters permitted on a single line, not counting the end-of-line terminator.</param>
        /// <param name="hangingIndent">
        ///     The number of spaces to add to each line except the first of each paragraph, thus creating a hanging
        ///     indentation.</param>
        public static IEnumerable<ConsoleColoredString> WordWrap(this ConsoleColoredString text, int maxWidth, int hangingIndent = 0)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (maxWidth < 1)
                throw new ArgumentOutOfRangeException(nameof(maxWidth), maxWidth, "maxWidth cannot be less than 1");
            if (hangingIndent < 0)
                throw new ArgumentOutOfRangeException(nameof(hangingIndent), hangingIndent, "hangingIndent cannot be negative.");
            if (text == null || text.Length == 0)
                return Enumerable.Empty<ConsoleColoredString>();

            return StringExtensions.wordWrap(
                text.Split(new string[] { "\r\n", "\r", "\n" }),
                maxWidth,
                hangingIndent,
                (txt, substrIndex) => txt.Substring(substrIndex).Split(new string[] { " " }, options: StringSplitOptions.RemoveEmptyEntries),
                cc => cc.Length,
                txt =>
                {
                    // Count the number of spaces at the start of the paragraph
                    int indentLen = 0;
                    while (indentLen < txt.Length && txt.CharAt(indentLen) == ' ')
                        indentLen++;
                    return indentLen;
                },
                num => new string(' ', num),
                () => new List<ConsoleColoredString>(),
                list => list.Sum(c => c.Length),
                (list, cc) => { list.Add(cc); },
                list => new ConsoleColoredString(list),
                (str, start, length) => length == null ? str.Substring(start) : str.Substring(start, length.Value),
                (str1, str2) => str1 + str2);
        }

        /// <summary>
        ///     Colors the specified string in the specified console color.</summary>
        /// <param name="str">
        ///     The string to color.</param>
        /// <param name="foreground">
        ///     The foreground color to color the string in.</param>
        /// <param name="background">
        ///     The background color to color the string in.</param>
        /// <returns>
        ///     A potentially colorful string.</returns>
        public static ConsoleColoredString Color(this string str, ConsoleColor? foreground, ConsoleColor? background = null)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            return new ConsoleColoredString(str, foreground, background);
        }

        /// <summary>
        ///     Colors the specified character in the specified console color.</summary>
        /// <param name="ch">
        ///     The character to color.</param>
        /// <param name="foreground">
        ///     The foreground color to color the character in.</param>
        /// <param name="background">
        ///     The background color to color the character in.</param>
        /// <returns>
        ///     A potentially colorful character.</returns>
        public static ConsoleColoredChar Color(this char ch, ConsoleColor? foreground, ConsoleColor? background = null)
        {
            return new ConsoleColoredChar(ch, foreground, background);
        }

        /// <summary>
        ///     Colors the specified range within the specified string in a specified color.</summary>
        /// <param name="str">
        ///     The string to partially colour.</param>
        /// <param name="index">
        ///     The index at which to start colouring.</param>
        /// <param name="length">
        ///     The number of characters to colour.</param>
        /// <param name="foreground">
        ///     The foreground color to assign to the range of characters.</param>
        /// <param name="background">
        ///     The background color to assign to the range of characters.</param>
        /// <returns>
        ///     A potentially colorful string.</returns>
        public static ConsoleColoredString ColorSubstring(this string str, int index, int length, ConsoleColor? foreground, ConsoleColor? background = null)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            if (index < 0 || index > str.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "index cannot be negative or greater than the length of the input string.");
            if (length < 0 || index + length > str.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "length cannot be negative or span beyond the end of the string.");

            return str.Substring(0, index) + str.Substring(index, length).Color(foreground, background) + str.Substring(index + length);
        }

        /// <summary>
        ///     Colors a range of characters beginning at a specified index within the specified string in a specified color.</summary>
        /// <param name="str">
        ///     The string to partially colour.</param>
        /// <param name="index">
        ///     The index at which to start colouring.</param>
        /// <param name="foreground">
        ///     The colour to assign to the characters starting from the character at <paramref name="index"/>.</param>
        /// <param name="background">
        ///     The background color to assign to the range of characters.</param>
        /// <returns>
        ///     A potentially colorful string.</returns>
        public static ConsoleColoredString ColorSubstring(this string str, int index, ConsoleColor? foreground, ConsoleColor? background = null)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            if (index < 0 || index > str.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "index cannot be negative or greater than the length of the input string.");

            return str.Substring(0, index) + str.Substring(index).Color(foreground, background);
        }

        /// <summary>
        ///     Returns the specified object as a colored string.</summary>
        /// <param name="obj">
        ///     The object to convert.</param>
        /// <param name="defaultForeground">
        ///     The foreground color to color the string in if it is not already a <see cref="ConsoleColoredString"/>.</param>
        /// <param name="defaultBackground">
        ///     The background color to color the string in if it is not already a <see cref="ConsoleColoredString"/>.</param>
        /// <returns>
        ///     A potentially colorful string.</returns>
        public static ConsoleColoredString ToConsoleColoredString(this object obj, ConsoleColor? defaultForeground = null, ConsoleColor? defaultBackground = null)
        {
            if (obj == null)
                return ConsoleColoredString.Empty;

            if (obj is ConsoleColoredChar cc)
                return cc.Character.ToString().Color(cc.Color, cc.BackgroundColor);
            return (obj as ConsoleColoredString) ?? obj.ToString().Color(defaultForeground, defaultBackground);
        }

        /// <summary>Equivalent to <see cref="IEnumerableExtensions.JoinString{T}"/>, but for <see cref="ConsoleColoredString"/>s.</summary>
        public static ConsoleColoredString JoinColoredString<T>(this IEnumerable<T> values, ConsoleColoredString separator = null, ConsoleColoredString prefix = null, ConsoleColoredString suffix = null, ConsoleColor defaultColor = ConsoleColor.Gray)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            using (var enumerator = values.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    return ConsoleColoredString.Empty;

                var list = new List<ConsoleColoredString>(values is ICollection<T> ? ((ICollection<T>) values).Count * 4 : 8);
                bool first = true;
                do
                {
                    if (!first && separator != null)
                        list.Add(separator);
                    first = false;
                    if (prefix != null)
                        list.Add(prefix);
                    if (enumerator.Current != null)
                        list.Add(enumerator.Current.ToConsoleColoredString(defaultColor));
                    if (suffix != null)
                        list.Add(suffix);
                }
                while (enumerator.MoveNext());
                return new ConsoleColoredString(list);
            }
        }
    }
}

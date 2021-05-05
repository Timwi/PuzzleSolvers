using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RT.Util.ExtensionMethods;

namespace RT.Util.Consoles
{
    /// <summary>
    ///     Encapsulates a string in which each character can have an associated <see cref="ConsoleColor"/>.</summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item><description>
    ///             Use <see cref="ConsoleUtil.Write(ConsoleColoredString,bool)"/> and <see
    ///             cref="ConsoleUtil.WriteLine(ConsoleColoredString,bool,RT.Util.Text.HorizontalTextAlignment)"/> to output
    ///             the string to the console.</description></item>
    ///         <item><description>
    ///             Each character has two optional <see cref="ConsoleColor"/> values associated with it, one indicating the
    ///             foreground color and one the background color. Those characters whose color is <c>null</c> are printed in
    ///             the default color of the console (which the user can customize in the console window UI).</description></item></list></remarks>
    public sealed class ConsoleColoredString : IEnumerable<ConsoleColoredChar>
    {
        /// <summary>Represents an empty colored string. This field is read-only.</summary>
        public static ConsoleColoredString Empty { get { return _empty ?? (_empty = new ConsoleColoredString()); } }
        private static ConsoleColoredString _empty = null;

        /// <summary>Represents the environment's newline, colored in the default color. This field is read-only.</summary>
        public static ConsoleColoredString NewLine { get { return _newline ?? (_newline = new ConsoleColoredString(Environment.NewLine, (ConsoleColor?) null)); } }
        private static ConsoleColoredString _newline = null;

        private readonly string _text;
        private readonly ConsoleColor?[] _foreground;
        private readonly ConsoleColor?[] _background;

        /// <summary>
        ///     Provides implicit conversion from <see cref="string"/> to <see cref="ConsoleColoredString"/>.</summary>
        /// <param name="input">
        ///     The string to convert.</param>
        public static implicit operator ConsoleColoredString(string input)
        {
            if (input == null)
                return null;
            return new ConsoleColoredString(input, null, (ConsoleColor?) null);
        }

        /// <summary>
        ///     Provides explicit conversion from <see cref="ConsoleColoredString"/> to <see cref="string"/> by discarding all
        ///     color information.</summary>
        /// <param name="input">
        ///     The string to convert.</param>
        public static explicit operator string(ConsoleColoredString input)
        {
            if (input == null)
                return null;
            return input._text;
        }

        /// <summary>Initializes an empty <see cref="ConsoleColoredString"/>.</summary>
        public ConsoleColoredString() { _text = ""; _foreground = new ConsoleColor?[0]; _background = new ConsoleColor?[0]; }

        /// <summary>
        ///     Constructs a <see cref="ConsoleColoredString"/> with the specified text and the specified colors.</summary>
        /// <param name="input">
        ///     The string containing the text to initialize this <see cref="ConsoleColoredString"/> to.</param>
        /// <param name="foreground">
        ///     The foreground color to assign to the whole string.</param>
        /// <param name="background">
        ///     The background color to assign to the whole string.</param>
        public ConsoleColoredString(string input, ConsoleColor? foreground, ConsoleColor? background = null)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            _text = input;
            var l = input.Length;
            _foreground = new ConsoleColor?[l];
            _background = new ConsoleColor?[l];
            if (foreground != null)
                for (int i = 0; i < l; i++)
                    _foreground[i] = foreground;
            if (background != null)
                for (int i = 0; i < l; i++)
                    _background[i] = background;
        }

        /// <summary>
        ///     Constructs a <see cref="ConsoleColoredString"/> with the specified text and the specified colors for each
        ///     character.</summary>
        /// <param name="input">
        ///     The string containing the text to initialize this <see cref="ConsoleColoredString"/> to. The length of this
        ///     string must match the number of elements in <paramref name="foregroundColors"/>.</param>
        /// <param name="foregroundColors">
        ///     The foreground colors to assign to each character in the string. The length of this array must match the
        ///     number of characters in <paramref name="input"/>.</param>
        /// <param name="backgroundColors">
        ///     The background colors to assign to each character in the string. The length of this array must match the
        ///     number of characters in <paramref name="input"/>. If <c>null</c>, the default color is used.</param>
        public ConsoleColoredString(string input, ConsoleColor?[] foregroundColors, ConsoleColor?[] backgroundColors = null)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (foregroundColors == null)
                throw new ArgumentNullException(nameof(foregroundColors));
            if (input.Length != foregroundColors.Length)
                throw new InvalidOperationException("The number of characters must match the number of foreground colors.");
            if (backgroundColors != null && input.Length != backgroundColors.Length)
                throw new InvalidOperationException("The number of characters must match the number of background colors.");
            _text = input;
            _foreground = foregroundColors;
            _background = backgroundColors ?? (new ConsoleColor?[input.Length]);
        }

        /// <summary>
        ///     Constructs a <see cref="ConsoleColoredString"/> by concatenating the specified <see
        ///     cref="ConsoleColoredString"/>s.</summary>
        /// <param name="strings">
        ///     Input strings to concatenate.</param>
        /// <remarks>
        ///     The color of each character in the input strings is preserved.</remarks>
        public ConsoleColoredString(params ConsoleColoredString[] strings)
            : this((ICollection<ConsoleColoredString>) strings)
        {
        }

        /// <summary>
        ///     Returns a new <see cref="ConsoleColoredString"/> in which all characters from the specified <paramref
        ///     name="startIndex"/> onwards have been removed.</summary>
        /// <param name="startIndex">
        ///     Index of the first character to remove.</param>
        public ConsoleColoredString Remove(int startIndex) => Remove(startIndex, Length - startIndex);

        /// <summary>
        ///     Returns a new <see cref="ConsoleColoredString"/> in which the specified range of characters has been removed.</summary>
        /// <param name="startIndex">
        ///     Index of the start of the range of characters to remove.</param>
        /// <param name="count">
        ///     Number of characters to remove from the <paramref name="startIndex"/> onwards.</param>
        public ConsoleColoredString Remove(int startIndex, int count)
        {
            if (startIndex < 0 || startIndex > Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex cannot be negative or greater than the length of the string.");
            if (count < 0 || startIndex + count > Length)
                throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative or extend beyond the end of the string.");

            var foreground = new ConsoleColor?[Length - count];
            Array.Copy(_foreground, 0, foreground, 0, startIndex);
            Array.Copy(_foreground, startIndex + count, foreground, startIndex, Length - startIndex - count);
            var background = new ConsoleColor?[Length - count];
            Array.Copy(_background, 0, background, 0, startIndex);
            Array.Copy(_background, startIndex + count, background, startIndex, Length - startIndex - count);
            return new ConsoleColoredString(_text.Remove(startIndex, count), foreground, background);
        }

        /// <summary>
        ///     Constructs a <see cref="ConsoleColoredString"/> by concatenating the specified <see
        ///     cref="ConsoleColoredString"/>s.</summary>
        /// <param name="strings">
        ///     Input strings to concatenate.</param>
        /// <remarks>
        ///     The color of each character in the input strings is preserved.</remarks>
        public ConsoleColoredString(ICollection<ConsoleColoredString> strings)
        {
            var builder = new StringBuilder();
            foreach (var str in strings)
                builder.Append(str._text);
            _text = builder.ToString();
            _foreground = new ConsoleColor?[_text.Length];
            _background = new ConsoleColor?[_text.Length];
            var index = 0;
            foreach (var str in strings)
            {
                Array.Copy(str._foreground, 0, _foreground, index, str.Length);
                Array.Copy(str._background, 0, _background, index, str.Length);
                index += str.Length;
            }
        }

        /// <summary>
        ///     Constructs a <see cref="ConsoleColoredString"/> by concatenating the specified <see
        ///     cref="ConsoleColoredChar"/>s.</summary>
        /// <param name="characters">
        ///     Input characters to concatenate.</param>
        /// <remarks>
        ///     The color of each character in the input strings is preserved.</remarks>
        public ConsoleColoredString(params ConsoleColoredChar[] characters)
        {
            var builder = new StringBuilder();
            foreach (var ch in characters)
                builder.Append(ch.Character);
            _text = builder.ToString();
            _foreground = new ConsoleColor?[_text.Length];
            _background = new ConsoleColor?[_text.Length];
            for (var i = 0; i < characters.Length; i++)
            {
                _foreground[i] = characters[i].Color;
                _background[i] = characters[i].BackgroundColor;
            }
        }

        /// <summary>Returns the number of characters in this <see cref="ConsoleColoredString"/>.</summary>
        public int Length { get { return _text.Length; } }
        /// <summary>Returns the raw text of this <see cref="ConsoleColoredString"/> by discarding all the color information.</summary>
        public override string ToString() { return _text; }

        /// <summary>
        ///     Concatenates two <see cref="ConsoleColoredString"/>s.</summary>
        /// <param name="string1">
        ///     First input string to concatenate.</param>
        /// <param name="string2">
        ///     Second input string to concatenate.</param>
        /// <remarks>
        ///     The color of each character in the input strings is preserved.</remarks>
        public static ConsoleColoredString operator +(ConsoleColoredString string1, ConsoleColoredString string2)
        {
            if (string1 == null || string1.Length == 0)
                return string2 ?? "";
            if (string2 == null || string2.Length == 0)
                return string1;
            return new ConsoleColoredString(string1, string2);
        }

        /// <summary>
        ///     Concatenates a string onto a <see cref="ConsoleColoredString"/>.</summary>
        /// <param name="string1">
        ///     First input string to concatenate.</param>
        /// <param name="string2">
        ///     Second input string to concatenate.</param>
        /// <remarks>
        ///     The color of each character in the first input string is preserved. The second input string is given the
        ///     console default color.</remarks>
        public static ConsoleColoredString operator +(ConsoleColoredString string1, string string2)
        {
            if (string1 == null || string1.Length == 0)
                return string2 ?? "";    // implicit conversion
            if (string2 == null || string2.Length == 0)
                return string1;

            var totalLength = string1._foreground.Length + string2.Length;
            var foreground = new ConsoleColor?[totalLength];
            var background = new ConsoleColor?[totalLength];
            Array.Copy(string1._foreground, foreground, string1.Length);
            Array.Copy(string1._background, background, string1.Length);
            return new ConsoleColoredString(string1._text + string2, foreground, background);
        }

        /// <summary>
        ///     Concatenates a <see cref="ConsoleColoredString"/> onto a string.</summary>
        /// <param name="string1">
        ///     First input string to concatenate.</param>
        /// <param name="string2">
        ///     Second input string to concatenate.</param>
        /// <remarks>
        ///     The color of each character in the second input string is preserved. The first input string is given the
        ///     console default color.</remarks>
        public static ConsoleColoredString operator +(string string1, ConsoleColoredString string2)
        {
            if (string2 == null || string2.Length == 0)
                return string1 ?? "";   // implicit conversion
            if (string1 == null || string1.Length == 0)
                return string2;

            var foreground = new ConsoleColor?[string1.Length + string2.Length];
            var background = new ConsoleColor?[string1.Length + string2.Length];
            Array.Copy(string2._foreground, 0, foreground, string1.Length, string2.Length);
            Array.Copy(string2._background, 0, background, string1.Length, string2.Length);
            return new ConsoleColoredString(string1 + string2._text, foreground, background);
        }

        /// <summary>
        ///     Constructs a <see cref="ConsoleColoredString"/> from an EggsML parse tree.</summary>
        /// <param name="node">
        ///     The root node of the EggsML parse tree.</param>
        /// <returns>
        ///     The <see cref="ConsoleColoredString"/> constructed from the EggsML parse tree.</returns>
        /// <remarks>
        ///     <para>
        ///         The following EggsML tags map to the following console colors:</para>
        ///     <list type="bullet">
        ///         <item><description>
        ///             <c>~</c> = black, or dark gray if inside a <c>*</c> tag</description></item>
        ///         <item><description>
        ///             <c>/</c> = dark blue, or blue if inside a <c>*</c> tag</description></item>
        ///         <item><description>
        ///             <c>$</c> = dark green, or green if inside a <c>*</c> tag</description></item>
        ///         <item><description>
        ///             <c>&amp;</c> = dark cyan, or cyan if inside a <c>*</c> tag</description></item>
        ///         <item><description>
        ///             <c>_</c> = dark red, or red if inside a <c>*</c> tag</description></item>
        ///         <item><description>
        ///             <c>%</c> = dark magenta, or magenta if inside a <c>*</c> tag</description></item>
        ///         <item><description>
        ///             <c>^</c> = dark yellow, or yellow if inside a <c>*</c> tag</description></item>
        ///         <item><description>
        ///             <c>=</c> = dark gray (independent of <c>*</c> tag)</description></item></list>
        ///     <para>
        ///         Text which is not inside any of the above color tags defaults to light gray, or white if inside a <c>*</c>
        ///         tag.</para></remarks>
        public static ConsoleColoredString FromEggsNode(EggsNode node)
        {
            StringBuilder text = new StringBuilder();
            List<ConsoleColor?> colors = new List<ConsoleColor?>();
            List<int> colorLengths = new List<int>();

            eggWalk(node, text, colors, colorLengths, null);

            var colArr = new ConsoleColor?[colorLengths.Sum()];
            var index = 0;
            for (int i = 0; i < colors.Count; i++)
            {
                var col = colors[i];
                for (int j = 0; j < colorLengths[i]; j++)
                {
                    colArr[index] = col;
                    index++;
                }
            }

            return new ConsoleColoredString(text.ToString(), colArr);
        }

        private static void eggWalk(EggsNode node, StringBuilder text, List<ConsoleColor?> colors, List<int> colorLengths, ConsoleColor? curColor)
        {
            var tag = node as EggsTag;
            if (tag != null)
            {
                bool curLight = curColor >= ConsoleColor.DarkGray;
                switch (tag.Tag)
                {
                    case '~': curColor = curLight ? ConsoleColor.DarkGray : ConsoleColor.Black; break;
                    case '/': curColor = curLight ? ConsoleColor.Blue : ConsoleColor.DarkBlue; break;
                    case '$': curColor = curLight ? ConsoleColor.Green : ConsoleColor.DarkGreen; break;
                    case '&': curColor = curLight ? ConsoleColor.Cyan : ConsoleColor.DarkCyan; break;
                    case '_': curColor = curLight ? ConsoleColor.Red : ConsoleColor.DarkRed; break;
                    case '%': curColor = curLight ? ConsoleColor.Magenta : ConsoleColor.DarkMagenta; break;
                    case '^': curColor = curLight ? ConsoleColor.Yellow : ConsoleColor.DarkYellow; break;
                    case '=': curColor = ConsoleColor.DarkGray; curLight = true; break;
                    case '*': if (!curLight) curColor = (ConsoleColor) ((int) (curColor ?? ConsoleColor.Gray) + 8); curLight = true; break;
                }
                foreach (var child in tag.Children)
                    eggWalk(child, text, colors, colorLengths, curColor);
            }
            else if (node is EggsText)
            {
                var txt = (EggsText) node;
                text.Append(txt.Text);
                colors.Add(curColor);
                colorLengths.Add(txt.Text.Length);
            }
        }

        /// <summary>
        ///     Returns the character at the specified index.</summary>
        /// <param name="index">
        ///     A character position in the current <see cref="ConsoleColoredString"/>.</param>
        /// <returns>
        ///     The character at the specified index.</returns>
        public char CharAt(int index)
        {
            if (index < 0 || index >= _text.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "index must be greater or equal to 0 and smaller than the length of the ConsoleColoredString.");
            return _text[index];
        }

        /// <summary>Equivalent to <see cref="string.IndexOf(char)"/>.</summary>
        public int IndexOf(char value) { return _text.IndexOf(value); }
        /// <summary>Equivalent to <see cref="string.IndexOf(string)"/>.</summary>
        public int IndexOf(string value) { return _text.IndexOf(value); }
        /// <summary>Equivalent to <see cref="string.IndexOf(char,int)"/>.</summary>
        public int IndexOf(char value, int startIndex) { return _text.IndexOf(value, startIndex); }
        /// <summary>Equivalent to <see cref="string.IndexOf(string,int)"/>.</summary>
        public int IndexOf(string value, int startIndex) { return _text.IndexOf(value, startIndex); }
        /// <summary>Equivalent to <see cref="string.IndexOf(string,StringComparison)"/>.</summary>
        public int IndexOf(string value, StringComparison comparisonType) { return _text.IndexOf(value, comparisonType); }
        /// <summary>Equivalent to <see cref="string.IndexOf(char,int,int)"/>.</summary>
        public int IndexOf(char value, int startIndex, int count) { return _text.IndexOf(value, startIndex, count); }
        /// <summary>Equivalent to <see cref="string.IndexOf(string,int,int)"/>.</summary>
        public int IndexOf(string value, int startIndex, int count) { return _text.IndexOf(value, startIndex, count); }
        /// <summary>Equivalent to <see cref="string.IndexOf(string,int,StringComparison)"/>.</summary>
        public int IndexOf(string value, int startIndex, StringComparison comparisonType) { return _text.IndexOf(value, startIndex, comparisonType); }
        /// <summary>Equivalent to <see cref="string.IndexOf(string,int,int,StringComparison)"/>.</summary>
        public int IndexOf(string value, int startIndex, int count, StringComparison comparisonType) { return _text.IndexOf(value, startIndex, count, comparisonType); }

        /// <summary>
        ///     Returns a new string in which a specified string is inserted at a specified index position in this instance.</summary>
        /// <param name="startIndex">
        ///     The zero-based index position of the insertion.</param>
        /// <param name="value">
        ///     The string to insert.</param>
        /// <returns>
        ///     A new string that is equivalent to this instance, but with <paramref name="value"/> inserted at position
        ///     <paramref name="startIndex"/>.</returns>
        public ConsoleColoredString Insert(int startIndex, ConsoleColoredString value)
        {
            if (startIndex < 0 || startIndex > Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex cannot be negative or greater than the length of the string.");
            return Substring(0, startIndex) + value + Substring(startIndex);
        }

        /// <summary>
        ///     Returns a string array that contains the substrings in this <see cref="ConsoleColoredString"/> that are
        ///     delimited by elements of a specified string array. Parameters specify the maximum number of substrings to
        ///     return and whether to return empty array elements.</summary>
        /// <param name="separator">
        ///     An array of strings that delimit the substrings in this <see cref="ConsoleColoredString"/>, an empty array
        ///     that contains no delimiters, or null.</param>
        /// <param name="count">
        ///     The maximum number of substrings to return, or null to return all.</param>
        /// <param name="options">
        ///     Specify <see cref="System.StringSplitOptions.RemoveEmptyEntries"/> to omit empty array elements from the array
        ///     returned, or <see cref="System.StringSplitOptions.None"/> to include empty array elements in the array
        ///     returned.</param>
        /// <returns>
        ///     A collection whose elements contain the substrings in this <see cref="ConsoleColoredString"/> that are
        ///     delimited by one or more strings in <paramref name="separator"/>.</returns>
        public IEnumerable<ConsoleColoredString> Split(string[] separator, int? count = null, StringSplitOptions options = StringSplitOptions.None)
        {
            if (separator == null)
            {
                if (_text.Length > 0 || (options & StringSplitOptions.RemoveEmptyEntries) == 0)
                    yield return this;
                yield break;
            }
            var index = 0;
            while (true)
            {
                var candidates = separator.Select(sep => new { Separator = sep, MatchIndex = _text.IndexOf(sep, index) }).Where(sep => sep.MatchIndex != -1).ToArray();
                if (!candidates.Any())
                {
                    if (index < _text.Length || (options & StringSplitOptions.RemoveEmptyEntries) == 0)
                        yield return Substring(index);
                    yield break;
                }
                var min = candidates.MinElement(a => a.MatchIndex);
                if (min.MatchIndex != index || (options & StringSplitOptions.RemoveEmptyEntries) == 0)
                    yield return Substring(index, min.MatchIndex - index);
                if (count != null)
                {
                    count = count.Value - 1;
                    if (count.Value == 0)
                        yield break;
                }
                index = min.MatchIndex + min.Separator.Length;
            }
        }

        /// <summary>
        ///     Retrieves a substring from this instance. The substring starts at a specified character position.</summary>
        /// <param name="startIndex">
        ///     The zero-based starting character position of a substring in this instance.</param>
        /// <returns>
        ///     A <see cref="ConsoleColoredString"/> object equivalent to the substring that begins at <paramref
        ///     name="startIndex"/> in this instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="startIndex"/> is less than zero or greater than the length of this instance.</exception>
        public ConsoleColoredString Substring(int startIndex)
        {
            return new ConsoleColoredString(_text.Substring(startIndex), _foreground.Subarray(startIndex), _background.Subarray(startIndex));
        }

        /// <summary>
        ///     Retrieves a substring from this instance. The substring starts at a specified character position and has a
        ///     specified length.</summary>
        /// <param name="startIndex">
        ///     The zero-based starting character position of a substring in this instance.</param>
        /// <param name="length">
        ///     The number of characters in the substring.</param>
        /// <returns>
        ///     A <see cref="ConsoleColoredString"/> equivalent to the substring of length length that begins at <paramref
        ///     name="startIndex"/> in this instance.</returns>
        public ConsoleColoredString Substring(int startIndex, int length)
        {
            return new ConsoleColoredString(_text.Substring(startIndex, length), _foreground.Subarray(startIndex, length), _background.Subarray(startIndex, length));
        }

        /// <summary>Outputs the current <see cref="ConsoleColoredString"/> to the console.</summary>
        internal void writeTo(System.IO.TextWriter writer)
        {
            int index = 0;
            Console.ResetColor();
            var defaultFc = Console.ForegroundColor;
            var defaultBc = Console.BackgroundColor;
            while (index < _text.Length)
            {
                ConsoleColor? fc = _foreground[index], bc = _background[index];
                Console.ForegroundColor = fc ?? defaultFc;
                Console.BackgroundColor = bc ?? defaultBc;
                var origIndex = index;
                do
                    index++;
                while (index < _text.Length && _foreground[index] == fc && _background[index] == bc);
                writer.Write(_text.Substring(origIndex, index - origIndex));
            }
            Console.ResetColor();
        }

        /// <summary>
        ///     Replaces each format item in a specified string with the string representation of a corresponding object in a
        ///     specified array.</summary>
        /// <param name="format">
        ///     A composite format string.</param>
        /// <param name="args">
        ///     An object array that contains zero or more objects to format.</param>
        /// <returns>
        ///     A copy of <paramref name="format"/> in which the format items have been replaced by the string representation
        ///     of the corresponding objects in <paramref name="args"/>.</returns>
        public static ConsoleColoredString Format(ConsoleColoredString format, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));
            return format.Fmt(args);
        }

        /// <summary>
        ///     Replaces the format item in a specified string with the string representation of a corresponding object in a
        ///     specified array. A specified parameter supplies culture-specific formatting information.</summary>
        /// <param name="provider">
        ///     An object that supplies culture-specific formatting information.</param>
        /// <param name="format">
        ///     A composite format string.</param>
        /// <param name="args">
        ///     An object array that contains zero or more objects to format.</param>
        /// <returns>
        ///     A copy of <paramref name="format"/> in which the format items have been replaced by the string representation
        ///     of the corresponding objects in <paramref name="args"/>.</returns>
        public static ConsoleColoredString Format(ConsoleColoredString format, IFormatProvider provider, params object[] args)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));
            return format.Fmt(provider, args);
        }

        /// <summary>Equivalent to <see cref="ConsoleColoredString.Format(ConsoleColoredString,object[])"/>.</summary>
        public ConsoleColoredString Fmt(params object[] args)
        {
            return Fmt(null, args);
        }

        /// <summary>Equivalent to <see cref="ConsoleColoredString.Format(ConsoleColoredString,IFormatProvider,object[])"/>.</summary>
        public ConsoleColoredString Fmt(IFormatProvider provider, params object[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            return FmtEnumerableInternal(FormatBehavior.Colored | FormatBehavior.Stringify, provider, args).JoinColoredString();
        }

        /// <summary>
        ///     Formats the specified objects into this format string. The result is an enumerable collection which enumerates
        ///     parts of the format string interspersed with the arguments as appropriate.</summary>
        public IEnumerable<object> FmtEnumerable(params object[] args)
        {
            return FmtEnumerable(null, args);
        }

        /// <summary>
        ///     Formats the specified objects into this format string. The result is an enumerable collection which enumerates
        ///     parts of the format string interspersed with the arguments as appropriate.</summary>
        public IEnumerable<object> FmtEnumerable(IFormatProvider provider, params object[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            return FmtEnumerableInternal(FormatBehavior.Colored, provider, args);
        }

        [Flags]
        internal enum FormatBehavior
        {
            Stringify = 1,
            Colored = 2
        }

        internal IEnumerable<object> FmtEnumerableInternal(FormatBehavior behavior, IFormatProvider provider, params object[] args)
        {
            var index = 0;
            var oldIndex = 0;
            var customFormatter = provider == null ? null : provider.GetFormat(typeof(ICustomFormatter)) as ICustomFormatter;
            var substring = (behavior & FormatBehavior.Colored) != 0
                ? Ut.Lambda((int ix, int length) => (object) Substring(ix, length))
                : Ut.Lambda((int ix, int length) => (object) _text.Substring(ix, length));

            while (index < _text.Length)
            {
                char ch = _text[index];
                if (ch == '{' && index < _text.Length - 1 && _text[index + 1] == '{')
                {
                    yield return substring(oldIndex, index + 1 - oldIndex);
                    index++;
                    oldIndex = index + 1;
                }
                else if (ch == '{' && index < _text.Length - 1 && _text[index + 1] >= '0' && _text[index + 1] <= '9')
                {
                    var implicitForeground = _foreground[index];
                    var implicitBackground = _background[index];
                    if (index > oldIndex)
                        yield return substring(oldIndex, index - oldIndex);
                    var num = 0;
                    var leftAlign = false;
                    var align = 0;
                    StringBuilder foregroundBuilder = null, backgroundBuilder = null, formatBuilder = null;

                    // Syntax: {num[,alignment][/[foreground]][+[background]][:format]}
                    // States: 0 = before first digit of num; 1 = during num; 2 = before align; 3 = during align; 4 = during foreground; 5 = during background; 6 = during format
                    var state = 0;

                    while (true)
                    {
                        index++;
                        if (index == _text.Length)
                            throw new FormatException("The specified format string is invalid.");
                        ch = _text[index];

                        if (ch == '}')
                        {
                            if (index + 1 == _text.Length || _text[index + 1] != '}' || state == 1 || state == 3)
                                break;
                            index++;
                        }
                        if (state == 6 && ch == '{' && index + 1 < _text.Length && _text[index + 1] == '{')
                            index++;

                        if ((state == 0 || state == 1) && ch >= '0' && ch <= '9')
                        {
                            num = (num * 10) + (ch - '0');
                            state = 1;
                        }
                        else if (state == 1 && ch == ',')
                            state = 2;
                        else if (state == 2 && ch == '-')
                        {
                            leftAlign = true;
                            state = 3;
                        }
                        else if ((state == 2 || state == 3) && ch >= '0' && ch <= '9')
                        {
                            align = (align * 10) + (ch - '0');
                            state = 3;
                        }
                        else if ((state == 1 || state == 3) && ch == '/')
                        {
                            foregroundBuilder = new StringBuilder();
                            state = 4;
                        }
                        else if ((state == 1 || state == 3 || state == 4) && ch == '+')
                        {
                            backgroundBuilder = new StringBuilder();
                            state = 5;
                        }
                        else if ((state == 1 || state == 3 || state == 4 || state == 5) && ch == ':')
                        {
                            formatBuilder = new StringBuilder();
                            state = 6;
                        }
                        else if (state == 4)
                            foregroundBuilder.Append(ch);
                        else if (state == 5)
                            backgroundBuilder.Append(ch);
                        else if (state == 6)
                            formatBuilder.Append(ch);
                        else
                            throw new FormatException("The specified format string is invalid.");
                    }

                    if (num >= args.Length)
                        throw new FormatException("The specified format string references an array index outside the bounds of the supplied arguments.");

                    var formatString = formatBuilder == null ? null : formatBuilder.ToString();

                    if (behavior == (FormatBehavior.Stringify | FormatBehavior.Colored))
                    {
                        if (args[num] != null)
                        {
                            var foregroundStr = foregroundBuilder == null ? null : foregroundBuilder.ToString();
                            var backgroundStr = backgroundBuilder == null ? null : backgroundBuilder.ToString();
                            ConsoleColor foreground = 0, background = 0;
                            if (foregroundStr != null && foregroundStr != "" && !EnumStrong.TryParse<ConsoleColor>(foregroundStr, out foreground, true))
                                throw new FormatException("The specified format string uses an invalid console color name ({0}).".Fmt(foregroundStr));
                            if (backgroundStr != null && backgroundStr != "" && !EnumStrong.TryParse<ConsoleColor>(backgroundStr, out background, true))
                                throw new FormatException("The specified format string uses an invalid console color name ({0}).".Fmt(backgroundStr));

                            var result = (args[num] as ConsoleColoredString) ?? (args[num] as ConsoleColoredChar?)?.ToConsoleColoredString();

                            // If the object is a ConsoleColoredString, use it (and color it if a color is explicitly specified)
                            if (result != null)
                            {
                                if (foregroundStr != null)
                                    result = result.Color(foregroundStr == "" ? (ConsoleColor?) null : foreground);
                                else if (implicitForeground != null)
                                    result = result.ColorWhereNull(implicitForeground.Value);
                                if (backgroundStr != null)
                                    result = result.ColorBackground(backgroundStr == "" ? (ConsoleColor?) null : background);
                                else if (implicitBackground != null)
                                    result = result.ColorBackgroundWhereNull(implicitBackground.Value);
                            }
                            // ... otherwise use IFormattable and/or the custom formatter (and then color the result of that, if specified).
                            else
                            {
                                var objFormattable = args[num] as IFormattable;
                                result = new ConsoleColoredString(
                                    formatString != null && objFormattable != null ? objFormattable.ToString(formatString, provider) :
                                    formatString != null && customFormatter != null ? customFormatter.Format(formatString, args[num], provider) :
                                    args[num].ToString(),
                                    foregroundStr == null ? implicitForeground : foregroundStr == "" ? (ConsoleColor?) null : foreground,
                                    backgroundStr == null ? implicitBackground : backgroundStr == "" ? (ConsoleColor?) null : background);
                            }

                            // Alignment
                            if (result.Length < align)
                                result = leftAlign ? result + new string(' ', align - result.Length) : new string(' ', align - result.Length) + result;
                            yield return result;
                        }
                    }
                    else if (behavior == FormatBehavior.Stringify)
                    {
                        if (args[num] != null)
                        {
                            var objFormattable = args[num] as IFormattable;
                            var result =
                                formatString != null && objFormattable != null ? objFormattable.ToString(formatString, provider) :
                                formatString != null && customFormatter != null ? customFormatter.Format(formatString, args[num], provider) :
                                args[num].ToString();

                            // Alignment
                            if (result.Length < align)
                                result = leftAlign ? result + new string(' ', align - result.Length) : new string(' ', align - result.Length) + result;
                            yield return result;
                        }
                    }
                    else
                        yield return args[num];

                    oldIndex = index + 1;
                }
                else if (ch == '}')
                {
                    yield return substring(oldIndex, index + 1 - oldIndex);
                    if (index < _text.Length - 1 && _text[index + 1] == '}')
                        index++;
                    oldIndex = index + 1;
                }
                index++;
            }
            if (index > oldIndex)
                yield return substring(oldIndex, index - oldIndex);
        }

        /// <summary>
        ///     Returns an array describing the foreground color of every character in the current string.</summary>
        /// <returns>
        ///     A copy of the internal color array. Modifying the returned array is safe.</returns>
        public ConsoleColor?[] GetColors()
        {
            return _foreground.ToArray();
        }

        /// <summary>
        ///     Returns an array describing the background color of every character in the current string.</summary>
        /// <returns>
        ///     A copy of the internal color array. Modifying the returned array is safe.</returns>
        public ConsoleColor?[] GetBackgroundColors()
        {
            return _background.ToArray();
        }

        /// <summary>
        ///     Gets the character and color at a specified character position in the current colored string.</summary>
        /// <param name="index">
        ///     A character position in the current colored string.</param>
        /// <returns>
        ///     A Unicode character with console colors.</returns>
        /// <exception cref="IndexOutOfRangeException">
        ///     <paramref name="index"/> is greater than or equal to the length of this string or less than zero.</exception>
        public ConsoleColoredChar this[int index]
        {
            get
            {
                if (index < 0 || index >= _text.Length)
                    throw new IndexOutOfRangeException("The index must be non-negative and smaller than the length of the string.");
                return new ConsoleColoredChar(_text[index], _foreground[index], _background[index]);
            }
        }

        /// <summary>
        ///     Changes the foreground colors (but not the background colors) of every character in the current string to the
        ///     specified console color.</summary>
        /// <param name="foreground">
        ///     The foreground color to set the string to, or <c>null</c> to use the console’s default foreground color.</param>
        /// <returns>
        ///     The current string but with the foreground colors changed.</returns>
        public ConsoleColoredString Color(ConsoleColor? foreground)
        {
            var newForeground = new ConsoleColor?[_text.Length];
            if (foreground != null)
                for (int i = 0; i < _text.Length; i++)
                    newForeground[i] = foreground;
            return new ConsoleColoredString(_text, newForeground, _background);
        }

        /// <summary>
        ///     Changes the colors of every character in the current string to the specified set of console colors.</summary>
        /// <param name="foreground">
        ///     The foreground color to set the string to, or <c>null</c> to use the console’s default foreground color.</param>
        /// <param name="background">
        ///     The background color to set the string to, or <c>null</c> to use the console’s default background color.</param>
        /// <returns>
        ///     The current string but with all the colors changed.</returns>
        public ConsoleColoredString Color(ConsoleColor? foreground, ConsoleColor? background)
        {
            var newForeground = new ConsoleColor?[_text.Length];
            var newBackground = new ConsoleColor?[_text.Length];
            if (foreground != null)
                for (int i = 0; i < _text.Length; i++)
                    newForeground[i] = foreground;
            if (background != null)
                for (int i = 0; i < _text.Length; i++)
                    newBackground[i] = background;
            return new ConsoleColoredString(_text, newForeground, newBackground);
        }

        /// <summary>
        ///     Changes the colors of every character in the current string to the specified console color only where there
        ///     isn’t already a color defined.</summary>
        /// <param name="foreground">
        ///     The foreground color to set the uncolored characters to, or <c>null</c> to use the console’s default
        ///     foreground color.</param>
        /// <param name="background">
        ///     The background color to set the uncolored characters to, or <c>null</c> to use the console’s default
        ///     background color.</param>
        /// <returns>
        ///     The current string but with the colors changed.</returns>
        public ConsoleColoredString ColorWhereNull(ConsoleColor foreground, ConsoleColor? background = null)
        {
            var newForeground = new ConsoleColor?[_text.Length];
            var newBackground = _background;

            for (int i = 0; i < _text.Length; i++)
                newForeground[i] = _foreground[i] ?? foreground;

            if (background != null)
            {
                newBackground = new ConsoleColor?[_text.Length];
                for (int i = 0; i < _text.Length; i++)
                    newBackground[i] = _background[i] ?? background;
            }

            return new ConsoleColoredString(_text, newForeground, newBackground);
        }

        /// <summary>
        ///     Changes the background colors (but not the foreground colors) of every character in the current string to the
        ///     specified console color.</summary>
        /// <param name="background">
        ///     The background color to set the string to, or <c>null</c> to use the console’s default background color.</param>
        /// <returns>
        ///     The current string but with the background colors changed.</returns>
        public ConsoleColoredString ColorBackground(ConsoleColor? background)
        {
            var newBackground = new ConsoleColor?[_text.Length];
            if (background != null)
                for (int i = 0; i < _text.Length; i++)
                    newBackground[i] = background;
            return new ConsoleColoredString(_text, _foreground, newBackground);
        }

        /// <summary>
        ///     Changes the background colors of every character in the current string to the specified console color only
        ///     where there isn’t already a background color defined.</summary>
        /// <param name="background">
        ///     The background color to set the uncolored characters to, or <c>null</c> to use the console’s default
        ///     background color.</param>
        /// <returns>
        ///     The current string but with the background colors changed.</returns>
        public ConsoleColoredString ColorBackgroundWhereNull(ConsoleColor background)
        {
            var newBackground = new ConsoleColor?[_text.Length];
            for (int i = 0; i < _text.Length; i++)
                newBackground[i] = _background[i] ?? background;
            return new ConsoleColoredString(_text, _foreground, newBackground);
        }

        /// <summary>
        ///     Colors the specified range within the current string in a specified foreground color.</summary>
        /// <param name="index">
        ///     The index at which to start coloring.</param>
        /// <param name="length">
        ///     The number of characters to color.</param>
        /// <param name="foreground">
        ///     The foreground color to assign to the range of characters, or <c>null</c> to use the console’s default
        ///     foreground color.</param>
        /// <returns>
        ///     The current string but with some of the foreground colors changed.</returns>
        public ConsoleColoredString ColorSubstring(int index, int length, ConsoleColor? foreground)
        {
            if (index < 0 || index > _text.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "index cannot be negative or greater than the length of the input string.");
            if (length < 0 || index + length > _text.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "length cannot be negative or span beyond the end of the string.");
            return Substring(0, index) + Substring(index, length).Color(foreground) + Substring(index + length);
        }

        /// <summary>
        ///     Colors the specified range within the current string in the specified colors.</summary>
        /// <param name="index">
        ///     The index at which to start coloring.</param>
        /// <param name="length">
        ///     The number of characters to color.</param>
        /// <param name="foreground">
        ///     The foreground color to assign to the range of characters, or <c>null</c> to use the console’s default
        ///     foreground color.</param>
        /// <param name="background">
        ///     The background color to assign to the range of characters, or <c>null</c> to use the console’s default
        ///     background color.</param>
        /// <returns>
        ///     The current string but with some of the colors changed.</returns>
        public ConsoleColoredString ColorSubstring(int index, int length, ConsoleColor? foreground, ConsoleColor? background)
        {
            if (index < 0 || index > _text.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "index cannot be negative or greater than the length of the input string.");
            if (length < 0 || index + length > _text.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "length cannot be negative or span beyond the end of the string.");
            return Substring(0, index) + Substring(index, length).Color(foreground).ColorBackground(background) + Substring(index + length);
        }

        /// <summary>
        ///     Colors a range of characters beginning at a specified index within the current string in a specified
        ///     foreground color.</summary>
        /// <param name="index">
        ///     The index at which to start coloring.</param>
        /// <param name="foreground">
        ///     The foreground color to assign to the characters starting from the character at <paramref name="index"/>, or
        ///     <c>null</c> to use the console’s default foreground color.</param>
        /// <returns>
        ///     The current string but with some of the foreground colors changed.</returns>
        public ConsoleColoredString ColorSubstring(int index, ConsoleColor? foreground)
        {
            if (index < 0 || index > _text.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "index cannot be negative or greater than the length of the input string.");

            return Substring(0, index) + Substring(index).Color(foreground);
        }

        /// <summary>
        ///     Colors a range of characters beginning at a specified index within the current string in the specified colors.</summary>
        /// <param name="index">
        ///     The index at which to start coloring.</param>
        /// <param name="foreground">
        ///     The foreground color to assign to the characters starting from the character at <paramref name="index"/>, or
        ///     <c>null</c> to use the console’s default foreground color.</param>
        /// <param name="background">
        ///     The background color to assign to the characters starting from the character at <paramref name="index"/>, or
        ///     <c>null</c> to use the console’s default background color.</param>
        /// <returns>
        ///     The current string but with some of the colors changed.</returns>
        public ConsoleColoredString ColorSubstring(int index, ConsoleColor? foreground, ConsoleColor? background)
        {
            if (index < 0 || index > _text.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "index cannot be negative or greater than the length of the input string.");

            return Substring(0, index) + Substring(index).Color(foreground).ColorBackground(background);
        }

        /// <summary>
        ///     Colors the specified range within the current string in a specified background color.</summary>
        /// <param name="index">
        ///     The index at which to start coloring.</param>
        /// <param name="length">
        ///     The number of characters to color.</param>
        /// <param name="background">
        ///     The background color to assign to the range of characters, or <c>null</c> to use the console’s default
        ///     background color.</param>
        /// <returns>
        ///     The current string but with some of the background colors changed.</returns>
        public ConsoleColoredString ColorSubstringBackground(int index, int length, ConsoleColor? background)
        {
            if (index < 0 || index > _text.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "index cannot be negative or greater than the length of the input string.");
            if (length < 0 || index + length > _text.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "length cannot be negative or span beyond the end of the string.");
            return Substring(0, index) + Substring(index, length).ColorBackground(background) + Substring(index + length);
        }

        /// <summary>
        ///     Colors a range of characters beginning at a specified index within the current string in a specified
        ///     background color.</summary>
        /// <param name="index">
        ///     The index at which to start coloring.</param>
        /// <param name="background">
        ///     The background color to assign to the characters starting from the character at <paramref name="index"/>, or
        ///     <c>null</c> to use the console’s default background color.</param>
        /// <returns>
        ///     The current string but with some of the background colors changed.</returns>
        public ConsoleColoredString ColorSubstringBackground(int index, ConsoleColor? background)
        {
            if (index < 0 || index > _text.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "index cannot be negative or greater than the length of the input string.");

            return Substring(0, index) + Substring(index).ColorBackground(background);
        }

        /// <summary>
        ///     Returns a new <see cref="ConsoleColoredString"/> in which every occurrence of <paramref name="oldChar"/> is
        ///     replaced with <paramref name="newChar"/> while each character’s color remains unchanged.</summary>
        /// <param name="oldChar">
        ///     The character to search for.</param>
        /// <param name="newChar">
        ///     The character to replace every occurrence of <paramref name="oldChar"/> with.</param>
        /// <returns>
        ///     The new string after replacements.</returns>
        public ConsoleColoredString Replace(char oldChar, char newChar)
        {
            if (_text.Length == 0)
                return this;
            return new ConsoleColoredString(_text.Replace(oldChar, newChar), _foreground, _background);
        }

        /// <summary>
        ///     Returns a new <see cref="ConsoleColoredString"/> in which every occurrence of the text in <paramref
        ///     name="oldValue"/> is replaced with a new colored string.</summary>
        /// <param name="oldValue">
        ///     The substring to search for.</param>
        /// <param name="newValue">
        ///     The new colored string to replace every occurrence with.</param>
        /// <param name="comparison">
        ///     A string comparison to use.</param>
        /// <returns>
        ///     The new string after replacements.</returns>
        public ConsoleColoredString Replace(string oldValue, ConsoleColoredString newValue, StringComparison comparison = StringComparison.Ordinal)
        {
            if (oldValue == null)
                throw new ArgumentNullException(nameof(oldValue));
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));

            var index = 0;
            var newText = new List<ConsoleColoredString>();

            while (true)
            {
                var pos = _text.IndexOf(oldValue, index, comparison);
                if (pos == -1)
                {
                    if (index < _text.Length)
                        newText.Add(Substring(index));
                    return new ConsoleColoredString(newText);
                }

                if (pos > index)
                    newText.Add(Substring(index, pos - index));
                newText.Add(newValue);
                index = pos + oldValue.Length;
            }
        }

        /// <summary>
        ///     Returns a new <see cref="ConsoleColoredString"/> in which every occurrence of the text in <paramref
        ///     name="oldValue"/> is replaced with the text in <paramref name="newValue"/> colored by the color of the first
        ///     character in each match.</summary>
        /// <param name="oldValue">
        ///     The substring to search for.</param>
        /// <param name="newValue">
        ///     The new string to replace every occurrence with.</param>
        /// <param name="comparison">
        ///     A string comparison to use.</param>
        /// <returns>
        ///     The new string after replacements.</returns>
        public ConsoleColoredString ReplaceText(string oldValue, string newValue, StringComparison comparison = StringComparison.Ordinal)
        {
            if (oldValue == null)
                throw new ArgumentNullException(nameof(oldValue));
            if (oldValue.Length == 0)
                throw new ArgumentException("oldValue cannot be the empty string.", nameof(oldValue));
            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));

            var index = 0;
            var newText = new List<ConsoleColoredString>();

            while (true)
            {
                var pos = _text.IndexOf(oldValue, index, comparison);
                if (pos == -1)
                {
                    if (index < _text.Length)
                        newText.Add(Substring(index));
                    return new ConsoleColoredString(newText);
                }

                if (pos > index)
                    newText.Add(Substring(index, pos - index));
                newText.Add(newValue.Color(_foreground[pos], _background[pos]));
                index = pos + oldValue.Length;
            }
        }

        /// <summary>
        ///     Returns a new string that right-aligns the characters in this instance by padding them with spaces on the
        ///     left, for a specified total length.</summary>
        /// <param name="totalWidth">
        ///     The number of characters in the resulting string, equal to the number of original characters plus any
        ///     additional padding characters.</param>
        public ConsoleColoredString PadLeft(int totalWidth) => PadLeft(totalWidth, ' ');
        /// <summary>
        ///     Returns a new string that right-aligns the characters in this instance by padding them on the left with a
        ///     specified colored character, for a specified total length.</summary>
        /// <param name="totalWidth">
        ///     The number of characters in the resulting string, equal to the number of original characters plus any
        ///     additional padding characters.</param>
        /// <param name="paddingChar">
        ///     A colored padding character.</param>
        public ConsoleColoredString PadLeft(int totalWidth, ConsoleColoredChar paddingChar)
        {
            if (totalWidth < 0)
                throw new ArgumentOutOfRangeException(nameof(totalWidth), "Total width cannot be negative.");
            if (Length >= totalWidth)
                return this;
            return this + new ConsoleColoredString(new string(paddingChar.Character, totalWidth - Length), paddingChar.Color, paddingChar.BackgroundColor);
        }

        /// <summary>
        ///     Returns a new string that left-aligns the characters in this instance by padding them with spaces on the
        ///     right, for a specified total length.</summary>
        /// <param name="totalWidth">
        ///     The number of characters in the resulting string, equal to the number of original characters plus any
        ///     additional padding characters.</param>
        public ConsoleColoredString PadRight(int totalWidth) => PadRight(totalWidth, ' ');
        /// <summary>
        ///     Returns a new string that left-aligns the characters in this instance by padding them on the right with a
        ///     specified colored character, for a specified total length.</summary>
        /// <param name="totalWidth">
        ///     The number of characters in the resulting string, equal to the number of original characters plus any
        ///     additional padding characters.</param>
        /// <param name="paddingChar">
        ///     A colored padding character.</param>
        public ConsoleColoredString PadRight(int totalWidth, ConsoleColoredChar paddingChar)
        {
            if (totalWidth < 0)
                throw new ArgumentOutOfRangeException(nameof(totalWidth), "Total width cannot be negative.");
            if (Length >= totalWidth)
                return this;
            return this + new ConsoleColoredString(new string(paddingChar.Character, totalWidth - Length), paddingChar.Color, paddingChar.BackgroundColor);
        }

        /// <summary>
        ///     Returns a string in which the coloration of this ConsoleColoredString is represented as user-readable text.</summary>
        public string ToDebugString
        {
            get
            {
                var sb = new StringBuilder();
                ConsoleColor? fg = null, bg = null;
                for (int i = 0; i < _text.Length; i++)
                {
                    if (_foreground[i] != fg || _background[i] != bg)
                    {
                        fg = _foreground[i];
                        bg = _background[i];
                        sb.Append("<{0}/{1}>".Fmt(fg, bg));
                    }
                    sb.Append(_text[i]);
                }
                return sb.ToString();
            }
        }

        /// <summary>Implements <see cref="IEnumerable{T}.GetEnumerator"/>.</summary>
        public IEnumerator<ConsoleColoredChar> GetEnumerator() => Enumerable.Range(0, Length).Select(ix => this[ix]).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Contains a character and a console foreground and background color.</summary>
    public struct ConsoleColoredChar
    {
        /// <summary>Gets the character.</summary>
        public char Character { get; private set; }
        /// <summary>Gets the foreground color. <c>null</c> indicates to use the console’s default foreground color.</summary>
        public ConsoleColor? Color { get; private set; }
        /// <summary>Gets the background color. <c>null</c> indicates to use the console’s default background color.</summary>
        public ConsoleColor? BackgroundColor { get; private set; }
        /// <summary>
        ///     Constructor.</summary>
        /// <param name="character">
        ///     The character.</param>
        /// <param name="foreground">
        ///     The foreground color.</param>
        /// <param name="background">
        ///     The background color.</param>
        public ConsoleColoredChar(char character, ConsoleColor? foreground, ConsoleColor? background = null)
        {
            Character = character;
            Color = foreground;
            BackgroundColor = background;
        }

        /// <summary>
        ///     Concatenates two <see cref="ConsoleColoredChar"/>s into a <see cref="ConsoleColoredString"/>.</summary>
        /// <param name="char1">
        ///     First input character to concatenate.</param>
        /// <param name="char2">
        ///     Second input character to concatenate.</param>
        /// <remarks>
        ///     The color of each character in the input strings is preserved.</remarks>
        public static ConsoleColoredString operator +(ConsoleColoredChar char1, ConsoleColoredChar char2)
        {
            return new ConsoleColoredString(string.Concat(char1.Character, char2.Character), new[] { char1.Color, char2.Color }, new[] { char1.BackgroundColor, char2.BackgroundColor });
        }

        /// <summary>
        ///     Concatenates a <see cref="ConsoleColoredChar"/> onto a string and returns a <see
        ///     cref="ConsoleColoredString"/>.</summary>
        /// <param name="char1">
        ///     First input character to concatenate.</param>
        /// <param name="string2">
        ///     Second input string to concatenate.</param>
        /// <remarks>
        ///     The color of the character is preserved. The string is given the console default color.</remarks>
        public static ConsoleColoredString operator +(ConsoleColoredChar char1, string string2)
        {
            if (string2 == null || string2.Length == 0)
                return char1.ToConsoleColoredString();

            var totalLength = string2.Length + 1;
            var foreground = new ConsoleColor?[totalLength];
            foreground[0] = char1.Color;
            var background = new ConsoleColor?[totalLength];
            background[0] = char1.BackgroundColor;
            return new ConsoleColoredString(char1.Character + string2, foreground, background);
        }

        /// <summary>
        ///     Concatenates a string onto a <see cref="ConsoleColoredChar"/> and returns a <see
        ///     cref="ConsoleColoredString"/>.</summary>
        /// <param name="string1">
        ///     First input string to concatenate.</param>
        /// <param name="char2">
        ///     Second input character to concatenate.</param>
        /// <remarks>
        ///     The color of the character is preserved. The string is given the console default color.</remarks>
        public static ConsoleColoredString operator +(string string1, ConsoleColoredChar char2)
        {
            if (string1 == null || string1.Length == 0)
                return char2.ToConsoleColoredString(); ;

            var foreground = new ConsoleColor?[string1.Length + 1];
            foreground[string1.Length] = char2.Color;
            var background = new ConsoleColor?[string1.Length + 1];
            background[string1.Length] = char2.BackgroundColor;
            return new ConsoleColoredString(string1 + char2.Character, foreground, background);
        }

        /// <summary>
        ///     Implicitly converts an uncolored <c>char</c> to a <see cref="ConsoleColoredChar"/> with no color.</summary>
        /// <param name="ch">
        ///     The character to convert.</param>
        public static implicit operator ConsoleColoredChar(char ch) => new ConsoleColoredChar(ch, null);
    }
}

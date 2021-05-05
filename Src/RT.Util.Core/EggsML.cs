using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using RT.Util.Collections;
using RT.Util.Consoles;
using RT.Util.ExtensionMethods;

namespace RT.Util
{
    /// <summary>
    ///     Implements a parser for the minimalist text mark-up language EggsML.</summary>
    /// <remarks>
    ///     <para>
    ///         The “rules” of EggsML are, in summary:</para>
    ///     <list type="bullet">
    ///         <item><description>
    ///             In EggsML, the following non-alphanumeric characters are “special” (have meaning): <c>~ @ # $ % ^ &amp; *
    ///             _ = + / \ | [ ] { } &lt; &gt; ` "</c></description></item>
    ///         <item><description>
    ///             All other characters are always literal.</description></item>
    ///         <item><description>
    ///             All the special characters can be escaped by doubling them.</description></item>
    ///         <item><description>
    ///             The characters <c>~ @ # $ % ^ &amp; * _ = + / \ | [ { &lt;</c> can be used to open a “tag”.</description></item>
    ///         <item><description>
    ///             Tags that start with <c>[ { &lt;</c> are closed with <c>] } &gt;</c>. All other tags are closed with the
    ///             same character.</description></item>
    ///         <item><description>
    ///             Tags can be nested arbitrarily. In order to start a nested tag of the same character as its immediate
    ///             parent, triple the tag character. For example, <c>*one ***two* three*</c> contains an asterisk tag nested
    ///             inside another asterisk tag, while <c>*one *two* three*</c> would be parsed as two asterisk tags, one
    ///             containing “one ” and the other containing “ three”.</description></item>
    ///         <item><description>
    ///             The backtick character (<c>`</c>) can be used to “unjoin” multiple copies of the same character. For
    ///             example, <c>**</c> is a literal asterisk, but <c>*`*</c> is an empty tag containing no text.</description></item>
    ///         <item><description>
    ///             The double-quote character (<c>"</c>) can be used to escape long strings of special characters, e.g. URLs.</description></item></list></remarks>
    public static class EggsML
    {
        /// <summary>Returns all characters that have a special meaning in EggsML.</summary>
        public static string SpecialCharacters => "~@#$%^&*_=+/\\[]{}<>|`\"";

        /// <summary>
        ///     Parses the specified EggsML input.</summary>
        /// <param name="input">
        ///     The EggsML text to parse.</param>
        /// <returns>
        ///     The resulting parse-tree.</returns>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item><description>
        ///             Tags are parsed into instances of <see cref="EggsTag"/>.</description></item>
        ///         <item><description>
        ///             The top-level nodes are contained in an instance of <see cref="EggsTag"/> whose <see
        ///             cref="EggsTag.Tag"/> property is set to null.</description></item>
        ///         <item><description>
        ///             All the literal text is parsed into instances of <see cref="EggsText"/>. All continuous text is
        ///             consolidated, so there are no two consecutive EggsText instances in any list of children.</description></item></list></remarks>
        /// <exception cref="EggsMLParseException">
        ///     Invalid syntax was encountered. The exception object contains the string index at which the error was
        ///     detected.</exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input"/> was null.</exception>
        public static EggsNode Parse(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var curTag = new EggsTag(null, 0);
            if (input.Length == 0)
                return curTag;

            var curText = "";
            var curTextIndex = 0;
            var index = 0;
            var stack = new Stack<EggsTag>();

            while (input.Length > 0)
            {
                var pos = input.IndexOf(ch => SpecialCharacters.Contains(ch));

                // If no more special characters, we are done
                if (pos == -1)
                {
                    curText += input;
                    break;
                }

                // Find out the length of a run of special characters
                var idx = pos + 1;
                while (idx < input.Length && input[idx] == input[pos])
                    idx++;
                int runLength = idx - pos;

                if (runLength % 2 == 0)
                {
                    curText += input.Substring(0, pos) + new string(input[pos], runLength / 2);
                    input = input.Substring(idx);
                    index += idx;
                    continue;
                }

                index += pos;
                if (runLength > 3 && input[pos] != '"')
                    throw new EggsMLParseException("Five or more consecutive same characters not allowed unless number is even.", index, runLength);
                if (runLength == 3 && (alwaysCloses(input[pos]) || input[pos] == '`'))
                    throw new EggsMLParseException("Three consecutive same closing-tag characters or backticks not allowed.", index, 3);

                if (pos > 0)
                {
                    curText += input.Substring(0, pos);
                    input = input.Substring(pos);
                    continue;
                }

                switch (input[0])
                {
                    case '`':
                        input = input.Substring(1);
                        index++;
                        continue;

                    case '"':
                        pos = input.IndexOf('"', 1);
                        if (pos == -1)
                            throw new EggsMLParseException(@"Closing ‘""’ missing", index, 1);
                        while (pos < input.Length - 1 && input[pos + 1] == '"')
                        {
                            pos = input.IndexOf('"', pos + 2);
                            if (pos == -1)
                                throw new EggsMLParseException(@"Closing ‘""’ missing", index, 1);
                        }
                        curText += input.Substring(1, pos - 1).Replace("\"\"", "\"");
                        input = input.Substring(pos + 1);
                        index += pos + 1;
                        continue;

                    default:
                        // Are we opening a new tag?
                        if ((runLength == 3 || alwaysOpens(input[0]) || input[0] != opposite(curTag.Tag)) && !alwaysCloses(input[0]))
                        {
                            if (!string.IsNullOrEmpty(curText))
                                curTag.Add(new EggsText(curText, curTextIndex));
                            stack.Push(curTag);
                            curTag = new EggsTag(input[0], index);
                            index += runLength;
                            curText = "";
                            curTextIndex = index;
                            input = input.Substring(runLength);
                            continue;
                        }
                        // Are we closing a tag?
                        else if (input[0] == opposite(curTag.Tag))
                        {
                            if (!string.IsNullOrEmpty(curText))
                                curTag.Add(new EggsText(curText, curTextIndex));
                            var prevTag = stack.Pop();
                            prevTag.Add(curTag);
                            curTag = prevTag;
                            index++;
                            curText = "";
                            curTextIndex = index;
                            input = input.Substring(1);
                            continue;
                        }
                        else if (alwaysCloses(input[0]))
                        {
                            if (curTag.Tag == null)
                                throw new EggsMLParseException(@"Tag ‘{0}’ unexpected.".Fmt(input[0]), index, 1);
                            else
                                throw new EggsMLParseException(@"Tag ‘{0}’ unexpected; expected closing ‘{1}’".Fmt(input[0], opposite(curTag.Tag)), index, 1, curTag.Index);
                        }
                        throw new EggsMLParseException(@"Character ‘{0}’ unexpected; expected closing ‘{1}’".Fmt(input[0], opposite(curTag.Tag)), index, 1, curTag.Index);
                }
            }

            if (stack.Count > 0)
                throw new EggsMLParseException(@"Closing ‘{0}’ missing".Fmt(opposite(curTag.Tag)), index, 0, curTag.Index);

            if (!string.IsNullOrEmpty(curText))
                curTag.Add(new EggsText(curText, curTextIndex));

            return curTag;
        }

        /// <summary>
        ///     Escapes the input string such that it can be used in EggsML syntax. The result will either have no special
        ///     characters in it or be entirely enclosed in double-quotes.</summary>
        public static string Escape(string input) =>
            !input.Any(ch => SpecialCharacters.Contains(ch))
                ? input
                : input.All(ch => ch == '"')
                    ? new string('"', input.Length * 2)
                    : @"""" + input.Replace(@"""", @"""""") + @"""";

        internal static char? opposite(char? p) => p == '[' ? ']' : p == '<' ? '>' : p == '{' ? '}' : p;
        internal static bool alwaysOpens(char? p) { return p == '[' || p == '<' || p == '{'; }
        private static bool alwaysCloses(char? p) { return p == ']' || p == '>' || p == '}'; }

        /// <summary>
        ///     Provides a delegate for <see cref="WordWrap&lt;TState&gt;"/> which renders a piece of text.</summary>
        /// <typeparam name="TState">
        ///     The type of the text state, e.g. font or color.</typeparam>
        /// <param name="state">
        ///     The state (font, color, etc.) the string is in.</param>
        /// <param name="text">
        ///     The string to render.</param>
        /// <param name="width">
        ///     The measured width of the string.</param>
        public delegate void EggRender<TState>(TState state, string text, int width);

        /// <summary>
        ///     Provides a delegate for <see cref="WordWrap&lt;TState&gt;"/> which measures the width of a string.</summary>
        /// <typeparam name="TState">
        ///     The type of the text state, e.g. font or color.</typeparam>
        /// <param name="state">
        ///     The state (font, color etc.) of the text.</param>
        /// <param name="text">
        ///     The text whose width to measure.</param>
        /// <returns>
        ///     The width of the text in any arbitrary unit, as long as the “width” parameter in the call to <see
        ///     cref="WordWrap&lt;TState&gt;"/> is in the same unit.</returns>
        public delegate int EggMeasure<TState>(TState state, string text);

        /// <summary>
        ///     Provides a delegate for <see cref="WordWrap&lt;TState&gt;"/> which advances to the next line.</summary>
        /// <typeparam name="TState">
        ///     The type of the text state, e.g. font or color.</typeparam>
        /// <param name="state">
        ///     The state (font, color etc.) of the text.</param>
        /// <param name="newParagraph">
        ///     ‘true’ if a new paragraph begins, ‘false’ if a word is being wrapped within a paragraph.</param>
        /// <param name="indent">
        ///     If <paramref name="newParagraph"/> is false, the indentation of the current paragraph as measured only by its
        ///     leading spaces; otherwise, zero.</param>
        /// <returns>
        ///     The indentation for the next line. Use this to implement, for example, hanging indents.</returns>
        public delegate int EggNextLine<TState>(TState state, bool newParagraph, int indent);

        /// <summary>
        ///     Provides a delegate for <see cref="WordWrap&lt;TState&gt;"/> which determines how the text state (font, color
        ///     etc.) changes for a given EggsML tag character. This delegate is called for all tags except for <c>+...+</c>
        ///     and <c>&lt;...&gt;</c>.</summary>
        /// <typeparam name="TState">
        ///     The type of the text state, e.g. font or color.</typeparam>
        /// <param name="oldState">
        ///     The previous state (for the parent tag).</param>
        /// <param name="eggTag">
        ///     The EggsML tag character.</param>
        /// <param name="parameter">
        ///     The contents of the immediately preceding <c>&lt;...&gt;</c> tag (if any), which can be used to parameterize
        ///     other tags.</param>
        /// <returns>
        ///     The next state (return the old state for all tags that should not have a meaning) and an integer indicating
        ///     the amount by which opening this tag has advanced the text position.</returns>
        public delegate (TState newState, int advance) EggNextState<TState>(TState oldState, char eggTag, string parameter);

        private sealed class EggWalkData<TState>
        {
            public bool AtStartOfLine;
            public List<string> WordPieces;
            public List<TState> WordPiecesState;
            public List<int> WordPiecesWidths;
            public int WordPiecesWidthsSum;
            public string Spaces;
            public TState SpaceState;
            public EggMeasure<TState> Measure;
            public EggRender<TState> Render;
            public EggNextLine<TState> AdvanceToNextLine;
            public EggNextState<TState> NextState;
            public int X, WrapWidth, ActualWidth, CurParagraphIndent;
            public string CurParameter;

            public void EggWalkWordWrap(EggsNode node, TState initialState)
            {
                if (node == null)
                    throw new ArgumentNullException(nameof(node));

                eggWalkWordWrapRecursive(node, initialState, false);

                if (WordPieces.Count > 0)
                    renderPieces(initialState);
            }

            private void eggWalkWordWrapRecursive(EggsNode node, TState state, bool curNowrap)
            {
                if (node is EggsTag tag)
                {
                    var newState = state;
                    if (tag.Tag == '+')
                        curNowrap = true;
                    else if (tag.Tag == '<')
                    {
                        if (CurParameter != null)
                            throw new InvalidOperationException("An angle-bracket tag must be immediately followed by another tag.");
                        CurParameter = tag.ToString(true);
                        return;
                    }
                    else if (tag.Tag != null)
                    {
                        var tup = NextState(state, tag.Tag.Value, CurParameter);
                        CurParameter = null;
                        newState = tup.newState;
                        X += tup.advance;
                    }
                    foreach (var child in tag.Children)
                        eggWalkWordWrapRecursive(child, newState, curNowrap);
                    if (CurParameter != null)
                        throw new InvalidOperationException("An angle-bracket tag must be immediately followed by another tag.");
                }
                else if (node is EggsText text)
                {
                    if (CurParameter != null)
                        throw new InvalidOperationException("An angle-bracket tag must be immediately followed by another tag.");
                    var txt = text.Text;
                    var i = 0;
                    while (i < txt.Length)
                    {
                        // Check whether we are looking at a whitespace character or not, and if not, find the end of the word.
                        int lengthOfWord = 0;
                        while (lengthOfWord + i < txt.Length && (curNowrap || !isWrappableAfter(txt, lengthOfWord + i)) && txt[lengthOfWord + i] != '\n')
                            lengthOfWord++;

                        if (lengthOfWord > 0)
                        {
                            // We are looking at a word. (It doesn’t matter whether we’re at the beginning of the word or in the middle of one.)
                            retry1:
                            string fragment = txt.Substring(i, lengthOfWord);
                            var fragmentWidth = Measure(state, fragment);
                            retry2:

                            // If we are at the start of a line, and the word itself doesn’t fit on a line by itself, we need to break the word up.
                            if (AtStartOfLine && X + WordPiecesWidthsSum + fragmentWidth > WrapWidth)
                            {
                                // We don’t know exactly where to break the word, so use binary search to discover where that is.
                                if (lengthOfWord > 1)
                                {
                                    lengthOfWord /= 2;
                                    goto retry1;
                                }

                                // If we get to here, ‘WordPieces’ contains as much of the word as fits into one line, and the next letter makes it too long.
                                // If ‘WordPieces’ is empty, we are at the beginning of a paragraph and the first letter already doesn’t fit.
                                if (WordPieces.Count > 0)
                                {
                                    // Render the part of the word that fits on the line and then move to the next line.
                                    renderPieces(state);
                                    advanceToNextLine(state, false);
                                }
                            }
                            else if (!AtStartOfLine && X + Measure(state, Spaces) + WordPiecesWidthsSum + fragmentWidth > WrapWidth)
                            {
                                // We have already rendered some text on this line, but the word we’re looking at right now doesn’t
                                // fit into the rest of the line, so leave the rest of this line blank and advance to the next line.
                                advanceToNextLine(state, false);

                                // In case the word also doesn’t fit on a line all by itself, go back to top (now that ‘AtStartOfLine’ is true)
                                // where it will check whether we need to break the word apart.
                                goto retry2;
                            }

                            // If we get to here, the current fragment fits on the current line (or it is a single character that overflows
                            // the line all by itself).
                            WordPieces.Add(fragment);
                            WordPiecesState.Add(state);
                            WordPiecesWidths.Add(fragmentWidth);
                            WordPiecesWidthsSum += fragmentWidth;
                            i += lengthOfWord;
                            continue;
                        }

                        // We encounter a whitespace character. All the word pieces fit on the current line, so render them.
                        if (WordPieces.Count > 0)
                        {
                            renderPieces(state);
                            AtStartOfLine = false;
                        }

                        if (txt[i] == '\n')
                        {
                            // If the whitespace character is actually a newline, start a new paragraph.
                            advanceToNextLine(state, true);
                            i++;
                        }
                        else
                        {
                            // Discover the extent of the spaces.
                            var lengthOfSpaces = 0;
                            while (lengthOfSpaces + i < txt.Length && isWrappableAfter(txt, lengthOfSpaces + i) && txt[lengthOfSpaces + i] != '\n')
                                lengthOfSpaces++;

                            Spaces = txt.Substring(i, lengthOfSpaces);
                            SpaceState = state;
                            i += lengthOfSpaces;

                            if (AtStartOfLine)
                            {
                                // If we are at the beginning of the line, treat these spaces as the paragraph’s indentation.
                                CurParagraphIndent += renderSpaces(Spaces, state);
                            }
                        }
                    }
                }
                else
                    throw new InvalidOperationException("An EggsNode is expected to be either EggsTag or EggsText, not {0}.".Fmt(node.GetType().FullName));
            }

            private static bool isWrappableAfter(string txt, int index)
            {
                // Return false for all the whitespace characters that should NOT be wrappable
                switch (txt[index])
                {
                    case '\u00a0':   // NO-BREAK SPACE
                    case '\u202f':    // NARROW NO-BREAK SPACE
                        return false;
                }

                // Return true for all the NON-whitespace characters that SHOULD be wrappable
                switch (txt[index])
                {
                    case '\u200b':   // ZERO WIDTH SPACE
                        return true;
                }

                // Apart from the above exceptions, wrap at whitespace characters.
                return char.IsWhiteSpace(txt, index);
            }

            private void advanceToNextLine(TState state, bool newParagraph)
            {
                if (newParagraph)
                    CurParagraphIndent = 0;
                X = AdvanceToNextLine(state, newParagraph, CurParagraphIndent);
                AtStartOfLine = true;
            }

            private int renderSpaces(string spaces, TState state)
            {
                var w = Measure(state, spaces);
                Render(state, spaces, w);
                X += w;
                ActualWidth = Math.Max(ActualWidth, X);
                return w;
            }

            private void renderPieces(TState state)
            {
                // Add a space if we are not at the beginning of the line.
                if (!AtStartOfLine)
                    renderSpaces(Spaces, SpaceState);
                for (int j = 0; j < WordPieces.Count; j++)
                    Render(WordPiecesState[j], WordPieces[j], WordPiecesWidths[j]);
                X += WordPiecesWidthsSum;
                ActualWidth = Math.Max(ActualWidth, X);
                WordPieces.Clear();
                WordPiecesState.Clear();
                WordPiecesWidths.Clear();
                WordPiecesWidthsSum = 0;
            }
        }

        /// <summary>
        ///     Word-wraps a given piece of EggsML, assuming that it is linearly flowing text. Newline (<c>\n</c>) characters
        ///     can be used to split the text into multiple paragraphs. See remarks for the special meaning of <c>+...+</c>
        ///     and <c>&lt;...&gt;</c>.</summary>
        /// <typeparam name="TState">
        ///     The type of the text state that an EggsML can change, e.g. font or color.</typeparam>
        /// <param name="node">
        ///     The root node of the EggsML tree to word-wrap.</param>
        /// <param name="initialState">
        ///     The initial text state.</param>
        /// <param name="wrapWidth">
        ///     The maximum width at which to word-wrap. This width can be measured in any unit, as long as <paramref
        ///     name="measure"/> uses the same unit.</param>
        /// <param name="measure">
        ///     A delegate that measures the width of any piece of text.</param>
        /// <param name="render">
        ///     A delegate that is called whenever a piece of text is ready to be rendered.</param>
        /// <param name="advanceToNextLine">
        ///     A delegate that is called to advance to the next line.</param>
        /// <param name="nextState">
        ///     A delegate that determines how each EggsML tag character modifies the state (font, color etc.).</param>
        /// <returns>
        ///     The maximum width of the text.</returns>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item><description>
        ///             The <c>+...+</c> tag marks text that may not be broken by wrapping (effectively turning all spaces
        ///             into non-breaking spaces).</description></item>
        ///         <item><description>
        ///             The <c>&lt;...&gt;</c> tag marks a parameter to an immediately following tag. For example, if the
        ///             input EggsML contains <c>&lt;X&gt;{Foo}</c>, the text “X” will be passed as the parameter to <paramref
        ///             name="nextState"/> when the <c>{</c> tag is processed.</description></item></list></remarks>
        public static int WordWrap<TState>(EggsNode node, TState initialState, int wrapWidth,
            EggMeasure<TState> measure, EggRender<TState> render, EggNextLine<TState> advanceToNextLine, EggNextState<TState> nextState)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (wrapWidth <= 0)
                throw new ArgumentException("Wrap width must be greater than zero.", nameof(wrapWidth));
            var data = new EggWalkData<TState>
            {
                AtStartOfLine = true,
                WordPieces = new List<string>(),
                WordPiecesState = new List<TState>(),
                WordPiecesWidths = new List<int>(),
                WordPiecesWidthsSum = 0,
                Measure = measure,
                Render = render,
                AdvanceToNextLine = advanceToNextLine,
                NextState = nextState,
                X = 0,
                WrapWidth = wrapWidth,
                ActualWidth = 0
            };
            data.EggWalkWordWrap(node, initialState);
            return data.ActualWidth;
        }
    }

    /// <summary>Contains a node in the <see cref="EggsML"/> parse tree.</summary>
    public abstract class EggsNode
    {
        /// <summary>Returns the EggsML parse tree as XML.</summary>
        public abstract object ToXml();

        /// <summary>The index in the original string where this node starts.</summary>
        public int Index { get; protected set; }

        /// <summary>Determines whether this node contains any textual content.</summary>
        public abstract bool HasText { get; }

        /// <summary>
        ///     Gets a reference to the parent node of this node. The root node is the only one for which this property is
        ///     null.</summary>
        public EggsTag Parent { get; internal set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="index">
        ///     The index within the original string where this node starts.</param>
        public EggsNode(int index) { Index = index; }

        /// <summary>
        ///     Turns a list of child nodes into EggsML mark-up.</summary>
        /// <param name="children">
        ///     List of children to turn into mark-up.</param>
        /// <param name="tag">
        ///     If non-null, assumes we are directly inside a tag with the specified character, causing necessary escaping to
        ///     be performed.</param>
        /// <returns>
        ///     EggsML mark-up representing the same tree structure as this node.</returns>
        protected static string stringify(List<EggsNode> children, char? tag)
        {
            if (children == null || children.Count == 0)
                return "";

            var sb = new StringBuilder();

            for (int i = 0; i < children.Count; i++)
            {
                var childStr = children[i].ToString();
                if (string.IsNullOrEmpty(childStr))
                    continue;

                // If the item is a tag, and it is the same tag character as the current one, we need to escape it by tripling it
                if (sb.Length > 0 && childStr.Length > 0 && childStr[0] == sb[sb.Length - 1])
                    sb.Append('`');
                if (tag != null && children[i] is EggsTag && ((EggsTag) children[i]).Tag == tag && !EggsML.alwaysOpens(tag))
                    sb.Append(new string(tag.Value, 2));
                sb.Append(childStr);
            }
            return sb.ToString();
        }

        /// <summary>Gets the text of this node and/or sub-nodes concatenated into one string.</summary>
        public string ToString(bool excludeSyntax)
        {
            if (excludeSyntax)
            {
                var builder = new StringBuilder();
                textify(builder);
                return builder.ToString();
            }
            else
                return ToString();
        }

        internal abstract void textify(StringBuilder builder);

        /// <summary>
        ///     Generates a sequence of <see cref="ConsoleColoredString"/>s from an EggsML parse tree by word-wrapping the
        ///     output at a specified character width.</summary>
        /// <param name="wrapWidth">
        ///     The number of characters at which to word-wrap the output.</param>
        /// <param name="hangingIndent">
        ///     The number of spaces to add to each line except the first of each paragraph, thus creating a hanging
        ///     indentation.</param>
        /// <returns>
        ///     The sequence of <see cref="ConsoleColoredString"/>s generated from the EggsML parse tree.</returns>
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
        ///         tag.</para>
        ///     <para>
        ///         Additionally, the <c>+</c> tag can be used to suppress word-wrapping within a certain stretch of text. In
        ///         other words, the contents of a <c>+</c> tag are treated as if they were a single word. Use this in
        ///         preference to U+00A0 (no-break space) as it is more explicit and more future-compatible in case
        ///         hyphenation is ever implemented here.</para></remarks>
        public IEnumerable<ConsoleColoredString> ToConsoleColoredStringWordWrap(int wrapWidth, int hangingIndent = 0)
        {
            var results = new List<ConsoleColoredString> { ConsoleColoredString.Empty };
            EggsML.WordWrap(this, ConsoleColor.Gray, wrapWidth,
                (color, text) => text.Length,
                (color, text, width) => { results[results.Count - 1] += new ConsoleColoredString(text, color); },
                (color, newParagraph, indent) =>
                {
                    var s = newParagraph ? 0 : indent + hangingIndent;
                    results.Add(new ConsoleColoredString(new string(' ', s), color));
                    return s;
                },
                (color, tag, parameter) =>
                {
                    bool curLight = color >= ConsoleColor.DarkGray;
                    switch (tag)
                    {
                        case '~': color = curLight ? ConsoleColor.DarkGray : ConsoleColor.Black; break;
                        case '/': color = curLight ? ConsoleColor.Blue : ConsoleColor.DarkBlue; break;
                        case '$': color = curLight ? ConsoleColor.Green : ConsoleColor.DarkGreen; break;
                        case '&': color = curLight ? ConsoleColor.Cyan : ConsoleColor.DarkCyan; break;
                        case '_': color = curLight ? ConsoleColor.Red : ConsoleColor.DarkRed; break;
                        case '%': color = curLight ? ConsoleColor.Magenta : ConsoleColor.DarkMagenta; break;
                        case '^': color = curLight ? ConsoleColor.Yellow : ConsoleColor.DarkYellow; break;
                        case '=': color = ConsoleColor.DarkGray; break;
                        case '*': color = curLight ? color : (ConsoleColor) ((int) color + 8); break;
                    }
                    return (color, 0);
                });
            if (results.Last().Length == 0)
                results.RemoveAt(results.Count - 1);
            return results;
        }
    }

    /// <summary>
    ///     Represents a node in the <see cref="EggsML"/> parse tree that corresponds to an EggsML tag or the top-level node.</summary>
    public sealed class EggsTag : EggsNode
    {
        /// <summary>
        ///     Adds a new child node to this tag’s children.</summary>
        /// <param name="child">
        ///     The child node to add.</param>
        internal void Add(EggsNode child) { child.Parent = this; _children.Add(child); }

        /// <summary>The children of this node.</summary>
        public ReadOnlyCollection<EggsNode> Children { get { return _children.AsReadOnly(ref _childrenCache); } }
        private ReadOnlyCollection<EggsNode> _childrenCache;

        /// <summary>The underlying collection containing the children of this node.</summary>
        private List<EggsNode> _children;

        /// <summary>Determines whether this node contains any textual content.</summary>
        public override bool HasText { get { return _children.Any(child => child.HasText); } }

        /// <summary>The character used to open the tag (e.g. “[”), or null if this is the top-level node.</summary>
        public char? Tag { get; private set; }

        /// <summary>
        ///     Constructs a new EggsML parse-tree node that represents an EggsML tag.</summary>
        /// <param name="tag">
        ///     The character used to open the tag (e.g. '[').</param>
        /// <param name="index">
        ///     The index in the original string where this tag was opened.</param>
        public EggsTag(char? tag, int index) : base(index) { Tag = tag; _children = new List<EggsNode>(); }

        /// <summary>
        ///     Constructs a new top-level EggsML parse-tree node containing the specified sub-nodes.</summary>
        /// <param name="nodes">
        ///     The sub-nodes contained in the root node.</param>
        public EggsTag(IEnumerable<EggsNode> nodes) : base(0) { Tag = null; _children = nodes.ToList(); }

        /// <summary>
        ///     Constructs a new EggsML parse-tree node that represents an EggsML tag containing the specified sub-nodes.</summary>
        /// <param name="tag">
        ///     The character used to open the tag (e.g. '[').</param>
        /// <param name="nodes">
        ///     The sub-nodes contained in the root node.</param>
        public EggsTag(char? tag, IEnumerable<EggsNode> nodes) : base(0) { Tag = tag; _children = nodes.ToList(); }

        /// <summary>
        ///     Reconstructs the original EggsML that is represented by this node.</summary>
        /// <remarks>
        ///     This does not necessarily return the same EggsML that was originally parsed. For example, redundant uses of
        ///     the <c>`</c> character are removed.</remarks>
        public override string ToString()
        {
            if (_children.Count == 0)
                return Tag == null ? "" : EggsML.alwaysOpens(Tag) ? Tag.ToString() + EggsML.opposite(Tag) : Tag + "`" + Tag;

            var childrenStr = stringify(_children, Tag);
            return Tag == null
                ? childrenStr
                : childrenStr.StartsWith(Tag)
                    ? childrenStr.EndsWith(EggsML.opposite(Tag))
                        ? Tag + "`" + childrenStr + "`" + EggsML.opposite(Tag)
                        : Tag + "`" + childrenStr + EggsML.opposite(Tag)
                    : childrenStr.EndsWith(EggsML.opposite(Tag))
                        ? Tag + childrenStr + "`" + EggsML.opposite(Tag)
                        : Tag + childrenStr + EggsML.opposite(Tag);
        }

        /// <summary>Returns an XML representation of this EggsML node.</summary>
        public override object ToXml()
        {
            string tagName;
            switch (Tag)
            {
                case null: tagName = "root"; break;
                case '~': tagName = "tilde"; break;
                case '@': tagName = "at"; break;
                case '#': tagName = "hash"; break;
                case '$': tagName = "dollar"; break;
                case '%': tagName = "percent"; break;
                case '^': tagName = "hat"; break;
                case '&': tagName = "and"; break;
                case '*': tagName = "star"; break;
                case '_': tagName = "underscore"; break;
                case '=': tagName = "equals"; break;
                case '+': tagName = "plus"; break;
                case '/': tagName = "slash"; break;
                case '\\': tagName = "backslash"; break;
                case '[': tagName = "square"; break;
                case '{': tagName = "curly"; break;
                case '<': tagName = "angle"; break;
                case '|': tagName = "pipe"; break;
                default:
                    throw new InvalidOperationException("Unexpected tag character ‘{0}’.".Fmt(Tag));
            }
            return new XElement(tagName, _children.Select(child => child.ToXml()));
        }

        internal override void textify(StringBuilder builder)
        {
            foreach (var child in _children)
                child.textify(builder);
        }
    }

    /// <summary>Represents a node in the <see cref="EggsML"/> parse tree that corresponds to a piece of text.</summary>
    public sealed class EggsText : EggsNode
    {
        /// <summary>The text contained in this node.</summary>
        public string Text { get; private set; }

        /// <summary>
        ///     Constructs a new EggsML text node.</summary>
        /// <param name="text">
        ///     The text for this node to contain.</param>
        /// <param name="index">
        ///     The index in the original string where this text starts.</param>
        public EggsText(string text, int index = 0)
            : base(index)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text), "The 'text' for an EggsText node cannot be null.");
        }

        /// <summary>
        ///     Reconstructs the original EggsML that is represented by this node.</summary>
        /// <remarks>
        ///     This does not necessarily return the same EggsML that was originally parsed. For example, redundant uses of
        ///     the <c>`</c> character are removed.</remarks>
        public override string ToString() =>
            Text.Where(ch => EggsML.SpecialCharacters.Contains(ch) && ch != '"').Take(3).Count() >= 3
                ? string.Concat("\"", Text.Replace("\"", "\"\""), "\"")
                : new string(Text.SelectMany(ch => EggsML.SpecialCharacters.Contains(ch) ? new char[] { ch, ch } : new char[] { ch }).ToArray());

        /// <summary>Returns an XML representation of this EggsML node.</summary>
        public override object ToXml() => Text;

        /// <summary>Determines whether this node contains any textual content.</summary>
        public override bool HasText => Text != null && Text.Length > 0;

        internal override void textify(StringBuilder builder) { builder.Append(Text); }
    }

    /// <summary>Represents a parse error encountered by the <see cref="EggsML"/> parser.</summary>
    [Serializable]
    public sealed class EggsMLParseException : Exception
    {
        /// <summary>The character index into the original string where the error occurred.</summary>
        public int Index { get; private set; }

        /// <summary>The length of the text in the original string where the error occurred.</summary>
        public int Length { get; private set; }

        /// <summary>
        ///     The character index of an earlier position in the original string where the error started (e.g. the start of a
        ///     tag that is missing its end tag).</summary>
        public int? FirstIndex { get; private set; }

        /// <summary>
        ///     Constructor.</summary>
        /// <param name="message">
        ///     Message.</param>
        /// <param name="index">
        ///     The character index into the original string where the error occurred.</param>
        /// <param name="length">
        ///     The length of the text in the original string where the error occurred.</param>
        /// <param name="firstIndex">
        ///     The character index of an earlier position in the original string where the error started (e.g. the start of a
        ///     tag that is missing its end tag).</param>
        /// <param name="inner">
        ///     An inner exception to pass to the base Exception class.</param>
        public EggsMLParseException(string message, int index, int length, int? firstIndex = null, Exception inner = null) : base(message, inner) { Index = index; Length = length; FirstIndex = firstIndex; }
    }
}

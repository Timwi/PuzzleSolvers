using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RT.Util.Consoles
{
    /// <summary>Implements a <see cref="TextWriter"/> which outputs text to the console in a predetermined <see cref="ConsoleColor"/>.</summary>
    /// <remarks>Every time a string is output to the console, the current console foreground colour is remembered and returned to its previous value after the output.</remarks>
    public sealed class ColoredConsoleOut : TextWriter
    {
        private ConsoleColor _color;

        /// <summary>Constructor.</summary>
        /// <param name="color">Specifies the colour in which to output text to the console.</param>
        public ColoredConsoleOut(ConsoleColor color) { _color = color; }

        /// <summary><see cref="TextWriter"/> requires this to be overridden. This returns <see cref="System.Text.Encoding.UTF8"/>.</summary>
        public override Encoding Encoding { get { return Encoding.UTF8; } }

        /// <summary>Writes the specified character to the console.</summary>
        public override void Write(char value)
        {
            Write(value.ToString());
        }

        /// <summary>Writes the specified string to the console.</summary>
        public override void Write(string value)
        {
            var before = Console.ForegroundColor;
            Console.ForegroundColor = _color;
            Console.Write(value);
            Console.ForegroundColor = before;
        }

        /// <summary>Outputs a newline to the console.</summary>
        public override void WriteLine()
        {
            Console.WriteLine();
        }

        /// <summary>Outputs the specified text plus a newline to the console.</summary>
        public override void WriteLine(string str)
        {
            Write(str);
            Console.WriteLine();
        }
    }
}

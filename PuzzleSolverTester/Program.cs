using System;
using System.Reflection;
using NUnit.Direct;
using NUnit.Framework;
using RT.Util;

namespace PuzzleSolverTester
{
    class Program
    {
        static void Main()
        {
            var tests = new SudokuTests();
            foreach (var method in typeof(SudokuTests).GetMethods())
                if (method.GetCustomAttribute<TestAttribute>() != null)
                {
                    Console.WriteLine(method);
                    NUnitDirect.InvokeDirect(method, tests);
                }

            Console.WriteLine("Done. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}

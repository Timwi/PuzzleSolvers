using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RT.Util;

namespace PuzzleSolverTester
{
    class Program
    {
        static void Main()
        {
            foreach (var fixture in typeof(PuzzleTestFixture).Assembly.GetTypes().Where(t => typeof(PuzzleTestFixture).IsAssignableFrom(t) && !t.IsAbstract))
            {
                var tests = Activator.CreateInstance(fixture);
                foreach (var method in fixture.GetMethods())
                    if (method.GetCustomAttribute<TestMethodAttribute>() != null)
                    {
                        Console.WriteLine(method);
                        method.Invoke(tests, null);
                    }
            }

            Console.WriteLine("Done. Press Enter to exit.");
            Console.ReadLine();
        }
    }
}

using System;
using System.Collections.Generic;

namespace PuzzleSolvers.Exotic
{
    public class LittleSandwichConstraint : Constraint
    {
        public LittleSandwichConstraint(IEnumerable<int> affectedCells, int sum, int digit1 = 1, int digit2 = 9)
            : base(affectedCells)
        {
            Sum = sum;
            Digit1 = digit1;
            Digit2 = digit2;
        }

        public int Sum { get; private set; }
        public int Digit1 { get; private set; }
        public int Digit2 { get; private set; }

#warning Implement this
        public override ConstraintResult Process(SolverState state)
        {
            throw new NotImplementedException();
        }
    }
}

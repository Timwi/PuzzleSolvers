using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util;

namespace PuzzleSolvers
{
    /// <summary>Describes a composite constraint in which one of a set of constraints must be met.</summary>
    public class OrConstraint : Constraint
    {
        /// <summary>The set of constraints, of which at least one must be satisfied.</summary>
        public Constraint[] Subconstraints { get; private set; }

        /// <summary>Constructor.</summary>
        public OrConstraint(IEnumerable<Constraint> subconstraints) : base(subconstraints.SelectMany(c => c.AffectedCells).Distinct())
        {
            Subconstraints = subconstraints.ToArray();
            _canReevaluate = Subconstraints.Any(c => c.CanReevaluate);
        }

        /// <summary>Constructor.</summary>
        public OrConstraint(params Constraint[] subconstraints) : this((IEnumerable<Constraint>) subconstraints) { }

        private sealed class SolverStateImpl : SolverState
        {
            public SolverState Parent;
            public List<int>[] Available;

            public override void MarkImpossible(int cell, Func<int, bool> isImpossible)
            {
                Available[cell].RemoveAll(v => isImpossible(v));
            }

            public override void MustBe(int cell, int value)
            {
                var wasThere = Available[cell].Contains(value);
                Available[cell].Clear();
                if (wasThere)
                    Available[cell].Add(value);
            }

            public override int? this[int cell] => Parent[cell];
            public override void MarkImpossible(int cell, int value) => Available[cell].Remove(value);
            public override bool IsImpossible(int cell, int value) => !Available[cell].Contains(value) || Parent.IsImpossible(cell, value);
            public override int? LastPlacedCell => Parent.LastPlacedCell;
            public override int MinValue => Parent.MinValue;
            public override int MaxValue => Parent.MaxValue;
            public override int GridSize => Parent.GridSize;
            public override bool AllSame<T>(int cell, Func<int, T> predicate, out T result) => throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override bool CanReevaluate => _canReevaluate;
        private readonly bool _canReevaluate;

        /// <inheritdoc/>
        public override ConstraintResult Process(SolverState state)
        {
            if (state.LastPlacedCell != null && !AffectedCells.Contains(state.LastPlacedCell.Value))
                return null;

            var innerAvailables = new List<int>[Subconstraints.Length][];
            List<Constraint> newSubconstraints = null;
            for (var sc = 0; sc < Subconstraints.Length; sc++)
            {
                var substate = new SolverStateImpl { Parent = state, Available = Ut.NewArray(state.GridSize, cell => state.Possible(cell).ToList()) };
                var result = Subconstraints[sc].Process(substate);
                innerAvailables[sc] = substate.Available;

                if (result is ConstraintReplace)
                    throw new NotImplementedException("The OrConstraint does not support subconstraints that replace themselves with new constraints.");
                else if (result is ConstraintViolation)
                    newSubconstraints ??= Subconstraints.Take(sc).ToList();
                else
                    newSubconstraints?.Add(Subconstraints[sc]);
            }

            foreach (var cell in AffectedCells)
                state.MarkImpossible(cell, value => innerAvailables.All(av => av == null || !av[cell].Contains(value)));

            if (newSubconstraints != null)
                return new OrConstraint(newSubconstraints);
            return null;
        }
    }
}

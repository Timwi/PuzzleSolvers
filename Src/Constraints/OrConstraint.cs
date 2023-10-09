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
            public bool[][] Takens;

            public override void MarkImpossible(int cell, Func<int, bool> isImpossible)
            {
                for (var value = Parent.MinValue; value <= Parent.MaxValue; value++)
                    if (isImpossible(value))
                        Takens[cell][value - Parent.MinValue] = true;
            }

            public override void MustBe(int cell, int value)
            {
                for (var otherValue = Parent.MinValue; otherValue <= Parent.MaxValue; otherValue++)
                    if (otherValue != value)
                        Takens[cell][otherValue - Parent.MinValue] = true;
            }

            public override int? this[int cell] => Parent[cell];
            public override void MarkImpossible(int cell, int value) { Takens[cell][value - Parent.MinValue] = true; }
            public override bool IsImpossible(int cell, int value) => Takens[cell][value - Parent.MinValue] || Parent.IsImpossible(cell, value);
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

            var innerTakens = new bool[Subconstraints.Length][][];
            List<Constraint> newSubconstraints = null;
            for (var sc = 0; sc < Subconstraints.Length; sc++)
            {
                var substate = new SolverStateImpl { Parent = state, Takens = Ut.NewArray<bool>(state.GridSize, state.MaxValue - state.MinValue + 1) };
                var result = Subconstraints[sc].Process(substate);
                innerTakens[sc] = substate.Takens;

                if (result is ConstraintReplace)
                    throw new NotImplementedException("The OrConstraint does not support subconstraints that replace themselves with new constraints.");
                else if (result is ConstraintViolation)
                {
                    if (newSubconstraints == null)
                        newSubconstraints = new List<Constraint>(Subconstraints.Take(sc));
                }
                else if (newSubconstraints != null)
                    newSubconstraints.Add(Subconstraints[sc]);
            }

            foreach (var cell in AffectedCells)
                state.MarkImpossible(cell, value => innerTakens.All(taken => taken == null || taken[cell][value - state.MinValue]));

            if (newSubconstraints != null)
                return new OrConstraint(newSubconstraints);
            return null;
        }
    }
}

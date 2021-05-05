using System;
using System.Collections.Generic;
using System.Linq;
using RT.Util.ExtensionMethods;

namespace RT.Util
{
    partial class Ut
    {
        /// <summary>
        ///     Given a set of values and a function that returns true when given this set, will efficiently remove items from
        ///     this set which are not essential for making the function return true. The relative order of items is
        ///     preserved. This method cannot generally guarantee that the result is optimal, but for some types of functions
        ///     the result will be guaranteed optimal.</summary>
        /// <typeparam name="T">
        ///     Type of the values in the set.</typeparam>
        /// <param name="items">
        ///     The set of items to reduce.</param>
        /// <param name="test">
        ///     The function that examines the set. Must always return the same value for the same set.</param>
        /// <param name="breadthFirst">
        ///     A value selecting a breadth-first or a depth-first approach. Depth-first is best at quickly locating a single
        ///     value which will be present in the final required set. Breadth-first is best at quickly placing a lower bound
        ///     on the total number of individual items in the required set.</param>
        /// <param name="skipConsistencyTest">
        ///     When the function is particularly slow, you might want to set this to true to disable calls which are not
        ///     required to reduce the set and are only there to ensure that the function behaves consistently.</param>
        /// <returns>
        ///     A hopefully smaller set of values that still causes the function to return true.</returns>
        public static IEnumerable<T> ReduceRequiredSet<T>(IEnumerable<T> items, Func<ReduceRequiredSetState<T>, bool> test, bool breadthFirst = false, bool skipConsistencyTest = false)
        {
            var state = new ReduceRequiredSetStateInternal<T>(items);

            if (!skipConsistencyTest)
                if (!test(state))
                    throw new Exception("The function does not return true for the original set.");

            while (state.AnyPartitions)
            {
                if (!skipConsistencyTest)
                    if (!test(state))
                        throw new Exception("The function is not consistently returning the same value for the same set, or there is an internal error in this algorithm.");

                var rangeToSplit = breadthFirst ? state.LargestRange : state.SmallestRange;
                int mid = (rangeToSplit.from + rangeToSplit.to) / 2;
                var split1 = (rangeToSplit.from, to: mid);
                var split2 = (from: mid + 1, rangeToSplit.to);

                state.ApplyTemporarySplit(rangeToSplit, split1);
                if (test(state))
                {
                    state.SolidifyTemporarySplit();
                    continue;
                }
                state.ApplyTemporarySplit(rangeToSplit, split2);
                if (test(state))
                {
                    state.SolidifyTemporarySplit();
                    continue;
                }
                state.ResetTemporarySplit();
                state.RemoveRange(rangeToSplit);
                state.AddRange(split1);
                state.AddRange(split2);
            }

            state.ResetTemporarySplit();
            return state.SetToTest;
        }

        /// <summary>
        ///     Encapsulates the state of the <see cref="ReduceRequiredSet"/> algorithm and exposes statistics about it.</summary>
        public abstract class ReduceRequiredSetState<T>
        {
            /// <summary>Internal; do not use.</summary>
            protected List<(int from, int to)> Ranges;
            /// <summary>Internal; do not use.</summary>
            protected List<T> Items;
            /// <summary>Internal; do not use.</summary>
            protected (int from, int to)? ExcludedRange, IncludedRange;

            /// <summary>
            ///     Enumerates every item that is known to be in the final required set. "Definitely" doesn't mean that there
            ///     exists no subset resulting in "true" without these members. Rather, it means that the algorithm will
            ///     definitely return these values, and maybe some others too.</summary>
            public IEnumerable<T> DefinitelyRequired { get { return Ranges.Where(r => r.from == r.to).Select(r => Items[r.from]); } }
            /// <summary>
            ///     Gets the current number of partitions containing uncertain items. The more of these, the slower the
            ///     algorithm will converge from here onwards.</summary>
            public int PartitionsCount { get { return Ranges.Count - Ranges.Count(r => r.from == r.to); } }
            /// <summary>
            ///     Gets the number of items in the smallest partition. This is the value that is halved upon a successful
            ///     depth-first iteration.</summary>
            public int SmallestPartitionSize { get { return Ranges.Where(r => r.from != r.to).Min(r => r.to - r.from + 1); } }
            /// <summary>
            ///     Gets the number of items in the largest partition. This is the value that is halved upon a successful
            ///     breadth-first iteration.</summary>
            public int LargestPartitionSize { get { return Ranges.Max(r => r.to - r.from + 1); } }
            /// <summary>Gets the total number of items about which the algorithm is currently undecided.</summary>
            public int ItemsRemaining { get { return Ranges.Where(r => r.from != r.to).Sum(r => r.to - r.from + 1); } }

            /// <summary>Gets the set of items for which the function should be evaluated in the current step.</summary>
            public IEnumerable<T> SetToTest
            {
                get
                {
                    var ranges = Ranges.AsEnumerable();
                    if (ExcludedRange != null)
                        ranges = ranges.Where(r => !r.Equals(ExcludedRange.Value));
                    if (IncludedRange != null)
                        ranges = ranges.Concat(IncludedRange.Value);
                    return ranges
                        .SelectMany(range => Enumerable.Range(range.from, range.to - range.from + 1))
                        .Order()
                        .Select(i => Items[i]);
                }
            }
        }

        internal sealed class ReduceRequiredSetStateInternal<T> : ReduceRequiredSetState<T>
        {
            public ReduceRequiredSetStateInternal(IEnumerable<T> items)
            {
                Items = items.ToList();
                Ranges = new List<(int from, int to)>();
                Ranges.Add((0, Items.Count - 1));
            }

            public bool AnyPartitions { get { return Ranges.Any(r => r.from != r.to); } }
            public (int from, int to) LargestRange { get { return Ranges.MaxElement(t => t.to - t.from); } }
            public (int from, int to) SmallestRange { get { return Ranges.Where(r => r.from != r.to).MinElement(t => t.to - t.from); } }

            public void AddRange((int from, int to) range) { Ranges.Add(range); }
            public void RemoveRange((int from, int to) range) { if (!Ranges.Remove(range)) throw new InvalidOperationException("Ut.ReduceRequiredSet has a bug. Code: 826432"); }

            public void ResetTemporarySplit()
            {
                ExcludedRange = IncludedRange = null;
            }
            public void ApplyTemporarySplit((int from, int to) rangeToSplit, (int from, int to) splitRange)
            {
                ExcludedRange = rangeToSplit;
                IncludedRange = splitRange;
            }

            public void SolidifyTemporarySplit()
            {
                RemoveRange(ExcludedRange.Value);
                AddRange(IncludedRange.Value);
                ResetTemporarySplit();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RT.Util
{
    partial class Ut
    {
        /// <summary>
        ///     Runs all of the specified actions in parallel, each in a thread of its own.</summary>
        /// <param name="actions">
        ///     Actions to run.</param>
        public static void Parallel(params Action<int>[] actions)
        {
            Parallel(int.MaxValue, actions);
        }

        /// <summary>
        ///     Runs the specified actions partly in parallel by using no more than the specified maximum number of threads.</summary>
        /// <param name="maxSimultaneous">
        ///     Maximum number of concurrent threads allowed.</param>
        /// <param name="actions">
        ///     Actions to run. The parameter is the index of the thread running it.</param>
        public static void Parallel(int maxSimultaneous, params Action<int>[] actions)
        {
            if (maxSimultaneous < 1)
                throw new ArgumentException("maxSimultaneous cannot be zero or negative.", nameof(maxSimultaneous));

            if (actions == null || actions.Length == 0)
                return;

            var exceptionLock = new object();
            Exception exception = null;
#if !DEBUG
            actions = actions.Select(act => new Action<int>(i =>
            {
                try
                {
                    act(i);
                }
                catch (Exception e)
                {
                    lock (exceptionLock)
                        exception = e;
                }
            })).ToArray();
#endif

            var threads = new List<Thread>(actions.Length);

            if (maxSimultaneous > actions.Length)
            {
                // Just fire them all off
                for (var idFor = 0; idFor < actions.Length; idFor++)
                {
                    var id = idFor; // capture the correct value for the lambda
                    var thread = new Thread(new ThreadStart(() => actions[id](id)));
                    thread.Start();
                    threads.Add(thread);
                }
            }
            else
            {
                // Create a queue and have each thread take actions from the queue as needed
                var todo = new Queue<Action<int>>(actions);
                for (int idFor = 0; idFor < maxSimultaneous; idFor++)
                {
                    var id = idFor; // capture the correct value for the lambda
                    var thread = new Thread(() =>
                    {
                        while (true)
                        {
                            Action<int> action;
                            lock (todo)
                            {
                                if (todo.Count == 0)
                                    return;
                                action = todo.Dequeue();
                            }
                            action(id);
                        }
                    });
                    thread.Start();
                    threads.Add(thread);
                }
            }
            foreach (var thread in threads)
            {
                thread.Join();
                lock (exceptionLock)
                    if (exception != null)
                        throw new Exception(exception.Message, exception);
            }
        }

        /// <summary>
        ///     Runs the specified action in parallel for each item in the input collection.</summary>
        /// <typeparam name="T">
        ///     Type of the items in the collection.</typeparam>
        /// <param name="items">
        ///     Input collection of items to pass to the action.</param>
        /// <param name="action">
        ///     Action to run for each element.</param>
        public static void ParallelForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            ParallelForEach(items, int.MaxValue, action);
        }

        /// <summary>
        ///     Runs the specified action in parallel for each item in the input collection.</summary>
        /// <typeparam name="T">
        ///     Type of the items in the collection.</typeparam>
        /// <param name="items">
        ///     Input collection of items to pass to the action.</param>
        /// <param name="action">
        ///     Action to run for each element. The second parameter is the index of the thread running it.</param>
        public static void ParallelForEach<T>(this IEnumerable<T> items, Action<T, int> action)
        {
            ParallelForEach(items, int.MaxValue, action);
        }

        /// <summary>
        ///     Runs the specified action in parallel for each item in the input collection, using no more than the specified
        ///     maximum number of threads.</summary>
        /// <typeparam name="T">
        ///     Type of the items in the collection.</typeparam>
        /// <param name="items">
        ///     Input collection of items to pass to the action.</param>
        /// <param name="maxSimultaneous">
        ///     Maximum number of concurrent threads allowed.</param>
        /// <param name="action">
        ///     Action to run for each element.</param>
        public static void ParallelForEach<T>(this IEnumerable<T> items, int maxSimultaneous, Action<T> action)
        {
            var actions = items.Select(item => new Action<int>(i => action(item))).ToArray();
            Parallel(maxSimultaneous, actions);
        }

        /// <summary>
        ///     Runs the specified action in parallel for each item in the input collection, using no more than the specified
        ///     maximum number of threads.</summary>
        /// <typeparam name="T">
        ///     Type of the items in the collection.</typeparam>
        /// <param name="items">
        ///     Input collection of items to pass to the action.</param>
        /// <param name="maxSimultaneous">
        ///     Maximum number of concurrent threads allowed.</param>
        /// <param name="action">
        ///     Action to run for each element. The second parameter is the index of the thread running it.</param>
        public static void ParallelForEach<T>(this IEnumerable<T> items, int maxSimultaneous, Action<T, int> action)
        {
            var actions = items.Select(item => new Action<int>(i => action(item, i))).ToArray();
            Parallel(maxSimultaneous, actions);
        }

        /// <summary>
        ///     Runs the specified function in parallel for each item in the input collection and returns a collection
        ///     containing the concatenation of all the results of the function calls.</summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="items"/>.</typeparam>
        /// <typeparam name="TResult">
        ///     The type of the elements of the sequence returned by <paramref name="selector"/>.</typeparam>
        /// <param name="items">
        ///     Input collection of items to pass to the function.</param>
        /// <param name="selector">
        ///     Function that returns a collection for each input item.</param>
        public static IEnumerable<TResult> ParallelSelectMany<TSource, TResult>(this IEnumerable<TSource> items, Func<TSource, IEnumerable<TResult>> selector)
        {
            return ParallelSelectMany(items, int.MaxValue, selector);
        }

        /// <summary>
        ///     Runs the specified function in parallel for each item in the input collection and returns a collection
        ///     containing the concatenation of all the results of the function calls.</summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="items"/>.</typeparam>
        /// <typeparam name="TResult">
        ///     The type of the elements of the sequence returned by <paramref name="selector"/>.</typeparam>
        /// <param name="items">
        ///     Input collection of items to pass to the function.</param>
        /// <param name="maxSimultaneous">
        ///     Maximum number of concurrent threads allowed.</param>
        /// <param name="selector">
        ///     Function that returns a collection for each input item.</param>
        public static IEnumerable<TResult> ParallelSelectMany<TSource, TResult>(this IEnumerable<TSource> items, int maxSimultaneous, Func<TSource, IEnumerable<TResult>> selector)
        {
            var list = new List<TResult>();
            items.ParallelForEach(maxSimultaneous, source =>
            {
                var result = selector(source).ToList();
                lock (list)
                    list.AddRange(result);
            });
            return list;
        }

        /// <summary>
        ///     Runs the specified function in parallel for each item in the input collection and returns a collection
        ///     containing the results of the function calls.</summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="items"/>.</typeparam>
        /// <typeparam name="TResult">
        ///     The type of the elements returned by <paramref name="selector"/>.</typeparam>
        /// <param name="items">
        ///     Input collection of items to pass to the function.</param>
        /// <param name="selector">
        ///     Function that returns a result object for each input item.</param>
        public static IEnumerable<TResult> ParallelSelect<TSource, TResult>(this IEnumerable<TSource> items, Func<TSource, TResult> selector)
        {
            return ParallelSelect(items, int.MaxValue, selector);
        }

        /// <summary>
        ///     Runs the specified function in parallel for each item in the input collection and returns a collection
        ///     containing the results of the function calls.</summary>
        /// <typeparam name="TSource">
        ///     The type of the elements of <paramref name="items"/>.</typeparam>
        /// <typeparam name="TResult">
        ///     The type of the elements returned by <paramref name="selector"/>.</typeparam>
        /// <param name="items">
        ///     Input collection of items to pass to the function.</param>
        /// <param name="maxSimultaneous">
        ///     Maximum number of concurrent threads allowed.</param>
        /// <param name="selector">
        ///     Function that returns a result object for each input item.</param>
        public static IEnumerable<TResult> ParallelSelect<TSource, TResult>(this IEnumerable<TSource> items, int maxSimultaneous, Func<TSource, TResult> selector)
        {
            var list = new List<TResult>();
            items.ParallelForEach(maxSimultaneous, source =>
            {
                var result = selector(source);
                lock (list)
                    list.Add(result);
            });
            return list;
        }
    }
}

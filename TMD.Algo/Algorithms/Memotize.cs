#region License

/*
Copyright (c) 2010, the TMD.Algo authors.
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of TMD.Algo nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#endregion

using System;
using System.Collections.Generic;

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Provides methods for memotizing functions.
    /// </summary>
    public static class Memotize
    {
        /// <summary>
        /// Creates a memotized wrapper for a function.
        /// If the function is recursive, consider using <see cref="CreateMemotizedRecursive{TState, TResult}"/>.
        /// Input function must be state-free.
        /// Uses a dictionary as storage.
        /// </summary>
        /// <typeparam name="TState">
        /// Input type. Must have a useful default equality comparer.
        /// </typeparam>
        /// <typeparam name="TResult">
        /// Result type.
        /// </typeparam>
        /// <param name="inputFunction">
        /// Non-memotized function which maps input to results.
        /// </param>
        /// <returns>
        /// A memotized version of the input function.
        /// </returns>
        public static Func<TState, TResult> CreateMemotized<TState, TResult>(this Func<TState, TResult> inputFunction)
            where TState : notnull
        {
            Dictionary<TState, TResult> lookup = new Dictionary<TState, TResult>();
            return input =>
            {
                if (lookup.TryGetValue(input, out TResult result))
                    return result;
                result = inputFunction(input);
                lookup[input] = result;
                return result;
            };
        }

        /// <summary>
        /// Creates a memotized wrapper for a two-argument function.
        /// Uses a dictionary as storage.
        /// </summary>
        /// <typeparam name="T1">First argument type.</typeparam>
        /// <typeparam name="T2">Second argument type.</typeparam>
        /// <typeparam name="TResult">Result type.</typeparam>
        /// <param name="inputFunction">Non-memotized function mapping (T1, T2) to results.</param>
        /// <returns>A memotized version of the input function.</returns>
        public static Func<T1, T2, TResult> CreateMemotized<T1, T2, TResult>(this Func<T1, T2, TResult> inputFunction)
            where T1 : notnull
            where T2 : notnull
        {
            Dictionary<(T1, T2), TResult> lookup = new Dictionary<(T1, T2), TResult>();
            return (arg1, arg2) =>
            {
                var key = (arg1, arg2);
                if (lookup.TryGetValue(key, out TResult result))
                    return result;
                result = inputFunction(arg1, arg2);
                lookup[key] = result;
                return result;
            };
        }

        /// <summary>
        /// Creates a memotized wrapper for a function.
        /// Input function must be state-free.
        /// Uses a flat array as storage.
        /// </summary>
        /// <typeparam name="TResult">
        /// Result type.
        /// </typeparam>
        /// <param name="inputFunction">
        /// Non-memotized function which maps input to results.
        /// </param>
        /// <param name="min">
        /// Minimum possible input value.
        /// </param>
        /// <param name="max">
        /// Maximum possible input value.
        /// </param>
        /// <returns>
        /// A memotized version of the input function.
        /// </returns>
        public static Func<long, TResult> CreateMemotized<TResult>(this Func<long, TResult> inputFunction, long min, long max)
        {
            if (max < min) throw new ArgumentException("max must be greater than or equal to min.", nameof(max));
            long totalSize = max - min + 1;
            if (totalSize > 50_000_000)
                throw new ArgumentException("Total state space exceeds maximum supported array size.", nameof(max));

            int size = (int)totalSize;
            bool[] set = new bool[size];
            TResult[] lookup = new TResult[size];
            return input =>
            {
                int loc = (int)(input - min);
                if (set[loc])
                    return lookup[loc];
                TResult result = inputFunction(input);
                lookup[loc] = result;
                set[loc] = true;
                return result;
            };
        }

        /// <summary>
        /// Creates a memotized wrapper for a two-argument function bounded by integer ranges.
        /// Uses a 2D flat array as storage for O(1) lookups.
        /// </summary>
        public static Func<int, int, TResult> CreateMemotized<TResult>(
            this Func<int, int, TResult> inputFunction,
            int min1, int max1,
            int min2, int max2)
        {
            if (max1 < min1) throw new ArgumentException("max1 must be greater than or equal to min1.", nameof(max1));
            if (max2 < min2) throw new ArgumentException("max2 must be greater than or equal to min2.", nameof(max2));
            long range1 = (long)max1 - min1 + 1;
            long range2 = (long)max2 - min2 + 1;
            long totalSize = range1 * range2;
            if (totalSize > 50_000_000)
                throw new ArgumentException("Total state space exceeds maximum supported array size.", nameof(max2));

            int size = (int)totalSize;
            bool[] set = new bool[size];
            TResult[] lookup = new TResult[size];
            int stride = (int)range2;
            return (arg1, arg2) =>
            {
                int loc = (arg1 - min1) * stride + (arg2 - min2);
                if (set[loc])
                    return lookup[loc];
                TResult result = inputFunction(arg1, arg2);
                lookup[loc] = result;
                set[loc] = true;
                return result;
            };
        }

        /// <summary>
        /// Creates a memotized recursive function.
        /// The recursive function receives the memotized delegate as its first argument.
        /// Uses a dictionary as storage.
        /// </summary>
        /// <typeparam name="TState">Input state type.</typeparam>
        /// <typeparam name="TResult">Result type.</typeparam>
        /// <param name="recursiveFunction">
        /// The recursive function receiving the memotized recurse delegate and the input argument.
        /// </param>
        /// <returns>A memotized recursive function.</returns>
        public static Func<TState, TResult> CreateMemotizedRecursive<TState, TResult>(
            Func<Func<TState, TResult>, TState, TResult> recursiveFunction)
            where TState : notnull
        {
            Dictionary<TState, TResult> lookup = new Dictionary<TState, TResult>();
            Func<TState, TResult> self = null!;
            self = input =>
            {
                if (lookup.TryGetValue(input, out TResult result))
                    return result;
                result = recursiveFunction(self, input);
                lookup[input] = result;
                return result;
            };
            return self;
        }

        /// <summary>
        /// Creates a memotized recursive function with two arguments.
        /// The recursive function receives the memotized delegate as its first argument.
        /// Uses a dictionary as storage.
        /// </summary>
        /// <typeparam name="T1">First argument type.</typeparam>
        /// <typeparam name="T2">Second argument type.</typeparam>
        /// <typeparam name="TResult">Result type.</typeparam>
        /// <param name="recursiveFunction">
        /// The recursive function receiving the memotized recurse delegate and the two arguments.
        /// </param>
        /// <returns>A memotized recursive function.</returns>
        public static Func<T1, T2, TResult> CreateMemotizedRecursive<T1, T2, TResult>(
            Func<Func<T1, T2, TResult>, T1, T2, TResult> recursiveFunction)
            where T1 : notnull
            where T2 : notnull
        {
            Dictionary<(T1, T2), TResult> lookup = new Dictionary<(T1, T2), TResult>();
            Func<T1, T2, TResult> self = null!;
            self = (arg1, arg2) =>
            {
                var key = (arg1, arg2);
                if (lookup.TryGetValue(key, out TResult result))
                    return result;
                result = recursiveFunction(self, arg1, arg2);
                lookup[key] = result;
                return result;
            };
            return self;
        }

        /// <summary>
        /// Creates a memotized recursive function bounded by an integer range.
        /// Uses a flat array as storage.
        /// </summary>
        /// <typeparam name="TResult">Result type.</typeparam>
        /// <param name="min">Minimum possible input value (inclusive).</param>
        /// <param name="max">Maximum possible input value (inclusive).</param>
        /// <param name="recursiveFunction">
        /// The recursive function receiving the memotized recurse delegate and the input argument.
        /// </param>
        /// <returns>A memotized recursive function.</returns>
        public static Func<long, TResult> CreateMemotizedRecursive<TResult>(
            long min, long max,
            Func<Func<long, TResult>, long, TResult> recursiveFunction)
        {
            if (max < min) throw new ArgumentException("max must be greater than or equal to min.", nameof(max));
            long totalSize = max - min + 1;
            if (totalSize > 50_000_000)
                throw new ArgumentException("Total state space exceeds maximum supported array size.", nameof(max));

            int size = (int)totalSize;
            bool[] set = new bool[size];
            TResult[] lookup = new TResult[size];
            Func<long, TResult> self = null!;
            self = input =>
            {
                int loc = (int)(input - min);
                if (set[loc])
                    return lookup[loc];
                TResult result = recursiveFunction(self, input);
                lookup[loc] = result;
                set[loc] = true;
                return result;
            };
            return self;
        }

        /// <summary>
        /// Creates a memotized recursive function for two integer arguments bounded by given ranges.
        /// Uses a 2D flat array as storage for O(1) lookups.
        /// </summary>
        /// <typeparam name="TResult">Result type.</typeparam>
        /// <param name="min1">Minimum value for the first argument (inclusive).</param>
        /// <param name="max1">Maximum value for the first argument (inclusive).</param>
        /// <param name="min2">Minimum value for the second argument (inclusive).</param>
        /// <param name="max2">Maximum value for the second argument (inclusive).</param>
        /// <param name="recursiveFunction">
        /// The recursive function receiving the memotized recurse delegate and the two state arguments.
        /// </param>
        /// <returns>A memotized recursive function.</returns>
        public static Func<int, int, TResult> CreateMemotizedRecursive<TResult>(
            int min1, int max1,
            int min2, int max2,
            Func<Func<int, int, TResult>, int, int, TResult> recursiveFunction)
        {
            if (max1 < min1) throw new ArgumentException("max1 must be greater than or equal to min1.", nameof(max1));
            if (max2 < min2) throw new ArgumentException("max2 must be greater than or equal to min2.", nameof(max2));
            long range1 = (long)max1 - min1 + 1;
            long range2 = (long)max2 - min2 + 1;
            long totalSize = range1 * range2;
            if (totalSize > 50_000_000)
                throw new ArgumentException("Total state space exceeds maximum supported array size.", nameof(max2));

            int size = (int)totalSize;
            bool[] set = new bool[size];
            TResult[] lookup = new TResult[size];
            int stride = (int)range2;
            Func<int, int, TResult> self = null!;
            self = (arg1, arg2) =>
            {
                int loc = (arg1 - min1) * stride + (arg2 - min2);
                if (set[loc])
                    return lookup[loc];
                TResult result = recursiveFunction(self, arg1, arg2);
                lookup[loc] = result;
                set[loc] = true;
                return result;
            };
            return self;
        }
    }
}
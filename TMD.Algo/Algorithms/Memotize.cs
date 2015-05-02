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
        /// Creates a memotized wrapper for a function.  If the function is recursive, it must define its recursion in terms of the output of this function, not itself.
        /// Input function must be state-free.
        /// Uses a dictionary as storage.
        /// </summary>
        /// <typeparam name="TState">
        /// Input type.  Must have a useful default equality comparer.
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
        {
            Dictionary<TState, TResult> lookup = new Dictionary<TState, TResult>();
            return input =>
            {
                TResult result;
                if (lookup.TryGetValue(input, out result))
                    return result;
                result = inputFunction(input);
                lookup[input] = result;
                return result;
            };
        }

        /// <summary>
        /// Creates a memotized wrapper for a function.  If the function is recursive, it must define its recursion in terms of the output of this function, not itself.
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
            bool[] set = new bool[max - min + 1];
            TResult[] lookup = new TResult[max - min + 1];
            return input =>
            {
                long loc = input - min;
                if (set[loc])
                    return lookup[loc];
                lookup[loc] = inputFunction(input);
                set[loc] = true;
                return lookup[loc];
            };
        }
    }
}
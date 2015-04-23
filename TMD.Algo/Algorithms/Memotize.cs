﻿#region License
/*
Copyright (c) 2010, Gareth Pearce (www.themissingdocs.net)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of the www.themissingdocs.net nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// <typeparam name="T">
        /// Input type.
        /// </typeparam>
        /// <typeparam name="K">
        /// Result type.
        /// </typeparam>
        /// <param name="inputFunction">
        /// Non-memotized function which maps input to results.
        /// </param>
        /// <returns>
        /// A memotized version of the input function.
        /// </returns>
        public static Func<T, K> CreateMemotized<T, K>(this Func<T, K> inputFunction)
        {
            Dictionary<T, K> lookup = new Dictionary<T, K>();
            return delegate(T input)
            {
                K result; 
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
        /// <typeparam name="K">
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
        public static Func<long, K> CreateMemotized<K>(this Func<long, K> inputFunction, long min, long max)
        {
            bool[] set = new bool[max - min + 1];
            K[] lookup = new K[max - min + 1];
            return delegate(long input)
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
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
    /// Algorithms which rely on a sequence containing a pattern.
    /// </summary>
    public static class Pattern
    {
        /// <summary>
        /// Determines the sum of the first 'sumTo' values in a sequence containing a pattern.
        /// Runs in O(n) where n is the length of the pattern cycle plus any lead-in.
        /// </summary>
        /// <typeparam name="T">
        /// Type of element representing the internal state driving the pattern.
        /// </typeparam>
        /// <param name="first">
        /// Pattern seed.
        /// </param>
        /// <param name="generatorWithValue">
        /// Function which maps state to valuation of that state and the next state.
        /// </param>
        /// <param name="sumTo">
        /// Number of entries to sum.
        /// </param>
        /// <returns>
        /// The sum of the first 'sumTo' entries.
        /// </returns>
        public static double SumPattern<T>(T first, Func<T, Tuple<double, T>> generatorWithValue, long sumTo)
            where T : notnull
        {
            Dictionary<T, double> values = new Dictionary<T, double>();
            Dictionary<T, int> indicies = new Dictionary<T, int>();
            List<T> seen = new List<T>();
            T current = first;
            while (seen.Count < sumTo)
            {
                if (indicies.ContainsKey(current))
                    break;
                var res = generatorWithValue(current);
                double value = res.Item1;
                values[current] = value;
                indicies[current] = seen.Count;
                seen.Add(current);
                current = res.Item2;
            }
            double sumSoFar = 0.0;
            for (int i = 0; i < seen.Count; i++)
            {
                sumSoFar += values[seen[i]];
            }
            if (seen.Count == sumTo)
                return sumSoFar;
            int pos = indicies[current];
            long length = seen.Count - pos;
            long loops = (sumTo - seen.Count)/length;
            double loopSum = 0.0;
            for (int i = pos; i < seen.Count; i++)
            {
                loopSum += values[seen[i]];
            }
            sumSoFar += loops*loopSum;
            long leftOvers = (sumTo - seen.Count)%length;
            for (int i = 0; i < leftOvers; i++)
            {
                sumSoFar += values[seen[i + pos]];
            }
            return sumSoFar;
        }

        /// <summary>
        /// Determines the 'findAt' value in a sequence containing a pattern.
        /// Runs in O(n) where n is the length of the pattern cycle plus any lead-in.
        /// </summary>
        /// <typeparam name="T">
        /// Type of element representing the internal state driving the pattern.
        /// </typeparam>
        /// <param name="first">
        /// Pattern seed.
        /// </param>
        /// <param name="generator">
        /// Function which maps state to the next state.
        /// </param>
        /// <param name="findAt">
        /// Number of the entry to find. 0 based.
        /// </param>
        /// <returns>
        /// The value at the 'findAt' index in the pattern.
        /// </returns>
        public static T FindInPattern<T>(T first, Func<T, T> generator, long findAt)
            where T : notnull
        {
            Dictionary<T, int> indicies = new Dictionary<T, int>();
            List<T> seen = new List<T>();
            T current = first;
            while (seen.Count < findAt)
            {
                if (indicies.ContainsKey(current))
                    break;
                T next = generator(current);
                indicies[current] = seen.Count;
                seen.Add(current);
                current = next;
            }
            if (seen.Count == findAt)
                return current;
            int pos = indicies[current];
            long length = seen.Count - pos;
            int leftOvers = (int)((findAt - seen.Count)%length);
            return seen[pos + leftOvers];
        }

        /// <summary>
        /// Computes the Knuth-Morris-Pratt (KMP) prefix function <c>pi</c> for <paramref name="pattern"/> in O(n) time,
        /// where <c>pi[i]</c> is the length of the longest proper prefix of <c>pattern[0..i]</c> that is also a suffix.
        /// </summary>
        /// <param name="pattern">Input character span.</param>
        /// <returns>The prefix function array of length <c>pattern.Length</c>.</returns>
        public static int[] ComputePrefixFunction(ReadOnlySpan<char> pattern)
        {
            return ComputePrefixFunction<char>(pattern);
        }

        /// <summary>
        /// Computes the Knuth-Morris-Pratt (KMP) prefix function <c>pi</c> for <paramref name="pattern"/> in O(n) time,
        /// where <c>pi[i]</c> is the length of the longest proper prefix of <c>pattern[0..i]</c> that is also a suffix.
        /// </summary>
        /// <typeparam name="T">Element type supporting equality comparison.</typeparam>
        /// <param name="pattern">Input sequence span.</param>
        /// <returns>The prefix function array of length <c>pattern.Length</c>.</returns>
        public static int[] ComputePrefixFunction<T>(ReadOnlySpan<T> pattern) where T : IEquatable<T>
        {
            if (pattern.IsEmpty)
                return [];

            int[] pi = new int[pattern.Length];
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 1; i < pattern.Length; i++)
            {
                int j = pi[i - 1];
                while (j > 0 && !comparer.Equals(pattern[i], pattern[j]))
                    j = pi[j - 1];
                if (comparer.Equals(pattern[i], pattern[j]))
                    j++;
                pi[i] = j;
            }
            return pi;
        }

        /// <summary>
        /// Computes the Z-array for <paramref name="text"/> in O(n) time, where <c>z[i]</c> is the length of the
        /// longest common prefix between <c>text</c> and the suffix starting at index <c>i</c> (<c>z[0] = text.Length</c>).
        /// </summary>
        /// <param name="text">Input character span.</param>
        /// <returns>The Z-array of length <c>text.Length</c>.</returns>
        public static int[] ComputeZArray(ReadOnlySpan<char> text)
        {
            return ComputeZArray<char>(text);
        }

        /// <summary>
        /// Computes the Z-array for <paramref name="text"/> in O(n) time, where <c>z[i]</c> is the length of the
        /// longest common prefix between <c>text</c> and the suffix starting at index <c>i</c> (<c>z[0] = text.Length</c>).
        /// </summary>
        /// <typeparam name="T">Element type supporting equality comparison.</typeparam>
        /// <param name="text">Input sequence span.</param>
        /// <returns>The Z-array of length <c>text.Length</c>.</returns>
        public static int[] ComputeZArray<T>(ReadOnlySpan<T> text) where T : IEquatable<T>
        {
            if (text.IsEmpty)
                return [];

            int n = text.Length;
            int[] z = new int[n];
            z[0] = n;
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            int left = 0;
            int right = 0;
            for (int i = 1; i < n; i++)
            {
                if (i < right)
                    z[i] = Math.Min(right - i, z[i - left]);
                while (i + z[i] < n && comparer.Equals(text[z[i]], text[i + z[i]]))
                    z[i]++;
                if (i + z[i] > right)
                {
                    left = i;
                    right = i + z[i];
                }
            }
            return z;
        }
    }
}
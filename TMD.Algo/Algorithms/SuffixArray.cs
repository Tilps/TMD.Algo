#region License

/*
Copyright (c) 2026, the TMD.Algo authors.
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
    /// Suffix array and Kasai's longest common prefix (LCP) structure built in O(n log n) time.
    /// </summary>
    public class SuffixArray
    {
        private readonly int[] order;
        private readonly int[] rank;
        private readonly int[] lcp;

        private SuffixArray(int length, int[] order, int[] rank, int[] lcp)
        {
            Length = length;
            this.order = order;
            this.rank = rank;
            this.lcp = lcp;
        }

        /// <summary>
        /// Gets the length of the indexed string.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Gets the suffix array <c>SA</c>, where <c>Order[i]</c> is the starting index of the i-th lexicographically smallest suffix.
        /// </summary>
        public IReadOnlyList<int> Order => order;

        /// <summary>
        /// Gets the inverse suffix array, where <c>Rank[pos]</c> is the position of suffix <c>pos</c> in <see cref="Order"/>.
        /// </summary>
        public IReadOnlyList<int> Rank => rank;

        /// <summary>
        /// Gets Kasai's LCP array of length <c>max(0, Length - 1)</c>, where <c>Lcp[i]</c> is the longest common prefix
        /// length of <c>Order[i]</c> and <c>Order[i + 1]</c>.
        /// </summary>
        public IReadOnlyList<int> Lcp => lcp;

        /// <summary>
        /// Builds a <see cref="SuffixArray"/> and LCP array for <paramref name="text"/> in O(n log n) time.
        /// </summary>
        /// <param name="text">Input string.</param>
        /// <returns>The constructed <see cref="SuffixArray"/>.</returns>
        public static SuffixArray Build(string text)
        {
            ArgumentNullException.ThrowIfNull(text);
            return Build(text.AsSpan());
        }

        /// <summary>
        /// Builds a <see cref="SuffixArray"/> and LCP array for <paramref name="text"/> in O(n log n) time.
        /// </summary>
        /// <param name="text">Input character span.</param>
        /// <returns>The constructed <see cref="SuffixArray"/>.</returns>
        public static SuffixArray Build(ReadOnlySpan<char> text)
        {
            int n = text.Length;
            if (n == 0)
                return new SuffixArray(0, [], [], []);
            if (n == 1)
                return new SuffixArray(1, [0], [0], []);

            int[] sa = new int[n];
            int[] rk = new int[n];
            int[] tmpSa = new int[n];
            int[] tmpRk = new int[n];

            // Coordinate-compress initial single-character keys so arbitrary UTF-16 chars work in O(n log n).
            ulong[] initSort = new ulong[n];
            for (int i = 0; i < n; i++)
                initSort[i] = ((ulong)text[i] << 32) | (uint)i;
            Array.Sort(initSort);

            int classes = 1;
            sa[0] = (int)(uint)initSort[0];
            rk[sa[0]] = 0;
            for (int i = 1; i < n; i++)
            {
                int idx = (int)(uint)initSort[i];
                if ((initSort[i] >> 32) != (initSort[i - 1] >> 32))
                    classes++;
                sa[i] = idx;
                rk[idx] = classes - 1;
            }

            int[] cnt = new int[n];
            for (int k = 1; k < n && classes < n; k <<= 1)
            {
                int p = 0;
                for (int i = n - k; i < n; i++)
                    tmpSa[p++] = i;
                for (int i = 0; i < n; i++)
                {
                    if (sa[i] >= k)
                        tmpSa[p++] = sa[i] - k;
                }

                Array.Clear(cnt, 0, classes);
                for (int i = 0; i < n; i++)
                    cnt[rk[tmpSa[i]]]++;
                for (int i = 1; i < classes; i++)
                    cnt[i] += cnt[i - 1];
                for (int i = n - 1; i >= 0; i--)
                {
                    int idx = tmpSa[i];
                    sa[--cnt[rk[idx]]] = idx;
                }

                tmpRk[sa[0]] = 0;
                int newClasses = 1;
                for (int i = 1; i < n; i++)
                {
                    int cur = sa[i];
                    int prev = sa[i - 1];
                    int curSecond = cur + k < n ? rk[cur + k] : -1;
                    int prevSecond = prev + k < n ? rk[prev + k] : -1;
                    if (rk[cur] != rk[prev] || curSecond != prevSecond)
                        newClasses++;
                    tmpRk[cur] = newClasses - 1;
                }

                (rk, tmpRk) = (tmpRk, rk);
                classes = newClasses;
            }

            for (int i = 0; i < n; i++)
                rk[sa[i]] = i;

            int[] lcpArr = new int[n - 1];
            int h = 0;
            for (int i = 0; i < n; i++)
            {
                int r = rk[i];
                if (r == n - 1)
                {
                    h = 0;
                    continue;
                }
                int j = sa[r + 1];
                while (i + h < n && j + h < n && text[i + h] == text[j + h])
                    h++;
                lcpArr[r] = h;
                if (h > 0)
                    h--;
            }

            return new SuffixArray(n, sa, rk, lcpArr);
        }

        /// <summary>
        /// Computes the number of distinct non-empty substrings in the indexed string in O(n) time.
        /// </summary>
        /// <returns>Count of distinct non-empty substrings.</returns>
        public long CountDistinctSubstrings()
        {
            long total = (long)Length * (Length + 1) / 2;
            for (int i = 0; i < lcp.Length; i++)
                total -= lcp[i];
            return total;
        }
    }
}

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
using System.Numerics;

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Maintains the upper or lower envelope of 2D lines <c>y = m * x + b</c> added in monotone slope order
    /// (Convex Hull Trick), supporting O(log n) binary-search queries and O(1) amortized monotone queries.
    /// </summary>
    /// <typeparam name="T">Floating-point scalar type implementing <see cref="IFloatingPoint{T}"/> (such as <see cref="double"/> or <see cref="decimal"/>).</typeparam>
    public class MonotoneLineHull<T> where T : IFloatingPoint<T>
    {
        private readonly List<(T Slope, T Intercept)> lines = [];
        private readonly bool maximize;

        /// <summary>
        /// Initializes a <see cref="MonotoneLineHull{T}"/>.
        /// </summary>
        /// <param name="maximize">
        /// If true, maintains the upper envelope <c>max(m * x + b)</c> (requires non-decreasing slopes);
        /// if false, maintains the lower envelope <c>min(m * x + b)</c> (requires non-increasing slopes).
        /// </param>
        public MonotoneLineHull(bool maximize = true)
        {
            this.maximize = maximize;
        }

        /// <summary>
        /// Gets the number of non-redundant lines currently on the hull.
        /// </summary>
        public int Count => lines.Count;

        /// <summary>
        /// Removes all lines from the hull.
        /// </summary>
        public void Clear() => lines.Clear();

        /// <summary>
        /// Adds the line <c>y = slope * x + intercept</c> to the hull.
        /// Slopes must be non-decreasing when maximizing or non-increasing when minimizing.
        /// </summary>
        /// <param name="slope">Line slope.</param>
        /// <param name="intercept">Line y-intercept.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="slope"/> violates monotonicity.</exception>
        public void AddLine(T slope, T intercept)
        {
            if (lines.Count > 0)
            {
                var (lastSlope, lastIntercept) = lines[^1];
                if (maximize ? slope < lastSlope : slope > lastSlope)
                    throw new ArgumentException("Slopes must be added in monotone order matching maximize mode.", nameof(slope));

                if (slope == lastSlope)
                {
                    bool incomingDominated = maximize ? intercept <= lastIntercept : intercept >= lastIntercept;
                    if (incomingDominated)
                        return;
                    lines.RemoveAt(lines.Count - 1);
                }
            }

            while (lines.Count >= 2 && IsRedundant(lines[^2], lines[^1], (slope, intercept)))
                lines.RemoveAt(lines.Count - 1);

            lines.Add((slope, intercept));
        }

        /// <summary>
        /// Evaluates the optimal hull value at <paramref name="x"/> in O(log Count) time.
        /// </summary>
        /// <param name="x">Query x-coordinate.</param>
        /// <returns>The maximum or minimum <c>m * x + b</c> over all added lines.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the hull is empty.</exception>
        public T Query(T x)
        {
            if (lines.Count == 0)
                throw new InvalidOperationException("Cannot query an empty line hull.");

            int lo = 0;
            int hi = lines.Count - 1;
            while (lo < hi)
            {
                int mid = lo + ((hi - lo) >> 1);
                if (IsNextBetterOrEqual(mid, x))
                    lo = mid + 1;
                else
                    hi = mid;
            }
            return Evaluate(lines[lo], x);
        }

        /// <summary>
        /// Evaluates the optimal hull value at non-decreasing <paramref name="x"/> in amortized O(1) time
        /// by advancing <paramref name="pointer"/> along the hull.
        /// </summary>
        /// <param name="x">Query x-coordinate (must be greater than or equal to prior monotone queries on this pointer).</param>
        /// <param name="pointer">Persistent cursor initialized to 0 before the monotone sweep.</param>
        /// <returns>The maximum or minimum <c>m * x + b</c> over all added lines.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the hull is empty.</exception>
        public T QueryMonotone(T x, ref int pointer)
        {
            if (lines.Count == 0)
                throw new InvalidOperationException("Cannot query an empty line hull.");

            if (pointer < 0)
                pointer = 0;
            else if (pointer >= lines.Count)
                pointer = lines.Count - 1;

            while (pointer + 1 < lines.Count && IsNextBetterOrEqual(pointer, x))
                pointer++;

            return Evaluate(lines[pointer], x);
        }

        private static bool IsRedundant(
            (T Slope, T Intercept) a,
            (T Slope, T Intercept) b,
            (T Slope, T Intercept) c)
        {
            // Intersection x(b, c) <= x(a, b) reduces identically for both upper and lower monotone hulls:
            // (b2 - b3) * (m2 - m1) <= (b1 - b2) * (m3 - m2).
            T lhs = (b.Intercept - c.Intercept) * (b.Slope - a.Slope);
            T rhs = (a.Intercept - b.Intercept) * (c.Slope - b.Slope);
            return lhs <= rhs;
        }

        private static T Evaluate((T Slope, T Intercept) line, T x) =>
            line.Slope * x + line.Intercept;

        private bool IsNextBetterOrEqual(int index, T x)
        {
            T cur = Evaluate(lines[index], x);
            T nxt = Evaluate(lines[index + 1], x);
            return maximize ? nxt >= cur : nxt <= cur;
        }
    }
}

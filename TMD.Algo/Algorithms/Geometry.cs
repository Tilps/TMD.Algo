#region License
/*
Copyright (c) 2008, the TMD.Algo authors.
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
    /// Provides geometry methods.
    /// </summary>
    public static class Geometry
    {
        /// <summary>
        /// Gets the cross product of two vectors.
        /// </summary>
        /// <param name="x1">
        /// X component of first vector.
        /// </param>
        /// <param name="y1">
        /// Y component of first vector.
        /// </param>
        /// <param name="x2">
        /// X component of Second vector.
        /// </param>
        /// <param name="y2">
        /// Y component of Second vector.
        /// </param>
        /// <returns>
        /// The signed cross product of vector 1 against vector 2.
        /// </returns>
        public static long CrossProduct(long x1, long y1, long x2, long y2)
        {
            return x1 * y2 - x2 * y1;
        }
        // right hand rule - two consecutive segments turn clockwise if seg1 cross seg2 is negative.


        /// <summary>
        /// Given a set of points, determines the convex hull which contains them.
        /// </summary>
        /// <param name="points">
        /// Points to construct hull for.
        /// </param>
        /// <returns>
        /// The list of points in the convex hull.
        /// </returns>
        public static List<KeyValuePair<long, long>> ConvexHull(List<KeyValuePair<long, long>> points)
        {
            if (points.Count < 3)
                throw new ArgumentException("Points were insufficient to form a polygon", "points");

            KeyValuePair<long, long> p0 = points[0];
            for (int i = 1; i < points.Count; i++)
            {
                if (points[i].Value < p0.Value)
                    p0 = points[i];
                else if (points[i].Value == p0.Value && points[i].Key < p0.Key)
                    p0 = points[i];
            }
            List<KeyValuePair<long, long>> otherPoints = new List<KeyValuePair<long, long>>();
            foreach (KeyValuePair<long, long> point in points)
            {
                if (point.Key != p0.Key || point.Value != p0.Value)
                    otherPoints.Add(point);
            }
            AngleComparer comparer = new AngleComparer(p0);
            otherPoints.Sort(comparer);
            List<KeyValuePair<long, long>> equalityFilteredPoints = new List<KeyValuePair<long, long>>();
            KeyValuePair<long, long> cur = otherPoints[0];
            long curSqDist = SqDist(cur.Key, cur.Value, p0.Key, p0.Value);
            for (int i = 1; i < otherPoints.Count; i++)
            {
                if (comparer.Compare(cur, otherPoints[i]) != 0)
                {
                    equalityFilteredPoints.Add(cur);
                    cur = otherPoints[i];
                }
                else
                {
                    long nextDist = SqDist(otherPoints[i].Key, otherPoints[i].Value, p0.Key, p0.Value);
                    if (nextDist > curSqDist)
                    {
                        cur = otherPoints[0];
                        curSqDist = nextDist;
                    }
                }
            }
            equalityFilteredPoints.Add(cur);
            if (equalityFilteredPoints.Count < 2)
                throw new ArgumentException("Points were colinear and/or coincident.", "points");

            List<KeyValuePair<long, long>> stack = new List<KeyValuePair<long, long>>();
            stack.Add(p0);
            stack.Add(equalityFilteredPoints[0]);
            stack.Add(equalityFilteredPoints[1]);
            for (int i = 2; i < equalityFilteredPoints.Count; i++)
            {
                KeyValuePair<long, long> top = stack[stack.Count-1];
                KeyValuePair<long, long> nextToTop = stack[stack.Count-2];
                KeyValuePair<long, long> trial = equalityFilteredPoints[i];
                while (CrossProduct(top.Key - nextToTop.Key, top.Value - nextToTop.Value, trial.Key - top.Key, trial.Value - top.Value) < 0)
                {
                    stack.RemoveAt(stack.Count - 1);
                    top = stack[stack.Count - 1];
                    nextToTop = stack[stack.Count - 2];
                }
                stack.Add(trial);
            }
            return stack;
        }

        private static long SqDist(long x1, long y1, long x2, long y2)
        {
            long dx = x1 - x2;
            long dy = y1 - y2;
            return dx * dx + dy * dy;
        }
    }

    /// <summary>
    /// Compares points by their rotation around another point.
    /// </summary>
    public class AngleComparer : IComparer<KeyValuePair<long, long>>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="centre">
        /// Point to compare angles relative to.
        /// </param>
        public AngleComparer(KeyValuePair<long, long> centre)
        {
            this.centre = centre;
        }
        private KeyValuePair<long, long> centre;

        #region IComparer<KeyValuePair<long,long>> Members

        /// <summary>
        /// Compares two points relative to the centre to see whether they are clockwise or anticlockwise from each other.
        /// </summary>
        /// <param name="x">
        /// First point.
        /// </param>
        /// <param name="y">
        /// Second points.
        /// </param>
        /// <returns>
        /// A positive number if x is anticlockwise from y, otherwise a negative number.
        /// </returns>
        public int Compare(KeyValuePair<long, long> x, KeyValuePair<long, long> y)
        {
            long res = Geometry.CrossProduct(x.Key - centre.Key, x.Value - centre.Value, y.Key - centre.Key, y.Value - centre.Value);
            if (res > 0)
                return -1;
            else if (res < 0)
                return 1;
            return 0;
        }

        #endregion
    }
}

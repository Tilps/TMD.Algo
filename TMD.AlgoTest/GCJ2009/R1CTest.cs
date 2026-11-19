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
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2009
{
    [TestFixture]
    public class R1CTest : BaseGCJTest
    {
        #region Question 1 - All Your Base

        private static Func<long> Q1Solver(GCJTestCase test)
        {
            test.Get(out string symbols);
            return () => Q1Solver(symbols);
        }

        private static long Q1Solver(string symbols)
        {
            Dictionary<char, int> digitMap = new() { [symbols[0]] = 1 };
            foreach (char ch in symbols)
            {
                if (!digitMap.ContainsKey(ch))
                {
                    int nextDigit = digitMap.Count == 1 ? 0 : digitMap.Count;
                    digitMap[ch] = nextDigit;
                }
            }

            int digitBase = Math.Max(digitMap.Count, 2);
            int[] lsbDigits = new int[symbols.Length];
            for (int i = 0; i < symbols.Length; i++)
            {
                lsbDigits[symbols.Length - 1 - i] = digitMap[symbols[i]];
            }

            return (long)lsbDigits.ToValueAsDigits(digitBase);
        }

        [Test]
        public void Q1Sample()
        {
            Test(Q1Solver);
        }

        [Test]
        public void Q1Small()
        {
            Test(Q1Solver);
        }

        [Test]
        public void Q1Large()
        {
            Test(Q1Solver);
        }

        #endregion

        #region Question 2 - Center of Mass

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n);
            double sx = 0, sy = 0, sz = 0;
            double svx = 0, svy = 0, svz = 0;
            for (int i = 0; i < n; i++)
            {
                test.Get(out int x, out int y, out int z, out int vx, out int vy, out int vz);
                sx += x;
                sy += y;
                sz += z;
                svx += vx;
                svy += vy;
                svz += vz;
            }
            return () => Q2Solver(n, sx, sy, sz, svx, svy, svz);
        }

        private static string Q2Solver(int n, double sx, double sy, double sz, double svx, double svy, double svz)
        {
            double vSq = svx * svx + svy * svy + svz * svz;
            double tMin = 0.0;
            if (vSq > 0.0)
            {
                tMin = -(sx * svx + sy * svy + sz * svz) / vSq;
                if (tMin <= 0.0)
                    tMin = 0.0;
            }

            double dx = sx + tMin * svx;
            double dy = sy + tMin * svy;
            double dz = sz + tMin * svz;
            double dMin = Math.Sqrt(dx * dx + dy * dy + dz * dz) / n;
            return $"{dMin:F8} {tMin:F8}";
        }

        [Test]
        public void Q2Sample()
        {
            Test(Q2Solver, 1e-5);
        }

        [Test]
        public void Q2Small()
        {
            Test(Q2Solver, 1e-5);
        }

        [Test]
        public void Q2Large()
        {
            Test(Q2Solver, 1e-5);
        }

        #endregion

        #region Question 3 - Bribe the Prisoners

        private static Func<int> Q3Solver(GCJTestCase test)
        {
            test.Get(out int p, out int q);
            test.Get(out int[] prisoners);
            return () => Q3Solver(p, prisoners);
        }

        private static int Q3Solver(int p, int[] prisoners)
        {
            int[] pts = [0, .. prisoners, p + 1];
            var solve = Memotize.CreateMemotizedRecursive<int, int, int>((recurse, leftIdx, rightIdx) =>
            {
                if (rightIdx - leftIdx <= 1)
                    return 0;

                int spanCost = pts[rightIdx] - pts[leftIdx] - 2;
                int best = int.MaxValue;
                for (int k = leftIdx + 1; k < rightIdx; k++)
                {
                    int cost = spanCost + recurse(leftIdx, k) + recurse(k, rightIdx);
                    if (cost < best)
                        best = cost;
                }
                return best;
            });

            return solve(0, pts.Length - 1);
        }

        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Solver);
        }

        [Test]
        public void Q3Large()
        {
            Test(Q3Solver);
        }

        #endregion
    }
}

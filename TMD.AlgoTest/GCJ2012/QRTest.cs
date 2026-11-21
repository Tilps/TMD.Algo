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
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2012
{
    [TestFixture]
    public class QRTest : BaseGCJTest
    {
        #region Question 1 - Speaking in Tongues

        private const string GooglereseMap = "yhesocvxduiglbkrztnwjpfmaq";

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out string line);
            return () => Q1Solver(line);
        }

        private static string Q1Solver(string line)
        {
            char[] chars = line.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char ch = chars[i];
                if (ch >= 'a' && ch <= 'z')
                    chars[i] = GooglereseMap[ch - 'a'];
            }
            return new string(chars);
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

        #endregion

        #region Question 2 - Dancing With the Googlers

        private static Func<int> Q2Solver(GCJTestCase test)
        {
            test.Get(out int[] tokens);
            return () => Q2Solver(tokens);
        }

        private static int Q2Solver(int[] tokens)
        {
            int n = tokens[0];
            int s = tokens[1];
            int p = tokens[2];

            int baseCount = 0;
            int needSurprise = 0;
            for (int i = 0; i < n; i++)
            {
                int t = tokens[3 + i];
                if ((t + 2) / 3 >= p)
                    baseCount++;
                else if (t >= 2 && t <= 28 && (t + 4) / 3 >= p)
                    needSurprise++;
            }

            return baseCount + Math.Min(s, needSurprise);
        }

        [Test]
        public void Q2Sample()
        {
            Test(Q2Solver);
        }

        [Test]
        public void Q2Small()
        {
            Test(Q2Solver);
        }

        [Test]
        public void Q2Large()
        {
            Test(Q2Solver);
        }

        #endregion

        #region Question 3 - Recycled Numbers

        private static Func<int> Q3Solver(GCJTestCase test)
        {
            test.Get(out int a, out int b);
            return () => Q3Solver(a, b);
        }

        private static int Q3Solver(int a, int b)
        {
            int pow10 = 1;
            while (pow10 * 10 <= a)
                pow10 *= 10;

            int count = 0;
            for (int n = a; n <= b; n++)
            {
                int cur = n;
                while (true)
                {
                    cur = (cur / 10) + (cur % 10) * pow10;
                    if (cur == n)
                        break;
                    if (cur > n && cur <= b)
                        count++;
                }
            }
            return count;
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
            SlowTest(Q3Solver);
        }

        #endregion

        #region Question 4 - Hall of Mirrors

        private static Func<int> Q4Solver(GCJTestCase test)
        {
            test.Get(out int h, out int _, out int d);
            test.GetLines(h, out string[] grid);
            return () => Q4Solver(d, grid);
        }

        private static bool TraceGeneralRay(string[] grid, int r0, int c0, int d, int a, int b, int sr, int sc)
        {
            int r = r0, c = c0;
            long nextR = b;
            long nextC = a;
            long twoB = 2L * b;
            long twoA = 2L * a;
            long stepCenter = 2L * a * b;
            long nextCenter = stepCenter;
            long distFactor = (long)a * a + (long)b * b;
            long maxDistSq = 4L * a * a * b * b * d * d;

            while (true)
            {
                long t = Math.Min(nextCenter, Math.Min(nextR, nextC));
                if (t * t * distFactor > maxDistSq)
                    return false;

                if (t == nextCenter)
                {
                    if (r == r0 && c == c0)
                        return true;
                    nextCenter += stepCenter;
                }
                else if (nextR < nextC)
                {
                    if (grid[r + sr][c] == '#')
                        sr = -sr;
                    else
                        r += sr;
                    nextR += twoB;
                }
                else if (nextC < nextR)
                {
                    if (grid[r][c + sc] == '#')
                        sc = -sc;
                    else
                        c += sc;
                    nextC += twoA;
                }
                else
                {
                    bool diag = grid[r + sr][c + sc] == '#';
                    bool horz = grid[r + sr][c] == '#';
                    bool vert = grid[r][c + sc] == '#';
                    if (!diag)
                    {
                        r += sr;
                        c += sc;
                    }
                    else if (!horz && !vert)
                    {
                        return false;
                    }
                    else if (horz && vert)
                    {
                        sr = -sr;
                        sc = -sc;
                    }
                    else if (horz)
                    {
                        sr = -sr;
                        c += sc;
                    }
                    else
                    {
                        sc = -sc;
                        r += sr;
                    }
                    nextR += twoB;
                    nextC += twoA;
                }
            }
        }

        private static int Q4Solver(int d, string[] grid)
        {
            int h = grid.Length;
            int w = grid[0].Length;
            int r0 = -1, c0 = -1;
            for (int r = 0; r < h && r0 < 0; r++)
            {
                for (int c = 0; c < w; c++)
                {
                    if (grid[r][c] == 'X')
                    {
                        r0 = r;
                        c0 = c;
                        break;
                    }
                }
            }

            int total = 0;

            // Cardinal rays
            ReadOnlySpan<int> cardR = [-1, 1, 0, 0];
            ReadOnlySpan<int> cardC = [0, 0, -1, 1];
            for (int dir = 0; dir < 4; dir++)
            {
                int sr = cardR[dir];
                int sc = cardC[dir];
                int r = r0 + sr;
                int c = c0 + sc;
                int steps = 1;
                while (grid[r][c] != '#')
                {
                    r += sr;
                    c += sc;
                    steps++;
                }
                if (2 * steps - 1 <= d)
                    total++;
            }

            // General primitive rays across all 4 quadrants
            for (int a = 1; a <= d; a++)
            {
                for (int b = 1; a * a + b * b <= d * d; b++)
                {
                    if (MathFormulas.Gcd(a, b) != 1)
                        continue;
                    if (TraceGeneralRay(grid, r0, c0, d, a, b, 1, 1)) total++;
                    if (TraceGeneralRay(grid, r0, c0, d, a, b, 1, -1)) total++;
                    if (TraceGeneralRay(grid, r0, c0, d, a, b, -1, 1)) total++;
                    if (TraceGeneralRay(grid, r0, c0, d, a, b, -1, -1)) total++;
                }
            }

            return total;
        }

        [Test]
        public void Q4Sample()
        {
            Test(Q4Solver);
        }

        [Test]
        public void Q4Small()
        {
            Test(Q4Solver);
        }

        [Test]
        public void Q4Large()
        {
            SlowTest(Q4Solver);
        }

        #endregion
    }
}

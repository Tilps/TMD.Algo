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
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2013
{
    [TestFixture]
    public class R1CTest : BaseGCJTest
    {
        #region Question 1 - Consonants

        private static Func<long> Q1Solver(GCJTestCase test)
        {
            test.Get(out string name, out int n);
            return () => Q1Solver(name, n);
        }

        private static bool IsConsonant(char ch) =>
            ch != 'a' && ch != 'e' && ch != 'i' && ch != 'o' && ch != 'u';

        private static long Q1Solver(string name, int n)
        {
            int streak = 0;
            int lastStart = -1;
            long total = 0;
            for (int i = 0; i < name.Length; i++)
            {
                if (IsConsonant(name[i]))
                    streak++;
                else
                    streak = 0;

                if (streak >= n)
                    lastStart = i - n + 1;

                if (lastStart >= 0)
                    total += lastStart + 1;
            }
            return total;
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

        #region Question 2 - Pogo

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int x, out int y);
            return () => Q2Solver(x, y);
        }

        private static string Q2Solver(int x, int y)
        {
            long d = (long)Math.Abs(x) + Math.Abs(y);
            int n = 1;
            long sum = 1;
            while (sum < d || ((sum - d) & 1L) != 0)
            {
                n++;
                sum += n;
            }

            char[] moves = new char[n];
            for (int k = n; k >= 1; k--)
            {
                if (Math.Abs(x) > Math.Abs(y))
                {
                    if (x > 0)
                    {
                        moves[k - 1] = 'E';
                        x -= k;
                    }
                    else
                    {
                        moves[k - 1] = 'W';
                        x += k;
                    }
                }
                else
                {
                    if (y > 0)
                    {
                        moves[k - 1] = 'N';
                        y -= k;
                    }
                    else
                    {
                        moves[k - 1] = 'S';
                        y += k;
                    }
                }
            }
            return new string(moves);
        }

        private static readonly char[] LineSeps = ['\r', '\n'];
        private static readonly char[] TokenSeps = [' ', '\t'];

        private static void ValidateQ2(string input, string expectedOutput, List<string> results)
        {
            string[] inLines = input.Split(LineSeps, StringSplitOptions.RemoveEmptyEntries);
            int t = int.Parse(inLines[0]);
            string[] expLines = expectedOutput.Split(LineSeps, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(t, expLines.Length);
            Assert.AreEqual(t, results.Count);

            for (int i = 0; i < t; i++)
            {
                string[] coords = inLines[i + 1].Split(TokenSeps, StringSplitOptions.RemoveEmptyEntries);
                long targetX = long.Parse(coords[0]);
                long targetY = long.Parse(coords[1]);

                string[] expParts = expLines[i].Split(TokenSeps, StringSplitOptions.RemoveEmptyEntries);
                string[] actParts = results[i].Split(TokenSeps, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(3, actParts.Length);
                string path = actParts[2];
                int maxSteps = t <= 50 ? 500 : expParts[2].Length;
                Assert.IsTrue(path.Length <= maxSteps, $"Case #{i + 1}: length {path.Length} > max {maxSteps}");

                long x = 0, y = 0;
                for (int step = 1; step <= path.Length; step++)
                {
                    switch (path[step - 1])
                    {
                        case 'N': y += step; break;
                        case 'S': y -= step; break;
                        case 'E': x += step; break;
                        case 'W': x -= step; break;
                        default: Assert.Fail($"Invalid direction '{path[step - 1]}' in case #{i + 1}"); break;
                    }
                }
                Assert.AreEqual(targetX, x, $"Case #{i + 1} X mismatch");
                Assert.AreEqual(targetY, y, $"Case #{i + 1} Y mismatch");
            }
        }

        [Test]
        public void Q2Sample()
        {
            Test(Q2Solver, ValidateQ2);
        }

        [Test]
        public void Q2Small()
        {
            Test(Q2Solver, ValidateQ2);
        }

        [Test]
        public void Q2Large()
        {
            Test(Q2Solver, ValidateQ2);
        }

        #endregion

        #region Question 3 - The Great Wall

        private readonly struct TribeSpec
        {
            public readonly int FirstDay;
            public readonly int Count;
            public readonly int West;
            public readonly int East;
            public readonly int Strength;
            public readonly int DeltaDay;
            public readonly int DeltaPos;
            public readonly int DeltaStrength;

            public TribeSpec(int[] p)
            {
                FirstDay = p[0];
                Count = p[1];
                West = p[2];
                East = p[3];
                Strength = p[4];
                DeltaDay = p[5];
                DeltaPos = p[6];
                DeltaStrength = p[7];
            }
        }

        private readonly struct WallAttack
        {
            public readonly int Day;
            public readonly int West;
            public readonly int East;
            public readonly int Strength;

            public WallAttack(int day, int west, int east, int strength)
            {
                Day = day;
                West = west;
                East = east;
                Strength = strength;
            }
        }

        private static Func<int> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            TribeSpec[] tribes = new TribeSpec[n];
            for (int i = 0; i < n; i++)
            {
                test.Get(out int[] parts);
                tribes[i] = new TribeSpec(parts);
            }
            return () => Q3Solver(tribes);
        }

        private static int Q3Solver(TribeSpec[] tribes)
        {
            int totalAttacks = 0;
            foreach (TribeSpec tr in tribes)
                totalAttacks += tr.Count;

            WallAttack[] attacks = new WallAttack[totalAttacks];
            int[] rawCoords = new int[2 * totalAttacks];
            int ai = 0;
            int ci = 0;

            foreach (TribeSpec tr in tribes)
            {
                int day = tr.FirstDay;
                int w = tr.West;
                int e = tr.East;
                int s = tr.Strength;
                for (int j = 0; j < tr.Count; j++)
                {
                    attacks[ai++] = new WallAttack(day, w, e, s);
                    rawCoords[ci++] = w;
                    rawCoords[ci++] = e;
                    day += tr.DeltaDay;
                    w += tr.DeltaPos;
                    e += tr.DeltaPos;
                    s += tr.DeltaStrength;
                }
            }

            Array.Sort(rawCoords);
            int uniqueCount = 0;
            for (int i = 0; i < rawCoords.Length; i++)
            {
                if (uniqueCount == 0 || rawCoords[i] != rawCoords[uniqueCount - 1])
                    rawCoords[uniqueCount++] = rawCoords[i];
            }
            ReadOnlySpan<int> coords = rawCoords.AsSpan(0, uniqueCount);

            Array.Sort(attacks, static (a, b) => a.Day.CompareTo(b.Day));

            int leafCount = uniqueCount - 1;
            LazySegmentTree<int, int, RangeMinChmaxInt32Policy> wall = new(leafCount, 0);
            int[] mappedL = new int[totalAttacks];
            int[] mappedR = new int[totalAttacks];

            int succeeded = 0;
            int idx = 0;
            while (idx < attacks.Length)
            {
                int day = attacks[idx].Day;
                int end = idx;
                while (end < attacks.Length && attacks[end].Day == day)
                {
                    int l = coords.BinarySearch(attacks[end].West);
                    int r = coords.BinarySearch(attacks[end].East);
                    mappedL[end] = l;
                    mappedR[end] = r;
                    if (wall.Query(l, r) < attacks[end].Strength)
                        succeeded++;
                    end++;
                }

                for (int j = idx; j < end; j++)
                    wall.Update(mappedL[j], mappedR[j], attacks[j].Strength);

                idx = end;
            }
            return succeeded;
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
    }
}

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
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2013
{
    [TestFixture]
    public class R2Test : BaseGCJTest
    {
        #region Question 1 - Ticket Swapping

        private const long TicketMod = 1000002013L;

        private readonly struct StationEvent : IComparable<StationEvent>
        {
            public readonly long Station;
            public readonly long Count;
            public readonly bool IsExit;

            public StationEvent(long station, long count, bool isExit)
            {
                Station = station;
                Count = count;
                IsExit = isExit;
            }

            public int CompareTo(StationEvent other)
            {
                int cmp = Station.CompareTo(other.Station);
                return cmp != 0 ? cmp : IsExit.CompareTo(other.IsExit);
            }
        }

        private static Func<long> Q1Solver(GCJTestCase test)
        {
            test.Get(out long n, out int m);
            (long O, long E, long P)[] trips = new (long, long, long)[m];
            for (int i = 0; i < m; i++)
                test.Get(out trips[i]);
            return () => Q1Solver(trips);
        }

        private static long DiscountMod(long d, long p)
        {
            long tri = ((d * (d - 1)) >> 1) % TicketMod;
            return (tri * (p % TicketMod)) % TicketMod;
        }

        private static long Q1Solver((long O, long E, long P)[] trips)
        {
            int m = trips.Length;
            long originalDiscount = 0;
            StationEvent[] events = new StationEvent[2 * m];
            for (int i = 0; i < m; i++)
            {
                var (o, e, p) = trips[i];
                originalDiscount = (originalDiscount + DiscountMod(e - o, p)) % TicketMod;
                events[2 * i] = new StationEvent(o, p, false);
                events[2 * i + 1] = new StationEvent(e, p, true);
            }

            Array.Sort(events);

            Stack<(long Station, long Count)> active = new();
            long swappedDiscount = 0;
            foreach (StationEvent ev in events)
            {
                if (!ev.IsExit)
                {
                    active.Push((ev.Station, ev.Count));
                }
                else
                {
                    long rem = ev.Count;
                    while (rem > 0)
                    {
                        var (s, cnt) = active.Pop();
                        long take = Math.Min(rem, cnt);
                        swappedDiscount = (swappedDiscount + DiscountMod(ev.Station - s, take)) % TicketMod;
                        rem -= take;
                        if (cnt > take)
                            active.Push((s, cnt - take));
                    }
                }
            }

            return ((swappedDiscount - originalDiscount) % TicketMod + TicketMod) % TicketMod;
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

        #region Question 2 - Many Prizes

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n, out long p);
            return () => Q2Solver(n, p);
        }

        private static long LowRankCanWin(int n, long p)
        {
            if (p <= 0)
                return -1L;
            int w = 0;
            while ((1L << (n - w)) > p)
                w++;
            return (1L << n) - (1L << w);
        }

        private static string Q2Solver(int n, long p)
        {
            long total = 1L << n;
            long z = LowRankCanWin(n, p);
            long y = total - 2 - LowRankCanWin(n, total - p);
            return $"{y} {z}";
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

        #region Question 3 - Erdos-Szekeres

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out int[] a);
            test.Get(out int[] b);
            return () => Q3Solver(n, a, b);
        }

        private static string Q3Solver(int n, int[] a, int[] b)
        {
            Graph<int, int> g = new();
            for (int i = 0; i < n; i++)
                g.AddVertex(i);

            int[] last = new int[n + 1];
            Array.Fill(last, -1);
            for (int i = 0; i < n; i++)
            {
                int v = a[i];
                if (last[v] != -1)
                    g.AddDirectedEdge(i, last[v], 1);
                if (v > 1 && last[v - 1] != -1)
                    g.AddDirectedEdge(last[v - 1], i, 1);
                last[v] = i;
            }

            Array.Fill(last, -1);
            for (int i = n - 1; i >= 0; i--)
            {
                int v = b[i];
                if (last[v] != -1)
                    g.AddDirectedEdge(i, last[v], 1);
                if (v > 1 && last[v - 1] != -1)
                    g.AddDirectedEdge(last[v - 1], i, 1);
                last[v] = i;
            }

            List<int> order = g.LexicographicalTopologicalSort();
            int[] x = new int[n];
            for (int rank = 0; rank < n; rank++)
                x[order[rank]] = rank + 1;
            return string.Join(" ", x);
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

        #region Question 4 - Multiplayer Pong

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out BigInteger a, out BigInteger b);
            test.Get(out BigInteger n, out BigInteger m);
            test.Get(out BigInteger v, out BigInteger w);
            test.Get(out BigInteger y, out BigInteger x, out BigInteger vy, out BigInteger vx);
            return () => Q4Solver(a, b, n, m, v, w, y, x, vy, vx);
        }

        private static BigInteger? FirstFailBounce(
            BigInteger u0,
            BigInteger step1,
            BigInteger teamSize,
            BigInteger speed,
            BigInteger width,
            BigInteger scaledHeight)
        {
            BigInteger dBig = step1 * teamSize;
            BigInteger maxTravel = 2 * width * speed * teamSize;
            BigInteger twoH = 2 * scaledHeight;
            BigInteger d = dBig % twoH;
            if (d > scaledHeight)
            {
                d = twoH - d;
                u0 = (u0 + dBig) % twoH;
            }
            if (maxTravel >= d)
                return null;

            BigInteger safeL = (twoH - d - maxTravel + 1) / 2;
            BigInteger safeR = (twoH - d + maxTravel) / 2;
            if (safeL > safeR)
                return teamSize;

            BigInteger y0 = u0 % scaledHeight;
            BigInteger low = (safeR + 1) % scaledHeight;
            BigInteger high = safeL - 1;
            BigInteger? k = Modulo.MinStepInRange(scaledHeight, y0, step1, low, high);
            return k.HasValue ? k.Value + teamSize : null;
        }

        private static string Q4Solver(
            BigInteger a, BigInteger b,
            BigInteger n, BigInteger m,
            BigInteger v, BigInteger w,
            BigInteger y, BigInteger x, BigInteger vy, BigInteger vx)
        {
            if (vx.IsZero || vy.IsZero)
                return "DRAW";

            bool flippedX = false;
            if (vx < 0)
            {
                x = b - x;
                vx = -vx;
                (n, m) = (m, n);
                (v, w) = (w, v);
                flippedX = true;
            }
            if (vy < 0)
            {
                y = a - y;
                vy = -vy;
            }

            BigInteger scaledH = a * vx;
            BigInteger scaledY = y * vx;
            BigInteger step1 = 2 * b * vy;

            BigInteger? rFail = FirstFailBounce(scaledY + (b - x) * vy, step1, m, w, b, scaledH);
            BigInteger? lFail = FirstFailBounce(scaledY + (2 * b - x) * vy, step1, n, v, b, scaledH);

            if (!rFail.HasValue && !lFail.HasValue)
                return "DRAW";

            bool leftWins = rFail.HasValue && (!lFail.HasValue || rFail.Value <= lFail.Value);
            BigInteger bounces = leftWins ? rFail!.Value : lFail!.Value;
            if (flippedX)
                leftWins = !leftWins;

            return $"{(leftWins ? "LEFT" : "RIGHT")} {bounces}";
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
            Test(Q4Solver);
        }

        #endregion
    }
}

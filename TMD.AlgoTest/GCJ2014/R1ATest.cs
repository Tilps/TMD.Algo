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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using NUnit.Framework;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2014
{
    [TestFixture]
    public class R1ATest : BaseGCJTest
    {
        #region Question 1 - Charging Chaos

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n, out int l);
            test.Get(out string[] outlets);
            test.Get(out string[] devices);
            return () => Q1Solver(n, outlets, devices);
        }

        private static string Q1Solver(int n, string[] outletStrs, string[] deviceStrs)
        {
            ulong[] outlets = new ulong[n];
            ulong[] devices = new ulong[n];
            HashSet<ulong> deviceSet = [with(capacity: n)];

            for (int i = 0; i < n; i++)
            {
                outlets[i] = Convert.ToUInt64(outletStrs[i], 2);
                devices[i] = Convert.ToUInt64(deviceStrs[i], 2);
                deviceSet.Add(devices[i]);
            }

            int best = int.MaxValue;
            ulong targetDevice = devices[0];
            for (int i = 0; i < n; i++)
            {
                ulong mask = outlets[i] ^ targetDevice;
                int flips = BitOperations.PopCount(mask);
                if (flips >= best)
                    continue;

                bool ok = true;
                for (int j = 0; j < n; j++)
                {
                    if (!deviceSet.Contains(outlets[j] ^ mask))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                    best = flips;
            }

            return best == int.MaxValue ? "NOT POSSIBLE" : best.ToString();
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

        #region Question 2 - Full Binary Tree

        private static Func<int> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n);
            (int U, int V)[] edges = new (int, int)[n - 1];
            for (int i = 0; i < n - 1; i++)
            {
                test.Get(out int u, out int v);
                edges[i] = (u - 1, v - 1);
            }
            return () => Q2Solver(n, edges);
        }

        private static void InsertTop3(
            ref (int Size, int Neighbor) t0,
            ref (int Size, int Neighbor) t1,
            ref (int Size, int Neighbor) t2,
            ref int count,
            int size,
            int neighbor)
        {
            var item = (size, neighbor);
            if (count == 0)
            {
                t0 = item;
                count = 1;
            }
            else if (count == 1)
            {
                if (size > t0.Size) { t1 = t0; t0 = item; }
                else { t1 = item; }
                count = 2;
            }
            else if (count == 2)
            {
                if (size > t0.Size) { t2 = t1; t1 = t0; t0 = item; }
                else if (size > t1.Size) { t2 = t1; t1 = item; }
                else { t2 = item; }
                count = 3;
            }
            else
            {
                if (size > t0.Size) { t2 = t1; t1 = t0; t0 = item; }
                else if (size > t1.Size) { t2 = t1; t1 = item; }
                else if (size > t2.Size) { t2 = item; }
            }
        }

        private static int BestPairExcluding(
            (int Size, int Neighbor) t0,
            (int Size, int Neighbor) t1,
            (int Size, int Neighbor) t2,
            int count,
            int exclude)
        {
            int used = 0;
            int sum = 0;
            if (count >= 1 && t0.Neighbor != exclude)
            {
                sum += t0.Size;
                used++;
            }
            if (count >= 2 && t1.Neighbor != exclude)
            {
                sum += t1.Size;
                used++;
            }
            if (used < 2 && count >= 3 && t2.Neighbor != exclude)
            {
                sum += t2.Size;
                used++;
            }
            return used == 2 ? 1 + sum : 1;
        }

        private static int Q2Solver(int n, (int U, int V)[] edges)
        {
            List<int>[] adj = new List<int>[n];
            for (int i = 0; i < n; i++)
                adj[i] = [];
            foreach (var (u, v) in edges)
            {
                adj[u].Add(v);
                adj[v].Add(u);
            }

            int[] parent = new int[n];
            Array.Fill(parent, -1);
            int[] order = new int[n];
            bool[] visited = new bool[n];
            visited[0] = true;
            int head = 0, tail = 0;
            order[tail++] = 0;
            while (head < tail)
            {
                int u = order[head++];
                foreach (int v in adj[u])
                {
                    if (!visited[v])
                    {
                        visited[v] = true;
                        parent[v] = u;
                        order[tail++] = v;
                    }
                }
            }

            // Bottom-up pass: top-2 downward branches at each node.
            int[] dpDown = new int[n];
            for (int idx = n - 1; idx >= 0; idx--)
            {
                int u = order[idx];
                (int Size, int Neighbor) t0 = default, t1 = default, t2 = default;
                int cnt = 0;
                foreach (int v in adj[u])
                {
                    if (v != parent[u])
                        InsertTop3(ref t0, ref t1, ref t2, ref cnt, dpDown[v], v);
                }
                dpDown[u] = cnt >= 2 ? 1 + t0.Size + t1.Size : 1;
            }

            // Top-down rerooting pass: inject parent's upward size into each node's top-3.
            int[] upSize = new int[n];
            int bestKeep = 1;
            for (int idx = 0; idx < n; idx++)
            {
                int u = order[idx];
                (int Size, int Neighbor) t0 = default, t1 = default, t2 = default;
                int cnt = 0;
                if (u != 0)
                    InsertTop3(ref t0, ref t1, ref t2, ref cnt, upSize[u], parent[u]);
                foreach (int v in adj[u])
                {
                    if (v != parent[u])
                        InsertTop3(ref t0, ref t1, ref t2, ref cnt, dpDown[v], v);
                }

                int keep = cnt >= 2 ? 1 + t0.Size + t1.Size : 1;
                if (keep > bestKeep)
                    bestKeep = keep;

                foreach (int v in adj[u])
                {
                    if (v != parent[u])
                        upSize[v] = BestPairExcluding(t0, t1, t2, cnt, v);
                }
            }

            return n - bestKeep;
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

        #region Question 3 - Proper Shuffle

        private static readonly ConcurrentDictionary<int, double[]> LogWeightCache = new();

        private static double[] GetLogWeights(int n)
        {
            return LogWeightCache.GetOrAdd(n, static size =>
            {
                double[] logWeights = new double[size * size];
                double[] raw = new double[size];
                double pMove = 1.0 / size;
                double pStay = 1.0 - pMove;

                for (int j = 0; j < size; j++)
                {
                    Array.Clear(raw);
                    raw[j] = 1.0;
                    double scale = 1.0;
                    double offset = 0.0;
                    for (int k = size - 1; k >= 0; k--)
                    {
                        double oldVk = scale * raw[k] + offset;
                        scale *= pStay;
                        offset = pStay * offset + pMove * oldVk;
                        raw[k] = (pMove - offset) / scale;
                    }

                    for (int i = 0; i < size; i++)
                    {
                        double prob = scale * raw[i] + offset;
                        logWeights[i * size + j] = Math.Log(size * prob);
                    }
                }
                return logWeights;
            });
        }

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out int[] perm);
            return () => Q3Solver(n, perm);
        }

        private static string Q3Solver(int n, int[] perm)
        {
            double[] logWeights = GetLogWeights(n);
            double llr = 0.0;
            for (int j = 0; j < n; j++)
                llr += logWeights[perm[j] * n + j];
            return llr > 0.0 ? "BAD" : "GOOD";
        }

        private static readonly char[] LineSeps = ['\r', '\n'];
        private static readonly char[] TokenSeps = [' ', '\t'];

        private static void ValidateQ3(string input, string expectedOutput, List<string> results)
        {
            string[] inLines = input.Split(LineSeps, StringSplitOptions.RemoveEmptyEntries);
            int t = int.Parse(inLines[0]);
            string[] expLines = expectedOutput.Split(LineSeps, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(t, expLines.Length);
            Assert.AreEqual(t, results.Count);

            int correct = 0;
            for (int i = 0; i < t; i++)
            {
                string[] actTok = results[i].Split(TokenSeps, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(3, actTok.Length);
                Assert.IsTrue(actTok[2] is "GOOD" or "BAD", $"Unexpected label '{actTok[2]}' on case #{i + 1}");

                string[] expTok = expLines[i].Split(TokenSeps, StringSplitOptions.RemoveEmptyEntries);
                if (string.Equals(actTok[2], expTok[2], StringComparison.OrdinalIgnoreCase))
                    correct++;
            }

            if (t == 120)
                Assert.IsTrue(correct >= 109, $"Got {correct} / 120 correct, needs at least 109.");
        }

        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver, ValidateQ3);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Solver, ValidateQ3);
        }

        #endregion
    }
}

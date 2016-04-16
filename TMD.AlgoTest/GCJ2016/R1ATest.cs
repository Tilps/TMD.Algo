#region License

/*
Copyright (c) 2016, the TMD.Algo authors.
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
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Algorithms.Generic;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2016
{
    [TestFixture]
    public class R1ATest : BaseGCJTest
    {
        private Func<string> Q1Solver(GCJTestCase test)
        {
            string S;
            test.Get(out S);
            return () => Q1Solver(S);
        }

        private string Q1Solver(string S)
        {
            return new string(S.Aggregate(new Dequeue<char>(), (d, a) =>
            {
                if (d.Count > 0 && d.Front <= a) d.PushFront(a);
                else d.PushBack(a);
                return d;
            }).ToArray());
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
            SlowTest(Q1Solver);
        }

        private Func<int[]> Q2Solver(GCJTestCase test)
        {
            int N;
            test.Get(out N);
            int[][] seqs = new int[2 * N - 1][];
            for (int i = 0; i < seqs.Length; i++)
            {
                int[] seq;
                test.Get(out seq);
                seqs[i] = seq;
            }
            return () => Q2Solver(N, seqs);
        }

        private int[] Q2Solver(int N, int[][] seqs)
        {
            // Flatten all values into a single enumeration, cluster by value, filter by cluster size being odd, reduce to the cluster value and return sorted.
            return seqs.SelectMany(a => a).GroupBy(a => a).Where(a => a.Count() % 2 == 1).Select(a => a.Key).ToSortedArray();
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
            SlowTest(Q2Solver);
        }

        private Func<int> Q3Solver(GCJTestCase test)
        {
            int N;
            test.Get(out N);
            int[] bffs;
            test.Get(out bffs);
            return () => Q3Solver(bffs);
        }

        private int Q3Solver(int[] bffs)
        {
            // Change indexing to 0 based.
            for (int i = 0; i < bffs.Length; i++)
            {
                bffs[i]--;
            }
            HashSet<int> terminals = new HashSet<int>();
            terminals.UnionWith(Enumerable.Range(0, bffs.Length).Where(a=>bffs[bffs[a]] == a));
            int longestCycle = 0;
            bool[] cycleChecked = new bool[bffs.Length];
            bool[] isCycle = new bool[bffs.Length];
            for (int i = 0; i < bffs.Length; i++)
            {
                if (cycleChecked[i]) continue;
                if (terminals.Contains(i)) continue;
                HashSet<int> seen = new HashSet<int>();
                int cur = i;
                seen.Add(cur);
                List<int> cycleOrder = new List<int>();
                cycleOrder.Add(cur);
                bool cycle = true;
                while (true)
                {
                    int next = bffs[cur];
                    if (isCycle[next])
                    {
                        // Connects to real cycle, don't try and work out real cycle length, just mark as connecting to cycle.
                        cycle = false;
                        foreach (int pos in cycleOrder) isCycle[pos] = true;
                        break;
                    }
                    if (cycleChecked[next])
                    {
                        // Must flow to a terminal.
                        cycle = false;
                        break;
                    }
                    if (terminals.Contains(next))
                    {
                        // has flowed to a terminal.
                        cycle = false;
                        break;
                    }
                    if (seen.Contains(next))
                    {
                        // A true cycle found.
                        break;
                    }
                    cur = next;
                    seen.Add(cur);
                    cycleOrder.Add(cur);
                }
                foreach (int pos in cycleOrder) cycleChecked[pos] = true;
                if (cycle)
                {
                    int first = bffs[cycleOrder.Last()];
                    int index = cycleOrder.IndexOf(first);
                    int trueCycleLength = cycleOrder.Count - index;
                    if (trueCycleLength > longestCycle)
                    {
                        longestCycle = trueCycleLength;
                    }
                    foreach (int pos in cycleOrder)
                    {
                        isCycle[pos] = true;
                    }
                }
            }

            int[] distToTerminal = new int[bffs.Length];
            int[] terminalForPath = new int[bffs.Length];
            Dictionary<int, int> maxDistToTerminal = new Dictionary<int, int>();
            foreach (int pos in terminals) maxDistToTerminal.Add(pos, 0);
            for (int i = 0; i < bffs.Length; i++)
            {
                if (isCycle[i]) continue;
                if (terminals.Contains(i)) continue;
                if (distToTerminal[i] > 0) continue;
                int cur = i;
                List<int> orderToTerminal = new List<int>();
                int endDist = 0;
                // All non-terminal cycles already detected and removed from consideration, so this should always terminate.
                while (!terminals.Contains(cur))
                {
                    if (distToTerminal[cur] > 0)
                    {
                        endDist = distToTerminal[cur];
                        break;
                    }
                    orderToTerminal.Add(cur);
                    cur = bffs[cur];
                }
                int terminal = terminals.Contains(cur) ? cur : terminalForPath[cur];
                for (int j = orderToTerminal.Count - 1; j >= 0; j--)
                {
                    int pos = orderToTerminal[j];
                    distToTerminal[pos] = endDist + (orderToTerminal.Count - j);
                    terminalForPath[pos] = terminal;
                }
                int dist = endDist + orderToTerminal.Count;
                int maxDist = maxDistToTerminal[terminal];
                if (dist > maxDist)
                {
                    maxDistToTerminal[terminal] = dist;
                }
            }
            int terminalSum = terminals.Sum(pos => 1 + maxDistToTerminal[pos]);
            return Math.Max(terminalSum, longestCycle);
        }


        // The validator for these tests doesn't match the real validator. As such, changes to Q3Solver could reasonably cause failure.
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
    }
}

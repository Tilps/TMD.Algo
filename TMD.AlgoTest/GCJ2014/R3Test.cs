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
using System.Globalization;
using NUnit.Framework;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2014
{
    [TestFixture]
    public class R3Test : BaseGCJTest
    {
        #region Question 1 - Magical, Marvelous Tour

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n, out long p, out long q, out long r, out long s);
            return () => Q1Solver(n, p, q, r, s).ToString("G17", CultureInfo.InvariantCulture);
        }

        private static double Q1Solver(int n, long p, long q, long r, long s)
        {
            long[] pref = new long[n + 1];
            for (int i = 0; i < n; i++)
                pref[i + 1] = pref[i] + ((i * p + q) % r + s);

            long total = pref[n];
            long bestSolveig = total;
            int split = 1;

            for (int a = 0; a < n; a++)
            {
                if (split < a + 1)
                    split = a + 1;
                long left = pref[a];
                while (split < n && pref[split] - left < total - pref[split])
                    split++;

                long opt1 = Math.Max(left, Math.Max(pref[split] - left, total - pref[split]));
                if (opt1 < bestSolveig)
                    bestSolveig = opt1;

                if (split - 1 >= a + 1)
                {
                    int prev = split - 1;
                    long opt2 = Math.Max(left, Math.Max(pref[prev] - left, total - pref[prev]));
                    if (opt2 < bestSolveig)
                        bestSolveig = opt2;
                }
            }

            return (double)(total - bestSolveig) / total;
        }

        [Test]
        public void Q1Sample()
        {
            Test(Q1Solver, 1e-9);
        }

        [Test]
        public void Q1Small()
        {
            Test(Q1Solver, 1e-9);
        }

        [Test]
        public void Q1Large()
        {
            SlowTest(Q1Solver, 1e-9);
        }

        #endregion

        #region Question 2 - Last Hit

        private static Func<int> Q2Solver(GCJTestCase test)
        {
            test.Get(out int p, out int q, out int n);
            (int H, int G)[] monsters = new (int, int)[n];
            for (int i = 0; i < n; i++)
                test.Get(out monsters[i]);
            return () => Q2Solver(p, q, monsters);
        }

        private static int Q2Solver(int p, int q, (int H, int G)[] monsters)
        {
            int maxShots = 10 * monsters.Length + 1;
            int[] dp = new int[maxShots + 1];
            int[] next = new int[maxShots + 1];
            Array.Fill(dp, -1);
            dp[1] = 0;
            int curMaxS = 1;

            foreach (var (h, g) in monsters)
            {
                int tSkip = (h + q - 1) / q;
                int tKill = (h - 1) / q;
                int rem = h - tKill * q;
                int dKill = (rem + p - 1) / p;

                int nextMaxS = curMaxS + tSkip;
                Array.Fill(next, -1, 0, nextMaxS + 1);

                for (int s = 0; s <= curMaxS; s++)
                {
                    int val = dp[s];
                    if (val < 0)
                        continue;

                    int nsSkip = s + tSkip;
                    if (val > next[nsSkip])
                        next[nsSkip] = val;

                    if (s + tKill >= dKill)
                    {
                        int nsKill = s + tKill - dKill;
                        int cand = val + g;
                        if (cand > next[nsKill])
                            next[nsKill] = cand;
                    }
                }

                (dp, next) = (next, dp);
                curMaxS = nextMaxS;
            }

            int best = 0;
            for (int s = 0; s <= curMaxS; s++)
            {
                if (dp[s] > best)
                    best = dp[s];
            }
            return best;
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

        #region Question 3 - Crime House

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            bool[] isEnter = new bool[n];
            int[] ids = new int[n];
            for (int i = 0; i < n; i++)
            {
                test.Get(out string kind, out int id);
                isEnter[i] = kind[0] == 'E';
                ids[i] = id;
            }
            return () => Q3Solver(n, isEnter, ids);
        }

        private static string Q3Solver(int n, bool[] isEnter, int[] ids)
        {
            const int MaxId = 2000;
            int eCount = 0;
            int lCount = 0;
            int[] firstOcc = new int[MaxId + 1];
            int[] lastOcc = new int[MaxId + 1];
            int[] nextOcc = new int[n];
            Array.Fill(firstOcc, -1);
            Array.Fill(lastOcc, -1);
            Array.Fill(nextOcc, -1);

            for (int i = 0; i < n; i++)
            {
                if (isEnter[i]) eCount++;
                else lCount++;

                int id = ids[i];
                if (id > 0)
                {
                    if (firstOcc[id] == -1)
                        firstOcc[id] = i;
                    else
                        nextOcc[lastOcc[id]] = i;
                    lastOcc[id] = i;
                }
            }

            bool[] inside = new bool[MaxId + 1];
            int[] nextIdx = new int[MaxId + 1];
            PriorityQueue<int, int> outNeedLeave = new();
            PriorityQueue<int, int> inNeedEnter = new();
            PriorityQueue<int, int> inNeedLeave = new();
            Stack<int> freeInIds = new();

            bool CanSimulate(int s)
            {
                Array.Clear(inside);
                Array.Copy(firstOcc, nextIdx, MaxId + 1);
                outNeedLeave.Clear();
                inNeedEnter.Clear();
                inNeedLeave.Clear();
                freeInIds.Clear();

                void Sync(int id)
                {
                    int nt = nextIdx[id];
                    if (inside[id])
                    {
                        if (nt == -1)
                            freeInIds.Push(id);
                        else if (isEnter[nt])
                            inNeedEnter.Enqueue(id, nt);
                        else
                            inNeedLeave.Enqueue(id, -nt);
                    }
                    else if (nt != -1 && !isEnter[nt])
                    {
                        outNeedLeave.Enqueue(id, nt);
                    }
                }

                for (int id = 1; id <= MaxId; id++)
                {
                    if (firstOcc[id] != -1)
                        Sync(id);
                }

                int unknownInside = 0;
                int insideCount = 0;

                int PopOutNeedLeave()
                {
                    while (outNeedLeave.TryDequeue(out int id, out int prio))
                    {
                        if (!inside[id] && nextIdx[id] == prio && !isEnter[prio])
                            return id;
                    }
                    return 0;
                }

                int PopInNeedEnter()
                {
                    while (inNeedEnter.TryDequeue(out int id, out int prio))
                    {
                        if (inside[id] && nextIdx[id] == prio && isEnter[prio])
                            return id;
                    }
                    return 0;
                }

                int PopInNeedLeave()
                {
                    while (inNeedLeave.TryDequeue(out int id, out int negPrio))
                    {
                        int prio = -negPrio;
                        if (inside[id] && nextIdx[id] == prio && !isEnter[prio])
                            return id;
                    }
                    return 0;
                }

                int PopFreeKnown()
                {
                    while (freeInIds.Count > 0)
                    {
                        int id = freeInIds.Pop();
                        if (inside[id] && nextIdx[id] == -1)
                            return id;
                    }
                    return 0;
                }

                // Process initial s virtual E 0 entries.
                for (int k = 0; k < s; k++)
                {
                    int cand = PopOutNeedLeave();
                    insideCount++;
                    if (cand > 0)
                    {
                        inside[cand] = true;
                        Sync(cand);
                    }
                    else
                    {
                        unknownInside++;
                    }
                }

                for (int i = 0; i < n; i++)
                {
                    int id = ids[i];
                    if (isEnter[i])
                    {
                        if (id > 0)
                        {
                            if (inside[id])
                                return false;
                            inside[id] = true;
                            insideCount++;
                            nextIdx[id] = nextOcc[i];
                            Sync(id);
                        }
                        else
                        {
                            int cand = PopOutNeedLeave();
                            insideCount++;
                            if (cand > 0)
                            {
                                inside[cand] = true;
                                Sync(cand);
                            }
                            else
                            {
                                unknownInside++;
                            }
                        }
                    }
                    else
                    {
                        if (id > 0)
                        {
                            if (!inside[id])
                                return false;
                            inside[id] = false;
                            insideCount--;
                            nextIdx[id] = nextOcc[i];
                            Sync(id);
                        }
                        else
                        {
                            if (insideCount == 0)
                                return false;
                            insideCount--;
                            int cEnter = PopInNeedEnter();
                            if (cEnter > 0)
                            {
                                inside[cEnter] = false;
                                Sync(cEnter);
                            }
                            else if (unknownInside > 0)
                            {
                                unknownInside--;
                            }
                            else
                            {
                                int cFree = PopFreeKnown();
                                if (cFree > 0)
                                {
                                    inside[cFree] = false;
                                }
                                else
                                {
                                    int cLeave = PopInNeedLeave();
                                    inside[cLeave] = false;
                                    Sync(cLeave);
                                }
                            }
                        }
                    }
                }

                return true;
            }

            int minS = Math.Max(0, lCount - eCount);
            int maxS = lCount;
            if (!CanSimulate(maxS))
                return "CRIME TIME";

            int lo = minS;
            int hi = maxS;
            int bestS = maxS;
            while (lo <= hi)
            {
                int mid = lo + ((hi - lo) >> 1);
                if (CanSimulate(mid))
                {
                    bestS = mid;
                    hi = mid - 1;
                }
                else
                {
                    lo = mid + 1;
                }
            }

            return (bestS + eCount - lCount).ToString(CultureInfo.InvariantCulture);
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

        #region Question 4 - Willow

        private static Func<int> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n);
            int[] coins = new int[n];
            for (int i = 0; i < n; i++)
                test.Get(out coins[i]);
            (int U, int V)[] edges = new (int, int)[n - 1];
            for (int i = 0; i < n - 1; i++)
            {
                test.Get(out int j);
                edges[i] = (i, j - 1);
            }
            return () => Q4Solver(n, coins, edges);
        }

        private static int Q4Solver(int n, int[] coins, (int U, int V)[] edges)
        {
            List<int>[] adj = new List<int>[n];
            for (int i = 0; i < n; i++)
                adj[i] = [];

            int[,] edgeId = new int[n, n + 1];
            int eCount = 3 * n - 2;
            int[] eAt = new int[eCount];
            int[] eFrom = new int[eCount];

            for (int i = 0; i < edges.Length; i++)
            {
                var (u, v) = edges[i];
                adj[u].Add(v);
                adj[v].Add(u);

                int e0 = 2 * i;
                int e1 = 2 * i + 1;
                edgeId[u, v] = e0;
                eAt[e0] = u;
                eFrom[e0] = v;

                edgeId[v, u] = e1;
                eAt[e1] = v;
                eFrom[e1] = u;
            }

            for (int u = 0; u < n; u++)
            {
                int ev = 2 * (n - 1) + u;
                edgeId[u, n] = ev;
                eAt[ev] = u;
                eFrom[ev] = n;
            }

            // All-pairs first step on tree paths in O(N^2).
            int[,] nextNodeTo = new int[n, n];
            int[] queue = new int[n];
            for (int s = 0; s < n; s++)
            {
                for (int j = 0; j < n; j++)
                    nextNodeTo[s, j] = -1;
                nextNodeTo[s, s] = s;
                int head = 0, tail = 0;
                foreach (int nb in adj[s])
                {
                    nextNodeTo[s, nb] = nb;
                    queue[tail++] = nb;
                }
                while (head < tail)
                {
                    int cur = queue[head++];
                    int step = nextNodeTo[s, cur];
                    foreach (int nxt in adj[cur])
                    {
                        if (nextNodeTo[s, nxt] == -1)
                        {
                            nextNodeTo[s, nxt] = step;
                            queue[tail++] = nxt;
                        }
                    }
                }
            }

            int[] bestCoins = new int[eCount];
            Array.Fill(bestCoins, -1);
            int[] top0 = new int[eCount];
            int[] top1 = new int[eCount];
            int[] top2 = new int[eCount];
            Array.Fill(top0, -1);
            Array.Fill(top1, -1);
            Array.Fill(top2, -1);

            int ComputeBest(int eid)
            {
                if (bestCoins[eid] != -1)
                    return bestCoins[eid];

                int u = eAt[eid];
                int pi = eFrom[eid];
                int bestChild = 0;
                int n0 = -1, n1 = -1, n2 = -1;
                int b0 = -1, b1 = -1, b2 = -1;

                foreach (int nb in adj[u])
                {
                    if (nb == pi)
                        continue;
                    int val = ComputeBest(edgeId[nb, u]);
                    if (val > bestChild)
                        bestChild = val;

                    if (val > b0)
                    {
                        b2 = b1; n2 = n1;
                        b1 = b0; n1 = n0;
                        b0 = val; n0 = nb;
                    }
                    else if (val > b1)
                    {
                        b2 = b1; n2 = n1;
                        b1 = val; n1 = nb;
                    }
                    else if (val > b2)
                    {
                        b2 = val; n2 = nb;
                    }
                }

                top0[eid] = n0;
                top1[eid] = n1;
                top2[eid] = n2;
                return bestCoins[eid] = coins[u] + bestChild;
            }

            for (int e = 0; e < eCount; e++)
                ComputeBest(e);

            int NextBestExcept(int eid, int ex1, int ex2 = -1)
            {
                int c = top0[eid];
                if (c != ex1 && c != ex2) return c;
                c = top1[eid];
                if (c != ex1 && c != ex2) return c;
                c = top2[eid];
                if (c != ex1 && c != ex2) return c;
                return -1;
            }

            int MaxCoinsFromNeighbor(int nb, int from) =>
                nb == -1 ? 0 : bestCoins[edgeId[nb, from]];

            int[] memoBranch = new int[eCount * eCount];
            int[] memoRec = new int[eCount * eCount];
            Array.Fill(memoBranch, int.MinValue);
            Array.Fill(memoRec, int.MinValue);

            int BranchOff(int e1, int e2)
            {
                int key = e1 * eCount + e2;
                int cached = memoBranch[key];
                if (cached != int.MinValue)
                    return cached;

                int i = eAt[e1];
                int pi = eFrom[e1];
                int j = eAt[e2];
                int pj = eFrom[e2];

                int res;
                if (i == j)
                {
                    int n1 = NextBestExcept(e1, pj);
                    int n2 = NextBestExcept(e1, pj, n1);
                    res = MaxCoinsFromNeighbor(n2, i);
                }
                else
                {
                    int nj = nextNodeTo[j, i];
                    int optNow = MaxCoinsFromNeighbor(NextBestExcept(e2, nj), j);
                    int optLater = (nj == i ? 0 : coins[nj]) + BranchOff(e1, edgeId[nj, j]);
                    res = Math.Max(optNow, optLater);
                }

                return memoBranch[key] = res;
            }

            int Rec(int e1, int e2)
            {
                int key = e1 * eCount + e2;
                int cached = memoRec[key];
                if (cached != int.MinValue)
                    return cached;

                int i = eAt[e1];
                int pi = eFrom[e1];
                int j = eAt[e2];
                int pj = eFrom[e2];

                int res;
                if (i == j)
                {
                    int ni = NextBestExcept(e1, pj);
                    int nj = NextBestExcept(e1, pj, ni);
                    res = MaxCoinsFromNeighbor(ni, i) - MaxCoinsFromNeighbor(nj, i);
                }
                else
                {
                    int step = nextNodeTo[i, j];
                    int opt1 = (step == j ? 0 : coins[step]) - Rec(e2, edgeId[step, i]);
                    int nb = NextBestExcept(e1, step);
                    if (nb != -1)
                    {
                        int opt2 = MaxCoinsFromNeighbor(nb, i) - BranchOff(e1, e2);
                        res = Math.Max(opt1, opt2);
                    }
                    else
                    {
                        res = opt1;
                    }
                }

                return memoRec[key] = res;
            }

            int bestStart = int.MinValue;
            for (int u = 0; u < n; u++)
            {
                int e1 = edgeId[u, n];
                int worstReply = int.MaxValue;
                for (int v = 0; v < n; v++)
                {
                    int baseDiff = coins[u] - (u == v ? 0 : coins[v]);
                    int total = baseDiff + Rec(e1, edgeId[v, n]);
                    if (total < worstReply)
                        worstReply = total;
                }
                if (worstReply > bestStart)
                    bestStart = worstReply;
            }

            return bestStart;
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

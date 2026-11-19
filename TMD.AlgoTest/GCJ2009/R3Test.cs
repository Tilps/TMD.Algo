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

namespace TMD.AlgoTest.GCJ2009
{
    [TestFixture]
    public class R3Test : BaseGCJTest
    {
        #region Question 1 - EZ-Sokoban

        private static Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out int r, out int c);
            test.GetLines(r, out string[] board);
            return () => Q1Solver(r, c, board);
        }

        private static int Q1Solver(int rows, int cols, string[] board)
        {
            bool[] isWall = new bool[rows * cols];
            List<int> startBoxes = [];
            List<int> goalBoxes = [];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int cell = r * cols + c;
                    char ch = board[r][c];
                    if (ch == '#')
                    {
                        isWall[cell] = true;
                    }
                    else if (ch == 'o')
                    {
                        startBoxes.Add(cell);
                    }
                    else if (ch == 'x')
                    {
                        goalBoxes.Add(cell);
                    }
                    else if (ch == 'w')
                    {
                        startBoxes.Add(cell);
                        goalBoxes.Add(cell);
                    }
                }
            }

            int boxCount = startBoxes.Count;
            if (boxCount == 0)
                return 0;

            static long Pack(ReadOnlySpan<int> sortedBoxes)
            {
                long packed = 0;
                for (int i = sortedBoxes.Length - 1; i >= 0; i--)
                {
                    packed = (packed << 8) | (uint)sortedBoxes[i];
                }
                return packed;
            }

            static void Unpack(long state, Span<int> boxes)
            {
                for (int i = 0; i < boxes.Length; i++)
                {
                    boxes[i] = (int)(state & 0xFF);
                    state >>= 8;
                }
            }

            bool IsConnected(ReadOnlySpan<int> boxes)
            {
                if (boxes.Length <= 1)
                    return true;

                int visitedMask = 1;
                Span<int> queue = stackalloc int[5];
                int head = 0;
                int tail = 0;
                queue[tail++] = 0;

                while (head < tail)
                {
                    int cur = queue[head++];
                    int r1 = boxes[cur] / cols;
                    int c1 = boxes[cur] % cols;

                    for (int next = 0; next < boxes.Length; next++)
                    {
                        if ((visitedMask & (1 << next)) != 0)
                            continue;

                        int r2 = boxes[next] / cols;
                        int c2 = boxes[next] % cols;
                        if (Math.Abs(r1 - r2) + Math.Abs(c1 - c2) == 1)
                        {
                            visitedMask |= 1 << next;
                            queue[tail++] = next;
                        }
                    }
                }

                return visitedMask == (1 << boxes.Length) - 1;
            }

            startBoxes.Sort();
            goalBoxes.Sort();
            long startState = Pack(startBoxes.ToArray());
            long goalState = Pack(goalBoxes.ToArray());
            int[] dirs = [-cols, cols, -1, 1];
            int[] dRow = [-1, 1, 0, 0];
            int[] dCol = [0, 0, -1, 1];

            IEnumerable<long> GetNeighbors(long state)
            {
                int[] boxes = new int[boxCount];
                int[] nextBoxes = new int[boxCount];
                Unpack(state, boxes);
                bool stable = IsConnected(boxes);

                bool ContainsBox(int cell)
                {
                    for (int k = 0; k < boxCount; k++)
                    {
                        if (boxes[k] == cell)
                            return true;
                    }
                    return false;
                }

                List<long> nextStates = [];
                for (int i = 0; i < boxCount; i++)
                {
                    int cell = boxes[i];
                    int r = cell / cols;
                    int c = cell % cols;

                    for (int d = 0; d < 4; d++)
                    {
                        int fr = r + dRow[d];
                        int fc = c + dCol[d];
                        int br = r - dRow[d];
                        int bc = c - dCol[d];

                        if (fr < 0 || fr >= rows || fc < 0 || fc >= cols ||
                            br < 0 || br >= rows || bc < 0 || bc >= cols)
                        {
                            continue;
                        }

                        int frontCell = cell + dirs[d];
                        int backCell = cell - dirs[d];
                        if (isWall[frontCell] || isWall[backCell] ||
                            ContainsBox(frontCell) || ContainsBox(backCell))
                        {
                            continue;
                        }

                        boxes.AsSpan().CopyTo(nextBoxes);
                        nextBoxes[i] = frontCell;
                        nextBoxes.AsSpan().Sort();
                        if (!stable && !IsConnected(nextBoxes))
                            continue;

                        nextStates.Add(Pack(nextBoxes));
                    }
                }

                return nextStates;
            }

            return Search.Bfs(startState, goalState, GetNeighbors);
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

        #endregion

        #region Question 2 - Alphabetomials

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out string polynomial, out int k);
            test.Get(out int n);
            test.GetLines(n, out string[] words);
            return () => Q2Solver(polynomial, k, words);
        }

        private static string Q2Solver(string polynomial, int k, string[] words)
        {
            var mod = new Modulo(10009);
            int[,] wordFreq = new int[words.Length, 26];
            for (int i = 0; i < words.Length; i++)
            {
                foreach (char ch in words[i])
                {
                    wordFreq[i, ch - 'a']++;
                }
            }

            string[] terms = polynomial.Split('+');
            long[] totals = new long[k];

            foreach (string term in terms)
            {
                int m = term.Length;
                int maskCount = 1 << m;
                long[] singleWord = new long[maskCount];
                singleWord[0] = words.Length % 10009;

                for (int w = 0; w < words.Length; w++)
                {
                    long[] prod = new long[maskCount];
                    prod[0] = 1;
                    for (int bit = 0; bit < m; bit++)
                    {
                        int letterCount = wordFreq[w, term[bit] - 'a'];
                        int bitMask = 1 << bit;
                        for (int prevMask = 0; prevMask < bitMask; prevMask++)
                        {
                            prod[prevMask | bitMask] = mod.Multiply(prod[prevMask], letterCount);
                        }
                    }

                    for (int mask = 1; mask < maskCount; mask++)
                    {
                        singleWord[mask] = mod.Add(singleWord[mask], prod[mask]);
                    }
                }

                long[] dpCur = new long[maskCount];
                long[] dpNext = new long[maskCount];
                Array.Copy(singleWord, dpCur, maskCount);
                int fullMask = maskCount - 1;
                totals[0] = mod.Add(totals[0], dpCur[fullMask]);

                for (int step = 1; step < k; step++)
                {
                    for (int mask = 0; mask < maskCount; mask++)
                    {
                        long sum = 0;
                        for (int sub = mask; ; sub = BitTricks.PrevSubset(sub, mask))
                        {
                            sum = (sum + dpCur[sub] * singleWord[mask ^ sub]) % 10009;
                            if (sub == 0)
                                break;
                        }
                        dpNext[mask] = sum;
                    }

                    totals[step] = mod.Add(totals[step], dpNext[fullMask]);
                    (dpCur, dpNext) = (dpNext, dpCur);
                }
            }

            return string.Join(' ', totals);
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

        #endregion

        #region Question 3 - Football Team

        private static Func<int> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out (int X, int Y)[] players);
            return () => Q3Solver(players);
        }

        private static int Q3Solver((int X, int Y)[] players)
        {
            int n = players.Length;
            Array.Sort(players, (a, b) => a.X.CompareTo(b.X));

            var graph = new Graph<int, int>();
            List<int>[] adj = new List<int>[n];
            for (int i = 0; i < n; i++)
            {
                graph.AddVertex(i);
                adj[i] = [];
            }

            int[] nextInRow = new int[32];
            Array.Fill(nextInRow, -1);

            for (int i = n - 1; i >= 0; i--)
            {
                int y = players[i].Y;
                for (int dy = -1; dy <= 1; dy++)
                {
                    int targetRow = y + dy;
                    if (targetRow >= 1 && targetRow <= 30 && nextInRow[targetRow] != -1)
                    {
                        int j = nextInRow[targetRow];
                        graph.AddUndirectedEdge(i, j, 1);
                        adj[i].Add(j);
                        adj[j].Add(i);
                    }
                }
                nextInRow[y] = i;
            }

            if (graph.EdgeCount == 0)
                return 1;

            if (IsTwoColorable(n, adj))
                return 2;

            List<List<int>> blocks = graph.GetBiconnectedVertexComponents();
            int[] globalToLocal = new int[n];
            Array.Fill(globalToLocal, -1);

            foreach (List<int> block in blocks)
            {
                if (block.Count <= 2)
                    continue;

                if (!IsBlockThreeColorable(block, adj, globalToLocal))
                    return 4;
            }

            return 3;
        }

        private static bool IsTwoColorable(int n, List<int>[] adj)
        {
            int[] color = new int[n];
            Array.Fill(color, -1);
            Queue<int> queue = new();

            for (int start = 0; start < n; start++)
            {
                if (color[start] != -1)
                    continue;

                color[start] = 0;
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    int u = queue.Dequeue();
                    int nextColor = color[u] ^ 1;
                    foreach (int v in adj[u])
                    {
                        if (color[v] == -1)
                        {
                            color[v] = nextColor;
                            queue.Enqueue(v);
                        }
                        else if (color[v] != nextColor)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private static bool IsBlockThreeColorable(List<int> block, List<int>[] globalAdj, int[] globalToLocal)
        {
            int m = block.Count;
            for (int i = 0; i < m; i++)
                globalToLocal[block[i]] = i;

            List<int>[] localAdj = new List<int>[m];
            int[] deg = new int[m];
            bool[] removed = new bool[m];
            for (int i = 0; i < m; i++)
            {
                localAdj[i] = [];
                foreach (int gv in globalAdj[block[i]])
                {
                    int lv = globalToLocal[gv];
                    if (lv >= 0)
                        localAdj[i].Add(lv);
                }
                deg[i] = localAdj[i].Count;
            }

            for (int i = 0; i < m; i++)
                globalToLocal[block[i]] = -1;

            // Iteratively peel vertices of degree <= 2.
            Queue<int> peelQueue = new();
            for (int i = 0; i < m; i++)
            {
                if (deg[i] <= 2)
                {
                    removed[i] = true;
                    peelQueue.Enqueue(i);
                }
            }

            int remainingCount = m;
            while (peelQueue.Count > 0)
            {
                int u = peelQueue.Dequeue();
                remainingCount--;
                foreach (int v in localAdj[u])
                {
                    if (!removed[v] && --deg[v] <= 2)
                    {
                        removed[v] = true;
                        peelQueue.Enqueue(v);
                    }
                }
            }

            if (remainingCount == 0)
                return true;

            int[] coreMap = new int[m];
            Array.Fill(coreMap, -1);
            int coreSize = 0;
            for (int i = 0; i < m; i++)
            {
                if (!removed[i])
                    coreMap[i] = coreSize++;
            }

            int[][] coreAdj = new int[coreSize][];
            for (int i = 0; i < m; i++)
            {
                int cu = coreMap[i];
                if (cu < 0)
                    continue;
                List<int> list = [];
                foreach (int lv in localAdj[i])
                {
                    if (coreMap[lv] >= 0)
                        list.Add(coreMap[lv]);
                }
                coreAdj[cu] = [.. list];
            }

            int[] color = new int[coreSize];
            int[] avail = new int[coreSize];
            Array.Fill(avail, 0b111);
            int unassigned = coreSize;
            Stack<(int Node, int PrevAvail)> undo = new();

            bool Assign(int u, int cMask, int checkpoint)
            {
                color[u] = cMask;
                avail[u] = cMask;
                unassigned--;
                Queue<int> forced = new();

                foreach (int v in coreAdj[u])
                {
                    if (color[v] != 0)
                    {
                        if (color[v] == cMask)
                            return false;
                        continue;
                    }

                    int nextMask = avail[v] & ~cMask;
                    if (nextMask == 0)
                        return false;
                    if (nextMask != avail[v])
                    {
                        undo.Push((v, avail[v]));
                        avail[v] = nextMask;
                        if (BitOperations.PopCount((uint)nextMask) == 1)
                            forced.Enqueue(v);
                    }
                }

                while (forced.Count > 0)
                {
                    int f = forced.Dequeue();
                    if (color[f] != 0)
                    {
                        if (color[f] != avail[f])
                            return false;
                        continue;
                    }
                    int fMask = avail[f];
                    color[f] = fMask;
                    unassigned--;
                    foreach (int v in coreAdj[f])
                    {
                        if (color[v] != 0)
                        {
                            if (color[v] == fMask)
                                return false;
                            continue;
                        }
                        int nextMask = avail[v] & ~fMask;
                        if (nextMask == 0)
                            return false;
                        if (nextMask != avail[v])
                        {
                            undo.Push((v, avail[v]));
                            avail[v] = nextMask;
                            if (BitOperations.PopCount((uint)nextMask) == 1)
                                forced.Enqueue(v);
                        }
                    }
                }

                return true;
            }

            void Rollback(int targetUndoCount, int[] savedColor, int savedUnassigned)
            {
                while (undo.Count > targetUndoCount)
                {
                    var (node, prevAvail) = undo.Pop();
                    avail[node] = prevAvail;
                }
                Array.Copy(savedColor, color, coreSize);
                unassigned = savedUnassigned;
            }

            // Break symmetry on vertex 0 and one of its neighbors.
            if (!Assign(0, 1, 0))
                return false;
            if (unassigned > 0 && coreAdj[0].Length > 0 && !Assign(coreAdj[0][0], 2, undo.Count))
                return false;

            bool SearchColoring()
            {
                if (unassigned == 0)
                    return true;

                int pick = -1;
                int bestCount = 4;
                for (int i = 0; i < coreSize; i++)
                {
                    if (color[i] != 0)
                        continue;
                    int cnt = BitOperations.PopCount((uint)avail[i]);
                    if (cnt < bestCount)
                    {
                        bestCount = cnt;
                        pick = i;
                        if (cnt == 1)
                            break;
                    }
                }

                int mask = avail[pick];
                int[] savedColor = new int[coreSize];
                Array.Copy(color, savedColor, coreSize);
                int savedUnassigned = unassigned;
                int savedUndoCount = undo.Count;
                int savedPickAvail = avail[pick];

                for (int bit = 1; bit <= 4; bit <<= 1)
                {
                    if ((mask & bit) == 0)
                        continue;

                    if (Assign(pick, bit, savedUndoCount) && SearchColoring())
                        return true;

                    Rollback(savedUndoCount, savedColor, savedUnassigned);
                    avail[pick] = savedPickAvail;
                }

                return false;
            }

            return SearchColoring();
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

        #region Question 4 - Interesting Ranges

        private static readonly Modulo ModQ4 = new(1_000_000_007);

        private static Func<long> Q4Solver(GCJTestCase test)
        {
            test.Get(out BigInteger l, out BigInteger r);
            return () => Q4Solver(l, r);
        }

        private static long Q4Solver(BigInteger l, BigInteger r)
        {
            BigInteger totalCount = r - l + 2;
            BigInteger ones = CountOddPalindromePrefixes(r) - (l >= 2 ? CountOddPalindromePrefixes(l - 2) : BigInteger.Zero);
            BigInteger zeros = totalCount - ones;

            long Choose2Mod(BigInteger count)
            {
                long a = (long)(count % 1_000_000_007);
                long num = ModQ4.Multiply(a, ModQ4.Subtract(a, 1));
                return ModQ4.Divide(num, 2);
            }

            return ModQ4.Add(Choose2Mod(ones), Choose2Mod(zeros));
        }

        internal static BigInteger CountOddPalindromePrefixes(BigInteger x)
        {
            if (x <= 0)
                return BigInteger.Zero;

            int smallLimit = x <= 100 ? (int)x : 100;
            int onesSmall = 0;
            bool odd = false;
            for (int i = 1; i <= smallLimit; i++)
            {
                if (IsSmallPalindrome(i))
                    odd = !odd;
                if (odd)
                    onesSmall++;
            }

            if (x <= 100)
                return onesSmall;

            BigInteger totalOnes = onesSmall;
            BigInteger tenPowDMinus1 = 100;
            for (int d = 3; ; d++)
            {
                BigInteger firstPal = tenPowDMinus1 + 1;
                if (firstPal > x)
                    break;

                int halfLen = (d + 1) / 2;
                BigInteger p10 = BigInteger.Pow(10, halfLen - 1);
                BigInteger blockLen = (d & 1) == 1 ? p10 : 11 * p10;
                BigInteger tenPowD = tenPowDMinus1 * 10;
                BigInteger lastPal = tenPowD - 1;

                if (x >= lastPal)
                {
                    BigInteger evenBlockCount = 45 * BigInteger.Pow(10, halfLen - 2);
                    totalOnes += evenBlockCount * blockLen;
                    tenPowDMinus1 = tenPowD;
                    continue;
                }

                string s = x.ToString();
                BigInteger h = BigInteger.Parse(s[..halfLen]);
                if (BuildPalindrome(h, d) > x)
                    h--;

                BigInteger completeEven = (h - p10 + 1) / 2;
                totalOnes += completeEven * blockLen;
                if (!h.IsEven)
                {
                    return totalOnes;
                }

                totalOnes += x - BuildPalindrome(h, d) + 1;
                return totalOnes;
            }

            return totalOnes;
        }

        private static BigInteger BuildPalindrome(BigInteger half, int totalDigits)
        {
            string left = half.ToString();
            int mirrorLen = totalDigits - left.Length;
            char[] full = new char[totalDigits];
            for (int i = 0; i < left.Length; i++)
                full[i] = left[i];
            for (int i = 0; i < mirrorLen; i++)
                full[left.Length + i] = left[mirrorLen - 1 - i];
            return BigInteger.Parse(full);
        }

        private static bool IsSmallPalindrome(int v)
        {
            if (v < 10)
                return true;
            return v % 10 == v / 10;
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

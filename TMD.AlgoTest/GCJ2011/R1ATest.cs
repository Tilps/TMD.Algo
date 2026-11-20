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
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2011
{
    [TestFixture]
    public class R1ATest : BaseGCJTest
    {
        #region Question 1 - FreeCell Statistics

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out long n, out int pd, out int pg);
            return () => Q1Solver(n, pd, pg);
        }

        private static string Q1Solver(long n, int pd, int pg)
        {
            if ((pg == 100 && pd < 100) || (pg == 0 && pd > 0))
                return "Broken";

            long minD = 100 / MathFormulas.Gcd(100, pd);
            return minD <= n ? "Possible" : "Broken";
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

        #region Question 2 - The Killer Word

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n, out int m);
            test.GetLines(n, out string[] words);
            test.GetLines(m, out string[] lists);
            return () => Q2Solver(words, lists);
        }

        private static string Q2Solver(string[] words, string[] lists)
        {
            int n = words.Length;
            int[] charMask = new int[n];
            ushort[] posMask = new ushort[n * 26];
            List<int>[] byLen = new List<int>[11];
            for (int len = 1; len <= 10; len++)
                byLen[len] = [];

            for (int i = 0; i < n; i++)
            {
                string w = words[i];
                int cm = 0;
                int rowOffset = i * 26;
                for (int p = 0; p < w.Length; p++)
                {
                    int c = w[p] - 'a';
                    cm |= 1 << c;
                    posMask[rowOffset + c] |= (ushort)(1 << p);
                }
                charMask[i] = cm;
                byLen[w.Length].Add(i);
            }

            int[] groupUnion = new int[11];
            for (int len = 1; len <= 10; len++)
            {
                int um = 0;
                foreach (int w in byLen[len])
                    um |= charMask[w];
                groupUnion[len] = um;
            }

            int[] buf = new int[n];
            var letterComparers = new Comparer<int>[26];
            for (int c = 0; c < 26; c++)
            {
                int col = c;
                letterComparers[c] = Comparer<int>.Create((a, b) => posMask[a * 26 + col].CompareTo(posMask[b * 26 + col]));
            }

            string[] answers = new string[lists.Length];
            for (int li = 0; li < lists.Length; li++)
            {
                string list = lists[li];
                int bestMistakes = -1;
                int bestIdx = int.MaxValue;

                void RecordWord(int wordIdx, int mistakes)
                {
                    if (mistakes > bestMistakes || (mistakes == bestMistakes && wordIdx < bestIdx))
                    {
                        bestMistakes = mistakes;
                        bestIdx = wordIdx;
                    }
                }

                void Refine(int lo, int hi, int step, int mistakes, int unionMask)
                {
                    if (hi - lo == 1)
                    {
                        RecordWord(buf[lo], mistakes);
                        return;
                    }

                    while (step < 26 && (unionMask & (1 << (list[step] - 'a'))) == 0)
                        step++;

                    if (step >= 26)
                    {
                        for (int i = lo; i < hi; i++)
                            RecordWord(buf[i], mistakes);
                        return;
                    }

                    int c = list[step] - 'a';
                    int mid = lo;
                    int zeroUnion = 0;
                    for (int i = lo; i < hi; i++)
                    {
                        int w = buf[i];
                        if (posMask[w * 26 + c] == 0)
                        {
                            zeroUnion |= charMask[w];
                            buf[i] = buf[mid];
                            buf[mid] = w;
                            mid++;
                        }
                    }

                    if (mid > lo)
                        Refine(lo, mid, step + 1, mistakes + 1, zeroUnion);

                    if (hi - mid == 1)
                    {
                        RecordWord(buf[mid], mistakes);
                    }
                    else
                    {
                        Array.Sort(buf, mid, hi - mid, letterComparers[c]);
                        int runStart = mid;
                        while (runStart < hi)
                        {
                            ushort pm = posMask[buf[runStart] * 26 + c];
                            int runUnion = charMask[buf[runStart]];
                            int runEnd = runStart + 1;
                            while (runEnd < hi && posMask[buf[runEnd] * 26 + c] == pm)
                            {
                                runUnion |= charMask[buf[runEnd]];
                                runEnd++;
                            }
                            Refine(runStart, runEnd, step + 1, mistakes, runUnion);
                            runStart = runEnd;
                        }
                    }
                }

                for (int len = 1; len <= 10; len++)
                {
                    var grp = byLen[len];
                    if (grp.Count == 0)
                        continue;
                    if (grp.Count == 1)
                    {
                        RecordWord(grp[0], 0);
                        continue;
                    }
                    grp.CopyTo(buf, 0);
                    Refine(0, grp.Count, 0, 0, groupUnion[len]);
                }

                answers[li] = words[bestIdx];
            }

            return string.Join(" ", answers);
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

        #region Question 3 - Pseudominion

        private readonly record struct Card(int Index, int Draw, int Score, int Turns);

        private static Func<int> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n);
            List<Card> cards = [];
            for (int i = 0; i < n; i++)
            {
                test.Get(out int c, out int s, out int t);
                cards.Add(new Card(i, c, s, t));
            }
            test.Get(out int m);
            for (int i = 0; i < m; i++)
            {
                test.Get(out int c, out int s, out int t);
                cards.Add(new Card(n + i, c, s, t));
            }
            return () => Q3Solver(n, cards);
        }

        private static int Q3Solver(int initialHand, List<Card> cards)
        {
            int k = cards.Count;
            List<Card> tCards = [];
            List<Card> c0Cards = [];
            List<Card> c1Cards = [];
            List<Card> c2Cards = [];

            foreach (Card card in cards)
            {
                if (card.Turns > 0)
                    tCards.Add(card);
                else if (card.Draw == 0)
                    c0Cards.Add(card);
                else if (card.Draw == 1)
                    c1Cards.Add(card);
                else
                    c2Cards.Add(card);
            }

            int[] handForDraw = new int[k + 1];
            int[] tScore = new int[k + 1];
            int[] tTurns = new int[k + 1];

            for (int d = 0; d <= k; d++)
            {
                int h = Math.Min(k, initialHand + d);
                int score = 0;
                int netTurns = 1;
                while (true)
                {
                    int extraDraw = 0;
                    score = 0;
                    netTurns = 1;
                    for (int i = 0; i < tCards.Count; i++)
                    {
                        if (tCards[i].Index >= h)
                            break;
                        extraDraw += tCards[i].Draw;
                        score += tCards[i].Score;
                        netTurns += tCards[i].Turns - 1;
                    }
                    int nh = Math.Min(k, initialHand + d + extraDraw);
                    if (nh == h)
                        break;
                    h = nh;
                }
                handForDraw[d] = h;
                tScore[d] = score;
                tTurns[d] = netTurns;
            }

            int n1 = c1Cards.Count;
            int n2 = c2Cards.Count;
            int dim1 = (n1 + 1) * (n1 + 2) / 2;
            int dim2 = (n2 + 1) * (n2 + 2) / 2;
            int[] memo = new int[dim1 * dim2];
            Array.Fill(memo, -1);

            List<int> scratch = [with(capacity: k)];

            int TopScoreSum(int hand, int turns, int c1, int c2)
            {
                scratch.Clear();
                for (int i = 0; i < c0Cards.Count; i++)
                {
                    if (c0Cards[i].Index < hand)
                        scratch.Add(c0Cards[i].Score);
                    else
                        break;
                }
                for (int i = c1; i < n1; i++)
                {
                    if (c1Cards[i].Index < hand)
                        scratch.Add(c1Cards[i].Score);
                    else
                        break;
                }
                for (int i = c2; i < n2; i++)
                {
                    if (c2Cards[i].Index < hand)
                        scratch.Add(c2Cards[i].Score);
                    else
                        break;
                }
                scratch.Sort((a, b) => b.CompareTo(a));
                int take = Math.Min(turns, scratch.Count);
                int sum = 0;
                for (int i = 0; i < take; i++)
                    sum += scratch[i];
                return sum;
            }

            int Solve(int c1, int c2, int p1, int p2)
            {
                int d = Math.Min(k, p1 + 2 * p2);
                int hand = handForDraw[d];
                int turns = tTurns[d] - (p1 + p2);
                if (turns <= 0)
                    return 0;

                int i1 = c1 * (c1 + 1) / 2 + p1;
                int i2 = c2 * (c2 + 1) / 2 + p2;
                int key = i1 * dim2 + i2;
                if (memo[key] >= 0)
                    return memo[key];

                if (turns == 1 || hand == k)
                {
                    int terminal = TopScoreSum(hand, turns, c1, c2);
                    memo[key] = terminal;
                    return terminal;
                }

                // Option 0: stop playing Draw > 0 cards and greedily take C0 cards in current hand.
                int best = TopScoreSum(hand, turns, n1, n2);

                if (c1 < n1 && c1Cards[c1].Index < hand)
                {
                    int nd = Math.Min(k, d + 1);
                    int playC1 = c1Cards[c1].Score + (tScore[nd] - tScore[d]) + Solve(c1 + 1, c2, p1 + 1, p2);
                    int skipC1 = Solve(c1 + 1, c2, p1, p2);
                    best = Math.Max(best, Math.Max(playC1, skipC1));
                }

                if (c2 < n2 && c2Cards[c2].Index < hand)
                {
                    int nd = Math.Min(k, d + 2);
                    int playC2 = c2Cards[c2].Score + (tScore[nd] - tScore[d]) + Solve(c1, c2 + 1, p1, p2 + 1);
                    int skipC2 = Solve(c1, c2 + 1, p1, p2);
                    best = Math.Max(best, Math.Max(playC2, skipC2));
                }

                memo[key] = best;
                return best;
            }

            return tScore[0] + Solve(0, 0, 0, 0);
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

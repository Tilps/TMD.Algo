using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2019;

[TestFixture]
public class R2Test : BaseGCJTest
{
    #region Q1 New Elements: Part 1

    [Test]
    public void Q1Sample() => Test(Q1);

    [Test]
    public void Q1Small() => Test(Q1);

    [Test]
    public void Q1Large() => Test(Q1);

    private static Func<string> Q1(GCJTestCase testCase)
    {
        testCase.Get(out int n);
        testCase.GetLines(n, out long[] c, out long[] j);
        return () =>
        {
            HashSet<(long Num, long Den)> slopes = [];
            for (int a = 0; a < n; a++)
            {
                for (int b = a + 1; b < n; b++)
                {
                    long dc = c[a] - c[b];
                    long dj = j[b] - j[a];
                    if ((dc > 0 && dj > 0) || (dc < 0 && dj < 0))
                    {
                        long num = Math.Abs(dc);
                        long den = Math.Abs(dj);
                        long g = MathFormulas.Gcd(num, den);
                        slopes.Add((num / g, den / g));
                    }
                }
            }

            return (slopes.Count + 1).ToString(CultureInfo.InvariantCulture);
        };
    }

    #endregion

    #region Q2 Pottery Lottery

    [Test]
    public void Q2Small() => InteractiveTest(Q2Solve, Q2Judge);

    private static void Q2Solve(GCJInteractiveChannel io)
    {
        const int numVases = 20;
        const int fillVases = 14;
        const int fillPerVase = 5;

        int t = int.Parse(io.ReadRequiredLine(), CultureInfo.InvariantCulture);
        for (int tc = 0; tc < t; tc++)
        {
            int[] estCounts = new int[numVases + 1];
            int cand1 = -1;
            int cand2 = -1;
            int winVase = -1;
            int loseVase = -1;

            for (int day = 1; day <= 100; day++)
            {
                int readDay = int.Parse(io.ReadRequiredLine(), CultureInfo.InvariantCulture);
                Assert.AreEqual(day, readDay, $"Case #{tc + 1}: day mismatch.");

                if (day <= fillVases * fillPerVase)
                {
                    int vase = ((day - 1) % fillVases) + 1;
                    io.WriteLine($"{vase} 1");
                }
                else if (day <= fillVases * fillPerVase + (numVases - fillVases))
                {
                    int vase = fillVases + (day - fillVases * fillPerVase);
                    io.WriteLine($"{vase} 0");
                    string[] resp = io.ReadRequiredLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    estCounts[vase] = int.Parse(resp[0], CultureInfo.InvariantCulture);

                    if (day == fillVases * fillPerVase + (numVases - fillVases))
                    {
                        cand1 = fillVases + 1;
                        cand2 = fillVases + 2;
                        if (estCounts[cand2] < estCounts[cand1])
                            (cand1, cand2) = (cand2, cand1);

                        for (int v = fillVases + 3; v <= numVases; v++)
                        {
                            if (estCounts[v] < estCounts[cand1])
                            {
                                cand2 = cand1;
                                cand1 = v;
                            }
                            else if (estCounts[v] < estCounts[cand2])
                            {
                                cand2 = v;
                            }
                        }
                    }
                }
                else if (day <= 94)
                {
                    int bestVase = -1;
                    int lowestEst = int.MaxValue;
                    for (int v = fillVases + 1; v <= numVases; v++)
                    {
                        if (v == cand1 || v == cand2)
                            continue;
                        if (estCounts[v] < lowestEst)
                        {
                            lowestEst = estCounts[v];
                            bestVase = v;
                        }
                    }
                    estCounts[bestVase]++;
                    io.WriteLine($"{bestVase} 1");
                }
                else if (day == 95)
                {
                    io.WriteLine($"{cand1} 0");
                    string[] resp = io.ReadRequiredLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    estCounts[cand1] = int.Parse(resp[0], CultureInfo.InvariantCulture);
                }
                else if (day == 96)
                {
                    io.WriteLine($"{cand2} 0");
                    string[] resp = io.ReadRequiredLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    estCounts[cand2] = int.Parse(resp[0], CultureInfo.InvariantCulture);

                    if (estCounts[cand1] <= estCounts[cand2])
                    {
                        winVase = cand1;
                        loseVase = cand2;
                    }
                    else
                    {
                        winVase = cand2;
                        loseVase = cand1;
                    }
                }
                else if (day <= 99)
                {
                    io.WriteLine($"{loseVase} 1");
                }
                else
                {
                    io.WriteLine($"{winVase} 100");
                }
            }
        }
    }

    private static void Q2Judge(GCJInteractiveChannel io)
    {
        const int totalCases = 250;
        const int requiredSuccesses = 225;
        const int nPeople = 100;
        const int nVases = 20;
        Random rng = new(412435 + 1);

        io.WriteLine(totalCases.ToString(CultureInfo.InvariantCulture));
        int wins = 0;

        for (int tc = 1; tc <= totalCases; tc++)
        {
            int[] choices = new int[nPeople - 1];
            for (int i = 0; i < choices.Length; i++)
                choices[i] = rng.Next(1, nVases + 1);
            rng.Shuffle(choices);

            List<int>[] vases = new List<int>[nVases];
            for (int v = 0; v < nVases; v++)
                vases[v] = [];

            for (int day = 1; day <= nPeople; day++)
            {
                io.WriteLine(day.ToString(CultureInfo.InvariantCulture));
                if (day <= nPeople - 1)
                    vases[choices[day - 1] - 1].Add(day);

                string line = io.ReadRequiredLine();
                Assert.IsFalse(line.Contains('\n') || line.Contains('\r'));
                string[] tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(2, tokens.Length, $"Case #{tc}: expected 2 tokens.");

                int vase = int.Parse(tokens[0], CultureInfo.InvariantCulture);
                int person = int.Parse(tokens[1], CultureInfo.InvariantCulture);
                Assert.That(vase, Is.InRange(1, nVases));
                Assert.That(person, Is.InRange(0, nPeople));

                if (person > 0)
                {
                    if (day == nPeople)
                        Assert.AreEqual(nPeople, person, $"Case #{tc}: must place own token on last day.");
                    vases[vase - 1].Add(person);
                }
                else
                {
                    Assert.Less(day, nPeople, $"Case #{tc}: cannot inspect on last day.");
                    List<int> content = [.. vases[vase - 1]];
                    content.Sort();
                    if (content.Count == 0)
                        io.WriteLine("0");
                    else
                        io.WriteLine($"{content.Count} {string.Join(" ", content)}");
                }
            }

            if (Won(vases))
                wins++;
        }

        Assert.IsNull(io.ReadLine(), "Additional input after all cases finish.");
        Assert.GreaterOrEqual(wins, requiredSuccesses, $"Won {wins}/{totalCases}, need {requiredSuccesses}.");

        static bool Won(List<int>[] vases)
        {
            int minCnt = int.MaxValue;
            foreach (var v in vases)
                minCnt = Math.Min(minCnt, v.Count);

            int winIdx = -1;
            int minTieCount = 0;
            for (int i = 0; i < vases.Length; i++)
            {
                if (vases[i].Count == minCnt)
                {
                    winIdx = i;
                    minTieCount++;
                }
            }

            if (minTieCount != 1)
                return false;

            List<int> winVase = vases[winIdx];
            HashSet<int> distinct = [];
            bool hasPlayer = false;
            foreach (int token in winVase)
            {
                if (!distinct.Add(token))
                    return false;
                if (token == nPeople)
                    hasPlayer = true;
            }
            return hasPlayer;
        }
    }

    #endregion

    #region Q3 New Elements: Part 2

    [Test]
    public void Q3Sample() => Test(Q3);

    [Test]
    public void Q3Small() => Test(Q3);

    [Test]
    public void Q3Large() => Test(Q3);

    private static Func<string> Q3(GCJTestCase testCase)
    {
        testCase.Get(out int n);
        testCase.GetLines(n, out long[] c, out long[] j);
        return () =>
        {
            BigFraction low = BigFraction.Zero;
            BigFraction? high = null;

            for (int i = 0; i + 1 < n; i++)
            {
                long dc = c[i + 1] - c[i];
                long dj = j[i + 1] - j[i];
                if (dc <= 0 && dj <= 0)
                    return "IMPOSSIBLE";

                if (dj > 0 && dc < 0)
                {
                    BigFraction bound = new(-dc, dj);
                    if (bound > low)
                        low = bound;
                }
                else if (dj < 0 && dc > 0)
                {
                    BigFraction bound = new(dc, -dj);
                    if (!high.HasValue || bound < high.Value)
                        high = bound;
                }
            }

            if (high.HasValue && low >= high.Value)
                return "IMPOSSIBLE";

            BigFraction best = BigFraction.SimplestBetween(low, high);
            return $"{best.Denominator} {best.Numerator}";
        };
    }

    #endregion

    #region Q4 Contransmutation

    [Test]
    public void Q4Sample() => Test(Q4);

    [Test]
    public void Q4Small1() => Test(Q4);

    [Test]
    public void Q4Small2() => Test(Q4);

    [Test]
    public void Q4Large() => Test(Q4);

    private static Func<string> Q4(GCJTestCase testCase)
    {
        testCase.Get(out int m);
        testCase.GetLines(m, out int[] r1Raw, out int[] r2Raw);
        testCase.Get(out long[] g);
        return () =>
        {
            const long mod = 1_000_000_007L;
            int[] r1 = new int[m];
            int[] r2 = new int[m];
            for (int i = 0; i < m; i++)
            {
                r1[i] = r1Raw[i] - 1;
                r2[i] = r2Raw[i] - 1;
            }

            bool[] producible = new bool[m];
            Queue<int> queue = new();
            for (int i = 0; i < m; i++)
            {
                if (g[i] > 0)
                {
                    producible[i] = true;
                    queue.Enqueue(i);
                }
            }

            while (queue.Count > 0)
            {
                int u = queue.Dequeue();
                if (!producible[r1[u]])
                {
                    producible[r1[u]] = true;
                    queue.Enqueue(r1[u]);
                }
                if (!producible[r2[u]])
                {
                    producible[r2[u]] = true;
                    queue.Enqueue(r2[u]);
                }
            }

            if (!producible[0])
                return "0";

            List<int>[] revProd = new List<int>[m];
            for (int i = 0; i < m; i++)
                revProd[i] = [];

            var graph = new Graph<int, int>();
            for (int i = 0; i < m; i++)
                graph.AddVertex(i);

            for (int u = 0; u < m; u++)
            {
                if (!producible[u])
                    continue;
                revProd[r1[u]].Add(u);
                revProd[r2[u]].Add(u);
                graph.AddDirectedEdge(u, r1[u], 1);
                if (r2[u] != r1[u])
                    graph.AddDirectedEdge(u, r2[u], 1);
            }

            bool[] canReachLead = new bool[m];
            canReachLead[0] = true;
            queue.Enqueue(0);
            while (queue.Count > 0)
            {
                int v = queue.Dequeue();
                foreach (int u in revProd[v])
                {
                    if (!canReachLead[u])
                    {
                        canReachLead[u] = true;
                        queue.Enqueue(u);
                    }
                }
            }

            List<List<int>> sccs = graph.GetStronglyConnectedComponents();
            int[] sccId = new int[m];
            for (int id = 0; id < sccs.Count; id++)
            {
                foreach (int v in sccs[id])
                    sccId[v] = id;
            }

            for (int u = 0; u < m; u++)
            {
                if (!producible[u])
                    continue;
                int a = r1[u];
                int b = r2[u];
                if ((canReachLead[a] && sccId[b] == sccId[u]) ||
                    (canReachLead[b] && sccId[a] == sccId[u]))
                {
                    return "UNBOUNDED";
                }
            }

            // On the remaining canReachLead subgraph with lead's outgoing edges removed,
            // edges form a DAG toward lead (vertex 0). Propagate DP backwards from lead.
            int[] remOut = new int[m];
            List<int>[] revDag = new List<int>[m];
            for (int i = 0; i < m; i++)
                revDag[i] = [];

            for (int u = 1; u < m; u++)
            {
                if (!canReachLead[u])
                    continue;
                if (canReachLead[r1[u]])
                {
                    remOut[u]++;
                    revDag[r1[u]].Add(u);
                }
                if (canReachLead[r2[u]])
                {
                    remOut[u]++;
                    revDag[r2[u]].Add(u);
                }
            }

            long[] ways = new long[m];
            ways[0] = 1;
            queue.Enqueue(0);
            while (queue.Count > 0)
            {
                int v = queue.Dequeue();
                long wv = ways[v];
                foreach (int u in revDag[v])
                {
                    ways[u] += wv;
                    if (ways[u] >= mod)
                        ways[u] -= mod;
                    if (--remOut[u] == 0)
                        queue.Enqueue(u);
                }
            }

            long ans = 0;
            for (int u = 0; u < m; u++)
            {
                if (canReachLead[u] && g[u] > 0)
                    ans = (ans + (g[u] % mod) * ways[u]) % mod;
            }

            return ans.ToString(CultureInfo.InvariantCulture);
        };
    }

    #endregion
}

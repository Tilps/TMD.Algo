using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2019;

[TestFixture]
public class FinalsTest : BaseGCJTest
{
    #region Q1 Board Meeting

    [Test]
    public void Q1Small() => InteractiveTest(Q1Solve, io => Q1Judge(io, testNumber: 0));

    [Test]
    public void Q1Large() => InteractiveTest(Q1Solve, io => Q1Judge(io, testNumber: 1));

    private static void Q1Solve(GCJInteractiveChannel io)
    {
        string[] header = io.ReadRequiredLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int t = int.Parse(header[0], CultureInfo.InvariantCulture);
        long m = long.Parse(header[2], CultureInfo.InvariantCulture);

        for (int tc = 0; tc < t; tc++)
        {
            long shift = 4 * m;
            long loK = 2 * m;
            long hiK = 6 * m;

            List<long> uList = [];
            RecoverTransformedCoords(-shift, signB: 1, shift, loK, hiK, uList);

            List<long> vList = [];
            RecoverTransformedCoords(-shift, signB: -1, shift, loK, hiK, vList);

            io.WriteLine("READY");
            while (true)
            {
                string line = io.ReadRequiredLine();
                if (line.Equals("DONE", StringComparison.OrdinalIgnoreCase))
                    break;
                if (line.Equals("ERROR", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Case #{tc + 1}: judge returned ERROR.");

                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                long cx = long.Parse(parts[0], CultureInfo.InvariantCulture);
                long cy = long.Parse(parts[1], CultureInfo.InvariantCulture);

                long sumU = 0;
                long cu = cx + cy;
                foreach (long u in uList)
                    sumU += Math.Abs(cu - u);

                long sumV = 0;
                long cv = cx - cy;
                foreach (long v in vList)
                    sumV += Math.Abs(cv - v);

                long ans = (sumU + sumV) >> 1;
                io.WriteLine(ans.ToString(CultureInfo.InvariantCulture));
            }
        }

        long Query(long a, long b)
        {
            io.WriteLine($"{a} {b}");
            string resp = io.ReadRequiredLine();
            if (resp.Equals("ERROR", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Judge returned ERROR for query ({a}, {b}).");
            return long.Parse(resp, CultureInfo.InvariantCulture);
        }

        void RecoverTransformedCoords(long fixedA, long signB, long shift, long loK, long hiK, List<long> outputCoords)
        {
            Dictionary<long, long> cache = [];

            long GetG(long k)
            {
                if (!cache.TryGetValue(k, out long val))
                {
                    val = Query(fixedA, signB * k);
                    cache[k] = val;
                }
                return val;
            }

            long gLo = GetG(loK);
            long gHi = GetG(hiK);
            long gHiNext = GetG(hiK + 1);
            int n = (int)(gHiNext - gHi);

            void Split(long lo, long hi, long valLo, long valHi, int cBefore, int cUpToHi)
            {
                int cnt = cUpToHi - cBefore;
                if (cnt == 0)
                    return;

                long sumT = (long)cnt * hi - (valHi - valLo - (long)cBefore * (hi - lo));
                if (cnt == 1)
                {
                    outputCoords.Add(sumT - shift);
                    return;
                }
                if (sumT == (long)cnt * lo)
                {
                    for (int i = 0; i < cnt; i++)
                        outputCoords.Add(lo - shift);
                    return;
                }
                if (sumT == (long)cnt * hi)
                {
                    for (int i = 0; i < cnt; i++)
                        outputCoords.Add(hi - shift);
                    return;
                }

                long mid = sumT / cnt;
                long gMid = GetG(mid);
                long gMidNext = GetG(mid + 1);
                int sMid = (int)(gMidNext - gMid);

                if (sMid == cUpToHi && sumT == (long)cnt * mid)
                {
                    for (int i = 0; i < cnt; i++)
                        outputCoords.Add(mid - shift);
                    return;
                }

                Split(lo, mid, valLo, gMid, cBefore, sMid);
                Split(mid + 1, hi, gMidNext, valHi, sMid, cUpToHi);
            }

            Split(loK, hiK, gLo, gHi, 0, n);
        }
    }

    private static void Q1Judge(GCJInteractiveChannel io, int testNumber)
    {
        const int numCases = 15;
        const int maxCoord = 1_000_000;
        const int maxRequestCoord = 10 * maxCoord;
        const int maxRequests = 1000;
        int nMax = testNumber == 0 ? 1 : 10;

        Random rng = new(926153 + testNumber);
        List<(List<(int X, int Y)> Kings, List<(int X, int Y)> Requests)> cases = [];

        List<(int X, int Y)> BuildRequests(List<(int X, int Y)> kings)
        {
            List<(int X, int Y)> reqs = [.. kings];
            int half = maxRequests / 2;
            for (int i = 0; i < half; i++)
            {
                int bound = rng.Next(0, 2) == 0 ? maxCoord : maxRequestCoord;
                reqs.Add((rng.Next(-bound, bound + 1), rng.Next(-bound, bound + 1)));
            }
            while (reqs.Count < maxRequests)
            {
                var (x0, y0) = kings[rng.Next(kings.Count)];
                int slope = rng.Next(0, 2) == 0 ? -1 : 1;
                int xPlusY = slope * x0 + y0 + rng.Next(-3, 4);
                while (true)
                {
                    int x = rng.Next(-maxRequestCoord, maxRequestCoord + 1);
                    int y = xPlusY - slope * x;
                    if (y >= -maxRequestCoord && y <= maxRequestCoord)
                    {
                        reqs.Add((x, y));
                        break;
                    }
                }
            }
            (int X, int Y)[] arr = [.. reqs];
            rng.Shuffle(arr);
            return [.. arr];
        }

        if (testNumber == 0)
        {
            foreach (int sx in new[] { -1, 1 })
            {
                foreach (int sy in new[] { -1, 1 })
                {
                    List<(int X, int Y)> k = [(sx * maxCoord, sy * maxCoord)];
                    cases.Add((k, BuildRequests(k)));
                }
            }
        }
        else
        {
            List<(int X, int Y)> grid = [];
            for (int sx = -1; sx <= 1; sx++)
                for (int sy = -1; sy <= 1; sy++)
                    grid.Add((sx * maxCoord, sy * maxCoord));
            cases.Add((grid, BuildRequests(grid)));

            void AddDiagonals(int[] slopes)
            {
                int[] offsets = new int[slopes.Length];
                HashSet<int> usedOffsets = [];
                while (true)
                {
                    usedOffsets.Clear();
                    for (int i = 0; i < slopes.Length; i++)
                    {
                        offsets[i] = rng.Next(-maxCoord / 10, maxCoord / 10 + 1);
                        usedOffsets.Add(offsets[i]);
                    }
                    if (usedOffsets.Count == slopes.Length)
                        break;
                }

                HashSet<(int X, int Y)> kSet = [];
                while (kSet.Count < nMax)
                {
                    int dIdx = rng.Next(slopes.Length);
                    int x = rng.Next(-maxCoord, maxCoord + 1);
                    int y = slopes[dIdx] * x + offsets[dIdx];
                    if (y >= -maxCoord && y <= maxCoord)
                        kSet.Add((x, y));
                }
                List<(int X, int Y)> k = [.. kSet];
                cases.Add((k, BuildRequests(k)));
            }

            AddDiagonals([1]);
            AddDiagonals([-1]);
            AddDiagonals([1, 1, 1]);
            AddDiagonals([-1, 1]);

            void AddClusters(int numClusters, int cxSize, int cySize)
            {
                (int X, int Y)[] clusters = new (int, int)[numClusters];
                while (true)
                {
                    for (int i = 0; i < numClusters; i++)
                        clusters[i] = (rng.Next(-maxCoord, maxCoord - cxSize + 2), rng.Next(-maxCoord, maxCoord - cySize + 2));

                    int minDist = int.MaxValue;
                    for (int i = 0; i < numClusters; i++)
                    {
                        for (int j = 0; j < i; j++)
                        {
                            int d = Math.Abs(clusters[i].X - clusters[j].X) + Math.Abs(clusters[i].Y - clusters[j].Y);
                            if (d < minDist)
                                minDist = d;
                        }
                    }
                    if (minDist >= 2 * (cxSize + cySize))
                        break;
                }

                HashSet<(int X, int Y)> kSet = [];
                while (kSet.Count < nMax)
                {
                    int cIdx = rng.Next(numClusters);
                    kSet.Add((rng.Next(cxSize) + clusters[cIdx].X, rng.Next(cySize) + clusters[cIdx].Y));
                }
                List<(int X, int Y)> k = [.. kSet];
                cases.Add((k, BuildRequests(k)));
            }

            AddClusters(1, 3, 4);
            AddClusters(3, 2, 2);

            List<(int X, int Y)> yutak =
            [
                (111, 111), (112, 111), (111, 112), (112, 112), (-111, -111),
                (-112, 111), (-111, -112), (112, -111), (-112, -111), (-111, 112)
            ];
            cases.Add((yutak, BuildRequests(yutak)));

            List<(int X, int Y)> extreme =
            [
                (-maxCoord, -maxCoord), (maxCoord, maxCoord),
                (-maxCoord + 1, maxCoord - 1), (maxCoord - 1, -maxCoord + 1)
            ];
            cases.Add((extreme, BuildRequests(extreme)));

            List<(int X, int Y)> tricky2 =
            [
                (-maxCoord, -maxCoord), (maxCoord, maxCoord),
                (-maxCoord + 1, maxCoord - 1), (maxCoord - 1, -maxCoord + 1),
                (-maxCoord + 2, -maxCoord + 2), (maxCoord - 2, maxCoord - 2),
                (-maxCoord + 3, maxCoord - 3), (maxCoord - 3, -maxCoord + 3),
                (-maxCoord + 4, -maxCoord + 4), (maxCoord - 4, maxCoord - 4)
            ];
            cases.Add((tricky2, BuildRequests(tricky2)));
        }

        while (cases.Count < numCases)
        {
            int nMin = (int)Math.Round(0.7 * nMax);
            int n = rng.Next(nMin, nMax + 1);
            HashSet<(int X, int Y)> kSet = [];
            while (kSet.Count < n)
                kSet.Add((rng.Next(-maxCoord, maxCoord + 1), rng.Next(-maxCoord, maxCoord + 1)));
            List<(int X, int Y)> k = [.. kSet];
            cases.Add((k, BuildRequests(k)));
        }

        io.WriteLine($"{cases.Count} {nMax} {maxCoord} {maxRequests}");
        for (int tc = 0; tc < cases.Count; tc++)
        {
            var (kings, reqs) = cases[tc];
            int received = 0;
            while (true)
            {
                string line = io.ReadRequiredLine();
                if (line.Trim().Equals("READY", StringComparison.OrdinalIgnoreCase))
                    break;
                string[] parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(2, parts.Length);
                received++;
                Assert.LessOrEqual(received, maxRequests, $"Case #{tc + 1}: exceeded {maxRequests} requests.");
                long a = long.Parse(parts[0], CultureInfo.InvariantCulture);
                long b = long.Parse(parts[1], CultureInfo.InvariantCulture);
                Assert.That(a, Is.InRange(-maxRequestCoord, maxRequestCoord));
                Assert.That(b, Is.InRange(-maxRequestCoord, maxRequestCoord));

                long total = 0;
                foreach (var (kx, ky) in kings)
                    total += Math.Max(Math.Abs(a - kx), Math.Abs(b - ky));
                io.WriteLine(total.ToString(CultureInfo.InvariantCulture));
            }

            foreach (var (rx, ry) in reqs)
            {
                io.WriteLine($"{rx} {ry}");
                long guess = long.Parse(io.ReadRequiredLine(), CultureInfo.InvariantCulture);
                long expected = 0;
                foreach (var (kx, ky) in kings)
                    expected += Math.Max(Math.Abs(rx - kx), Math.Abs(ry - ky));
                Assert.AreEqual(expected, guess, $"Case #{tc + 1}: wrong answer for ({rx}, {ry}).");
            }

            io.WriteLine("DONE");
        }

        Assert.IsNull(io.ReadLine(), "Additional input after all cases finish.");
    }

    #endregion

    #region Q2 Sorting Permutation Unit

    [Test]
    public void Q2Sample() => Test(Q2, ValidateSortingPermutationUnit);

    [Test]
    public void Q2Small() => Test(Q2, ValidateSortingPermutationUnit);

    [Test]
    public void Q2Large() => Test(Q2, ValidateSortingPermutationUnit);

    private static Func<string[]> Q2(GCJTestCase testCase)
    {
        testCase.Get(out int pLimit, out int sLimit, out int k, out int n);
        int[][] arrays = new int[k][];
        for (int i = 0; i < k; i++)
        {
            testCase.Get(out int[] row);
            arrays[i] = row;
        }

        return () =>
        {
            List<string> lines = [];
            if (n == 2)
            {
                lines.Add("1");
                lines.Add("2 1");
                for (int i = 0; i < k; i++)
                    lines.Add(arrays[i][0] <= arrays[i][1] ? "0" : "1 1");
                return [.. lines];
            }

            int m = n - 1;
            int[] rotBasis = FindDiameterFourRotations(m);
            int numPerms = 1 + rotBasis.Length;
            lines.Add(numPerms.ToString(CultureInfo.InvariantCulture));

            int[][] perms = new int[numPerms][];
            perms[0] = new int[n];
            for (int i = 0; i < n; i++)
                perms[0][i] = i;
            (perms[0][m - 1], perms[0][m]) = (perms[0][m], perms[0][m - 1]);

            for (int rIdx = 0; rIdx < rotBasis.Length; rIdx++)
            {
                int sh = rotBasis[rIdx];
                int[] p = new int[n];
                for (int i = 0; i < m; i++)
                    p[i] = (i + sh) % m;
                p[m] = m;
                perms[1 + rIdx] = p;
            }

            foreach (int[] p in perms)
            {
                int[] oneBased = new int[n];
                for (int i = 0; i < n; i++)
                    oneBased[i] = p[i] + 1;
                lines.Add(string.Join(" ", oneBased));
            }

            // Precompute shortest rotation sequence in Z_m using BFS over rotBasis.
            List<int>[] shortestRot = new List<int>[m];
            shortestRot[0] = [];
            Queue<int> bfs = new();
            bfs.Enqueue(0);
            while (bfs.Count > 0)
            {
                int cur = bfs.Dequeue();
                for (int rIdx = 0; rIdx < rotBasis.Length; rIdx++)
                {
                    int nxt = (cur + rotBasis[rIdx]) % m;
                    if (shortestRot[nxt] == null)
                    {
                        List<int> path = [.. shortestRot[cur], 2 + rIdx];
                        shortestRot[nxt] = path;
                        bfs.Enqueue(nxt);
                    }
                }
            }

            for (int idx = 0; idx < k; idx++)
            {
                int[] a = arrays[idx];
                (int Val, int Pos)[] sorted = new (int, int)[n];
                for (int i = 0; i < n; i++)
                    sorted[i] = (a[i], i);
                Array.Sort(sorted);

                int[] cur = new int[n];
                for (int rank = 0; rank < n; rank++)
                    cur[sorted[rank].Pos] = rank;

                int rotOffset = 0;
                List<int> ops = [];

                void ApplyRot(int delta)
                {
                    if (delta == 0)
                        return;
                    foreach (int op in shortestRot[delta])
                    {
                        ops.Add(op);
                        int[] perm = perms[op - 1];
                        int[] nxt = new int[n];
                        for (int j = 0; j < n; j++)
                            nxt[j] = cur[perm[j]];
                        cur = nxt;
                    }
                    rotOffset = (rotOffset + delta) % m;
                }

                void ApplySwap()
                {
                    ops.Add(1);
                    (cur[m - 1], cur[m]) = (cur[m], cur[m - 1]);
                }

                while (true)
                {
                    if (cur[m] != m)
                    {
                        int targetSlot = cur[m];
                        int curBufferSlot = (m - 1 + rotOffset) % m;
                        int delta = (targetSlot - curBufferSlot + m) % m;
                        ApplyRot(delta);
                        ApplySwap();
                    }
                    else
                    {
                        int bestDelta = -1;
                        int bestSteps = int.MaxValue;
                        for (int phys = 0; phys < m; phys++)
                        {
                            int logical = (phys + rotOffset) % m;
                            if (cur[phys] != logical)
                            {
                                int d = (phys - (m - 1) + m) % m;
                                if (shortestRot[d].Count < bestSteps)
                                {
                                    bestSteps = shortestRot[d].Count;
                                    bestDelta = d;
                                }
                            }
                        }

                        if (bestDelta < 0)
                            break;

                        ApplyRot(bestDelta);
                        ApplySwap();
                    }
                }

                if (rotOffset != 0)
                    ApplyRot((m - rotOffset) % m);

                Assert.LessOrEqual(ops.Count, sLimit);
                lines.Add(ops.Count == 0
                    ? "0"
                    : $"{ops.Count} {string.Join(" ", ops)}");
            }

            return [.. lines];
        };

        static int[] FindDiameterFourRotations(int m)
        {
            if (m <= 4)
            {
                int[] trivial = new int[m - 1];
                for (int i = 0; i < m - 1; i++)
                    trivial[i] = i + 1;
                return trivial;
            }

            int[] known = m == 49 ? [1, 3, 12, 20] : [1, 3, 9, 27];
            List<int> filtered = [];
            foreach (int v in known)
            {
                int r = v % m;
                if (r > 0 && !filtered.Contains(r))
                    filtered.Add(r);
            }
            if (MaxDist(m, filtered) <= 4)
                return [.. filtered];

            Random rng = new(201902 + m);
            while (true)
            {
                int[] trial = [1, rng.Next(2, m), rng.Next(2, m), rng.Next(2, m)];
                if (trial[1] == trial[2] || trial[1] == trial[3] || trial[2] == trial[3])
                    continue;
                if (MaxDist(m, trial) <= 4)
                    return trial;
            }
        }

        static int MaxDist(int m, IReadOnlyList<int> basis)
        {
            int[] dist = new int[m];
            Array.Fill(dist, -1);
            dist[0] = 0;
            Queue<int> q = new();
            q.Enqueue(0);
            int maxD = 0;
            int seen = 1;
            while (q.Count > 0)
            {
                int u = q.Dequeue();
                foreach (int s in basis)
                {
                    int v = (u + s) % m;
                    if (dist[v] < 0)
                    {
                        dist[v] = dist[u] + 1;
                        if (dist[v] > maxD)
                            maxD = dist[v];
                        seen++;
                        q.Enqueue(v);
                    }
                }
            }
            return seen == m ? maxD : int.MaxValue;
        }
    }

    private static void ValidateSortingPermutationUnit(string input, string expectedOutput, List<string> actualCases)
    {
        string[] inputLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        int numCases = int.Parse(inputLines[0], CultureInfo.InvariantCulture);
        Assert.AreEqual(numCases, actualCases.Count);

        int ptr = 1;
        for (int tc = 0; tc < numCases; tc++)
        {
            string[] hdr = inputLines[ptr++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int pLimit = int.Parse(hdr[0], CultureInfo.InvariantCulture);
            int sLimit = int.Parse(hdr[1], CultureInfo.InvariantCulture);
            int k = int.Parse(hdr[2], CultureInfo.InvariantCulture);
            int n = int.Parse(hdr[3], CultureInfo.InvariantCulture);

            int[][] arrays = new int[k][];
            for (int i = 0; i < k; i++)
            {
                string[] toks = inputLines[ptr++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                arrays[i] = new int[n];
                for (int j = 0; j < n; j++)
                    arrays[i][j] = int.Parse(toks[j], CultureInfo.InvariantCulture);
            }

            string[] lines = actualCases[tc].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual($"Case #{tc + 1}:", lines[0].TrimEnd(), $"Case #{tc + 1}: header line must end after colon.");

            int pCount = int.Parse(lines[1].Trim(), CultureInfo.InvariantCulture);
            Assert.That(pCount, Is.InRange(1, pLimit), $"Case #{tc + 1}: invalid P'={pCount}.");
            Assert.AreEqual(2 + pCount + k, lines.Length, $"Case #{tc + 1}: wrong line count.");

            int[][] perms = new int[pCount][];
            for (int i = 0; i < pCount; i++)
            {
                string[] toks = lines[2 + i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(n, toks.Length);
                perms[i] = new int[n];
                bool[] seen = new bool[n];
                for (int j = 0; j < n; j++)
                {
                    int val = int.Parse(toks[j], CultureInfo.InvariantCulture) - 1;
                    Assert.That(val, Is.InRange(0, n - 1));
                    Assert.IsFalse(seen[val]);
                    seen[val] = true;
                    perms[i][j] = val;
                }
            }

            for (int i = 0; i < k; i++)
            {
                string[] toks = lines[2 + pCount + i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int sCount = int.Parse(toks[0], CultureInfo.InvariantCulture);
                Assert.That(sCount, Is.InRange(0, sLimit));
                Assert.AreEqual(sCount + 1, toks.Length);

                int[] cur = (int[])arrays[i].Clone();
                for (int step = 0; step < sCount; step++)
                {
                    int op = int.Parse(toks[1 + step], CultureInfo.InvariantCulture) - 1;
                    Assert.That(op, Is.InRange(0, pCount - 1));
                    int[] perm = perms[op];
                    int[] nxt = new int[n];
                    for (int j = 0; j < n; j++)
                        nxt[j] = cur[perm[j]];
                    cur = nxt;
                }

                for (int j = 0; j + 1 < n; j++)
                    Assert.LessOrEqual(cur[j], cur[j + 1], $"Case #{tc + 1}, array {i}: not sorted.");
            }
        }
    }

    #endregion

    #region Q3 Won't Sum? Must Now

    private static readonly BigInteger[] Pow10 = BuildPow10(45);

    private static BigInteger[] BuildPow10(int maxExp)
    {
        BigInteger[] res = new BigInteger[maxExp + 1];
        res[0] = BigInteger.One;
        for (int i = 1; i <= maxExp; i++)
            res[i] = res[i - 1] * 10;
        return res;
    }

    [Test]
    public void Q3Sample() => Test(Q3, ValidateWontSum);

    [Test]
    public void Q3Small() => Test(Q3, ValidateWontSum);

    [Test]
    public void Q3Large() => Test(Q3, ValidateWontSum);

    private static Func<string> Q3(GCJTestCase testCase)
    {
        testCase.Get(out string sStr);
        return () =>
        {
            if (IsPalindrome(sStr))
                return sStr;

            BigInteger s = BigInteger.Parse(sStr, CultureInfo.InvariantCulture);
            if (TryTwoPalindromes(s, out string? a2, out string? b2))
                return $"{a2} {b2}";

            foreach (BigInteger p in EnumeratePalindromes())
            {
                if (p >= s)
                    break;
                if (TryTwoPalindromes(s - p, out string? a3, out string? b3))
                    return $"{p} {a3} {b3}";
            }

            throw new InvalidOperationException($"Failed to decompose {sStr} into <= 3 palindromes.");
        };

        static bool IsPalindrome(string t)
        {
            int l = 0, r = t.Length - 1;
            while (l < r)
            {
                if (t[l] != t[r])
                    return false;
                l++;
                r--;
            }
            return true;
        }

        static IEnumerable<BigInteger> EnumeratePalindromes()
        {
            for (int len = 1; ; len++)
            {
                int halfLen = (len + 1) / 2;
                long start = (long)Pow10[halfLen - 1];
                long end = (long)Pow10[halfLen];
                for (long half = start; half < end; half++)
                {
                    string left = half.ToString(CultureInfo.InvariantCulture);
                    StringBuilder sb = new(len);
                    sb.Append(left);
                    int mirrorStart = (len & 1) == 1 ? halfLen - 2 : halfLen - 1;
                    for (int i = mirrorStart; i >= 0; i--)
                        sb.Append(left[i]);
                    yield return BigInteger.Parse(sb.ToString(), CultureInfo.InvariantCulture);
                }
            }
        }

        static bool TryTwoPalindromes(BigInteger s, out string? aStr, out string? bStr)
        {
            string str = s.ToString(CultureInfo.InvariantCulture);
            int d = str.Length;
            int[] digits = new int[d];
            for (int i = 0; i < d; i++)
                digits[i] = str[i] - '0';

            if (digits[0] == 0)
            {
                aStr = null;
                bStr = null;
                return false;
            }

            // Only allow lenA == d - 1 when the leading digit of S is 1 (since A, B < 10^(d-1) => A + B < 2*10^(d-1)).
            int minLenA = (d >= 2 && digits[0] == 1) ? d - 1 : d;
            for (int lenA = d; lenA >= minLenA; lenA--)
            {
                int topCarry = d - lenA; // 0 or 1
                if (TryEqualLengthDp(digits, lenA, topCarry, out aStr, out bStr))
                    return true;
                if (lenA >= 2 && TryDiffOneLengthDp(digits, lenA, topCarry, out aStr, out bStr))
                    return true;

                for (int lenB = lenA - 2; lenB >= 1; lenB--)
                {
                    if (TryOverhangChunkPeel(s, lenA, lenB, firstCall: true, out BigInteger bigA, out BigInteger bigB))
                    {
                        aStr = bigA.ToString(CultureInfo.InvariantCulture);
                        bStr = bigB.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                }
            }

            aStr = null;
            bStr = null;
            return false;
        }

        static bool TryEqualLengthDp(int[] digits, int len, int topCarry, out string? aStr, out string? bStr)
        {
            if (len == 1)
            {
                int sVal = topCarry * 10 + digits[^1];
                if (sVal is >= 2 and <= 18)
                {
                    int da = Math.Min(9, sVal - 1);
                    int db = sVal - da;
                    aStr = da.ToString(CultureInfo.InvariantCulture);
                    bStr = db.ToString(CultureInfo.InvariantCulture);
                    return true;
                }
                aStr = null;
                bStr = null;
                return false;
            }

            int shiftL = digits.Length - len;
            int nHalf = (len + 1) / 2;
            bool[,] dead = new bool[nHalf + 1, 4];
            int[] aDigits = new int[len];
            int[] bDigits = new int[len];

            bool Dfs(int i, int cL, int cR)
            {
                int j = len - 1 - i;
                if (i > j)
                    return cL == cR;

                int mask = (cL << 1) | cR;
                if (dead[i, mask])
                    return false;

                if (i == j)
                {
                    int sum = 10 * cL + digits[shiftL + i] - cR;
                    if (sum >= 0 && sum <= 18)
                    {
                        aDigits[i] = Math.Min(9, sum);
                        bDigits[i] = sum - aDigits[i];
                        return true;
                    }
                    dead[i, mask] = true;
                    return false;
                }

                for (int nextCL = 0; nextCL <= 1; nextCL++)
                {
                    int leftSum = 10 * cL + digits[shiftL + i] - nextCL;
                    int minSum = i == 0 ? 2 : 0;
                    if (leftSum < minSum || leftSum > 18)
                        continue;

                    int rightTotal = leftSum + cR;
                    if (rightTotal % 10 != digits[shiftL + j])
                        continue;
                    int nextCR = rightTotal / 10;

                    if (Dfs(i + 1, nextCL, nextCR))
                    {
                        int da = i == 0 ? Math.Min(9, leftSum - 1) : Math.Min(9, leftSum);
                        int db = leftSum - da;
                        aDigits[i] = aDigits[j] = da;
                        bDigits[i] = bDigits[j] = db;
                        return true;
                    }
                }

                dead[i, mask] = true;
                return false;
            }

            if (!Dfs(0, topCarry, 0))
            {
                aStr = null;
                bStr = null;
                return false;
            }

            aStr = DigitsToString(aDigits);
            bStr = DigitsToString(bDigits);
            return true;
        }

        static bool TryDiffOneLengthDp(int[] digits, int lenA, int topCarry, out string? aStr, out string? bStr)
        {
            int lenB = lenA - 1;
            int shiftL = digits.Length - lenA;
            int half = lenA / 2;
            bool[,,,] dead = new bool[half + 1, 2, 2, 10];
            int[] aDigits = new int[lenA];
            int[] bDigits = new int[lenB];

            bool Dfs(int i, int cL, int cR, int prevB)
            {
                if (i == half)
                {
                    if ((lenA & 1) == 0)
                        return cL == cR;

                    int midSum = 10 * cL + digits[shiftL + i] - cR;
                    int aMid = midSum - prevB;
                    if (aMid >= 0 && aMid <= 9)
                    {
                        aDigits[i] = aMid;
                        return true;
                    }
                    return false;
                }

                if (dead[i, cL, cR, prevB])
                    return false;

                int j = lenA - 1 - i;
                for (int nextCL = 0; nextCL <= 1; nextCL++)
                {
                    int da = 10 * cL + digits[shiftL + i] - prevB - nextCL;
                    int minA = i == 0 ? 1 : 0;
                    if (da < minA || da > 9)
                        continue;

                    int newB = (digits[shiftL + j] - da - cR + 20) % 10;
                    if (i == 0 && newB == 0)
                        continue;

                    int nextCR = (da + newB + cR) / 10;
                    if (Dfs(i + 1, nextCL, nextCR, newB))
                    {
                        aDigits[i] = aDigits[j] = da;
                        bDigits[i] = bDigits[lenB - 1 - i] = newB;
                        return true;
                    }
                }

                dead[i, cL, cR, prevB] = true;
                return false;
            }

            if (!Dfs(0, topCarry, 0, 0))
            {
                aStr = null;
                bStr = null;
                return false;
            }

            aStr = DigitsToString(aDigits);
            bStr = DigitsToString(bDigits);
            return true;
        }

        static bool TryOverhangChunkPeel(
            BigInteger rem,
            int lenA,
            int lenB,
            bool firstCall,
            out BigInteger a,
            out BigInteger b)
        {
            if (lenB == 0)
            {
                if (rem.Sign >= 0 && rem < Pow10[lenA] && (!firstCall || (lenA == 1 ? rem >= 1 : rem >= Pow10[lenA - 1])) &&
                    IsPaddedPalindrome(rem, lenA))
                {
                    a = rem;
                    b = BigInteger.Zero;
                    return true;
                }
                a = BigInteger.Zero;
                b = BigInteger.Zero;
                return false;
            }

            int h = lenA - lenB;
            if (lenB <= h)
            {
                BigInteger powHigh = Pow10[lenA - lenB];
                BigInteger q = rem / powHigh;
                BigInteger minX = firstCall ? Pow10[lenB - 1] : BigInteger.Zero;
                BigInteger maxX = Pow10[lenB] - BigInteger.One;

                for (BigInteger x = q; x >= q - BigInteger.One; x--)
                {
                    if (x < minX || x > maxX)
                        continue;
                    BigInteger revX = ReversePadded(x, lenB);
                    BigInteger candB = (rem - revX) % Pow10[lenB];
                    if (candB.Sign < 0)
                        candB += Pow10[lenB];
                    if (firstCall && candB < Pow10[lenB - 1])
                        continue;
                    if (!IsPaddedPalindrome(candB, lenB))
                        continue;

                    BigInteger candA = rem - candB;
                    if (candA.Sign >= 0 && candA < Pow10[lenA] && (!firstCall || candA >= Pow10[lenA - 1]) &&
                        IsPaddedPalindrome(candA, lenA))
                    {
                        a = candA;
                        b = candB;
                        return true;
                    }
                }

                a = BigInteger.Zero;
                b = BigInteger.Zero;
                return false;
            }

            int step = Math.Min(h, lenB / 2);
            BigInteger powShift = Pow10[lenA - step];
            BigInteger qChunk = rem / powShift;
            BigInteger minX2 = firstCall ? Pow10[step - 1] : BigInteger.Zero;
            BigInteger maxX2 = Pow10[step] - BigInteger.One;
            BigInteger stepPow = Pow10[step];
            BigInteger maxInner = (Pow10[lenA - 2 * step] - BigInteger.One) + (Pow10[lenB - 2 * step] - BigInteger.One);

            for (BigInteger x = qChunk; x >= qChunk - BigInteger.One; x--)
            {
                if (x < minX2 || x > maxX2)
                    continue;
                BigInteger revX = ReversePadded(x, step);
                BigInteger yRight = (rem - revX) % stepPow;
                if (yRight.Sign < 0)
                    yRight += stepPow;
                if (firstCall && (yRight % 10).IsZero)
                    continue;

                BigInteger yLeft = ReversePadded(yRight, step);
                BigInteger contribA = x * powShift + revX;
                BigInteger contribB = yLeft * Pow10[lenB - step] + yRight;
                BigInteger diff = rem - contribA - contribB;
                if (diff.Sign < 0)
                    continue;
                BigInteger innerRem = diff / stepPow;
                if (innerRem > maxInner)
                    continue;

                if (TryOverhangChunkPeel(innerRem, lenA - 2 * step, lenB - 2 * step, false, out BigInteger inA, out BigInteger inB))
                {
                    a = contribA + inA * stepPow;
                    b = contribB + inB * stepPow;
                    return true;
                }
            }

            a = BigInteger.Zero;
            b = BigInteger.Zero;
            return false;
        }

        static BigInteger ReversePadded(BigInteger val, int width)
        {
            BigInteger res = BigInteger.Zero;
            BigInteger cur = val;
            for (int i = 0; i < width; i++)
            {
                cur = BigInteger.DivRem(cur, 10, out BigInteger rem);
                res = res * 10 + rem;
            }
            return res;
        }

        static bool IsPaddedPalindrome(BigInteger val, int width) => val == ReversePadded(val, width);

        static string DigitsToString(int[] d)
        {
            char[] c = new char[d.Length];
            for (int i = 0; i < d.Length; i++)
                c[i] = (char)('0' + d[i]);
            return new string(c);
        }
    }

    private static void ValidateWontSum(string input, string expectedOutput, List<string> actualCases)
    {
        string[] inputLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        int numCases = int.Parse(inputLines[0], CultureInfo.InvariantCulture);
        Assert.AreEqual(numCases, actualCases.Count);

        string[] expLines = expectedOutput.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        Assert.AreEqual(numCases, expLines.Length);

        for (int tc = 0; tc < numCases; tc++)
        {
            BigInteger s = BigInteger.Parse(inputLines[tc + 1].Trim(), CultureInfo.InvariantCulture);
            string expPrefix = $"Case #{tc + 1}:";
            int expTerms = expLines[tc][expPrefix.Length..].Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            string[] actLines = actualCases[tc].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(1, actLines.Length);
            Assert.IsTrue(actLines[0].StartsWith(expPrefix, StringComparison.Ordinal));
            string[] terms = actLines[0][expPrefix.Length..].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(expTerms, terms.Length, $"Case #{tc + 1}: expected {expTerms} terms, got {terms.Length}.");
            BigInteger sum = BigInteger.Zero;
            foreach (string term in terms)
            {
                Assert.IsFalse(term.StartsWith('0'), $"Case #{tc + 1}: leading zero in '{term}'.");
                for (int l = 0, r = term.Length - 1; l < r; l++, r--)
                    Assert.AreEqual(term[l], term[r], $"Case #{tc + 1}: '{term}' is not a palindrome.");
                BigInteger val = BigInteger.Parse(term, CultureInfo.InvariantCulture);
                Assert.That(val, Is.InRange(BigInteger.One, s));
                sum += val;
            }
            Assert.AreEqual(s, sum, $"Case #{tc + 1}: wrong sum.");
        }
    }

    #endregion

    #region Q4 Juggle Struggle: Part 1

    [Test]
    public void Q4Sample() => Test(Q4);

    [Test]
    public void Q4Small() => Test(Q4);

    [Test]
    public void Q4Large() => SlowTest(Q4);

    private readonly struct SweepEntry
    {
        public readonly long Dx;
        public readonly long Dy;
        public readonly int Idx;
        public readonly bool IsUpper;
        public readonly bool IsSecondSet;

        public SweepEntry(long dx, long dy, int idx, bool isUpper, bool isSecondSet)
        {
            Dx = dx;
            Dy = dy;
            Idx = idx;
            IsUpper = isUpper;
            IsSecondSet = isSecondSet;
        }
    }

    private readonly struct SweepEntryComparer : IComparer<SweepEntry>
    {
        public int Compare(SweepEntry a, SweepEntry b)
        {
            long cr = a.Dx * b.Dy - a.Dy * b.Dx;
            return cr > 0 ? -1 : 1;
        }
    }

    private static Func<string> Q4(GCJTestCase testCase)
    {
        testCase.Get(out int n);
        testCase.GetLines(2 * n, out long[] xs, out long[] ys);
        return () =>
        {
            Point2L[] pts = new Point2L[2 * n];
            for (int i = 0; i < 2 * n; i++)
                pts[i] = new Point2L(xs[i], ys[i]);

            int[] partner = new int[2 * n];
            Random rng = new(201904);
            int[] all = new int[2 * n];
            for (int i = 0; i < 2 * n; i++)
                all[i] = i;

            SweepEntry[] entries = new SweepEntry[2 * n];
            int p = all[rng.Next(all.Length)];
            int q = FindSplitPartner(pts, p, all, all.Length, null, 0, entries);
            partner[p] = q;
            partner[q] = p;

            int[] xArr = new int[n - 1];
            int[] yArr = new int[n - 1];
            int xLen = 0, yLen = 0;
            long px = pts[p].X, py = pts[p].Y;
            long pqx = pts[q].X - px, pqy = pts[q].Y - py;
            foreach (int id in all)
            {
                if (id == p || id == q)
                    continue;
                if (pqx * (pts[id].Y - py) - pqy * (pts[id].X - px) > 0)
                    xArr[xLen++] = id;
                else
                    yArr[yLen++] = id;
            }

            SolveOpposite(pts, partner, rng, xArr, yArr, xLen, entries);
            int[] oneBased = new int[2 * n];
            for (int i = 0; i < 2 * n; i++)
                oneBased[i] = partner[i] + 1;
            return string.Join(" ", oneBased);
        };

        static void SolveOpposite(
            Point2L[] pts,
            int[] partner,
            Random rng,
            int[] xList,
            int[] yList,
            int m,
            SweepEntry[] entries)
        {
            if (m == 0)
                return;
            if (m == 1)
            {
                partner[xList[0]] = yList[0];
                partner[yList[0]] = xList[0];
                return;
            }

            int p = xList[rng.Next(m)];
            int q = FindSplitPartner(pts, p, xList, m, yList, m, entries);
            partner[p] = q;
            partner[q] = p;

            long px = pts[p].X, py = pts[p].Y;
            long pqx = pts[q].X - px, pqy = pts[q].Y - py;
            int[] x1 = new int[m], y1 = new int[m], x2 = new int[m], y2 = new int[m];
            int c1 = 0, c2 = 0, cy1 = 0, cy2 = 0;

            for (int i = 0; i < m; i++)
            {
                int x = xList[i];
                if (x == p)
                    continue;
                if (pqx * (pts[x].Y - py) - pqy * (pts[x].X - px) > 0)
                    x1[c1++] = x;
                else
                    x2[c2++] = x;
            }
            for (int i = 0; i < m; i++)
            {
                int y = yList[i];
                if (y == q)
                    continue;
                if (pqx * (pts[y].Y - py) - pqy * (pts[y].X - px) < 0)
                    y1[cy1++] = y;
                else
                    y2[cy2++] = y;
            }

            SolveOpposite(pts, partner, rng, x1, y1, c1, entries);
            SolveOpposite(pts, partner, rng, x2, y2, c2, entries);
        }

        static int FindSplitPartner(
            Point2L[] pts,
            int pivot,
            int[] setA,
            int lenA,
            int[]? setB,
            int lenB,
            SweepEntry[] entries)
        {
            long ox = pts[pivot].X;
            long oy = pts[pivot].Y;
            int count = 0;
            int posSide = 0;
            int negSide = 0;

            for (int i = 0; i < lenA; i++)
            {
                int id = setA[i];
                if (id == pivot)
                    continue;
                long dx = pts[id].X - ox;
                long dy = pts[id].Y - oy;
                bool isUpper = dy > 0 || (dy == 0 && dx > 0);
                if (isUpper) posSide++; else negSide++;
                entries[count++] = new SweepEntry(isUpper ? dx : -dx, isUpper ? dy : -dy, id, isUpper, false);
            }

            if (setB != null)
            {
                for (int i = 0; i < lenB; i++)
                {
                    int id = setB[i];
                    long dx = pts[id].X - ox;
                    long dy = pts[id].Y - oy;
                    bool isUpper = dy > 0 || (dy == 0 && dx > 0);
                    if (isUpper) posSide++; else negSide++;
                    entries[count++] = new SweepEntry(isUpper ? dx : -dx, isUpper ? dy : -dy, id, isUpper, true);
                }
            }

            Array.Sort(entries, 0, count, default(SweepEntryComparer));

            bool requireSecond = setB != null;
            for (int i = 0; i < count; i++)
            {
                SweepEntry e = entries[i];
                if (e.IsUpper) posSide--; else negSide--;
                if (posSide == negSide && (!requireSecond || e.IsSecondSet))
                    return e.Idx;
                if (e.IsUpper) negSide++; else posSide++;
            }

            throw new InvalidOperationException("Failed to find bisecting partner.");
        }
    }

    #endregion

    #region Q5 Juggle Struggle: Part 2

    [Test]
    public void Q5Sample() => Test(Q5);

    [Test]
    public void Q5Small() => Test(Q5);

    [Test]
    public void Q5Large() => SlowTest(Q5);

    private readonly struct SlopeComparer : IComparer<int>
    {
        private readonly Point2L[] dirs;

        public SlopeComparer(Point2L[] dirs) => this.dirs = dirs;

        public int Compare(int i, int j)
        {
            long cr = dirs[i].X * dirs[j].Y - dirs[i].Y * dirs[j].X;
            return cr > 0 ? -1 : cr < 0 ? 1 : i.CompareTo(j);
        }
    }

    private static Func<string> Q5(GCJTestCase testCase)
    {
        testCase.Get(out int n);
        long[] x1 = new long[n];
        long[] y1 = new long[n];
        long[] x2 = new long[n];
        long[] y2 = new long[n];
        for (int i = 0; i < n; i++)
            testCase.Get(out x1[i], out y1[i], out x2[i], out y2[i]);
        return () =>
        {
            Point2L[] pA = new Point2L[n];
            Point2L[] pB = new Point2L[n];
            Point2L[] dir = new Point2L[n];
            int[] order = new int[n];

            for (int i = 0; i < n; i++)
            {
                Point2L a = new(x1[i], y1[i]);
                Point2L b = new(x2[i], y2[i]);
                Point2L d = b - a;
                if (d.Y < 0 || (d.Y == 0 && d.X < 0))
                {
                    (a, b) = (b, a);
                    d = new Point2L(-d.X, -d.Y);
                }
                pA[i] = a;
                pB[i] = b;
                dir[i] = d;
                order[i] = i;
            }

            Array.Sort(order, new SlopeComparer(dir));

            bool[] candF = new bool[n];
            for (int i = 0; i < n;)
            {
                int j = i + 1;
                while (j < n && dir[order[i]].X * dir[order[j]].Y - dir[order[i]].Y * dir[order[j]].X == 0)
                    j++;
                if (j - i >= 2)
                {
                    for (int k = i; k < j; k++)
                        candF[order[k]] = true;
                }
                i = j;
            }

            (List<Point2L> SortedA, List<Point2L> SortedB) SolveRange(int l, int r)
            {
                int len = r - l;
                if (len <= 0)
                    return ([], []);
                if (len == 1)
                    return ([pA[order[l]]], [pB[order[l]]]);

                if (len <= 16)
                {
                    for (int i = l; i < r; i++)
                    {
                        int si = order[i];
                        for (int j = i + 1; j < r; j++)
                        {
                            int sj = order[j];
                            if (!SegmentsIntersect(pA[si], pB[si], pA[sj], pB[sj]))
                            {
                                candF[si] = true;
                                candF[sj] = true;
                            }
                        }
                    }

                    List<Point2L> sa = new(len);
                    List<Point2L> sb = new(len);
                    for (int i = l; i < r; i++)
                    {
                        sa.Add(pA[order[i]]);
                        sb.Add(pB[order[i]]);
                    }
                    sa.Sort(CompareLexXY);
                    sb.Sort(CompareLexXY);
                    return (sa, sb);
                }

                int mid = l + (len >> 1);
                var (leftA, leftB) = SolveRange(l, mid);
                var (rightA, rightB) = SolveRange(mid, r);

                List<Point2L> hullRightA = BuildCcwHull(rightA);
                List<Point2L> hullRightB = BuildCcwHull(rightB);
                CheckExtremeWalk(l, mid, findMax: true, hullRightA);
                CheckExtremeWalk(l, mid, findMax: false, hullRightB);

                List<Point2L> hullLeftA = BuildCcwHull(leftA);
                List<Point2L> hullLeftB = BuildCcwHull(leftB);
                CheckExtremeWalk(mid, r, findMax: false, hullLeftA);
                CheckExtremeWalk(mid, r, findMax: true, hullLeftB);

                return (MergeLexXY(leftA, rightA), MergeLexXY(leftB, rightB));
            }

            void CheckExtremeWalk(int segL, int segR, bool findMax, List<Point2L> hull)
            {
                if (hull.Count == 0)
                    return;

                int firstSeg = order[segL];
                Point2L d0 = dir[firstSeg];
                int ptr = 0;
                long bestDot = d0.X * hull[0].Y - d0.Y * hull[0].X;
                for (int k = 1; k < hull.Count; k++)
                {
                    long v = d0.X * hull[k].Y - d0.Y * hull[k].X;
                    if (findMax ? v > bestDot : v < bestDot)
                    {
                        bestDot = v;
                        ptr = k;
                    }
                }

                int hLen = hull.Count;
                for (int pos = segL; pos < segR; pos++)
                {
                    int segIdx = order[pos];
                    Point2L d = dir[segIdx];
                    if (hLen > 1)
                    {
                        while (true)
                        {
                            int nxt = ptr + 1 == hLen ? 0 : ptr + 1;
                            long stepVal = d.X * (hull[nxt].Y - hull[ptr].Y) - d.Y * (hull[nxt].X - hull[ptr].X);
                            if (findMax ? stepVal > 0 : stepVal < 0)
                            {
                                ptr = nxt;
                                if (ptr == 0 && pos == segL)
                                    break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    long extreme = d.X * hull[ptr].Y - d.Y * hull[ptr].X;
                    long baseline = d.X * pA[segIdx].Y - d.Y * pA[segIdx].X;
                    if (findMax ? extreme >= baseline : extreme <= baseline)
                        candF[segIdx] = true;
                }
            }

            SolveRange(0, n);

            bool[] isBad = new bool[n];
            for (int i = 0; i < n; i++)
            {
                if (!candF[i])
                    continue;
                for (int j = 0; j < n; j++)
                {
                    if (i != j && !SegmentsIntersect(pA[i], pB[i], pA[j], pB[j]))
                    {
                        isBad[i] = true;
                        isBad[j] = true;
                    }
                }
            }

            List<int> badIndices = [];
            for (int i = 0; i < n; i++)
            {
                if (isBad[i])
                    badIndices.Add(i + 1);
            }

            return badIndices.Count == 0 ? "MAGNIFICENT" : string.Join(" ", badIndices);
        };

        static bool SegmentsIntersect(Point2L p, Point2L q, Point2L r, Point2L s)
        {
            Point2L pq = q - p;
            long c1 = pq.X * (r.Y - p.Y) - pq.Y * (r.X - p.X);
            long c2 = pq.X * (s.Y - p.Y) - pq.Y * (s.X - p.X);
            if ((c1 > 0 && c2 > 0) || (c1 < 0 && c2 < 0))
                return false;

            Point2L rs = s - r;
            long c3 = rs.X * (p.Y - r.Y) - rs.Y * (p.X - r.X);
            long c4 = rs.X * (q.Y - r.Y) - rs.Y * (q.X - r.X);
            if ((c3 > 0 && c4 > 0) || (c3 < 0 && c4 < 0))
                return false;

            return true;
        }

        static int CompareLexXY(Point2L a, Point2L b)
        {
            int cmp = a.X.CompareTo(b.X);
            return cmp != 0 ? cmp : a.Y.CompareTo(b.Y);
        }

        static List<Point2L> MergeLexXY(List<Point2L> left, List<Point2L> right)
        {
            List<Point2L> merged = new(left.Count + right.Count);
            int i = 0, j = 0;
            while (i < left.Count && j < right.Count)
            {
                if (CompareLexXY(left[i], right[j]) <= 0)
                    merged.Add(left[i++]);
                else
                    merged.Add(right[j++]);
            }
            while (i < left.Count) merged.Add(left[i++]);
            while (j < right.Count) merged.Add(right[j++]);
            return merged;
        }

        static List<Point2L> BuildCcwHull(List<Point2L> sortedPoints)
        {
            int count = sortedPoints.Count;
            if (count <= 2)
                return sortedPoints;

            List<Point2L> lower = new(count);
            foreach (Point2L p in sortedPoints)
            {
                while (lower.Count >= 2 && Cross(lower[^2], lower[^1], p) <= 0)
                    lower.RemoveAt(lower.Count - 1);
                lower.Add(p);
            }

            List<Point2L> upper = new(count);
            for (int i = count - 1; i >= 0; i--)
            {
                Point2L p = sortedPoints[i];
                while (upper.Count >= 2 && Cross(upper[^2], upper[^1], p) <= 0)
                    upper.RemoveAt(upper.Count - 1);
                upper.Add(p);
            }

            if (lower.Count == 0)
                return [];
            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);
            lower.AddRange(upper);
            return lower;
        }

        static long Cross(Point2L o, Point2L a, Point2L b) =>
            (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
    }

    #endregion

    #region Q6 Go To Considered Helpful

    [Test]
    public void Q6Sample() => Test(Q6);

    [Test]
    public void Q6Small() => Test(Q6);

    [Test]
    public void Q6Large() => SlowTest(Q6);

    private static Func<string> Q6(GCJTestCase testCase)
    {
        testCase.Get(out int r, out int c);
        testCase.GetLines(r, out string[] grid);
        return () =>
        {
            int total = r * c;
            int mr = -1, mc = -1, nr = -1, nc = -1;
            bool[] passable = new bool[total];

            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    char ch = grid[i][j];
                    int idx = i * c + j;
                    passable[idx] = ch != '#';
                    if (ch == 'M') { mr = i; mc = j; }
                    else if (ch == 'N') { nr = i; nc = j; }
                }
            }

            int mIdx = mr * c + mc;
            int nIdx = nr * c + nc;
            int[] queue = new int[total];
            int[] distM = new int[total];
            Array.Fill(distM, int.MaxValue);

            int qHead = 0, qTail = 0;
            distM[mIdx] = 0;
            queue[qTail++] = mIdx;
            while (qHead < qTail)
            {
                int u = queue[qHead++];
                int ur = u / c;
                int uc = u - ur * c;
                int nd = distM[u] + 1;

                if (ur > 0 && passable[u - c] && distM[u - c] == int.MaxValue) { distM[u - c] = nd; queue[qTail++] = u - c; }
                if (ur + 1 < r && passable[u + c] && distM[u + c] == int.MaxValue) { distM[u + c] = nd; queue[qTail++] = u + c; }
                if (uc > 0 && passable[u - 1] && distM[u - 1] == int.MaxValue) { distM[u - 1] = nd; queue[qTail++] = u - 1; }
                if (uc + 1 < c && passable[u + 1] && distM[u + 1] == int.MaxValue) { distM[u + 1] = nd; queue[qTail++] = u + 1; }
            }

            if (distM[nIdx] == int.MaxValue)
                return "IMPOSSIBLE";

            int best = distM[nIdx];
            int[] maxChain = new int[total];
            int[] stamp1 = new int[total];
            int[] dist1 = new int[total];
            int[] stamp2 = new int[total];
            int[] dist2 = new int[total];
            int[] visitedB = new int[total];
            int curStamp = 0;

            for (int tr2 = 0; tr2 < r; tr2++)
            {
                for (int tc2 = 0; tc2 < c; tc2++)
                {
                    if (!passable[tr2 * c + tc2])
                        continue;
                    int dr = nr - tr2;
                    int dc = nc - tc2;
                    if (dr == 0 && dc == 0)
                        continue;

                    int stepIdx = dr * c + dc;
                    int rStart = dr >= 0 ? r - 1 : 0;
                    int rEnd = dr >= 0 ? -1 : r;
                    int rStep = dr >= 0 ? -1 : 1;
                    int cStart = dc >= 0 ? c - 1 : 0;
                    int cEnd = dc >= 0 ? -1 : c;
                    int cStep = dc >= 0 ? -1 : 1;

                    for (int cr = rStart; cr != rEnd; cr += rStep)
                    {
                        bool rowHasNext = cr + dr >= 0 && cr + dr < r;
                        int rowBase = cr * c;
                        for (int cc = cStart; cc != cEnd; cc += cStep)
                        {
                            int idx = rowBase + cc;
                            if (!passable[idx])
                            {
                                maxChain[idx] = 0;
                            }
                            else if (rowHasNext && cc + dc >= 0 && cc + dc < c)
                            {
                                maxChain[idx] = 1 + maxChain[idx + stepIdx];
                            }
                            else
                            {
                                maxChain[idx] = 1;
                            }
                        }
                    }

                    for (int k = 2; ; k++)
                    {
                        int tr = nr - (k - 1) * dr;
                        int tc = nc - (k - 1) * dc;
                        if (tr < 0 || tr >= r || tc < 0 || tc >= c)
                            break;
                        int tIdx = tr * c + tc;
                        if (maxChain[tIdx] < k)
                            break;

                        curStamp++;
                        int visCount = 0;
                        qHead = 0;
                        qTail = 0;
                        stamp1[tIdx] = curStamp;
                        dist1[tIdx] = 0;
                        queue[qTail++] = tIdx;
                        visitedB[visCount++] = tIdx;

                        while (qHead < qTail)
                        {
                            int u = queue[qHead++];
                            int du = dist1[u];
                            if (du + 2 >= best)
                                continue;
                            int ur = u / c;
                            int uc = u - ur * c;
                            int nd = du + 1;

                            if (ur > 0 && maxChain[u - c] >= k && stamp1[u - c] != curStamp)
                            {
                                stamp1[u - c] = curStamp;
                                dist1[u - c] = nd;
                                queue[qTail++] = u - c;
                                visitedB[visCount++] = u - c;
                            }
                            if (ur + 1 < r && maxChain[u + c] >= k && stamp1[u + c] != curStamp)
                            {
                                stamp1[u + c] = curStamp;
                                dist1[u + c] = nd;
                                queue[qTail++] = u + c;
                                visitedB[visCount++] = u + c;
                            }
                            if (uc > 0 && maxChain[u - 1] >= k && stamp1[u - 1] != curStamp)
                            {
                                stamp1[u - 1] = curStamp;
                                dist1[u - 1] = nd;
                                queue[qTail++] = u - 1;
                                visitedB[visCount++] = u - 1;
                            }
                            if (uc + 1 < c && maxChain[u + 1] >= k && stamp1[u + 1] != curStamp)
                            {
                                stamp1[u + 1] = curStamp;
                                dist1[u + 1] = nd;
                                queue[qTail++] = u + 1;
                                visitedB[visCount++] = u + 1;
                            }
                        }

                        qHead = 0;
                        qTail = 0;
                        stamp2[tIdx] = curStamp;
                        dist2[tIdx] = 0;
                        queue[qTail++] = tIdx;
                        int kMinus1 = k - 1;

                        while (qHead < qTail)
                        {
                            int u = queue[qHead++];
                            int du = dist2[u];
                            if (du + 2 >= best)
                                continue;
                            int ur = u / c;
                            int uc = u - ur * c;
                            int nd = du + 1;

                            if (ur > 0 && maxChain[u - c] >= kMinus1 && stamp2[u - c] != curStamp)
                            {
                                stamp2[u - c] = curStamp;
                                dist2[u - c] = nd;
                                queue[qTail++] = u - c;
                            }
                            if (ur + 1 < r && maxChain[u + c] >= kMinus1 && stamp2[u + c] != curStamp)
                            {
                                stamp2[u + c] = curStamp;
                                dist2[u + c] = nd;
                                queue[qTail++] = u + c;
                            }
                            if (uc > 0 && maxChain[u - 1] >= kMinus1 && stamp2[u - 1] != curStamp)
                            {
                                stamp2[u - 1] = curStamp;
                                dist2[u - 1] = nd;
                                queue[qTail++] = u - 1;
                            }
                            if (uc + 1 < c && maxChain[u + 1] >= kMinus1 && stamp2[u + 1] != curStamp)
                            {
                                stamp2[u + 1] = curStamp;
                                dist2[u + 1] = nd;
                                queue[qTail++] = u + 1;
                            }
                        }

                        for (int vi = 0; vi < visCount; vi++)
                        {
                            int bIdx = visitedB[vi];
                            if (distM[bIdx] == int.MaxValue)
                                continue;
                            int nextLoopIdx = bIdx + stepIdx;
                            if ((uint)nextLoopIdx >= (uint)total || stamp2[nextLoopIdx] != curStamp)
                                continue;

                            int cand = distM[bIdx] + dist1[bIdx] + dist2[nextLoopIdx] + 1;
                            if (cand < best)
                                best = cand;
                        }
                    }
                }
            }

            return best.ToString(CultureInfo.InvariantCulture);
        };
    }

    #endregion
}

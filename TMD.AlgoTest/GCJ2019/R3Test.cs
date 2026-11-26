using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2019;

[TestFixture]
public class R3Test : BaseGCJTest
{
    #region Q1 Zillionim

    [Test]
    public void Q1Small1() => InteractiveTest(Q1Solve, io => Q1Judge(io, testNumber: 0));

    [Test]
    public void Q1Small2() => InteractiveTest(Q1Solve, io => Q1Judge(io, testNumber: 1));

    [Test]
    public void Q1Large() => InteractiveTest(Q1Solve, io => Q1Judge(io, testNumber: 2));

    private static void Q1Solve(GCJInteractiveChannel io)
    {
        const long b = 10_000_000_000L;
        const long c = b * 100L;

        string[] header = io.ReadRequiredLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int t = int.Parse(header[0], CultureInfo.InvariantCulture);

        for (int tc = 0; tc < t; tc++)
        {
            List<(long Start, long End)> segments = [(1L, c + 1L)];
            while (true)
            {
                long p = long.Parse(io.ReadRequiredLine(), CultureInfo.InvariantCulture);
                if (p == -2 || p == -3)
                    break;
                if (p == -1)
                    throw new InvalidOperationException($"Case #{tc + 1}: judge returned -1.");

                segments = ApplyZillionimPlay(segments, p, b);

                long move = ChooseZillionimMove(segments, b);
                segments = ApplyZillionimPlay(segments, move, b);
                io.WriteLine(move.ToString(CultureInfo.InvariantCulture));
            }
        }
    }

    private static long ChooseZillionimMove(List<(long Start, long End)> segments, long b)
    {
        foreach (var (s, e) in segments)
        {
            if (e - s >= 3 * b)
                return s + 2 * b;
        }

        foreach (var (s, e) in segments)
        {
            long len = e - s;
            if (len > 2 * b && len < 3 * b - 1)
                return s + (len - b) / 2;
        }

        foreach (var (s, e) in segments)
        {
            if (e - s > 2 * b)
                return s;
        }

        int singleCount = 0;
        int twoCount = 0;
        long singleStart = -1;
        long twoStart = -1;
        foreach (var (s, e) in segments)
        {
            if (e - s == 2 * b)
            {
                twoCount++;
                twoStart = s;
            }
            else
            {
                singleCount++;
                singleStart = s;
            }
        }

        int total = singleCount + twoCount;
        if ((total & 1) == 1)
        {
            if (singleStart >= 0)
                return singleStart;
            return twoStart + 1;
        }

        if (twoStart >= 0)
            return twoStart;
        return singleStart;
    }

    private static List<(long Start, long End)> ApplyZillionimPlay(List<(long Start, long End)> status, long p, long b)
    {
        List<(long Start, long End)> next = new(status.Count + 1);
        bool found = false;
        foreach (var (start, end) in status)
        {
            if (!found && p >= start && p <= end - b)
            {
                found = true;
                if (p - start >= b)
                    next.Add((start, p));
                if (end - (p + b) >= b)
                    next.Add((p + b, end));
            }
            else
            {
                next.Add((start, end));
            }
        }
        Assert.IsTrue(found, $"Invalid play {p}.");
        return next;
    }

    private static void Q1Judge(GCJInteractiveChannel io, int testNumber)
    {
        const int t = 500;
        int[] winThresholds = [300, 475, 499];
        const long b = 10_000_000_000L;
        const long c = b * 100L;
        int w = winThresholds[testNumber];

        io.WriteLine($"{t} {w}");
        int wins = 0;

        for (int game = 1; game <= t; game++)
        {
            Random rng = new(45282 + t * t + w * t + game + testNumber * 10000);
            List<(long Start, long End)> status = [(1L, c + 1L)];

            while (true)
            {
                if (status.Count == 0)
                {
                    io.WriteLine("-2");
                    wins++;
                    break;
                }

                long validPoints = 0;
                foreach (var (s, e) in status)
                    validPoints += e - s - b + 1;

                long pick = rng.NextInt64(validPoints);
                long aiMove = -1;
                foreach (var (s, e) in status)
                {
                    long count = e - s - b + 1;
                    if (pick < count)
                    {
                        aiMove = s + pick;
                        break;
                    }
                    pick -= count;
                }

                status = ApplyZillionimPlay(status, aiMove, b);
                if (status.Count == 0)
                {
                    io.WriteLine("-3");
                    break;
                }

                io.WriteLine(aiMove.ToString(CultureInfo.InvariantCulture));
                string resp = io.ReadRequiredLine();
                Assert.LessOrEqual(resp.Length, 100);
                long playerMove = long.Parse(resp, CultureInfo.InvariantCulture);
                status = ApplyZillionimPlay(status, playerMove, b);
            }
        }

        Assert.IsNull(io.ReadLine(), "Additional input after all cases finish.");
        Assert.GreaterOrEqual(wins, w, $"Got {wins}/{t} wins, needed {w}.");
    }

    #endregion

    #region Q2 Pancake Pyramid

    [Test]
    public void Q2Sample() => Test(Q2);

    [Test]
    public void Q2Small() => Test(Q2);

    [Test]
    public void Q2Large() => Test(Q2);

    private static Func<string> Q2(GCJTestCase testCase)
    {
        testCase.Get(out int s);
        testCase.Get(out long[] p);
        return () =>
        {
            const long mod = 1_000_000_007L;
            long[] pref = new long[s + 1];
            for (int i = 0; i < s; i++)
                pref[i + 1] = pref[i] + p[i];

            long[] cLeft = new long[s];
            long[] dLeft = new long[s];
            Stack<int> stack = new();

            for (int i = 0; i < s; i++)
            {
                long cl = 0;
                while (stack.Count > 0 && p[stack.Peek()] <= p[i])
                {
                    int t = stack.Pop();
                    long gap = p[t] * (i - 1 - t) - (pref[i] - pref[t + 1]);
                    cl = (cl + cLeft[t] + (gap % mod) * dLeft[t]) % mod;
                }
                int blocker = stack.Count == 0 ? -1 : stack.Peek();
                dLeft[i] = i - blocker;
                cLeft[i] = cl;
                stack.Push(i);
            }

            long[] cRight = new long[s];
            long[] dRight = new long[s];
            stack.Clear();

            for (int i = s - 1; i >= 0; i--)
            {
                long cr = 0;
                while (stack.Count > 0 && p[stack.Peek()] < p[i])
                {
                    int t = stack.Pop();
                    long gap = p[t] * (t - i - 1) - (pref[t] - pref[i + 1]);
                    cr = (cr + cRight[t] + (gap % mod) * dRight[t]) % mod;
                }
                int blocker = stack.Count == 0 ? s : stack.Peek();
                dRight[i] = blocker - i;
                cRight[i] = cr;
                stack.Push(i);
            }

            long ans = 0;
            for (int i = 0; i < s; i++)
                ans = (ans + cLeft[i] * dRight[i] + cRight[i] * dLeft[i]) % mod;

            return ans.ToString(CultureInfo.InvariantCulture);
        };
    }

    #endregion

    #region Q3 Datacenter Duplex

    [Test]
    public void Q3Sample() => Test(Q3, ValidateDatacenterDuplex);

    [Test]
    public void Q3Small() => Test(Q3, ValidateDatacenterDuplex);

    [Test]
    public void Q3Large() => Test(Q3, ValidateDatacenterDuplex);

    private static Func<string> Q3(GCJTestCase testCase)
    {
        testCase.Get(out int r, out int c);
        testCase.GetLines(r, out string[] grid);
        return () =>
        {
            int total = r * c;
            int[] parent = new int[total];
            for (int i = 0; i < total; i++)
                parent[i] = i;

            int components = total;
            int Find(int x)
            {
                int root = x;
                while (parent[root] != root)
                    root = parent[root];
                while (parent[x] != root)
                {
                    int p = parent[x];
                    parent[x] = root;
                    x = p;
                }
                return root;
            }

            bool Union(int a, int b)
            {
                int ra = Find(a);
                int rb = Find(b);
                if (ra == rb)
                    return false;
                parent[rb] = ra;
                components--;
                return true;
            }

            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    int id = i * c + j;
                    if (i + 1 < r && grid[i][j] == grid[i + 1][j])
                        Union(id, id + c);
                    if (j + 1 < c && grid[i][j] == grid[i][j + 1])
                        Union(id, id + 1);
                }
            }

            char[,] outGrid = new char[r - 1, c - 1];
            for (int i = 0; i < r - 1; i++)
            {
                for (int j = 0; j < c - 1; j++)
                {
                    int tl = i * c + j;
                    int tr = tl + 1;
                    int bl = tl + c;
                    int br = bl + 1;

                    if (grid[i][j] == grid[i + 1][j + 1] && Union(tl, br))
                        outGrid[i, j] = '\\';
                    else if (grid[i + 1][j] == grid[i][j + 1] && Union(bl, tr))
                        outGrid[i, j] = '/';
                    else
                        outGrid[i, j] = '.';
                }
            }

            if (components != 2)
                return "IMPOSSIBLE";

            StringBuilder sb = new();
            sb.Append("POSSIBLE");
            for (int i = 0; i < r - 1; i++)
            {
                sb.Append('\n');
                for (int j = 0; j < c - 1; j++)
                    sb.Append(outGrid[i, j]);
            }
            return sb.ToString();
        };
    }

    private static void ValidateDatacenterDuplex(string input, string expectedOutput, List<string> actualCases)
    {
        string[] inputLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        int numCases = int.Parse(inputLines[0], CultureInfo.InvariantCulture);
        Assert.AreEqual(numCases, actualCases.Count);

        bool[] expectedPossible = ParsePossibleFlags(expectedOutput, numCases);
        int ptr = 1;
        for (int tc = 0; tc < numCases; tc++)
        {
            string[] rc = inputLines[ptr++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int r = int.Parse(rc[0], CultureInfo.InvariantCulture);
            int c = int.Parse(rc[1], CultureInfo.InvariantCulture);
            string[] matrix = new string[r];
            for (int i = 0; i < r; i++)
                matrix[i] = inputLines[ptr++].Trim();

            string[] lines = actualCases[tc].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.GreaterOrEqual(lines.Length, 1);
            string prefix = $"Case #{tc + 1}:";
            Assert.IsTrue(lines[0].StartsWith(prefix, StringComparison.Ordinal));
            string verdict = lines[0][prefix.Length..].Trim();

            if (!expectedPossible[tc])
            {
                Assert.AreEqual("IMPOSSIBLE", verdict, $"Case #{tc + 1}: expected IMPOSSIBLE.");
                Assert.AreEqual(1, lines.Length);
                continue;
            }

            Assert.AreEqual("POSSIBLE", verdict, $"Case #{tc + 1}: expected POSSIBLE.");
            Assert.AreEqual(r, lines.Length, $"Case #{tc + 1}: wrong line count.");

            List<(int R, int C)>[,] adj = new List<(int, int)>[r, c];
            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    adj[i, j] = [];

            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    if (i + 1 < r && matrix[i][j] == matrix[i + 1][j])
                    {
                        adj[i, j].Add((i + 1, j));
                        adj[i + 1, j].Add((i, j));
                    }
                    if (j + 1 < c && matrix[i][j] == matrix[i][j + 1])
                    {
                        adj[i, j].Add((i, j + 1));
                        adj[i, j + 1].Add((i, j));
                    }
                }
            }

            for (int i = 0; i < r - 1; i++)
            {
                string row = lines[i + 1].Trim();
                Assert.AreEqual(c - 1, row.Length, $"Case #{tc + 1}: row {i} wrong length.");
                for (int j = 0; j < c - 1; j++)
                {
                    char ch = row[j];
                    Assert.IsTrue(ch is '.' or '\\' or '/');
                    if (ch == '\\')
                    {
                        Assert.AreEqual(matrix[i][j], matrix[i + 1][j + 1], $"Case #{tc + 1}: mismatched '\\' at ({i},{j}).");
                        adj[i, j].Add((i + 1, j + 1));
                        adj[i + 1, j + 1].Add((i, j));
                    }
                    else if (ch == '/')
                    {
                        Assert.AreEqual(matrix[i + 1][j], matrix[i][j + 1], $"Case #{tc + 1}: mismatched '/' at ({i},{j}).");
                        adj[i + 1, j].Add((i, j + 1));
                        adj[i, j + 1].Add((i + 1, j));
                    }
                }
            }

            bool[,] vis = new bool[r, c];
            int compCount = 0;
            Queue<(int R, int C)> q = new();
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    if (vis[i, j])
                        continue;
                    compCount++;
                    vis[i, j] = true;
                    q.Enqueue((i, j));
                    while (q.Count > 0)
                    {
                        var (cr, cc) = q.Dequeue();
                        foreach (var (nr, nc) in adj[cr, cc])
                        {
                            if (!vis[nr, nc])
                            {
                                vis[nr, nc] = true;
                                q.Enqueue((nr, nc));
                            }
                        }
                    }
                }
            }

            Assert.AreEqual(2, compCount, $"Case #{tc + 1}: expected 2 connected components, got {compCount}.");
        }
    }

    private static bool[] ParsePossibleFlags(string expectedOutput, int numCases)
    {
        bool[] flags = new bool[numCases];
        foreach (string line in expectedOutput.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (!line.StartsWith("Case #", StringComparison.Ordinal))
                continue;
            int colonIdx = line.IndexOf(':');
            int caseNum = int.Parse(line[6..colonIdx], CultureInfo.InvariantCulture);
            string word = line[(colonIdx + 1)..].Trim();
            flags[caseNum - 1] = !string.Equals(word, "IMPOSSIBLE", StringComparison.OrdinalIgnoreCase);
        }
        return flags;
    }

    #endregion

    #region Q4 Napkin Folding

    [Test]
    public void Q4Sample() => Test(Q4, ValidateNapkinFolding);

    [Test]
    public void Q4Small() => Test(Q4, ValidateNapkinFolding);

    [Test]
    public void Q4Large() => Test(Q4, ValidateNapkinFolding);

    private static Func<string> Q4(GCJTestCase testCase)
    {
        testCase.Get(out int n, out int k);
        testCase.GetLines(n, out long[] rawX, out long[] rawY);
        return () =>
        {
            long scale = 1;
            for (int i = 2; i <= k; i++)
                scale = MathFormulas.Lcm(scale, i);

            Point2L[] poly = new Point2L[n];
            for (int i = 0; i < n; i++)
                poly[i] = new Point2L(rawX[i] * scale, rawY[i] * scale);

            long totalArea2 = 0;
            for (int i = 0; i < n; i++)
            {
                Point2L a = poly[i];
                Point2L b = poly[(i + 1) % n];
                totalArea2 -= a.X * b.Y - a.Y * b.X;
            }
            if (totalArea2 <= 0 || totalArea2 % k != 0)
                return "IMPOSSIBLE";
            long targetArea2 = totalArea2 / k;

            List<(int Num, int Den)> fracs = [(0, 1)];
            for (int den = 2; den <= k; den++)
            {
                for (int num = 1; num < den; num++)
                {
                    if (MathFormulas.Gcd(num, den) == 1)
                        fracs.Add((num, den));
                }
            }
            fracs.Sort((a, b) => (a.Num * b.Den).CompareTo(b.Num * a.Den));

            int perEdge = fracs.Count;
            int m = n * perEdge;
            Point2L[] boundary = new Point2L[m];
            int[] edgeOf = new int[m];
            int[] edgeStart = new int[n + 1];
            Dictionary<Point2L, int> pointIndex = new(m);
            Point2L[] edgeVec = new Point2L[n];

            for (int e = 0; e < n; e++)
            {
                Point2L u = poly[e];
                Point2L v = poly[(e + 1) % n];
                Point2L d = v - u;
                edgeVec[e] = d;
                edgeStart[e] = e * perEdge;
                for (int f = 0; f < perEdge; f++)
                {
                    int idx = edgeStart[e] + f;
                    var (num, den) = fracs[f];
                    Point2L pt = new(u.X + (d.X / den) * num, u.Y + (d.Y / den) * num);
                    boundary[idx] = pt;
                    edgeOf[idx] = e;
                    pointIndex[pt] = idx;
                }
            }
            edgeStart[n] = m;

            long[] prefixEdgeArea = new long[2 * n + 1];
            for (int e = 0; e < 2 * n; e++)
            {
                Point2L a = poly[e % n];
                Point2L b = poly[(e + 1) % n];
                prefixEdgeArea[e + 1] = prefixEdgeArea[e] - (a.X * b.Y - a.Y * b.X);
            }

            bool IsInsideDir(int bIdx, Point2L d)
            {
                Point2L p = boundary[bIdx];
                Point2L inVec = boundary[(bIdx - 1 + m) % m] - p;
                Point2L outVec = boundary[(bIdx + 1) % m] - p;
                long turn = inVec.X * outVec.Y - inVec.Y * outVec.X;
                long cOut = outVec.X * d.Y - outVec.Y * d.X;
                long cIn = d.X * inVec.Y - d.Y * inVec.X;
                if (turn > 0)
                    return cOut < 0 && cIn < 0;
                if (turn < 0)
                    return cOut < 0 || cIn < 0;
                return cOut < 0;
            }

            bool SegmentInsidePoly(int aIdx, int bIdx)
            {
                Point2L a = boundary[aIdx];
                Point2L b = boundary[bIdx];
                if (!IsInsideDir(aIdx, b - a) || !IsInsideDir(bIdx, a - b))
                    return false;

                long abx = b.X - a.X;
                long aby = b.Y - a.Y;
                long minX = Math.Min(a.X, b.X);
                long maxX = Math.Max(a.X, b.X);
                long minY = Math.Min(a.Y, b.Y);
                long maxY = Math.Max(a.Y, b.Y);

                for (int e = 0; e < n; e++)
                {
                    Point2L u = poly[e];
                    if (u != a && u != b &&
                        u.X >= minX && u.X <= maxX && u.Y >= minY && u.Y <= maxY &&
                        (abx * (u.Y - a.Y) - aby * (u.X - a.X)) == 0)
                    {
                        return false;
                    }

                    Point2L v = poly[(e + 1) % n];
                    if (a == u || a == v || b == u || b == v)
                        continue;

                    int s1 = Math.Sign(abx * (u.Y - a.Y) - aby * (u.X - a.X));
                    int s2 = Math.Sign(abx * (v.Y - a.Y) - aby * (v.X - a.X));
                    if (s1 == 0 || s2 == 0 || s1 == s2)
                        continue;

                    long uvx = v.X - u.X;
                    long uvy = v.Y - u.Y;
                    int s3 = Math.Sign(uvx * (a.Y - u.Y) - uvy * (a.X - u.X));
                    int s4 = Math.Sign(uvx * (b.Y - u.Y) - uvy * (b.X - u.X));
                    if (s3 != 0 && s4 != 0 && s3 != s4)
                        return false;
                }
                return true;
            }

            bool[] covered = new bool[m];
            Queue<(Point2L[] Poly, int FoldIdx)> bfs = new();

            for (int p = 0; p < m; p++)
            {
                Point2L ptP = boundary[p];
                int ep = edgeOf[p];
                Point2L vNext = poly[(ep + 1) % n];
                long pToVNext = -(ptP.X * vNext.Y - ptP.Y * vNext.X);

                for (int stepE = 1; stepE < n; stepE++)
                {
                    int endE = ep + stepE;
                    int eq = endE % n;
                    Point2L vEq = poly[eq];
                    Point2L dEq = edgeVec[eq];

                    long baseArea = pToVNext + (prefixEdgeArea[endE] - prefixEdgeArea[ep + 1]) - (vEq.X * ptP.Y - vEq.Y * ptP.X);
                    long slope = -((vEq.X - ptP.X) * dEq.Y - (vEq.Y - ptP.Y) * dEq.X);

                    int q = -1;
                    if (slope == 0)
                    {
                        if (baseArea == targetArea2)
                            q = edgeStart[eq];
                        else
                            continue;
                    }
                    else
                    {
                        long num = targetArea2 - baseArea;
                        long den = slope;
                        if (den < 0)
                        {
                            num = -num;
                            den = -den;
                        }
                        if (num < 0 || num >= den)
                            continue;

                        long g = MathFormulas.Gcd(num, den);
                        long reducedDen = den / g;
                        if (reducedDen > k)
                            continue;

                        long reducedNum = num / g;
                        Point2L ptQ = new(vEq.X + (dEq.X / reducedDen) * reducedNum, vEq.Y + (dEq.Y / reducedDen) * reducedNum);
                        if (!pointIndex.TryGetValue(ptQ, out q) || edgeOf[q] != eq)
                            continue;
                    }

                    if (!SegmentInsidePoly(p, q))
                        continue;

                    if (TryUnfold(p, q, ep, eq, boundary, edgeOf, edgeStart, edgeVec, poly, pointIndex, covered, bfs,
                            SegmentInsidePoly, k, out List<(Point2L A, Point2L B)>? folds))
                    {
                        StringBuilder sb = new();
                        sb.Append("POSSIBLE");
                        foreach (var (fa, fb) in folds!)
                        {
                            sb.Append('\n')
                                .Append(FormatCoord(fa.X, scale)).Append(' ')
                                .Append(FormatCoord(fa.Y, scale)).Append(' ')
                                .Append(FormatCoord(fb.X, scale)).Append(' ')
                                .Append(FormatCoord(fb.Y, scale));
                        }
                        return sb.ToString();
                    }
                }
            }

            return "IMPOSSIBLE";
        };

        static bool TryUnfold(
            int p,
            int q,
            int ep,
            int eq,
            Point2L[] boundary,
            int[] edgeOf,
            int[] edgeStart,
            Point2L[] edgeVec,
            Point2L[] poly,
            Dictionary<Point2L, int> pointIndex,
            bool[] covered,
            Queue<(Point2L[] Poly, int FoldIdx)> bfs,
            Func<int, int, bool> segmentInsidePoly,
            int k,
            out List<(Point2L A, Point2L B)>? resultFolds)
        {
            int n = poly.Length;
            int m = boundary.Length;
            Array.Fill(covered, false);
            bfs.Clear();

            int coveredCount = 0;
            for (int idx = p; idx != q; idx = (idx + 1) % m)
            {
                covered[idx] = true;
                coveredCount++;
            }

            List<Point2L> cornerList = [boundary[p]];
            for (int e = (ep + 1) % n; e != eq; e = (e + 1) % n)
                cornerList.Add(poly[e]);
            if (q != edgeStart[eq])
                cornerList.Add(poly[eq]);
            cornerList.Add(boundary[q]);

            Point2L[] corner = [.. cornerList];
            List<(Point2L A, Point2L B)> folds = [(boundary[p], boundary[q])];
            int regionsBuilt = 1;
            bfs.Enqueue((corner, corner.Length - 1));

            while (bfs.Count > 0)
            {
                var (curPoly, foldIdx) = bfs.Dequeue();
                int len = curPoly.Length;
                Point2L u = curPoly[foldIdx];
                Point2L v = curPoly[(foldIdx + 1) % len];
                Point2L d = v - u;
                Int128 len2 = (Int128)d.X * d.X + (Int128)d.Y * d.Y;

                Point2L[] reflected = new Point2L[len];
                for (int i = 0; i < len; i++)
                {
                    Point2L w = curPoly[i];
                    long wx = w.X - u.X;
                    long wy = w.Y - u.Y;
                    Int128 dot = (Int128)wx * d.X + (Int128)wy * d.Y;
                    Int128 numX = 2 * dot * d.X - (Int128)wx * len2;
                    Int128 numY = 2 * dot * d.Y - (Int128)wy * len2;
                    if (numX % len2 != 0 || numY % len2 != 0)
                    {
                        resultFolds = null;
                        return false;
                    }
                    Point2L rw = new(u.X + (long)(numX / len2), u.Y + (long)(numY / len2));
                    if (!pointIndex.ContainsKey(rw))
                    {
                        resultFolds = null;
                        return false;
                    }
                    reflected[len - 1 - i] = rw;
                }

                regionsBuilt++;
                if (regionsBuilt > k)
                {
                    resultFolds = null;
                    return false;
                }

                // In reversed array, curPoly[foldIdx] -> curPoly[nextFold] became edge (len - 1 - nextFold) -> (len - 1 - foldIdx).
                int skipEdge = foldIdx == len - 1 ? len - 1 : len - 2 - foldIdx;

                for (int i = 0; i < len; i++)
                {
                    if (i == skipEdge)
                        continue;

                    Point2L ea = reflected[i];
                    Point2L eb = reflected[(i + 1) % len];
                    int aIdx = pointIndex[ea];
                    int bIdx = pointIndex[eb];

                    int sharedEdge = FindSharedPolygonEdge(aIdx, bIdx, edgeOf, edgeStart, n, poly, ea, eb, out bool sameDir);
                    if (sharedEdge >= 0)
                    {
                        if (!sameDir)
                        {
                            resultFolds = null;
                            return false;
                        }

                        int jStart = aIdx;
                        int endVertexNext = edgeStart[(sharedEdge + 1) % n];
                        int jEnd = bIdx == endVertexNext ? edgeStart[sharedEdge + 1] : bIdx;
                        if (jStart >= jEnd)
                        {
                            resultFolds = null;
                            return false;
                        }
                        for (int j = jStart; j < jEnd; j++)
                        {
                            if (covered[j])
                            {
                                resultFolds = null;
                                return false;
                            }
                            covered[j] = true;
                            coveredCount++;
                        }
                    }
                    else
                    {
                        if (folds.Count >= k - 1 || !segmentInsidePoly(aIdx, bIdx))
                        {
                            resultFolds = null;
                            return false;
                        }
                        folds.Add((ea, eb));
                        bfs.Enqueue((reflected, i));
                    }
                }
            }

            if (regionsBuilt != k || coveredCount != m || folds.Count != k - 1)
            {
                resultFolds = null;
                return false;
            }

            for (int i = 0; i < folds.Count; i++)
            {
                for (int j = i + 1; j < folds.Count; j++)
                {
                    if (SegmentsCrossOrOverlapExcludingSharedEndpoint(folds[i].A, folds[i].B, folds[j].A, folds[j].B))
                    {
                        resultFolds = null;
                        return false;
                    }
                }
            }

            resultFolds = folds;
            return true;
        }

        static int FindSharedPolygonEdge(
            int aIdx,
            int bIdx,
            int[] edgeOf,
            int[] edgeStart,
            int n,
            Point2L[] poly,
            Point2L ea,
            Point2L eb,
            out bool sameDirection)
        {
            int ea0 = edgeOf[aIdx];
            int ea1 = aIdx == edgeStart[ea0] ? (ea0 + n - 1) % n : -1;
            int eb0 = edgeOf[bIdx];
            int eb1 = bIdx == edgeStart[eb0] ? (eb0 + n - 1) % n : -1;

            int matched = -1;
            if (ea0 == eb0 || ea0 == eb1)
                matched = ea0;
            else if (ea1 >= 0 && (ea1 == eb0 || ea1 == eb1))
                matched = ea1;

            if (matched < 0)
            {
                sameDirection = false;
                return -1;
            }

            Point2L dirEdge = poly[(matched + 1) % n] - poly[matched];
            Point2L segDir = eb - ea;
            sameDirection = (Int128)dirEdge.X * segDir.X + (Int128)dirEdge.Y * segDir.Y > 0;
            return matched;
        }

        static bool SegmentsCrossOrOverlapExcludingSharedEndpoint(Point2L a, Point2L b, Point2L c, Point2L d)
        {
            long abx = b.X - a.X;
            long aby = b.Y - a.Y;
            long cdx = d.X - c.X;
            long cdy = d.Y - c.Y;

            Int128 c1 = (Int128)abx * (c.Y - a.Y) - (Int128)aby * (c.X - a.X);
            Int128 c2 = (Int128)abx * (d.Y - a.Y) - (Int128)aby * (d.X - a.X);
            Int128 c3 = (Int128)cdx * (a.Y - c.Y) - (Int128)cdy * (a.X - c.X);
            Int128 c4 = (Int128)cdx * (b.Y - c.Y) - (Int128)cdy * (b.X - c.X);

            if (c1 == 0 && c2 == 0)
            {
                long a0 = Math.Min(a.X, b.X);
                long a1 = Math.Max(a.X, b.X);
                long b0 = Math.Min(c.X, d.X);
                long b1 = Math.Max(c.X, d.X);
                long ya0 = Math.Min(a.Y, b.Y);
                long ya1 = Math.Max(a.Y, b.Y);
                long yb0 = Math.Min(c.Y, d.Y);
                long yb1 = Math.Max(c.Y, d.Y);

                long ox = Math.Min(a1, b1) - Math.Max(a0, b0);
                long oy = Math.Min(ya1, yb1) - Math.Max(ya0, yb0);
                if (ox < 0 || oy < 0)
                    return false;
                if (ox == 0 && oy == 0)
                    return false; // Meet cleanly at a shared endpoint only.
                return true;
            }

            if ((c1 > 0 && c2 < 0) || (c1 < 0 && c2 > 0))
            {
                if ((c3 > 0 && c4 < 0) || (c3 < 0 && c4 > 0))
                    return true;
                if (c3 == 0 && a != c && a != d)
                    return true;
                if (c4 == 0 && b != c && b != d)
                    return true;
            }

            if ((c3 > 0 && c4 < 0) || (c3 < 0 && c4 > 0))
            {
                if (c1 == 0 && c != a && c != b)
                    return true;
                if (c2 == 0 && d != a && d != b)
                    return true;
            }

            return false;
        }

        static string FormatCoord(long scaledVal, long scale)
        {
            long g = MathFormulas.Gcd(Math.Abs(scaledVal), scale);
            return $"{scaledVal / g}/{scale / g}";
        }
    }

    private static void ValidateNapkinFolding(string input, string expectedOutput, List<string> actualCases)
    {
        string[] inputLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        int numCases = int.Parse(inputLines[0], CultureInfo.InvariantCulture);
        Assert.AreEqual(numCases, actualCases.Count);

        int[] caseK = new int[numCases];
        int ptr = 1;
        for (int tc = 0; tc < numCases; tc++)
        {
            string[] nk = inputLines[ptr++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int n = int.Parse(nk[0], CultureInfo.InvariantCulture);
            caseK[tc] = int.Parse(nk[1], CultureInfo.InvariantCulture);
            ptr += n;
        }

        List<List<string[]>> expectedCases = SplitTokensByCase(expectedOutput, numCases);
        for (int tc = 0; tc < numCases; tc++)
        {
            List<string[]> expLines = expectedCases[tc];
            string[] actLinesRaw = actualCases[tc].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.GreaterOrEqual(actLinesRaw.Length, 1);
            string prefix = $"Case #{tc + 1}:";
            Assert.IsTrue(actLinesRaw[0].StartsWith(prefix, StringComparison.Ordinal));

            List<string[]> actLines = new(actLinesRaw.Length);
            actLines.Add(actLinesRaw[0][prefix.Length..].Split(' ', StringSplitOptions.RemoveEmptyEntries));
            for (int i = 1; i < actLinesRaw.Length; i++)
                actLines.Add(actLinesRaw[i].Split(' ', StringSplitOptions.RemoveEmptyEntries));

            Assert.AreEqual(1, actLines[0].Length);
            string expVerdict = expLines[0][0].ToUpperInvariant();
            string actVerdict = actLines[0][0].ToUpperInvariant();
            Assert.AreEqual(expVerdict, actVerdict, $"Case #{tc + 1}: POSSIBLE/IMPOSSIBLE mismatch.");

            if (expVerdict == "IMPOSSIBLE")
            {
                Assert.AreEqual(1, actLines.Count);
                continue;
            }

            int k = caseK[tc];
            Assert.AreEqual(k - 1, actLines.Count - 1, $"Case #{tc + 1}: expected {k - 1} segments.");
            List<string> actPattern = NormalizeFoldingPattern(actLines.GetRange(1, k - 1));

            int numPatterns = int.Parse(expLines[1][0], CultureInfo.InvariantCulture);
            bool matched = false;
            for (int pIdx = 0; pIdx < numPatterns; pIdx++)
            {
                List<string> expPattern = NormalizeFoldingPattern(expLines.GetRange(2 + pIdx * (k - 1), k - 1));
                if (expPattern.Count == actPattern.Count)
                {
                    bool eq = true;
                    for (int i = 0; i < expPattern.Count; i++)
                    {
                        if (!string.Equals(expPattern[i], actPattern[i], StringComparison.Ordinal))
                        {
                            eq = false;
                            break;
                        }
                    }
                    if (eq)
                    {
                        matched = true;
                        break;
                    }
                }
            }
            Assert.IsTrue(matched, $"Case #{tc + 1}: folding pattern not found in valid pattern set.");
        }

        static List<string> NormalizeFoldingPattern(List<string[]> segments)
        {
            List<string> normalized = new(segments.Count);
            foreach (string[] seg in segments)
            {
                Assert.AreEqual(4, seg.Length);
                string ax = seg[0], ay = seg[1], bx = seg[2], by = seg[3];
                int cmpX = string.CompareOrdinal(ax, bx);
                if (cmpX > 0 || (cmpX == 0 && string.CompareOrdinal(ay, by) > 0))
                    (ax, ay, bx, by) = (bx, by, ax, ay);
                normalized.Add($"{ax}_{ay}_{bx}_{by}");
            }
            normalized.Sort(StringComparer.Ordinal);
            return normalized;
        }

        static List<List<string[]>> SplitTokensByCase(string text, int numCases)
        {
            List<List<string[]>> res = new(numCases);
            foreach (string line in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
            {
                string[] toks = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (toks.Length >= 2 && toks[0].Equals("Case", StringComparison.OrdinalIgnoreCase) && toks[1].StartsWith('#'))
                    res.Add([toks[2..]]);
                else
                    res[^1].Add(toks);
            }
            Assert.AreEqual(numCases, res.Count);
            return res;
        }
    }

    #endregion
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2018;

[TestFixture]
public class R3Test : BaseGCJTest
{
    #region Q1 Field Trip

    [Test]
    public void Q1Sample() => Test(Q1);

    [Test]
    public void Q1Small() => Test(Q1);

    [Test]
    public void Q1Large() => Test(Q1);

    private static Func<long> Q1(GCJTestCase testCase)
    {
        testCase.Get(out int n);
        long minR = long.MaxValue, maxR = long.MinValue;
        long minC = long.MaxValue, maxC = long.MinValue;
        for (int i = 0; i < n; i++)
        {
            testCase.Get(out long r, out long c);
            if (r < minR) minR = r;
            if (r > maxR) maxR = r;
            if (c < minC) minC = c;
            if (c > maxC) maxC = c;
        }

        return () => Math.Max((maxR - minR + 1) / 2, (maxC - minC + 1) / 2);
    }

    #endregion

    #region Q2 Name-Preserving Network

    private static readonly ConcurrentDictionary<int, (List<(int U, int V)> Edges, Dictionary<string, int> SigToOrig)>
        NetworkCache = new();

    [Test]
    public void Q2Small() => InteractiveTest(Q2Solve, io => Q2Judge(io, testNumber: 0));

    [Test]
    public void Q2Large() => InteractiveTest(Q2Solve, io => Q2Judge(io, testNumber: 1));

    private static (List<(int U, int V)> Edges, Dictionary<string, int> SigToOrig) GetOrBuildAsymmetricNetwork(int n)
    {
        return NetworkCache.GetOrAdd(n, static vertexCount =>
        {
            for (int attempt = 0; attempt < 5000; attempt++)
            {
                Random rng = new(vertexCount * 1009 + attempt * 17 + 7);
                bool[,] adj = new bool[vertexCount, vertexCount];
                for (int i = 0; i < vertexCount; i++)
                {
                    int n1 = (i + 1) % vertexCount;
                    int n2 = (i + 2) % vertexCount;
                    adj[i, n1] = adj[n1, i] = true;
                    adj[i, n2] = adj[n2, i] = true;
                }

                List<(int U, int V)> edges = new(2 * vertexCount);
                for (int i = 0; i < vertexCount; i++)
                    for (int j = i + 1; j < vertexCount; j++)
                        if (adj[i, j])
                            edges.Add((i, j));

                int swapCount = 12 * vertexCount;
                for (int step = 0; step < swapCount; step++)
                {
                    int e1 = rng.Next(edges.Count);
                    int e2 = rng.Next(edges.Count);
                    if (e1 == e2)
                        continue;

                    var (a, b) = edges[e1];
                    var (c, d) = edges[e2];
                    if (a == c || a == d || b == c || b == d)
                        continue;

                    bool flip = rng.Next(2) == 0;
                    int na1 = a, nb1 = flip ? d : c;
                    int na2 = b, nb2 = flip ? c : d;
                    if (adj[na1, nb1] || adj[na2, nb2])
                        continue;

                    adj[a, b] = adj[b, a] = false;
                    adj[c, d] = adj[d, c] = false;
                    adj[na1, nb1] = adj[nb1, na1] = true;
                    adj[na2, nb2] = adj[nb2, na2] = true;

                    if (IsGraphConnected(adj, vertexCount))
                    {
                        edges[e1] = (Math.Min(na1, nb1), Math.Max(na1, nb1));
                        edges[e2] = (Math.Min(na2, nb2), Math.Max(na2, nb2));
                    }
                    else
                    {
                        adj[na1, nb1] = adj[nb1, na1] = false;
                        adj[na2, nb2] = adj[nb2, na2] = false;
                        adj[a, b] = adj[b, a] = true;
                        adj[c, d] = adj[d, c] = true;
                    }
                }

                string[] sigs = ComputeVertexSignatures(vertexCount, adj);
                Dictionary<string, int> map = new(vertexCount, StringComparer.Ordinal);
                bool unique = true;
                for (int i = 0; i < vertexCount; i++)
                {
                    if (!map.TryAdd(sigs[i], i))
                    {
                        unique = false;
                        break;
                    }
                }

                if (unique)
                    return (edges, map);
            }

            throw new InvalidOperationException($"Failed to build asymmetric 4-regular graph for n={vertexCount}.");
        });
    }

    private static bool IsGraphConnected(bool[,] adj, int n)
    {
        bool[] vis = new bool[n];
        int[] q = new int[n];
        int head = 0, tail = 0;
        vis[0] = true;
        q[tail++] = 0;
        while (head < tail)
        {
            int u = q[head++];
            for (int v = 0; v < n; v++)
            {
                if (adj[u, v] && !vis[v])
                {
                    vis[v] = true;
                    q[tail++] = v;
                }
            }
        }
        return tail == n;
    }

    private static string[] ComputeVertexSignatures(int n, bool[,] adj)
    {
        int[,] a1 = new int[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                if (adj[i, j])
                    a1[i, j] = 1;

        int[,] a2 = MultiplyInt(a1, a1, n);
        int[,] a4 = MultiplyInt(a2, a2, n);
        int[,] a5 = MultiplyInt(a4, a1, n);
        int[,] a6 = MultiplyInt(a5, a1, n);
        int[,] a7 = MultiplyInt(a6, a1, n);

        string[] sigs = new string[n];
        int[] row5 = new int[n];
        int[] row6 = new int[n];
        int[] row7 = new int[n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                row5[j] = a5[i, j];
                row6[j] = a6[i, j];
                row7[j] = a7[i, j];
            }
            Array.Sort(row5);
            Array.Sort(row6);
            Array.Sort(row7);
            sigs[i] = string.Concat(
                a7[i, i].ToString(), "|",
                string.Join(',', row7), "|",
                string.Join(',', row6), "|",
                string.Join(',', row5));
        }

        return sigs;
    }

    private static int[,] MultiplyInt(int[,] left, int[,] right, int n)
    {
        int[,] res = new int[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int k = 0; k < n; k++)
            {
                int lik = left[i, k];
                if (lik == 0)
                    continue;
                for (int j = 0; j < n; j++)
                    res[i, j] += lik * right[k, j];
            }
        }
        return res;
    }

    private static void Q2Solve(GCJInteractiveChannel io)
    {
        int t = int.Parse(io.ReadRequiredLine());
        for (int tc = 0; tc < t; tc++)
        {
            string[] lu = io.ReadRequiredLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int l = int.Parse(lu[0]);
            var (edges, sigToOrig) = GetOrBuildAsymmetricNetwork(l);

            io.WriteLine(l.ToString());
            foreach (var (u, v) in edges)
                io.WriteLine($"{u + 1} {v + 1}");

            int nPerm = int.Parse(io.ReadRequiredLine());
            bool[,] permAdj = new bool[nPerm, nPerm];
            for (int e = 0; e < 2 * nPerm; e++)
            {
                string[] uv = io.ReadRequiredLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int u = int.Parse(uv[0]) - 1;
                int v = int.Parse(uv[1]) - 1;
                permAdj[u, v] = permAdj[v, u] = true;
            }

            string[] permSigs = ComputeVertexSignatures(nPerm, permAdj);
            int[] ans = new int[nPerm];
            for (int v = 0; v < nPerm; v++)
            {
                int orig = sigToOrig[permSigs[v]];
                ans[orig] = v + 1;
            }

            io.WriteLine(string.Join(' ', ans));
        }
    }

    private static void Q2Judge(GCJInteractiveChannel io, int testNumber)
    {
        const int numCases = 30;
        Random rng = new(31415);

        List<(int L, int U)> cases = new(numCases);
        if (testNumber == 0)
        {
            for (int i = 0; i < numCases; i++)
                cases.Add((10, 50));
        }
        else
        {
            int[] repeated = [10, 11, 16, 25];
            foreach (int x in repeated)
            {
                cases.Add((x, x));
                cases.Add((x, x));
            }
            for (int x = 29; x <= 50; x++)
                cases.Add((x, x));
        }

        Assert.AreEqual(numCases, cases.Count);
        for (int i = cases.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (cases[i], cases[j]) = (cases[j], cases[i]);
        }

        io.WriteLine(numCases.ToString());
        for (int tc = 1; tc <= numCases; tc++)
        {
            var (l, u) = cases[tc - 1];
            io.WriteLine($"{l} {u}");

            string nLine = io.ReadRequiredLine();
            string[] nTokens = nLine.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(1, nTokens.Length, $"Case #{tc}: expected 1 token for N.");
            Assert.IsTrue(int.TryParse(nTokens[0], out int n));
            Assert.That(n, Is.InRange(l, u), $"Case #{tc}: N={n} not in [{l}, {u}].");

            HashSet<int>[] g = new HashSet<int>[n];
            for (int i = 0; i < n; i++)
                g[i] = [];

            for (int e = 0; e < 2 * n; e++)
            {
                string[] uvTokens = io.ReadRequiredLine().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(2, uvTokens.Length, $"Case #{tc}: invalid edge token count.");
                Assert.IsTrue(int.TryParse(uvTokens[0], out int a));
                Assert.IsTrue(int.TryParse(uvTokens[1], out int b));
                Assert.That(a, Is.InRange(1, n));
                Assert.That(b, Is.InRange(1, n));
                Assert.AreNotEqual(a, b, $"Case #{tc}: self-loop at {a}.");
                g[a - 1].Add(b - 1);
                g[b - 1].Add(a - 1);
            }

            for (int i = 0; i < n; i++)
                Assert.AreEqual(4, g[i].Count, $"Case #{tc}: node {i + 1} degree != 4.");

            bool[,] adjCheck = new bool[n, n];
            for (int i = 0; i < n; i++)
                foreach (int j in g[i])
                    adjCheck[i, j] = true;
            Assert.IsTrue(IsGraphConnected(adjCheck, n), $"Case #{tc}: submitted graph not connected.");

            int[] p = Enumerable.Range(0, n).ToArray();
            for (int i = n - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (p[i], p[j]) = (p[j], p[i]);
            }
            for (int s = 0; s < 10; s++)
            {
                int i1 = rng.Next(n);
                int i2 = rng.Next(n - 1);
                if (i2 >= i1) i2++;
                (p[i1], p[i2]) = (p[i2], p[i1]);
            }

            SortedSet<int>[] h = new SortedSet<int>[n];
            for (int i = 0; i < n; i++)
                h[i] = [];
            for (int i = 0; i < n; i++)
                foreach (int j in g[i])
                    h[p[i]].Add(p[j]);

            io.WriteLine(n.ToString());
            for (int i = 0; i < n; i++)
            {
                foreach (int j in h[i])
                {
                    if (i < j)
                        io.WriteLine($"{i + 1} {j + 1}");
                }
            }

            string[] qTokens = io.ReadRequiredLine().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(n, qTokens.Length, $"Case #{tc}: expected {n} integers in permutation.");
            int[] q = new int[n];
            bool[] usedQ = new bool[n];
            for (int i = 0; i < n; i++)
            {
                Assert.IsTrue(int.TryParse(qTokens[i], out int val));
                Assert.That(val, Is.InRange(1, n));
                q[i] = val - 1;
                Assert.IsFalse(usedQ[q[i]], $"Case #{tc}: duplicate integer in permutation.");
                usedQ[q[i]] = true;
            }

            CollectionAssert.AreEqual(p, q, $"Case #{tc}: recovered permutation mismatch.");
        }

        Assert.IsNull(io.ReadLine(), "Additional output after all cases finish.");
    }

    #endregion

    #region Q3 Raise the Roof

    [Test]
    public void Q3Sample() => Test(Q3, ValidateRaiseTheRoof);

    [Test]
    public void Q3Small() => Test(Q3, ValidateRaiseTheRoof);

    [Test]
    public void Q3Large() => Test(Q3, ValidateRaiseTheRoof);

    private static Func<string> Q3(GCJTestCase testCase)
    {
        testCase.Get(out int n);
        Point3L[] pts = new Point3L[n];
        for (int i = 0; i < n; i++)
        {
            testCase.Get(out long x, out long y, out long h);
            pts[i] = new Point3L(x, y, h);
        }

        return () =>
        {
            bool[] used = new bool[n];
            int p = 0;
            for (int i = 1; i < n; i++)
            {
                if (pts[i].Z > pts[p].Z)
                    p = i;
            }
            used[p] = true;

            int q = -1;
            for (int i = 0; i < n; i++)
            {
                if (used[i])
                    continue;
                if (q < 0)
                {
                    q = i;
                    continue;
                }

                Int128 dqiX = pts[i].X - pts[p].X;
                Int128 dqiY = pts[i].Y - pts[p].Y;
                Int128 dqiZ = pts[p].Z - pts[i].Z;
                Int128 dist2I = dqiX * dqiX + dqiY * dqiY;

                Int128 dqqX = pts[q].X - pts[p].X;
                Int128 dqqY = pts[q].Y - pts[p].Y;
                Int128 dqqZ = pts[p].Z - pts[q].Z;
                Int128 dist2Q = dqqX * dqqX + dqqY * dqqY;

                // Minimize dqiZ^2 / dist2I
                if (dqiZ * dqiZ * dist2Q < dqqZ * dqqZ * dist2I)
                    q = i;
            }
            used[q] = true;

            List<int> revOrder = new(n) { p, q };
            for (int step = 2; step < n; step++)
            {
                long pqX = pts[q].X - pts[p].X;
                long pqY = pts[q].Y - pts[p].Y;

                int bestLeft = -1;
                Point3L normLeft = default;
                int bestRight = -1;
                Point3L normRight = default;

                for (int cand = 0; cand < n; cand++)
                {
                    if (used[cand])
                        continue;

                    long side = (pts[cand].X - pts[p].X) * pqY - (pts[cand].Y - pts[p].Y) * pqX;
                    if (side > 0)
                    {
                        if (bestLeft < 0 || normLeft.Dot128(pts[cand] - pts[bestLeft]) > 0)
                        {
                            bestLeft = cand;
                            normLeft = UpwardNormal(pts[p], pts[q], pts[bestLeft]);
                        }
                    }
                    else
                    {
                        if (bestRight < 0 || normRight.Dot128(pts[cand] - pts[bestRight]) > 0)
                        {
                            bestRight = cand;
                            normRight = UpwardNormal(pts[p], pts[q], pts[bestRight]);
                        }
                    }
                }

                int chosen = -1;
                if (bestLeft >= 0 && IsAboveAllUnused(pts, used, bestLeft, normLeft))
                    chosen = bestLeft;
                else if (bestRight >= 0 && IsAboveAllUnused(pts, used, bestRight, normRight))
                    chosen = bestRight;
                else
                    throw new InvalidOperationException("Failed to find valid next roof column.");

                used[chosen] = true;
                revOrder.Add(chosen);
                p = q;
                q = chosen;
            }

            int[] ans = new int[n];
            for (int i = 0; i < n; i++)
                ans[i] = revOrder[n - 1 - i] + 1;
            return string.Join(' ', ans);
        };
    }

    private static Point3L UpwardNormal(Point3L p, Point3L q, Point3L r)
    {
        Point3L normal = (q - p).Cross(r - q);
        if (normal.Z < 0)
            normal = -normal;
        return normal;
    }

    private static bool IsAboveAllUnused(Point3L[] pts, bool[] used, int rIdx, Point3L normal)
    {
        Point3L pr = pts[rIdx];
        for (int i = 0; i < pts.Length; i++)
        {
            if (used[i] || i == rIdx)
                continue;
            if (normal.Dot128(pts[i] - pr) > 0)
                return false;
        }
        return true;
    }

    private static void ValidateRaiseTheRoof(string input, string expectedOutput, List<string> actualCases)
    {
        string[] inLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        int t = int.Parse(inLines[0]);
        Assert.AreEqual(t, actualCases.Count);

        int idx = 1;
        for (int tc = 0; tc < t; tc++)
        {
            int n = int.Parse(inLines[idx++]);
            Point3L[] inp = new Point3L[n];
            for (int i = 0; i < n; i++)
            {
                string[] xyz = inLines[idx++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                inp[i] = new Point3L(long.Parse(xyz[0]), long.Parse(xyz[1]), long.Parse(xyz[2]));
            }

            string prefix = $"Case #{tc + 1}: ";
            Assert.IsTrue(actualCases[tc].StartsWith(prefix, StringComparison.Ordinal));
            string body = actualCases[tc].Substring(prefix.Length);
            Assert.IsFalse(body.Contains('\n') || body.Contains('\r'));
            string[] tokens = body.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(n, tokens.Length, $"Case #{tc + 1}: wrong number of vertices.");

            Point3L[] pts = new Point3L[n];
            bool[] seen = new bool[n];
            for (int i = 0; i < n; i++)
            {
                int col = int.Parse(tokens[i]);
                Assert.That(col, Is.InRange(1, n));
                Assert.IsFalse(seen[col - 1], $"Case #{tc + 1}: duplicate vertex {col}.");
                seen[col - 1] = true;
                pts[i] = inp[col - 1];
            }

            for (int i = 3; i < n; i++)
            {
                Point3L normal = UpwardNormal(pts[i - 2], pts[i - 1], pts[i]);
                for (int j = 0; j < i - 2; j++)
                {
                    Assert.LessOrEqual(normal.Dot128(pts[j] - pts[i]), (Int128)0,
                        $"Case #{tc + 1}: Roof #{i - 2} lowered at column {j}.");
                    Point3L baseJ = new(pts[j].X, pts[j].Y, 0);
                    Assert.LessOrEqual(normal.Dot128(baseJ - pts[i]), (Int128)0,
                        $"Case #{tc + 1}: Roof #{i - 2} went under ground at {j}.");
                }
            }
        }
    }

    #endregion

    #region Q4 Fence Construction

    [Test]
    public void Q4Sample() => Test(Q4, ValidateFenceConstruction);

    [Test]
    public void Q4Small() => Test(Q4, ValidateFenceConstruction);

    [Test]
    public void Q4Large() => Test(Q4, ValidateFenceConstruction);

    private static Func<string> Q4(GCJTestCase testCase)
    {
        testCase.Get(out int f, out int k);
        (long Ax, long Ay, long Bx, long By)[] fences = new (long, long, long, long)[f];
        for (int i = 0; i < f; i++)
            testCase.Get(out fences[i].Ax, out fences[i].Ay, out fences[i].Bx, out fences[i].By);

        return () =>
        {
            List<Point2L> points = [];
            Dictionary<Point2L, int> pointIndex = [];
            (int U, int V)[] edges = new (int U, int V)[f];
            for (int i = 0; i < f; i++)
            {
                Point2L p1 = new(fences[i].Ax, fences[i].Ay);
                Point2L p2 = new(fences[i].Bx, fences[i].By);
                edges[i] = (GetOrAddPoint(p1), GetOrAddPoint(p2));
            }

            int GetOrAddPoint(Point2L pt)
            {
                if (!pointIndex.TryGetValue(pt, out int id))
                {
                    id = points.Count;
                    points.Add(pt);
                    pointIndex[pt] = id;
                }
                return id;
            }

            List<PlanarFace> faces = Geometry.ExtractPlanarFaces(points.ToArray(), edges);
            List<int>[] edgeFaces = new List<int>[f];
            for (int i = 0; i < f; i++)
                edgeFaces[i] = [];

            List<int>[] faceEdges = new List<int>[faces.Count];
            for (int fi = 0; fi < faces.Count; fi++)
            {
                HashSet<int> unique = [.. faces[fi].Edges];
                faceEdges[fi] = [.. unique];
                foreach (int e in faceEdges[fi])
                    edgeFaces[e].Add(fi);
            }

            bool[] reached = new bool[f];
            bool[] faceExpanded = new bool[faces.Count];
            Queue<int> freeQueue = [];
            reached[k - 1] = true;
            int nextRequired = k - 1;
            int[] ans = new int[f];

            for (int step = f - 1; step >= 0; step--)
            {
                int cur;
                if (freeQueue.Count > 0)
                {
                    cur = freeQueue.Dequeue();
                }
                else
                {
                    if (nextRequired < 0 || !reached[nextRequired])
                        throw new InvalidOperationException($"Required fence {nextRequired + 1} unreachable.");
                    cur = nextRequired--;
                }

                ans[step] = cur + 1;

                foreach (int fi in edgeFaces[cur])
                {
                    if (faceExpanded[fi])
                        continue;
                    faceExpanded[fi] = true;

                    foreach (int nxt in faceEdges[fi])
                    {
                        if (!reached[nxt])
                        {
                            reached[nxt] = true;
                            if (nxt >= k)
                                freeQueue.Enqueue(nxt);
                        }
                    }
                }
            }

            return string.Join(' ', ans);
        };
    }

    private static void ValidateFenceConstruction(string input, string expectedOutput, List<string> actualCases)
    {
        string[] inLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        int t = int.Parse(inLines[0]);
        Assert.AreEqual(t, actualCases.Count);

        int idx = 1;
        for (int tc = 0; tc < t; tc++)
        {
            string[] fk = inLines[idx++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int f = int.Parse(fk[0]);
            int k = int.Parse(fk[1]);
            (long Ax, long Ay, long Bx, long By)[] fences = new (long, long, long, long)[f];
            for (int i = 0; i < f; i++)
            {
                string[] coords = inLines[idx++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                fences[i] = (
                    long.Parse(coords[0], CultureInfo.InvariantCulture),
                    long.Parse(coords[1], CultureInfo.InvariantCulture),
                    long.Parse(coords[2], CultureInfo.InvariantCulture),
                    long.Parse(coords[3], CultureInfo.InvariantCulture));
            }

            string prefix = $"Case #{tc + 1}: ";
            Assert.IsTrue(actualCases[tc].StartsWith(prefix, StringComparison.Ordinal));
            string body = actualCases[tc].Substring(prefix.Length);
            Assert.IsFalse(body.Contains('\n') || body.Contains('\r'));
            string[] tokens = body.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(f, tokens.Length, $"Case #{tc + 1}: wrong permutation size.");

            int[] perm = new int[f];
            bool[] used = new bool[f];
            int lastSeen = 0;
            for (int i = 0; i < f; i++)
            {
                int val = int.Parse(tokens[i], CultureInfo.InvariantCulture);
                Assert.That(val, Is.InRange(1, f));
                Assert.IsFalse(used[val - 1], $"Case #{tc + 1}: duplicate fence {val}.");
                used[val - 1] = true;
                if (val == lastSeen + 1)
                    lastSeen++;
                perm[i] = val - 1;
            }
            Assert.GreaterOrEqual(lastSeen, k, $"Case #{tc + 1}: first {k} fences not in relative order.");

            // Check feasibility per custom_judge.py using backward face propagation with perm's fixed order.
            List<Point2L> points = [];
            Dictionary<Point2L, int> pointIndex = [];
            (int U, int V)[] edges = new (int U, int V)[f];
            for (int i = 0; i < f; i++)
            {
                Point2L p1 = new(fences[i].Ax, fences[i].Ay);
                Point2L p2 = new(fences[i].Bx, fences[i].By);
                if (!pointIndex.TryGetValue(p1, out int u))
                {
                    u = points.Count;
                    points.Add(p1);
                    pointIndex[p1] = u;
                }
                if (!pointIndex.TryGetValue(p2, out int v))
                {
                    v = points.Count;
                    points.Add(p2);
                    pointIndex[p2] = v;
                }
                edges[i] = (u, v);
            }

            List<PlanarFace> faces = Geometry.ExtractPlanarFaces(points.ToArray(), edges);
            List<int>[] edgeFaces = new List<int>[f];
            for (int i = 0; i < f; i++)
                edgeFaces[i] = [];
            List<int>[] faceEdges = new List<int>[faces.Count];
            for (int fi = 0; fi < faces.Count; fi++)
            {
                HashSet<int> uniq = [.. faces[fi].Edges];
                faceEdges[fi] = [.. uniq];
                foreach (int e in faceEdges[fi])
                    edgeFaces[e].Add(fi);
            }

            bool[] reached = new bool[f];
            bool[] expanded = new bool[faces.Count];
            reached[perm[f - 1]] = true;
            for (int pos = f - 1; pos >= 0; pos--)
            {
                int edgeIdx = perm[pos];
                Assert.IsTrue(reached[edgeIdx],
                    $"Case #{tc + 1}: fence {edgeIdx + 1} at position {pos} cannot be built in backward order.");
                foreach (int fi in edgeFaces[edgeIdx])
                {
                    if (expanded[fi])
                        continue;
                    expanded[fi] = true;
                    foreach (int nxt in faceEdges[fi])
                        reached[nxt] = true;
                }
            }
        }
    }

    #endregion
}

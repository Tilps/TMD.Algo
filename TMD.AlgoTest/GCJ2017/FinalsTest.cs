using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2017;

[TestFixture]
public class FinalsTest : BaseGCJTest
{
    #region Q1 Dice Straight

    [Test]
    public void Q1Sample() => Test(Q1);

    [Test]
    public void Q1Small() => Test(Q1);

    [Test]
    public void Q1Large() => Test(Q1);

    private static Func<long> Q1(GCJTestCase testCase)
    {
        testCase.Get(out int n);
        var pairs = new List<(int Val, int Die)>(n * 6);
        int[] dieFaces = new int[6];
        for (int i = 0; i < n; i++)
        {
            testCase.Get(out int[] faces);
            Array.Copy(faces, dieFaces, 6);
            Array.Sort(dieFaces);
            int prev = -1;
            for (int j = 0; j < 6; j++)
            {
                if (dieFaces[j] != prev)
                {
                    pairs.Add((dieFaces[j], i));
                    prev = dieFaces[j];
                }
            }
        }

        return () =>
        {
            if (pairs.Count == 0)
                return 0;

            pairs.Sort((a, b) => a.Val != b.Val ? a.Val.CompareTo(b.Val) : a.Die.CompareTo(b.Die));

            int uniqueCount = 0;
            for (int i = 0; i < pairs.Count; i++)
            {
                if (i == 0 || pairs[i].Val != pairs[i - 1].Val)
                    uniqueCount++;
            }

            int[] uniqueVals = new int[uniqueCount];
            int[] edgeStart = new int[uniqueCount + 1];
            int[] adjDice = new int[pairs.Count];

            int uIdx = -1;
            for (int i = 0; i < pairs.Count; i++)
            {
                if (i == 0 || pairs[i].Val != pairs[i - 1].Val)
                {
                    uIdx++;
                    uniqueVals[uIdx] = pairs[i].Val;
                    edgeStart[uIdx] = i;
                }
                adjDice[i] = pairs[i].Die;
            }
            edgeStart[uniqueCount] = pairs.Count;

            int[] matchVal = new int[uniqueCount];
            Array.Fill(matchVal, -1);
            int[] matchDie = new int[n];
            Array.Fill(matchDie, -1);

            int[] visitedDie = new int[n];
            int[] fromVal = new int[n];
            int[] queue = new int[uniqueCount];
            int stamp = 0;

            int left = 0;
            int best = 0;

            for (int right = 0; right < uniqueCount; right++)
            {
                if (right > 0 && uniqueVals[right] != uniqueVals[right - 1] + 1)
                {
                    while (left < right)
                    {
                        int d = matchVal[left];
                        if (d != -1)
                        {
                            matchDie[d] = -1;
                            matchVal[left] = -1;
                        }
                        left++;
                    }
                }

                // Fast path: directly take an unmatched adjacent die if available.
                int rStart = edgeStart[right];
                int rEnd = edgeStart[right + 1];
                int directDie = -1;
                for (int e = rStart; e < rEnd; e++)
                {
                    int d = adjDice[e];
                    if (matchDie[d] == -1)
                    {
                        directDie = d;
                        break;
                    }
                }

                if (directDie != -1)
                {
                    matchVal[right] = directDie;
                    matchDie[directDie] = right;
                }
                else
                {
                    stamp++;
                    int qHead = 0;
                    int qTail = 0;
                    queue[qTail++] = right;
                    int foundDie = -1;

                    while (qHead < qTail && foundDie == -1)
                    {
                        int v = queue[qHead++];
                        int vStart = edgeStart[v];
                        int vEnd = edgeStart[v + 1];
                        for (int e = vStart; e < vEnd; e++)
                        {
                            int d = adjDice[e];
                            if (visitedDie[d] == stamp)
                                continue;
                            visitedDie[d] = stamp;
                            fromVal[d] = v;
                            int nextV = matchDie[d];
                            if (nextV == -1)
                            {
                                foundDie = d;
                                break;
                            }
                            queue[qTail++] = nextV;
                        }
                    }

                    if (foundDie == -1)
                    {
                        while (true)
                        {
                            int d = matchVal[left];
                            matchVal[left] = -1;
                            matchDie[d] = -1;
                            left++;
                            if (visitedDie[d] == stamp)
                            {
                                foundDie = d;
                                break;
                            }
                        }
                    }

                    int curDie = foundDie;
                    while (curDie != -1)
                    {
                        int v = fromVal[curDie];
                        int prevDie = matchVal[v];
                        matchVal[v] = curDie;
                        matchDie[curDie] = v;
                        curDie = prevDie;
                    }
                }

                int len = right - left + 1;
                if (len > best)
                    best = len;
            }

            return best;
        };
    }

    #endregion

    #region Q2 Operation

    [Test]
    public void Q2Sample() => Test(Q2);

    [Test]
    public void Q2Small() => Test(Q2);

    [Test]
    public void Q2Large() => Test(Q2);

    private enum OpKind : byte
    {
        Add,
        Sub,
        Zero,
        MulPos,
        MulVal,
        DivPos,
        DivVal
    }

    private readonly struct CompressedCard
    {
        public readonly OpKind Kind;
        public readonly BigInteger BigVal;
        public readonly int SmallVal;
        public readonly int AbsSmallVal;

        public CompressedCard(OpKind kind, BigInteger bigVal, int smallVal = 0)
        {
            Kind = kind;
            BigVal = bigVal;
            SmallVal = smallVal;
            AbsSmallVal = Math.Abs(smallVal);
        }
    }

    private static Func<string> Q2(GCJTestCase testCase)
    {
        testCase.Get(out long startVal, out int c);
        testCase.GetLines(c, out string[] ops, out long[] vals);

        return () =>
        {
            long addPos = 0;
            long addNeg = 0;
            bool hasZero = false;
            var posMuls = new List<int>();
            var negMuls = new List<int>();
            var posDivs = new List<int>();
            var negDivs = new List<int>();

            for (int i = 0; i < c; i++)
            {
                char op = ops[i][0];
                long v = vals[i];
                switch (op)
                {
                    case '+':
                        if (v > 0) addPos += v;
                        else if (v < 0) addNeg += -v;
                        break;
                    case '-':
                        if (v > 0) addNeg += v;
                        else if (v < 0) addPos += -v;
                        break;
                    case '*':
                        if (v == 0) hasZero = true;
                        else if (v > 1) posMuls.Add((int)v);
                        else if (v < 0) negMuls.Add((int)v);
                        break;
                    case '/':
                        if (v > 1) posDivs.Add((int)v);
                        else if (v < 0) negDivs.Add((int)v);
                        break;
                }
            }

            var cards = new List<CompressedCard>(12);
            if (addPos > 0)
                cards.Add(new CompressedCard(OpKind.Add, addPos));
            if (addNeg > 0)
                cards.Add(new CompressedCard(OpKind.Sub, addNeg));
            if (hasZero)
                cards.Add(new CompressedCard(OpKind.Zero, BigInteger.Zero));

            BigInteger prodPos = BigInteger.One;
            foreach (int v in posMuls)
                prodPos *= v;

            // Sort negative multiplications by |V| ascending; keep 2 (even count) or 3 (odd count)
            // smallest-magnitude negatives and merge the remaining even count into prodPos.
            negMuls.Sort((a, b) => Math.Abs(a).CompareTo(Math.Abs(b)));
            int keepNegMul = negMuls.Count <= 3
                ? negMuls.Count
                : ((negMuls.Count & 1) == 0 ? 2 : 3);
            for (int i = 0; i < keepNegMul; i++)
                cards.Add(new CompressedCard(OpKind.MulVal, negMuls[i], negMuls[i]));
            for (int i = keepNegMul; i < negMuls.Count; i++)
                prodPos *= Math.Abs(negMuls[i]);

            if (prodPos > BigInteger.One)
                cards.Add(new CompressedCard(OpKind.MulPos, prodPos));

            BigInteger divPos = BigInteger.One;
            foreach (int v in posDivs)
                divPos *= v;

            // Sort negative divisions by |V| ascending; keep both smallest and largest |V|
            // representatives (at most 3 if odd, 4 if even) so both sign-flipping into small
            // denominators and shrinking forced-negative answers are covered.
            negDivs.Sort((a, b) => Math.Abs(a).CompareTo(Math.Abs(b)));
            if (negDivs.Count <= 4)
            {
                for (int i = 0; i < negDivs.Count; i++)
                    cards.Add(new CompressedCard(OpKind.DivVal, negDivs[i], negDivs[i]));
            }
            else if ((negDivs.Count & 1) == 1)
            {
                cards.Add(new CompressedCard(OpKind.DivVal, negDivs[0], negDivs[0]));
                cards.Add(new CompressedCard(OpKind.DivVal, negDivs[1], negDivs[1]));
                for (int i = 2; i < negDivs.Count - 1; i++)
                    divPos *= Math.Abs(negDivs[i]);
                int last = negDivs[^1];
                cards.Add(new CompressedCard(OpKind.DivVal, last, last));
            }
            else
            {
                cards.Add(new CompressedCard(OpKind.DivVal, negDivs[0], negDivs[0]));
                cards.Add(new CompressedCard(OpKind.DivVal, negDivs[1], negDivs[1]));
                for (int i = 2; i < negDivs.Count - 2; i++)
                    divPos *= Math.Abs(negDivs[i]);
                cards.Add(new CompressedCard(OpKind.DivVal, negDivs[^2], negDivs[^2]));
                cards.Add(new CompressedCard(OpKind.DivVal, negDivs[^1], negDivs[^1]));
            }

            if (divPos > BigInteger.One)
                cards.Add(new CompressedCard(OpKind.DivPos, divPos));

            int m = cards.Count;
            if (m == 0)
                return $"{startVal} 1";

            int totalMasks = 1 << m;
            var denom = new BigInteger[totalMasks];
            denom[0] = BigInteger.One;
            for (int mask = 1; mask < totalMasks; mask++)
            {
                int lsb = mask & -mask;
                int bit = BitOperations.TrailingZeroCount((uint)lsb);
                int prev = mask ^ lsb;
                var card = cards[bit];
                if (card.Kind == OpKind.DivPos)
                    denom[mask] = denom[prev] * card.BigVal;
                else if (card.Kind == OpKind.DivVal)
                    denom[mask] = denom[prev] * card.AbsSmallVal;
                else
                    denom[mask] = denom[prev];
            }

            var dpMin = new BigInteger[totalMasks];
            var dpMax = new BigInteger[totalMasks];
            var seen = new bool[totalMasks];
            dpMin[0] = startVal;
            dpMax[0] = startVal;
            seen[0] = true;

            for (int mask = 0; mask < totalMasks - 1; mask++)
            {
                BigInteger curMin = dpMin[mask];
                BigInteger curMax = dpMax[mask];
                BigInteger curDen = denom[mask];

                for (int bit = 0; bit < m; bit++)
                {
                    if ((mask & (1 << bit)) != 0)
                        continue;

                    int nextMask = mask | (1 << bit);
                    var card = cards[bit];
                    BigInteger candMin, candMax;

                    switch (card.Kind)
                    {
                        case OpKind.Add:
                        {
                            BigInteger delta = card.BigVal * curDen;
                            candMin = curMin + delta;
                            candMax = curMax + delta;
                            break;
                        }
                        case OpKind.Sub:
                        {
                            BigInteger delta = card.BigVal * curDen;
                            candMin = curMin - delta;
                            candMax = curMax - delta;
                            break;
                        }
                        case OpKind.Zero:
                            candMin = BigInteger.Zero;
                            candMax = BigInteger.Zero;
                            break;
                        case OpKind.MulPos:
                            candMin = curMin * card.BigVal;
                            candMax = curMax * card.BigVal;
                            break;
                        case OpKind.MulVal:
                        {
                            BigInteger p1 = curMin * card.SmallVal;
                            BigInteger p2 = curMax * card.SmallVal;
                            candMin = BigInteger.Min(p1, p2);
                            candMax = BigInteger.Max(p1, p2);
                            break;
                        }
                        case OpKind.DivPos:
                            // Dividing (cur / curDen) by +D yields cur / (curDen * D),
                            // and curDen * D is already denom[nextMask].
                            candMin = curMin;
                            candMax = curMax;
                            break;
                        case OpKind.DivVal:
                            // Card.SmallVal < 0: dividing (cur / curDen) by -|D| yields (-cur) / (curDen * |D|).
                            candMin = -curMax;
                            candMax = -curMin;
                            break;
                        default:
                            throw new InvalidOperationException();
                    }

                    if (!seen[nextMask])
                    {
                        dpMin[nextMask] = candMin;
                        dpMax[nextMask] = candMax;
                        seen[nextMask] = true;
                    }
                    else
                    {
                        if (candMin < dpMin[nextMask])
                            dpMin[nextMask] = candMin;
                        if (candMax > dpMax[nextMask])
                            dpMax[nextMask] = candMax;
                    }
                }
            }

            var ans = new BigFraction(dpMax[totalMasks - 1], denom[totalMasks - 1]);
            return $"{ans.Numerator} {ans.Denominator}";
        };
    }

    #endregion

    #region Q3 Spanning Planning

    [Test]
    public void Q3Sample() => Test(Q3, ValidateSpanningPlanning);

    [Test]
    public void Q3Small() => Test(Q3, ValidateSpanningPlanning);

    private static readonly Dictionary<int, string> Q3Cache = new();
    private static readonly object Q3CacheLock = new();

    private static Func<string> Q3(GCJTestCase testCase)
    {
        testCase.Get(out int targetK);

        return () =>
        {
            if (targetK <= 22)
                return FormatCycle(targetK);

            lock (Q3CacheLock)
            {
                if (Q3Cache.TryGetValue(targetK, out string? cached))
                    return cached;
            }

            string solved = SolveSpanningPlanning(targetK);
            lock (Q3CacheLock)
            {
                Q3Cache[targetK] = solved;
            }
            return solved;
        };
    }

    private static string FormatCycle(int n)
    {
        var lines = new string[n + 1];
        lines[0] = n.ToString();
        char[] row = new char[n];
        for (int i = 0; i < n; i++)
        {
            Array.Fill(row, '0');
            row[(i + 1) % n] = '1';
            row[(i - 1 + n) % n] = '1';
            lines[i + 1] = new string(row);
        }
        return string.Join("\n", lines);
    }

    private static string FormatAdjacency(bool[,] adj, int n)
    {
        var lines = new string[n + 1];
        lines[0] = n.ToString();
        char[] row = new char[n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                row[j] = adj[i, j] ? '1' : '0';
            lines[i + 1] = new string(row);
        }
        return string.Join("\n", lines);
    }

    private static string SolveSpanningPlanning(int targetK)
    {
        var rng = new Random(20170300 + targetK);
        int[] candidateSizes = [14, 15, 16, 13, 12];
        double[] lap = new double[16 * 16];
        double[] inv = new double[16 * 16];

        for (int attempt = 0; ; attempt++)
        {
            int n = candidateSizes[(attempt / 40) % candidateSizes.Length];
            int m = n - 1;
            bool[,] adj = new bool[n, n];

            // Start with a random Prüfer/shuffled spanning tree + a few extra edges.
            int[] perm = new int[n];
            for (int i = 0; i < n; i++)
                perm[i] = i;
            for (int i = n - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (perm[i], perm[j]) = (perm[j], perm[i]);
            }
            for (int i = 1; i < n; i++)
            {
                int p = perm[rng.Next(i)];
                int c = perm[i];
                adj[p, c] = true;
                adj[c, p] = true;
            }

            int extraTarget = targetK < 300 ? rng.Next(2, 6) : targetK < 2000 ? rng.Next(4, 9) : rng.Next(5, 12);
            for (int e = 0; e < extraTarget; e++)
            {
                int u = rng.Next(n);
                int v = rng.Next(n);
                if (u != v)
                {
                    adj[u, v] = true;
                    adj[v, u] = true;
                }
            }

            int lastU = -1, lastV = -1;
            for (int step = 0; step < 50; step++)
            {
                if (!ComputeLaplacianInverseAndDet(adj, n, lap, inv, out double det))
                    break;

                if (det > 0.5 && det < 1e7)
                {
                    long rounded = (long)Math.Round(det);
                    if (rounded == targetK && MatrixHelper.CountSpanningTrees(adj) == targetK)
                        return FormatAdjacency(adj, n);
                }

                double bestScore = double.PositiveInfinity;
                int bestU = -1, bestV = -1;
                double secondScore = double.PositiveInfinity;
                int secondU = -1, secondV = -1;

                for (int u = 0; u < n; u++)
                {
                    for (int v = u + 1; v < n; v++)
                    {
                        if (u == lastU && v == lastV)
                            continue;

                        double quad = (u < m ? inv[u * m + u] : 0.0)
                                    + (v < m ? inv[v * m + v] : 0.0)
                                    - (u < m && v < m ? 2.0 * inv[u * m + v] : 0.0);

                        bool exists = adj[u, v];
                        double factor = exists ? (1.0 - quad) : (1.0 + quad);
                        if (factor <= 1e-4)
                            continue;

                        double nextDet = det * factor;
                        if (nextDet < 0.9 || nextDet > 1e8)
                            continue;

                        long nr = (long)Math.Round(nextDet);
                        if (nr == targetK)
                        {
                            adj[u, v] = !exists;
                            adj[v, u] = !exists;
                            if (MatrixHelper.CountSpanningTrees(adj) == targetK)
                                return FormatAdjacency(adj, n);
                            adj[u, v] = exists;
                            adj[v, u] = exists;
                        }

                        double score = Math.Abs(nextDet - targetK);
                        if (score < bestScore)
                        {
                            secondScore = bestScore;
                            secondU = bestU;
                            secondV = bestV;
                            bestScore = score;
                            bestU = u;
                            bestV = v;
                        }
                        else if (score < secondScore)
                        {
                            secondScore = score;
                            secondU = u;
                            secondV = v;
                        }
                    }
                }

                if (bestU == -1)
                    break;

                int pickU = bestU, pickV = bestV;
                if (secondU != -1 && rng.Next(5) == 0)
                {
                    pickU = secondU;
                    pickV = secondV;
                }

                bool cur = adj[pickU, pickV];
                adj[pickU, pickV] = !cur;
                adj[pickV, pickU] = !cur;
                lastU = pickU;
                lastV = pickV;
            }
        }
    }

    private static bool ComputeLaplacianInverseAndDet(bool[,] adj, int n, double[] a, double[] inv, out double det)
    {
        int m = n - 1;
        for (int i = 0; i < m; i++)
        {
            int deg = 0;
            for (int j = 0; j < n; j++)
            {
                if (adj[i, j])
                    deg++;
                if (j < m)
                    a[i * m + j] = adj[i, j] ? -1.0 : 0.0;
            }
            a[i * m + i] = deg;
            for (int j = 0; j < m; j++)
                inv[i * m + j] = (i == j) ? 1.0 : 0.0;
        }

        det = 1.0;
        for (int col = 0; col < m; col++)
        {
            int pivot = col;
            double maxVal = Math.Abs(a[col * m + col]);
            for (int r = col + 1; r < m; r++)
            {
                double v = Math.Abs(a[r * m + col]);
                if (v > maxVal)
                {
                    maxVal = v;
                    pivot = r;
                }
            }

            if (maxVal < 1e-12)
                return false;

            if (pivot != col)
            {
                det = -det;
                for (int j = 0; j < m; j++)
                {
                    (a[col * m + j], a[pivot * m + j]) = (a[pivot * m + j], a[col * m + j]);
                    (inv[col * m + j], inv[pivot * m + j]) = (inv[pivot * m + j], inv[col * m + j]);
                }
            }

            double diag = a[col * m + col];
            det *= diag;
            double invDiag = 1.0 / diag;
            for (int j = 0; j < m; j++)
            {
                a[col * m + j] *= invDiag;
                inv[col * m + j] *= invDiag;
            }

            for (int r = 0; r < m; r++)
            {
                if (r == col)
                    continue;
                double factor = a[r * m + col];
                if (factor == 0.0)
                    continue;
                for (int j = 0; j < m; j++)
                {
                    a[r * m + j] -= factor * a[col * m + j];
                    inv[r * m + j] -= factor * inv[col * m + j];
                }
            }
        }

        return !double.IsNaN(det) && !double.IsInfinity(det);
    }

    private static void ValidateSpanningPlanning(string input, string expectedOutput, List<string> actualLines)
    {
        var inputTokens = input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        int t = int.Parse(inputTokens[0]);
        Assert.That(actualLines.Count, Is.EqualTo(t), $"Expected {t} case outputs, got {actualLines.Count}");

        for (int tc = 1; tc <= t; tc++)
        {
            int expectedK = int.Parse(inputTokens[tc]);
            string[] caseLines = actualLines[tc - 1].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.That(caseLines.Length, Is.GreaterThanOrEqualTo(1), $"Missing output header for Case #{tc}");

            string prefix = $"Case #{tc}: ";
            Assert.That(caseLines[0], Does.StartWith(prefix), $"Case #{tc}: missing prefix");
            int y = int.Parse(caseLines[0].Substring(prefix.Length).Trim());
            Assert.That(y, Is.InRange(2, 22), $"Case #{tc}: y={y} out of range [2, 22]");
            Assert.That(caseLines.Length, Is.EqualTo(y + 1), $"Case #{tc}: expected {y + 1} lines, got {caseLines.Length}");

            bool[,] adj = new bool[y, y];
            for (int r = 0; r < y; r++)
            {
                string row = caseLines[r + 1].Trim();
                Assert.That(row.Length, Is.EqualTo(y), $"Case #{tc}: row {r} length != {y}");
                for (int c = 0; c < y; c++)
                {
                    Assert.That(row[c], Is.EqualTo('0').Or.EqualTo('1'), $"Case #{tc}: invalid matrix char");
                    adj[r, c] = row[c] == '1';
                }
            }

            for (int i = 0; i < y; i++)
            {
                Assert.That(adj[i, i], Is.False, $"Case #{tc}: self loop at {i}");
                for (int j = i + 1; j < y; j++)
                    Assert.That(adj[i, j], Is.EqualTo(adj[j, i]), $"Case #{tc}: asymmetric edge ({i},{j})");
            }

            BigInteger trees = MatrixHelper.CountSpanningTrees(adj);
            Assert.That(trees, Is.EqualTo((BigInteger)expectedK), $"Case #{tc}: spanning tree count {trees} != {expectedK}");
        }
    }

    #endregion

    #region Q4 Omnicircumnavigation

    [Test]
    public void Q4Sample() => Test(Q4);

    [Test]
    public void Q4Small() => Test(Q4);

    [Test]
    public void Q4Large() => Test(Q4);

    private readonly struct HalfPlaneNormal
    {
        public readonly int PointIndex;
        public readonly long A;
        public readonly long B;
        public readonly long C;

        public HalfPlaneNormal(int pointIndex, long a, long b, long c)
        {
            PointIndex = pointIndex;
            A = a;
            B = b;
            C = c;
        }
    }

    private static int CompareRayCcw128(long ax, long ay, long bx, long by)
    {
        int ha = (ay > 0 || (ay == 0 && ax > 0)) ? 0 : 1;
        int hb = (by > 0 || (by == 0 && bx > 0)) ? 0 : 1;
        if (ha != hb)
            return ha.CompareTo(hb);
        Int128 cross = (Int128)ax * by - (Int128)ay * bx;
        if (cross > 0) return -1;
        if (cross < 0) return 1;
        return 0;
    }

    private static Int128 Cross128(in HalfPlaneNormal u, in HalfPlaneNormal v)
        => (Int128)u.A * v.B - (Int128)u.B * v.A;

    private static Int128 Dot128(in HalfPlaneNormal u, in HalfPlaneNormal v)
        => (Int128)u.A * v.A + (Int128)u.B * v.B;

    private static bool HasPositiveParallelOverlap(in HalfPlaneNormal u, in HalfPlaneNormal v)
    {
        if (u.A != 0)
            return (Int128)u.C * Math.Abs(v.A) + (Int128)v.C * Math.Abs(u.A) > 0;
        return (Int128)u.C * Math.Abs(v.B) + (Int128)v.C * Math.Abs(u.B) > 0;
    }

    private static bool IsTighterSameDirection(in HalfPlaneNormal candidate, in HalfPlaneNormal existing)
    {
        if (candidate.A != 0)
            return (Int128)candidate.C * Math.Abs(existing.A) < (Int128)existing.C * Math.Abs(candidate.A);
        return (Int128)candidate.C * Math.Abs(existing.B) < (Int128)existing.C * Math.Abs(candidate.B);
    }

    private static Func<string> Q4(GCJTestCase testCase)
    {
        testCase.Get(out int n);
        testCase.GetLines(n, out long[] xs, out long[] ys, out long[] zs);

        return () =>
        {
            var uniqueSet = new HashSet<Point3L>(n);
            for (int i = 0; i < n; i++)
            {
                long g = MathFormulas.Gcd(Math.Abs(xs[i]), MathFormulas.Gcd(Math.Abs(ys[i]), Math.Abs(zs[i])));
                uniqueSet.Add(new Point3L(xs[i] / g, ys[i] / g, zs[i] / g));
            }

            foreach (var p in uniqueSet)
            {
                if (uniqueSet.Contains(-p))
                    return "YES";
            }

            if (uniqueSet.Count <= 2)
                return "NO";

            var pts = new Point3L[uniqueSet.Count];
            int idx = 0;
            foreach (var p in uniqueSet)
                pts[idx++] = p;

            // Permute/negate axes globally so pts[0].Z > 0 (reflection/permutation preserves existence of A*x+B*y+C*z > 0).
            if (pts[0].Z == 0)
            {
                if (pts[0].X != 0)
                {
                    for (int i = 0; i < pts.Length; i++)
                        pts[i] = new Point3L(pts[i].Z, pts[i].Y, pts[i].X);
                }
                else
                {
                    for (int i = 0; i < pts.Length; i++)
                        pts[i] = new Point3L(pts[i].X, pts[i].Z, pts[i].Y);
                }
            }
            if (pts[0].Z < 0)
            {
                for (int i = 0; i < pts.Length; i++)
                    pts[i] = new Point3L(pts[i].X, pts[i].Y, -pts[i].Z);
            }

            Point3L p0 = pts[0];
            var rawPlanes = new List<HalfPlaneNormal>(pts.Length - 1);
            for (int i = 1; i < pts.Length; i++)
            {
                Point3L pi = pts[i];
                long a = pi.X * p0.Z - pi.Z * p0.X;
                long b = pi.Y * p0.Z - pi.Z * p0.Y;
                long c = pi.Z;
                if (a == 0 && b == 0)
                {
                    if (c <= 0)
                        return "YES";
                    continue;
                }
                rawPlanes.Add(new HalfPlaneNormal(i, a, b, c));
            }

            if (rawPlanes.Count == 0)
                return "NO";

            rawPlanes.Sort((u, v) => CompareRayCcw128(u.A, u.B, v.A, v.B));

            var planes = new List<HalfPlaneNormal>(rawPlanes.Count);
            for (int i = 0; i < rawPlanes.Count; i++)
            {
                var cur = rawPlanes[i];
                if (planes.Count > 0 && CompareRayCcw128(planes[^1].A, planes[^1].B, cur.A, cur.B) == 0)
                {
                    if (IsTighterSameDirection(cur, planes[^1]))
                        planes[^1] = cur;
                }
                else
                {
                    planes.Add(cur);
                }
            }

            int m = planes.Count;
            if (m <= 1)
                return "NO";

            for (int i = 0; i < m; i++)
            {
                var cur = planes[i];
                var nxt = planes[(i + 1) % m];
                Int128 cr = Cross128(cur, nxt);
                if (cr < 0)
                    return "NO";
                if (cr == 0 && Dot128(cur, nxt) < 0)
                    return HasPositiveParallelOverlap(cur, nxt) ? "NO" : "YES";
            }

            var dq = new int[m * 2 + 5];
            int head = 0;
            int tail = -1;

            for (int i = 0; i < m; i++)
            {
                Point3L pi = pts[planes[i].PointIndex];
                while (tail - head >= 1)
                {
                    Point3L pa = pts[planes[dq[tail - 1]].PointIndex];
                    Point3L pb = pts[planes[dq[tail]].PointIndex];
                    if (Point3L.TripleProduct128(pa, pb, pi) <= 0)
                        tail--;
                    else
                        break;
                }
                while (tail - head >= 1)
                {
                    Point3L pa = pts[planes[dq[head]].PointIndex];
                    Point3L pb = pts[planes[dq[head + 1]].PointIndex];
                    if (Point3L.TripleProduct128(pa, pb, pi) <= 0)
                        head++;
                    else
                        break;
                }

                if (tail >= head && Cross128(planes[dq[tail]], planes[i]) <= 0)
                    return "YES";

                dq[++tail] = i;
            }

            bool changed = true;
            while (changed && tail - head >= 2)
            {
                changed = false;
                Point3L pHead = pts[planes[dq[head]].PointIndex];
                while (tail - head >= 2)
                {
                    Point3L pa = pts[planes[dq[tail - 1]].PointIndex];
                    Point3L pb = pts[planes[dq[tail]].PointIndex];
                    if (Point3L.TripleProduct128(pa, pb, pHead) <= 0)
                    {
                        tail--;
                        changed = true;
                    }
                    else
                    {
                        break;
                    }
                }

                if (tail - head < 2)
                    break;

                Point3L pTail = pts[planes[dq[tail]].PointIndex];
                while (tail - head >= 2)
                {
                    Point3L pa = pts[planes[dq[head]].PointIndex];
                    Point3L pb = pts[planes[dq[head + 1]].PointIndex];
                    if (Point3L.TripleProduct128(pa, pb, pTail) <= 0)
                    {
                        head++;
                        changed = true;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (tail - head < 2)
                return "YES";
            if (Cross128(planes[dq[tail]], planes[dq[head]]) <= 0)
                return "YES";

            return "NO";
        };
    }

    #endregion

    #region Q5 Stack Management

    [Test]
    public void Q5Sample() => Test(Q5Setup);

    [Test]
    public void Q5Small() => Test(Q5Setup);

    [Test]
    public void Q5Large() => SlowTest(Q5Setup);

    private readonly struct Card
    {
        public readonly int Val;
        public readonly int Suit;

        public Card(int val, int suit)
        {
            Val = val;
            Suit = suit;
        }
    }

    private static (int TestCount, Func<GCJTestCase, Func<string>> TestCaseHandler) Q5Setup(GCJTestCase globalHeader)
    {
        globalHeader.Get(out int p);
        var premade = new Card[p][];
        for (int i = 0; i < p; i++)
        {
            globalHeader.Get(out int[] line);
            int len = line[0];
            var cards = new Card[len];
            // Input lists cards from top to bottom; store bottom -> top so stack top is index ^1.
            for (int j = 0; j < len; j++)
                cards[len - 1 - j] = new Card(line[1 + 2 * j], line[2 + 2 * j]);
            premade[i] = cards;
        }

        globalHeader.Get(out int testCount);

        return (testCount, tc => Q5Case(tc, premade));
    }

    private static Func<string> Q5Case(GCJTestCase testCase, Card[][] premade)
    {
        testCase.Get(out int n, out int _);
        testCase.Get(out int[] stackIds);

        return () =>
        {
            var suitMap = new Dictionary<int, int>(n * 2);
            Card[][] stacks = new Card[n][];
            for (int i = 0; i < n; i++)
            {
                var src = premade[stackIds[i]];
                var cards = new Card[src.Length];
                for (int j = 0; j < src.Length; j++)
                {
                    int origSuit = src[j].Suit;
                    if (!suitMap.TryGetValue(origSuit, out int s))
                    {
                        s = suitMap.Count;
                        suitMap[origSuit] = s;
                    }
                    cards[j] = new Card(src[j].Val, s);
                }
                stacks[i] = cards;
            }

            int numSuits = suitMap.Count;
            if (numSuits < n)
                return "POSSIBLE";
            if (numSuits > n)
                return "IMPOSSIBLE";

            // Count cards and identify the highest (Ace) and second-highest (King) per suit.
            int[] suitCount = new int[n];
            int[] aceVal = new int[n];
            int[] kingVal = new int[n];
            Array.Fill(aceVal, -1);
            Array.Fill(kingVal, -1);
            for (int i = 0; i < n; i++)
            {
                foreach (var card in stacks[i])
                {
                    int s = card.Suit;
                    int v = card.Val;
                    suitCount[s]++;
                    if (v > aceVal[s])
                    {
                        kingVal[s] = aceVal[s];
                        aceVal[s] = v;
                    }
                    else if (v > kingVal[s])
                    {
                        kingVal[s] = v;
                    }
                }
            }

            // Step 4: Rule 1 greedy simulation only (remove lower of matching suit tops).
            int[] size = new int[n];
            for (int i = 0; i < n; i++)
                size[i] = stacks[i].Length;

            int[] topBySuit = new int[n];
            Array.Fill(topBySuit, -1);
            var q = new Queue<int>(n);
            for (int i = 0; i < n; i++)
            {
                if (size[i] > 0)
                    q.Enqueue(i);
            }

            while (q.Count > 0)
            {
                int i = q.Dequeue();
                if (size[i] == 0)
                    continue;

                Card top = stacks[i][size[i] - 1];
                int s = top.Suit;
                int other = topBySuit[s];
                if (other == -1 || other == i || size[other] == 0 || stacks[other][size[other] - 1].Suit != s)
                {
                    topBySuit[s] = i;
                    continue;
                }

                Card otherTop = stacks[other][size[other] - 1];
                if (top.Val < otherTop.Val)
                {
                    size[i]--;
                    if (size[i] > 0)
                        q.Enqueue(i);
                }
                else
                {
                    size[other]--;
                    topBySuit[s] = i;
                    if (size[other] > 0)
                        q.Enqueue(other);
                }
            }

            bool allLe1 = true;
            bool hasEmpty = false;
            for (int i = 0; i < n; i++)
            {
                if (size[i] == 0)
                    hasEmpty = true;
                else if (size[i] > 1)
                    allLe1 = false;
            }

            if (allLe1)
                return "POSSIBLE";
            if (!hasEmpty)
                return "IMPOSSIBLE";

            // Step 5: Build directed graph over suits whose Ace is at the bottom of an original stack.
            bool[] hasBottomAce = new bool[n];
            int[] bottomAceStack = new int[n];
            Array.Fill(bottomAceStack, -1);
            for (int i = 0; i < n; i++)
            {
                if (stacks[i].Length > 0)
                {
                    var bottom = stacks[i][0];
                    if (bottom.Val == aceVal[bottom.Suit])
                    {
                        hasBottomAce[bottom.Suit] = true;
                        bottomAceStack[bottom.Suit] = i;
                    }
                }
            }

            var adj = new List<int>?[n];
            bool[] isSource = new bool[n];
            bool[] isTarget = new bool[n];

            for (int s1 = 0; s1 < n; s1++)
            {
                if (!hasBottomAce[s1])
                    continue;

                isSource[s1] = (suitCount[s1] == 1);
                int st = bottomAceStack[s1];
                var cards = stacks[st];
                for (int j = 1; j < cards.Length; j++)
                {
                    var card = cards[j];
                    int s2 = card.Suit;
                    if (card.Val == aceVal[s2])
                    {
                        isTarget[s1] = true;
                    }
                    else if (suitCount[s2] >= 2 && card.Val == kingVal[s2] && s2 != s1 && hasBottomAce[s2])
                    {
                        adj[s1] ??= new List<int>(2);
                        adj[s1]!.Add(s2);
                    }
                }
            }

            bool[] visited = new bool[n];
            var bfs = new Queue<int>();
            for (int s = 0; s < n; s++)
            {
                if (hasBottomAce[s] && isSource[s])
                {
                    visited[s] = true;
                    bfs.Enqueue(s);
                }
            }

            while (bfs.Count > 0)
            {
                int u = bfs.Dequeue();
                if (isTarget[u])
                    return "POSSIBLE";

                var nextList = adj[u];
                if (nextList == null)
                    continue;

                foreach (int v in nextList)
                {
                    if (!visited[v])
                    {
                        visited[v] = true;
                        bfs.Enqueue(v);
                    }
                }
            }

            return "IMPOSSIBLE";
        };
    }

    #endregion

    #region Q6 Teleporters

    [Test]
    public void Q6Sample() => Test(Q6);

    [Test]
    public void Q6Small() => Test(Q6);

    [Test]
    public void Q6Large() => SlowTest(Q6);

    private static Func<string> Q6(GCJTestCase testCase)
    {
        testCase.Get(out int n);
        testCase.Get(out long px, out long py, out long pz);
        testCase.Get(out long qx, out long qy, out long qz);
        testCase.GetLines(n, out long[] tx, out long[] ty, out long[] tz);

        return () =>
        {
            const long Inf = 100_000_000_000_000L;
            var p = new Point3L(px, py, pz);
            var q = new Point3L(qx, qy, qz);
            var teleporters = new Point3L[n];
            long[] dP = new long[n];
            long[] dQ = new long[n];

            bool allLess = true;
            bool allGreater = true;

            for (int i = 0; i < n; i++)
            {
                teleporters[i] = new Point3L(tx[i], ty[i], tz[i]);
                dP[i] = p.ManhattanDistanceTo(teleporters[i]);
                dQ[i] = q.ManhattanDistanceTo(teleporters[i]);
                if (dP[i] == dQ[i])
                    return "1";
                if (dP[i] > dQ[i])
                    allLess = false;
                else
                    allGreater = false;
            }

            if (n == 1)
                return "IMPOSSIBLE";

            if (!allLess && !allGreater)
                return "2";

            if (allGreater)
                (dP, dQ) = (dQ, dP);

            // Deduplicate identical teleporter coordinates if multiple teleporters coincide.
            long[] baseMat = new long[n * n];
            bool anyDistinct = false;
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    long d = teleporters[i].ManhattanDistanceTo(teleporters[j]);
                    if (d > 0)
                        anyDistinct = true;
                    long capped = Math.Min(Inf, d);
                    baseMat[i * n + j] = capped;
                    baseMat[j * n + i] = capped;
                }
            }

            if (!anyDistinct)
                return "IMPOSSIBLE";

            var powers = new List<long[]>();
            var curMat = baseMat;
            long[] curVec = new long[n];
            long[] nextVec = new long[n];

            while (true)
            {
                powers.Add(curMat);
                ApplyMaxPlusVecMat(dP, curMat, nextVec, n, Inf);
                if (ReachesTarget(nextVec, dQ, n))
                    break;

                long[] squared = SquareSymmetricMaxPlus(curMat, n, Inf);
                curMat = squared;
            }

            int highestBit = powers.Count - 1;
            Array.Copy(dP, curVec, n);
            long steps = 0;

            for (int bit = highestBit - 1; bit >= 0; bit--)
            {
                ApplyMaxPlusVecMat(curVec, powers[bit], nextVec, n, Inf);
                if (!ReachesTarget(nextVec, dQ, n))
                {
                    Array.Copy(nextVec, curVec, n);
                    steps += 1L << bit;
                }
            }

            return (steps + 2).ToString();
        };
    }

    private static bool ReachesTarget(long[] vec, long[] target, int n)
    {
        for (int i = 0; i < n; i++)
        {
            if (vec[i] >= target[i])
                return true;
        }
        return false;
    }

    private static void ApplyMaxPlusVecMat(long[] vec, long[] mat, long[] dst, int n, long inf)
    {
        for (int j = 0; j < n; j++)
        {
            long best = 0;
            int colBase = j * n; // mat is symmetric: mat[i * n + j] == mat[j * n + i].
            for (int i = 0; i < n; i++)
            {
                long sum = vec[i] + mat[colBase + i];
                if (sum > best)
                    best = sum;
            }
            dst[j] = Math.Min(inf, best);
        }
    }

    private static long[] SquareSymmetricMaxPlus(long[] mat, int n, long inf)
    {
        long[] res = new long[n * n];
        for (int i = 0; i < n; i++)
        {
            int rowI = i * n;
            for (int j = i; j < n; j++)
            {
                int rowJ = j * n;
                long best = 0;
                for (int k = 0; k < n; k++)
                {
                    long sum = mat[rowI + k] + mat[rowJ + k];
                    if (sum > best)
                        best = sum;
                }
                long capped = Math.Min(inf, best);
                res[rowI + j] = capped;
                res[rowJ + i] = capped;
            }
        }
        return res;
    }

    #endregion
}

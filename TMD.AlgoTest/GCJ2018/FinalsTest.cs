using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2018;

[TestFixture]
public class FinalsTest : BaseGCJTest
{
    #region Q1 Jurisdiction Restrictions

    [Test]
    public void Q1Sample() => Test(Q1);

    [Test]
    public void Q1Small() => Test(Q1);

    [Test]
    public void Q1Large() => Test(Q1);

    private static Func<long> Q1(GCJTestCase testCase)
    {
        testCase.Get(out long r, out long c, out int s);
        long[] sr = new long[s];
        long[] sc = new long[s];
        long[] sd = new long[s];
        for (int i = 0; i < s; i++)
            testCase.Get(out sr[i], out sc[i], out sd[i]);

        return () =>
        {
            List<long> xsList = [1L, r + 1L];
            List<long> ysList = [1L, c + 1L];
            for (int i = 0; i < s; i++)
            {
                xsList.Add(Math.Max(1L, sr[i] - sd[i]));
                xsList.Add(Math.Min(r + 1L, sr[i] + sd[i] + 1L));
                ysList.Add(Math.Max(1L, sc[i] - sd[i]));
                ysList.Add(Math.Min(c + 1L, sc[i] + sd[i] + 1L));
            }

            long[] xs = xsList.Distinct().OrderBy(x => x).ToArray();
            long[] ys = ysList.Distinct().OrderBy(y => y).ToArray();

            Dictionary<int, long> maskArea = [];
            long totalBlocks = 0;

            for (int xi = 0; xi + 1 < xs.Length; xi++)
            {
                long x0 = xs[xi];
                long x1 = xs[xi + 1];
                long dx = x1 - x0;
                for (int yi = 0; yi + 1 < ys.Length; yi++)
                {
                    long y0 = ys[yi];
                    long y1 = ys[yi + 1];
                    long dy = y1 - y0;

                    int mask = 0;
                    int stationCount = 0;
                    for (int i = 0; i < s; i++)
                    {
                        if (x0 >= sr[i] - sd[i] && x1 - 1 <= sr[i] + sd[i] &&
                            y0 >= sc[i] - sd[i] && y1 - 1 <= sc[i] + sd[i])
                        {
                            mask |= 1 << i;
                        }
                        if (sr[i] >= x0 && sr[i] < x1 && sc[i] >= y0 && sc[i] < y1)
                            stationCount++;
                    }

                    if (mask == 0)
                        continue;

                    long area = dx * dy - stationCount;
                    if (area <= 0)
                        continue;

                    totalBlocks += area;
                    maskArea.TryGetValue(mask, out long prev);
                    maskArea[mask] = prev + area;
                }
            }

            (int Mask, long Area)[] groups = new (int, long)[maskArea.Count];
            int gIdx = 0;
            foreach (var kv in maskArea)
                groups[gIdx++] = (kv.Key, kv.Value);

            long CheckFlow(long capPerStation)
            {
                int src = 0;
                int snk = 1 + s + groups.Length;
                MaxFlow mf = new(snk + 1);
                for (int i = 0; i < s; i++)
                    mf.AddDirectedEdge(src, 1 + i, capPerStation);
                for (int gi = 0; gi < groups.Length; gi++)
                {
                    int node = 1 + s + gi;
                    int mask = groups[gi].Mask;
                    long area = groups[gi].Area;
                    for (int i = 0; i < s; i++)
                    {
                        if ((mask & (1 << i)) != 0)
                            mf.AddDirectedEdge(1 + i, node, area);
                    }
                    mf.AddDirectedEdge(node, snk, area);
                }
                return mf.ComputeMaxFlow(src, snk);
            }

            long loU = 0;
            long hiU = totalBlocks;
            while (loU < hiU)
            {
                long mid = loU + ((hiU - loU) >> 1);
                if (CheckFlow(mid) == totalBlocks)
                    hiU = mid;
                else
                    loU = mid + 1;
            }

            long loL = 0;
            long hiL = totalBlocks / s;
            while (loL < hiL)
            {
                long mid = loL + ((hiL - loL + 1) >> 1);
                if (CheckFlow(mid) == (long)s * mid)
                    loL = mid;
                else
                    hiL = mid - 1;
            }

            return loU - loL;
        };
    }

    #endregion

    #region Q2 Two-Tiling

    private const string TwoTilingAllowedChars =
        "!?0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    [Test]
    public void Q2Sample() => Test(Q2, ValidateTwoTiling);

    [Test]
    public void Q2Small() => SlowTest(Q2, ValidateTwoTiling);

    private static Func<string> Q2(GCJTestCase testCase)
    {
        string[] rawA = new string[3];
        string[] rawB = new string[3];
        for (int i = 0; i < 3; i++)
            testCase.Get(out rawA[i], out rawB[i]);

        return () =>
        {
            List<(int R, int C)>[] orientsA = BuildUniqueOrientations(rawA);
            List<(int R, int C)>[] orientsB = BuildUniqueOrientations(rawB);
            int sizeA = orientsA[0].Count;
            int sizeB = orientsB[0].Count;

            if (sizeA == 1 || sizeB == 1)
            {
                return BuildSingletonSolution(orientsA[0], orientsB[0], sizeA == 1);
            }

            int lcm = sizeA / (int)BigInteger.GreatestCommonDivisor(sizeA, sizeB) * sizeB;
            if (lcm > 64)
                return "IMPOSSIBLE";

            ulong[][] masksA = BuildCellIndexedMasks(orientsA);
            ulong[][] masksB = BuildCellIndexedMasks(orientsB);

            List<ulong> chosenA = [];
            List<ulong> chosenB = [];

            for (int startCell = 0; startCell <= 3; startCell++)
            {
                ulong forbidden = (1UL << startCell) - 1UL;
                HashSet<(ulong CovA, ulong CovB)> visited = [];

                foreach (ulong mA in masksA[startCell])
                {
                    if ((mA & forbidden) != 0)
                        continue;

                    foreach (ulong mB in masksB[startCell])
                    {
                        if ((mB & forbidden) != 0)
                            continue;

                        chosenA.Add(mA);
                        chosenB.Add(mB);
                        if (SearchTwoTiling(mA, mB, forbidden, sizeA, sizeB, lcm, masksA, masksB, visited, chosenA, chosenB))
                            return FormatTwoTilingResult(chosenA, chosenB);

                        chosenA.RemoveAt(chosenA.Count - 1);
                        chosenB.RemoveAt(chosenB.Count - 1);
                    }
                }
            }

            return "IMPOSSIBLE";
        };
    }

    private static string BuildSingletonSolution(
        List<(int R, int C)> tileA,
        List<(int R, int C)> tileB,
        bool aIsSingle)
    {
        char[,] boardA = new char[8, 8];
        char[,] boardB = new char[8, 8];
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                boardA[r, c] = '.';
                boardB[r, c] = '.';
            }
        }

        List<(int R, int C)> other = aIsSingle ? tileB : tileA;
        int idx = 0;
        foreach (var (r, c) in other)
        {
            char singleChar = TwoTilingAllowedChars[idx++];
            char multiChar = TwoTilingAllowedChars[0];
            if (aIsSingle)
            {
                boardA[r, c] = singleChar;
                boardB[r, c] = multiChar;
            }
            else
            {
                boardA[r, c] = multiChar;
                boardB[r, c] = singleChar;
            }
        }

        return FormatBoards(boardA, boardB);
    }

    private static bool SearchTwoTiling(
        ulong covA,
        ulong covB,
        ulong forbidden,
        int sizeA,
        int sizeB,
        int lcm,
        ulong[][] masksA,
        ulong[][] masksB,
        HashSet<(ulong CovA, ulong CovB)> visited,
        List<ulong> chosenA,
        List<ulong> chosenB)
    {
        if (covA == covB)
            return true;

        int maxCells = Math.Max(BitOperations.PopCount(covA), BitOperations.PopCount(covB));
        int minFinal = ((maxCells + lcm - 1) / lcm) * lcm;
        int availCells = 64 - BitOperations.PopCount(forbidden);
        if (minFinal > availCells)
            return false;

        if (!visited.Add((covA, covB)))
            return false;

        int bestCell = -1;
        bool bestNeedsA = false;
        int bestCount = int.MaxValue;

        ulong needA = covB & ~covA;
        while (needA != 0)
        {
            int c = BitOperations.TrailingZeroCount(needA);
            needA &= needA - 1;
            int cnt = 0;
            ulong block = covA | forbidden;
            foreach (ulong m in masksA[c])
            {
                if ((m & block) == 0)
                    cnt++;
            }

            if (cnt == 0)
                return false;
            if (cnt < bestCount)
            {
                bestCount = cnt;
                bestCell = c;
                bestNeedsA = true;
                if (bestCount == 1)
                    break;
            }
        }

        if (bestCount > 1)
        {
            ulong needB = covA & ~covB;
            while (needB != 0)
            {
                int c = BitOperations.TrailingZeroCount(needB);
                needB &= needB - 1;
                int cnt = 0;
                ulong block = covB | forbidden;
                foreach (ulong m in masksB[c])
                {
                    if ((m & block) == 0)
                        cnt++;
                }

                if (cnt == 0)
                    return false;
                if (cnt < bestCount)
                {
                    bestCount = cnt;
                    bestCell = c;
                    bestNeedsA = false;
                    if (bestCount == 1)
                        break;
                }
            }
        }

        ulong blockMask = (bestNeedsA ? covA : covB) | forbidden;
        ulong otherCov = bestNeedsA ? covB : covA;
        ulong[] rawCandidates = bestNeedsA ? masksA[bestCell] : masksB[bestCell];

        List<(ulong Mask, int NewDiff)> cands = new(bestCount);
        foreach (ulong m in rawCandidates)
        {
            if ((m & blockMask) == 0)
                cands.Add((m, BitOperations.PopCount(m & ~otherCov)));
        }

        if (cands.Count > 1)
            cands.Sort((x, y) => x.NewDiff.CompareTo(y.NewDiff));

        foreach (var (m, _) in cands)
        {
            if (bestNeedsA)
            {
                chosenA.Add(m);
                if (SearchTwoTiling(covA | m, covB, forbidden, sizeA, sizeB, lcm, masksA, masksB, visited, chosenA, chosenB))
                    return true;
                chosenA.RemoveAt(chosenA.Count - 1);
            }
            else
            {
                chosenB.Add(m);
                if (SearchTwoTiling(covA, covB | m, forbidden, sizeA, sizeB, lcm, masksA, masksB, visited, chosenA, chosenB))
                    return true;
                chosenB.RemoveAt(chosenB.Count - 1);
            }
        }

        return false;
    }

    private static string FormatTwoTilingResult(List<ulong> chosenA, List<ulong> chosenB)
    {
        char[,] boardA = new char[8, 8];
        char[,] boardB = new char[8, 8];
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                boardA[r, c] = '.';
                boardB[r, c] = '.';
            }
        }

        FillBoard(boardA, chosenA);
        FillBoard(boardB, chosenB);
        return FormatBoards(boardA, boardB);

        static void FillBoard(char[,] board, List<ulong> tiles)
        {
            for (int ti = 0; ti < tiles.Count; ti++)
            {
                char ch = TwoTilingAllowedChars[ti];
                ulong m = tiles[ti];
                while (m != 0)
                {
                    int bit = BitOperations.TrailingZeroCount(m);
                    m &= m - 1;
                    board[bit >> 3, bit & 7] = ch;
                }
            }
        }
    }

    private static string FormatBoards(char[,] boardA, char[,] boardB)
    {
        string[] lines = new string[9];
        lines[0] = "POSSIBLE";
        char[] rowA = new char[8];
        char[] rowB = new char[8];
        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                rowA[c] = boardA[r, c];
                rowB[c] = boardB[r, c];
            }
            lines[r + 1] = $"{new string(rowA)} {new string(rowB)}";
        }
        return string.Join('\n', lines);
    }

    private static List<(int R, int C)>[] BuildUniqueOrientations(string[] raw)
    {
        List<(int R, int C)> baseCells = [];
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
                if (raw[r][c] == '@')
                    baseCells.Add((r, c));

        HashSet<string> seen = [];
        List<List<(int R, int C)>> res = [];

        for (int flip = 0; flip < 2; flip++)
        {
            for (int rot = 0; rot < 4; rot++)
            {
                List<(int R, int C)> cur = new(baseCells.Count);
                int minR = int.MaxValue, minC = int.MaxValue;
                foreach (var (rr, cc) in baseCells)
                {
                    int r = rr, c = flip == 0 ? cc : 2 - cc;
                    for (int k = 0; k < rot; k++)
                        (r, c) = (c, 2 - r);
                    cur.Add((r, c));
                    if (r < minR) minR = r;
                    if (c < minC) minC = c;
                }

                for (int i = 0; i < cur.Count; i++)
                    cur[i] = (cur[i].R - minR, cur[i].C - minC);
                cur.Sort();

                string key = string.Join(';', cur);
                if (seen.Add(key))
                    res.Add(cur);
            }
        }

        return [.. res];
    }

    private static ulong[][] BuildCellIndexedMasks(List<(int R, int C)>[] orientations)
    {
        HashSet<ulong> allMasks = [];
        foreach (var orient in orientations)
        {
            int maxR = 0, maxC = 0;
            foreach (var (r, c) in orient)
            {
                if (r > maxR) maxR = r;
                if (c > maxC) maxC = c;
            }

            for (int dr = 0; dr <= 7 - maxR; dr++)
            {
                for (int dc = 0; dc <= 7 - maxC; dc++)
                {
                    ulong mask = 0;
                    foreach (var (r, c) in orient)
                        mask |= 1UL << (((dr + r) << 3) | (dc + c));
                    allMasks.Add(mask);
                }
            }
        }

        List<ulong>[] byCell = new List<ulong>[64];
        for (int c = 0; c < 64; c++)
            byCell[c] = [];

        foreach (ulong m in allMasks)
        {
            ulong tmp = m;
            while (tmp != 0)
            {
                int bit = BitOperations.TrailingZeroCount(tmp);
                tmp &= tmp - 1;
                byCell[bit].Add(m);
            }
        }

        ulong[][] result = new ulong[64][];
        for (int c = 0; c < 64; c++)
            result[c] = [.. byCell[c]];
        return result;
    }

    private static void ValidateTwoTiling(string input, string expectedOutput, List<string> actualCases)
    {
        string[] inLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        int t = int.Parse(inLines[0]);
        Assert.AreEqual(t, actualCases.Count);

        string[] expAllLines = expectedOutput.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        List<List<string>> expPerCase = new(t);
        foreach (string line in expAllLines)
        {
            if (line.StartsWith("Case #", StringComparison.OrdinalIgnoreCase))
            {
                int colon = line.IndexOf(':');
                string rest = line.Substring(colon + 1).Trim();
                expPerCase.Add(string.IsNullOrEmpty(rest) ? [] : [rest]);
            }
            else
            {
                expPerCase[^1].Add(line.Trim());
            }
        }
        Assert.AreEqual(t, expPerCase.Count);

        HashSet<char> allowed = [.. TwoTilingAllowedChars];
        int idx = 1;
        for (int tc = 0; tc < t; tc++)
        {
            string[] rawA = new string[3];
            string[] rawB = new string[3];
            for (int r = 0; r < 3; r++)
            {
                string[] rowTokens = inLines[idx++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                rawA[r] = rowTokens[0];
                rawB[r] = rowTokens[1];
            }

            HashSet<string> equivA = BuildNormalizedTileKeys(rawA);
            HashSet<string> equivB = BuildNormalizedTileKeys(rawB);

            bool expPossible = VerifyTwoTilingAttempt(expPerCase[tc], equivA, equivB, tc + 1);

            string[] actLinesRaw = actualCases[tc].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            string prefix = $"Case #{tc + 1}: ";
            Assert.IsTrue(actLinesRaw[0].StartsWith(prefix, StringComparison.Ordinal));
            List<string> actLines = [actLinesRaw[0].Substring(prefix.Length).Trim()];
            for (int i = 1; i < actLinesRaw.Length; i++)
                actLines.Add(actLinesRaw[i].Trim());

            bool actPossible = VerifyTwoTilingAttempt(actLines, equivA, equivB, tc + 1, allowed);
            Assert.AreEqual(expPossible, actPossible, $"Case #{tc + 1}: POSSIBLE/IMPOSSIBLE mismatch.");
        }
    }

    private static HashSet<string> BuildNormalizedTileKeys(string[] raw)
    {
        HashSet<string> res = [];
        foreach (var orient in BuildUniqueOrientations(raw))
            res.Add(string.Join(';', orient));
        return res;
    }

    private static bool VerifyTwoTilingAttempt(
        List<string> lines,
        HashSet<string> equivA,
        HashSet<string> equivB,
        int caseNum,
        HashSet<char>? allowed = null)
    {
        Assert.GreaterOrEqual(lines.Count, 1);
        if (lines.Count == 1)
        {
            Assert.AreEqual("IMPOSSIBLE", lines[0], $"Case #{caseNum}: expected IMPOSSIBLE.");
            return false;
        }

        Assert.AreEqual(9, lines.Count, $"Case #{caseNum}: expected 9 lines for POSSIBLE.");
        Assert.AreEqual("POSSIBLE", lines[0], $"Case #{caseNum}: header must be POSSIBLE.");

        string[] b0 = new string[8];
        string[] b1 = new string[8];
        for (int r = 0; r < 8; r++)
        {
            string[] pair = lines[r + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, pair.Length, $"Case #{caseNum}: row {r} must have 2 grids.");
            Assert.AreEqual(8, pair[0].Length);
            Assert.AreEqual(8, pair[1].Length);
            b0[r] = pair[0];
            b1[r] = pair[1];
        }

        Dictionary<char, List<(int R, int C)>> map0 = CollectCharPositions(b0, allowed);
        Dictionary<char, List<(int R, int C)>> map1 = CollectCharPositions(b1, allowed);
        CollectionAssert.AreEquivalent(map0['.'], map1['.'], $"Case #{caseNum}: empty cell sets differ.");
        map0.Remove('.');
        map1.Remove('.');
        Assert.Greater(map0.Count, 0, $"Case #{caseNum}: no red cells.");
        Assert.Greater(map1.Count, 0, $"Case #{caseNum}: no red cells.");

        foreach (var kv in map0)
        {
            string key = NormalizeCellList(kv.Value, caseNum, 0, kv.Key);
            Assert.IsTrue(equivA.Contains(key), $"Case #{caseNum}: board 0 char '{kv.Key}' invalid tile.");
        }
        foreach (var kv in map1)
        {
            string key = NormalizeCellList(kv.Value, caseNum, 1, kv.Key);
            Assert.IsTrue(equivB.Contains(key), $"Case #{caseNum}: board 1 char '{kv.Key}' invalid tile.");
        }

        return true;

        static Dictionary<char, List<(int R, int C)>> CollectCharPositions(string[] board, HashSet<char>? allowSet)
        {
            Dictionary<char, List<(int R, int C)>> map = new() { ['.'] = [] };
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    char ch = board[r][c];
                    if (ch != '.' && allowSet is not null)
                        Assert.IsTrue(allowSet.Contains(ch), $"Invalid tile character '{ch}'.");
                    if (!map.TryGetValue(ch, out var list))
                    {
                        list = [];
                        map[ch] = list;
                    }
                    list.Add((r, c));
                }
            }
            return map;
        }

        static string NormalizeCellList(List<(int R, int C)> pos, int cNum, int boardIdx, char ch)
        {
            int minR = int.MaxValue, maxR = int.MinValue;
            int minC = int.MaxValue, maxC = int.MinValue;
            foreach (var (r, c) in pos)
            {
                if (r < minR) minR = r;
                if (r > maxR) maxR = r;
                if (c < minC) minC = c;
                if (c > maxC) maxC = c;
            }
            Assert.LessOrEqual(maxR - minR, 2, $"Case #{cNum} board {boardIdx} char '{ch}' >3 tall.");
            Assert.LessOrEqual(maxC - minC, 2, $"Case #{cNum} board {boardIdx} char '{ch}' >3 wide.");

            List<(int R, int C)> norm = new(pos.Count);
            foreach (var (r, c) in pos)
                norm.Add((r - minR, c - minC));
            norm.Sort();
            return string.Join(';', norm);
        }
    }

    #endregion

    #region Q3 Go, Gophers!

    [Test]
    public void Q3Small() => InteractiveTest(Q3Solve, io => Q3Judge(io, testNumber: 0));

    [Test]
    public void Q3Large() => InteractiveTest(Q3Solve, io => Q3Judge(io, testNumber: 1));

    private readonly record struct GoFrac : IComparable<GoFrac>
    {
        public int Num { get; }
        public int Den { get; }

        public GoFrac(int num, int den)
        {
            int g = (int)BigInteger.GreatestCommonDivisor(num, den);
            Num = num / g;
            Den = den / g;
        }

        public int CompareTo(GoFrac other) =>
            ((long)Num * other.Den).CompareTo((long)other.Num * Den);

        public static bool operator <(GoFrac a, GoFrac b) => a.CompareTo(b) < 0;
        public static bool operator >(GoFrac a, GoFrac b) => a.CompareTo(b) > 0;
        public static bool operator <=(GoFrac a, GoFrac b) => a.CompareTo(b) <= 0;
        public static bool operator >=(GoFrac a, GoFrac b) => a.CompareTo(b) >= 0;
    }

    private static void Q3Solve(GCJInteractiveChannel io)
    {
        const int maxNConst = 25;
        const int maxTaste = 1_000_000;

        string? firstLine = io.ReadLine();
        if (firstLine is null)
            return;
        int numCases = int.Parse(firstLine);

        for (int tc = 0; tc < numCases; tc++)
        {
            _ = int.Parse(io.ReadRequiredLine()); // S = 100000
            List<int> possibleNs = [];
            for (int n = 2; n <= maxNConst; n++)
                possibleNs.Add(n);
            int totalSnacksUsed = 0;

            int Offer(int quality)
            {
                io.WriteLine(quality.ToString());
                int resp = int.Parse(io.ReadRequiredLine());
                if (resp < 0)
                    throw new InvalidOperationException("Judge reported wrong answer.");
                totalSnacksUsed++;
                return resp;
            }

            void FilterPossibleNs(GoFrac f)
            {
                for (int i = possibleNs.Count - 1; i >= 0; i--)
                {
                    if (possibleNs[i] % f.Den != 0)
                        possibleNs.RemoveAt(i);
                }
            }

            int ComputeLcm()
            {
                long r = 1;
                foreach (int n in possibleNs)
                    r = r / (long)BigInteger.GreatestCommonDivisor(r, n) * n;
                return (int)Math.Min(r, 1_000_000L);
            }

            static (int MinE, int MaxE) GetBounds(int s0, int kLen, int n, int k)
            {
                int s1 = s0 + kLen;
                int r0 = s0 / n;
                int r1 = (s1 - 1) / n;
                if (r0 == r1)
                    return (Math.Max(0, kLen - (n - k)), Math.Min(kLen, k));

                int a = (r0 + 1) * n - s0;
                int full = r1 - r0 - 1;
                int b = s1 - r1 * n;
                int minE = Math.Max(0, a - (n - k)) + full * k + Math.Max(0, b - (n - k));
                int maxE = Math.Min(a, k) + full * k + Math.Min(b, k);
                return (minE, maxE);
            }

            GoFrac MeasureExact(int quality, GoFrac loFrac, GoFrac hiFrac, bool strictAboveZero, bool strictBelowOne)
            {
                int r = ComputeLcm();
                int align = (r - (totalSnacksUsed % r)) % r;
                if (r <= 1000 && align + r <= 1200)
                {
                    for (int i = 0; i < align; i++)
                        Offer(quality);
                    int eaten = 0;
                    for (int i = 0; i < r; i++)
                        eaten += Offer(quality);
                    GoFrac exact = new(eaten, r);
                    FilterPossibleNs(exact);
                    return exact;
                }

                int s0 = totalSnacksUsed;
                int totalK = 0;
                int totalE = 0;

                while (true)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        totalE += Offer(quality);
                        totalK++;
                    }

                    GoFrac? single = null;
                    bool multiple = false;
                    foreach (int n in possibleNs)
                    {
                        for (int k = 0; k <= n; k++)
                        {
                            if (strictAboveZero && k == 0) continue;
                            if (strictBelowOne && k == n) continue;
                            GoFrac f = new(k, n);
                            if (f < loFrac || f > hiFrac) continue;
                            var (minE, maxE) = GetBounds(s0, totalK, n, k);
                            if (totalE >= minE && totalE <= maxE)
                            {
                                if (single is null)
                                    single = f;
                                else if (!single.Value.Equals(f))
                                {
                                    multiple = true;
                                    break;
                                }
                            }
                        }
                        if (multiple) break;
                    }

                    if (!multiple && single is not null)
                    {
                        FilterPossibleNs(single.Value);
                        return single.Value;
                    }

                    int curR = ComputeLcm();
                    int curAlign = (curR - (totalSnacksUsed % curR)) % curR;
                    if (curR <= 600 && curAlign + curR <= 650)
                    {
                        for (int i = 0; i < curAlign; i++)
                            Offer(quality);
                        int eaten = 0;
                        for (int i = 0; i < curR; i++)
                            eaten += Offer(quality);
                        GoFrac exact = new(eaten, curR);
                        FilterPossibleNs(exact);
                        return exact;
                    }
                }
            }

            // Note: higher quality value means MORE gophers eat! Taste level l is eaten when v >= l.
            // Wait: double check! EATEN = 1 if v >= taste_levels[snacks % n].
            // Minimum taste level L_min: first quality v where AT LEAST ONE gopher eats (some eaten).
            int minTaste;
            {
                int lo = 1, hi = maxTaste;
                while (lo < hi)
                {
                    int mid = lo + ((hi - lo) >> 1);
                    bool anyEaten = false;
                    int limit = 2 * possibleNs[^1] - 1;
                    for (int i = 0; i < limit; i++)
                    {
                        if (Offer(mid) == 1)
                        {
                            anyEaten = true;
                            break;
                        }
                    }

                    if (anyEaten)
                        hi = mid;
                    else
                        lo = mid + 1;
                }
                minTaste = lo;
            }

            // Maximum taste level L_max: smallest quality v where ALL gophers eat (zero uneaten).
            int maxTasteVal;
            {
                int lo = minTaste, hi = maxTaste;
                while (lo < hi)
                {
                    int mid = lo + ((hi - lo) >> 1);
                    bool anyUneaten = false;
                    int limit = 2 * possibleNs[^1] - 1;
                    for (int i = 0; i < limit; i++)
                    {
                        if (Offer(mid) == 0)
                        {
                            anyUneaten = true;
                            break;
                        }
                    }

                    if (anyUneaten)
                        lo = mid + 1;
                    else
                        hi = mid;
                }
                maxTasteVal = lo;
            }

            GoFrac pMin = MeasureExact(minTaste, new GoFrac(0, 1), new GoFrac(1, 1), true, true);
            if (possibleNs.Count > 1 && maxTasteVal > minTaste + 1)
            {
                GoFrac pMaxM1 = MeasureExact(maxTasteVal - 1, pMin, new GoFrac(1, 1), true, true);
                Stack<(int Lo, int Hi, GoFrac LoFrac, GoFrac HiFrac)> stack = new();
                if (pMaxM1 > pMin)
                    stack.Push((minTaste, maxTasteVal - 1, pMin, pMaxM1));

                while (stack.Count > 0 && possibleNs.Count > 1)
                {
                    var (lo, hi, loFrac, hiFrac) = stack.Pop();
                    if (hi <= lo + 1 || loFrac >= hiFrac)
                        continue;

                    int maxN = possibleNs[^1];
                    if (((long)hiFrac.Num * loFrac.Den - (long)loFrac.Num * hiFrac.Den) * maxN <
                        2L * hiFrac.Den * loFrac.Den)
                    {
                        continue;
                    }

                    int mid = lo + ((hi - lo) >> 1);
                    int r = ComputeLcm();
                    int align = (r - (totalSnacksUsed % r)) % r;
                    if (r <= 600 && align + r <= 650)
                    {
                        GoFrac pMid = MeasureExact(mid, loFrac, hiFrac, true, true);
                        if (possibleNs.Count <= 1)
                            break;
                        if (pMid > loFrac && mid > lo + 1)
                            stack.Push((lo, mid, loFrac, pMid));
                        if (hiFrac > pMid && hi > mid + 1)
                            stack.Push((mid, hi, pMid, hiFrac));
                        continue;
                    }

                    int s0 = totalSnacksUsed;
                    int totalK = 0;
                    int totalE = 0;
                    while (true)
                    {
                        int batch = totalK == 0 ? 25 : 50;
                        for (int i = 0; i < batch; i++)
                        {
                            totalE += Offer(mid);
                            totalK++;
                        }

                        bool canEqLo = false;
                        bool canEqHi = false;
                        bool canStrict = false;
                        GoFrac? single = null;
                        bool multiple = false;

                        foreach (int n in possibleNs)
                        {
                            for (int k = 1; k < n; k++)
                            {
                                GoFrac f = new(k, n);
                                if (f < loFrac || f > hiFrac) continue;
                                var (minE, maxE) = GetBounds(s0, totalK, n, k);
                                if (totalE < minE || totalE > maxE) continue;

                                if (f.Equals(loFrac)) canEqLo = true;
                                else if (f.Equals(hiFrac)) canEqHi = true;
                                else canStrict = true;

                                if (single is null) single = f;
                                else if (!single.Value.Equals(f)) multiple = true;
                            }
                        }

                        if (!canEqHi && !canStrict)
                        {
                            if (hi > mid + 1)
                                stack.Push((mid, hi, loFrac, hiFrac));
                            break;
                        }
                        if (!canEqLo && !canStrict)
                        {
                            if (mid > lo + 1)
                                stack.Push((lo, mid, loFrac, hiFrac));
                            break;
                        }
                        if (!multiple && single is not null)
                        {
                            FilterPossibleNs(single.Value);
                            if (single.Value > loFrac && mid > lo + 1)
                                stack.Push((lo, mid, loFrac, single.Value));
                            if (hiFrac > single.Value && hi > mid + 1)
                                stack.Push((mid, hi, single.Value, hiFrac));
                            break;
                        }

                        int curR = ComputeLcm();
                        int curAlign = (curR - (totalSnacksUsed % curR)) % curR;
                        if (!canEqLo && !canEqHi && curR <= 600 && curAlign + curR <= 650)
                        {
                            GoFrac pMid = MeasureExact(mid, loFrac, hiFrac, true, true);
                            if (pMid > loFrac && mid > lo + 1)
                                stack.Push((lo, mid, loFrac, pMid));
                            if (hiFrac > pMid && hi > mid + 1)
                                stack.Push((mid, hi, pMid, hiFrac));
                            break;
                        }
                    }
                }
            }

            io.WriteLine($"-{possibleNs[0]}");
        }
    }

    private static void Q3Judge(GCJInteractiveChannel io, int testNumber)
    {
        const int maxSnacks = 100_000;
        const int maxTaste = 1_000_000;
        const int minN = 2;
        const int maxN = 25;

        Random rng = new(92653);

        void RandomShuffle(int[] a)
        {
            for (int i = a.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (a[i], a[j]) = (a[j], a[i]);
            }
            if (a.Length > 1)
            {
                for (int s = 0; s < 10; s++)
                {
                    int i1 = rng.Next(a.Length);
                    int i2 = rng.Next(a.Length - 1);
                    if (i2 >= i1) i2++;
                    (a[i1], a[i2]) = (a[i2], a[i1]);
                }
            }
        }

        static void Noop(int[] _) { }

        static void Reverse(int[] a) => Array.Reverse(a);

        static Action<int[]> ApplyToSlices(Action<int[]> func, int slices) => a =>
        {
            int sliceSize = a.Length / slices;
            for (int i = 0; i < slices; i++)
            {
                int[] orig = new int[sliceSize];
                Array.Copy(a, i * sliceSize, orig, 0, sliceSize);
                func(orig);
                Array.Copy(orig, 0, a, i * sliceSize, sliceSize);
            }
        };

        Action<int[]> OnceInAWhile(Action<int[]> f) => a =>
        {
            if (rng.Next(10) == 0)
                f(a);
        };

        static int[] Interject(int[] rep, int[] singles)
        {
            List<int> res = new(singles.Length * (rep.Length + 1));
            foreach (int s in singles)
            {
                res.AddRange(rep);
                res.Add(s);
            }
            return [.. res];
        }

        static int[] Repeat(int val, int count) => Enumerable.Repeat(val, count).ToArray();
        static int[] Range(int start, int stop, int step = 1)
        {
            List<int> res = [];
            for (int v = start; v < stop; v += step)
                res.Add(v);
            return [.. res];
        }

        List<(int[] TasteLevels, Action<int[]> Reorder)> cases = [];
        if (testNumber == 0)
        {
            int[][] set1 =
            [
                [1, 2],
                [maxTaste - 1, maxTaste, maxTaste - 2],
                Range(86427, 86452),
                Range(23526, 23526 + 256 * 24, 256),
                Enumerable.Range(0, 20).Select(i => maxTaste - (1 << i)).ToArray(),
                Enumerable.Range(0, 24).Select(i => 2 + i * (i + 1) / 2).ToArray(),
                [.. Range(1000, 1013), .. Range(maxTaste - 1012, maxTaste - 1000)],
                [1, 2, maxTaste - 1, maxTaste],
                Enumerable.Range(0, 5).Select(i => (maxTaste - 1) * i / 4 + 1).ToArray(),
                Enumerable.Range(0, 24).Select(i => (maxTaste - 1) * i / 23 + 1).ToArray()
            ];
            foreach (int[] c in set1)
                cases.Add((c, RandomShuffle));
        }
        else
        {
            int mx = maxTaste;
            cases =
            [
                ([1, 1, 3], RandomShuffle),
                ([mx, mx - 1, mx - 1, mx - 2], ApplyToSlices(RandomShuffle, 2)),
                ([.. Repeat(1, 4), mx / 3, 2 * mx / 3, .. Repeat(mx, 6)], Reverse),
                (Interject([.. Repeat(2, 3), .. Repeat(mx - 1, 2)], [mx / 2, mx / 2 + 1]), ApplyToSlices(Reverse, 2)),
                (Interject([.. Repeat(mx / 5, 4), .. Repeat(4 * mx / 5, 2), 3 * mx / 5], [2 * mx / 5, 2 * mx / 5 + 1, 2 * mx / 5 + 2]), ApplyToSlices(Reverse, 3)),
                (Interject([17, 19, 2048, 2049, mx / 2 - 1000, mx / 2 + 1024, 3 * mx / 4 + 9, 3 * mx / 4 + 10, 3 * mx / 4 + 11, 999979, 999983], [mx / 2 - 3, mx / 2 - 2]), Noop),
                (Range(mx - 86452, mx - 86427), Noop),
                (Interject([.. Range(mx - 12, mx - 4), mx - 1, mx - 2], [mx - 4, mx - 3]), OnceInAWhile(ApplyToSlices(Reverse, 2))),
                (Interject([100000, 200000, 800000, 900000], [300000, 400000, 500000, 600000, 700000]), ApplyToSlices(Reverse, 5)),
                ([8, 8 * 8 * 8 * 8, 8 * 8 * 8 * 8 * 8 * 8, 8, 8 * 8 * 8 * 8, 8 * 8 * 8 * 8 * 8 * 8, 8, 8 * 8 * 8 * 8, 8 * 8 * 8 * 8 * 8 * 8, .. Repeat(8 * 8, 1), .. Repeat(8 * 8 * 8 * 8 * 8, 1), .. Repeat(8 * 8, 1), .. Repeat(8 * 8 * 8 * 8 * 8, 1), .. Repeat(8 * 8, 1), .. Repeat(8 * 8 * 8 * 8 * 8, 1), .. Repeat(8 * 8, 1), .. Repeat(8 * 8 * 8 * 8 * 8, 1), .. Repeat(8 * 8 * 8, 5)], OnceInAWhile(RandomShuffle))
            ];
            // Normalize case 10 cleanly as (8, 8^4, 8^6)*3 + (8^2, 8^5)*4 + (8^3)*5
            int[] p1 = [8, 4096, 262144];
            int[] p2 = [64, 32768];
            cases[9] = (
                [.. p1, .. p1, .. p1, .. p2, .. p2, .. p2, .. p2, .. Repeat(512, 5)],
                OnceInAWhile(RandomShuffle));
        }

        Assert.AreEqual(10, cases.Count);
        for (int i = cases.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (cases[i], cases[j]) = (cases[j], cases[i]);
        }

        io.WriteLine(cases.Count.ToString());
        for (int tc = 1; tc <= cases.Count; tc++)
        {
            var (baseTaste, reorder) = cases[tc - 1];
            int[] tasteLevels = (int[])baseTaste.Clone();
            int n = tasteLevels.Length;
            int snacks = 0;
            io.WriteLine(maxSnacks.ToString());

            while (true)
            {
                if (snacks % n == 0)
                    reorder(tasteLevels);

                string line = io.ReadRequiredLine();
                string[] tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(1, tokens.Length, $"Case #{tc}: expected 1 token.");
                Assert.IsTrue(int.TryParse(tokens[0], out int v), $"Case #{tc}: invalid integer '{tokens[0]}'.");
                Assert.IsFalse(v < -maxN || (v > -minN && v < 1) || v > maxTaste,
                    $"Case #{tc}: value {v} out of range.");

                if (v < 0)
                {
                    Assert.AreEqual(n, -v, $"Case #{tc}: guessed {-v} != true {n}.");
                    break;
                }

                snacks++;
                Assert.LessOrEqual(snacks, maxSnacks, $"Case #{tc}: exceeded {maxSnacks} snacks.");
                io.WriteLine(v >= tasteLevels[snacks % n] ? "1" : "0");
            }
        }

        Assert.IsNull(io.ReadLine(), "Additional input after all cases finish.");
    }

    #endregion

    #region Q4 The Cartesian Job

    [Test]
    public void Q4Sample() => Test(Q4, 1e-6);

    [Test]
    public void Q4Small() => Test(Q4, 1e-6);

    [Test]
    public void Q4Large() => Test(Q4, 1e-6);

    private static Func<double> Q4(GCJTestCase testCase)
    {
        testCase.Get(out int n);
        (long X, long Y, long Xp, long Yp)[] lasers = new (long, long, long, long)[n];
        for (int i = 0; i < n; i++)
            testCase.Get(out lasers[i].X, out lasers[i].Y, out lasers[i].Xp, out lasers[i].Yp);

        return () =>
        {
            Point2L posX = new(1, 0);
            Point2L negX = new(-1, 0);
            Point2L covStart = posX;
            Point2L covEnd = negX;

            List<(Point2L A, Point2L B)> rawChoices = [];

            static Point2L ReflectX(Point2L p) => new(p.X, -p.Y);
            static Point2L MinRay(Point2L a, Point2L b) => Geometry.CompareDirectionCCW(a, b) <= 0 ? a : b;
            static Point2L MaxRay(Point2L a, Point2L b) => Geometry.CompareDirectionCCW(a, b) >= 0 ? a : b;

            foreach (var (x, y, xp, yp) in lasers)
            {
                if (x == 0)
                    continue;

                Point2L u = new(-x, -y);
                Point2L v = new(-x, 1000L - y);
                Point2L sVec = x < 0 ? u : v;
                Point2L eVec = x < 0 ? v : u;
                Point2L d = new(xp - x, yp - y);

                Point2L rStart = new(d.X * sVec.X + d.Y * sVec.Y, d.X * sVec.Y - d.Y * sVec.X);
                Point2L rEnd = new(d.X * eVec.X + d.Y * eVec.Y, d.X * eVec.Y - d.Y * eVec.X);

                if (rStart.Y >= 0 && rEnd.Y >= 0)
                {
                    rawChoices.Add((rStart, rEnd));
                }
                else if (rStart.Y <= 0 && rEnd.Y <= 0)
                {
                    rawChoices.Add((ReflectX(rEnd), ReflectX(rStart)));
                }
                else if (rStart.Y < 0 && rEnd.Y > 0)
                {
                    Point2L rRef = ReflectX(rStart);
                    Point2L mn = MinRay(rEnd, rRef);
                    Point2L mx = MaxRay(rEnd, rRef);
                    covStart = MaxRay(covStart, mn);
                    if (Geometry.CompareDirectionCCW(mn, mx) < 0)
                        rawChoices.Add((mn, mx));
                }
                else // rStart.Y > 0 && rEnd.Y < 0
                {
                    Point2L rRef = ReflectX(rEnd);
                    Point2L mn = MinRay(rStart, rRef);
                    Point2L mx = MaxRay(rStart, rRef);
                    covEnd = MinRay(covEnd, mx);
                    if (Geometry.CompareDirectionCCW(mn, mx) < 0)
                        rawChoices.Add((mn, mx));
                }
            }

            if (Geometry.CompareDirectionCCW(covStart, covEnd) >= 0)
                return 0.0;

            List<Point2L> rays = [covStart, covEnd];
            List<(Point2L A, Point2L B)> clipped = [];
            foreach (var (ra, rb) in rawChoices)
            {
                Point2L ca = MaxRay(ra, covStart);
                Point2L cb = MinRay(rb, covEnd);
                if (Geometry.CompareDirectionCCW(ca, cb) < 0)
                {
                    clipped.Add((ca, cb));
                    rays.Add(ca);
                    rays.Add(cb);
                }
            }

            if (clipped.Count == 0)
                return 1.0;

            rays.Sort(Geometry.CompareDirectionCCW);
            List<Point2L> uniqueRays = [rays[0]];
            for (int i = 1; i < rays.Count; i++)
            {
                if (Geometry.CompareDirectionCCW(rays[i], uniqueRays[^1]) != 0)
                    uniqueRays.Add(rays[i]);
            }

            int m = uniqueRays.Count - 1;
            int FindRay(Point2L ray)
            {
                int lo = 0, hi = m;
                while (lo < hi)
                {
                    int mid = lo + ((hi - lo) >> 1);
                    int cmp = Geometry.CompareDirectionCCW(uniqueRays[mid], ray);
                    if (cmp < 0) lo = mid + 1;
                    else hi = mid;
                }
                return lo;
            }

            (int A, int B)[] intervals = new (int, int)[clipped.Count];
            for (int i = 0; i < clipped.Count; i++)
                intervals[i] = (FindRay(clipped[i].A), FindRay(clipped[i].B));

            Array.Sort(intervals, (x, y) => x.A != y.A ? x.A.CompareTo(y.A) : y.B.CompareTo(x.B));

            Dictionary<int, double> dp = new() { [0] = 1.0 };
            Dictionary<int, double> nextDp = [];
            int maxR = 0;
            double heistProb = 0.0;

            foreach (var (a, b) in intervals)
            {
                int newMaxR = Math.Max(maxR, b);
                nextDp.Clear();

                foreach (var (x, p) in dp)
                {
                    if (p < 1e-14)
                        continue;

                    if (a > x)
                    {
                        heistProb += p;
                        continue;
                    }

                    double half = 0.5 * p;
                    int x1 = Math.Min(Math.Max(x, b), maxR);
                    nextDp.TryGetValue(x1, out double cur1);
                    nextDp[x1] = cur1 + half;

                    int x2 = x;
                    nextDp.TryGetValue(x2, out double cur2);
                    nextDp[x2] = cur2 + half;
                }

                (dp, nextDp) = (nextDp, dp);
                maxR = newMaxR;
            }

            foreach (var (x, p) in dp)
            {
                if (x < m)
                    heistProb += p;
            }

            return heistProb;
        };
    }

    #endregion

    #region Q5 Swordmaster

    [Test]
    public void Q5Sample() => Test(Q5);

    [Test]
    public void Q5Small() => Test(Q5);

    [Test]
    public void Q5Large() => Test(Q5);

    private static Func<string> Q5(GCJTestCase testCase)
    {
        testCase.Get(out int n, out int p);
        int[][] attacks = new int[n][];
        int[][] defenses = new int[n][];
        for (int i = 0; i < n; i++)
        {
            testCase.Get(out int aCnt, out int dCnt);
            testCase.Get(out int[] atk);
            testCase.Get(out int[] def);
            for (int j = 0; j < aCnt; j++) atk[j]--;
            for (int j = 0; j < dCnt; j++) def[j]--;
            attacks[i] = atk;
            defenses[i] = def;
        }

        return () =>
        {
            // Condition 1: Check existence of set G1 via defense closure BFS from duelist 0.
            bool[] knownDef = new bool[p];
            int[] missingDef = new int[n];
            List<int>[] attackUsers = new List<int>[p];
            for (int a = 0; a < p; a++)
                attackUsers[a] = [];

            for (int opp = 1; opp < n; opp++)
            {
                missingDef[opp] = attacks[opp].Length;
                foreach (int a in attacks[opp])
                    attackUsers[a].Add(opp);
            }

            Queue<int> defQueue = [];
            foreach (int d in defenses[0])
            {
                if (!knownDef[d])
                {
                    knownDef[d] = true;
                    defQueue.Enqueue(d);
                }
            }

            bool[] beaten = new bool[n];
            int beatenCount = 0;
            while (defQueue.Count > 0)
            {
                int d = defQueue.Dequeue();
                foreach (int opp in attackUsers[d])
                {
                    if (beaten[opp])
                        continue;
                    missingDef[opp]--;
                    if (missingDef[opp] == 0)
                    {
                        beaten[opp] = true;
                        beatenCount++;
                        foreach (int newD in defenses[opp])
                        {
                            if (!knownDef[newD])
                            {
                                knownDef[newD] = true;
                                defQueue.Enqueue(newD);
                            }
                        }
                    }
                }
            }

            if (beatenCount < n - 1)
                return "NO";

            // Condition 2: Tarjan SCC on duelists + active attacks.
            List<int> activeAttacks = [];
            int[] attackToNode = new int[p];
            Array.Fill(attackToNode, -1);
            for (int i = 0; i < n; i++)
            {
                foreach (int a in attacks[i])
                {
                    if (attackToNode[a] < 0)
                    {
                        attackToNode[a] = n + activeAttacks.Count;
                        activeAttacks.Add(a);
                    }
                }
            }

            int totalNodes = n + activeAttacks.Count;
            List<int>[] graph = new List<int>[totalNodes];
            for (int i = 0; i < totalNodes; i++)
                graph[i] = [];

            int[] hasDefStamp = new int[p];
            for (int dIdx = 0; dIdx < n; dIdx++)
            {
                int stamp = dIdx + 1;
                foreach (int d in defenses[dIdx])
                    hasDefStamp[d] = stamp;

                for (int ai = 0; ai < activeAttacks.Count; ai++)
                {
                    int atk = activeAttacks[ai];
                    if (hasDefStamp[atk] != stamp)
                        graph[dIdx].Add(n + ai);
                }

                foreach (int a in attacks[dIdx])
                    graph[attackToNode[a]].Add(dIdx);
            }

            // Iterative Tarjan SCC
            int[] disc = new int[totalNodes];
            int[] low = new int[totalNodes];
            bool[] onStack = new bool[totalNodes];
            int[] comp = new int[totalNodes];
            int[] nextEdge = new int[totalNodes];
            Stack<int> tarjanStack = new();
            Stack<int> dfsStack = new();
            int time = 0;
            List<List<int>> sccs = [];

            for (int start = 0; start < totalNodes; start++)
            {
                if (disc[start] != 0)
                    continue;

                disc[start] = low[start] = ++time;
                tarjanStack.Push(start);
                onStack[start] = true;
                dfsStack.Push(start);

                while (dfsStack.Count > 0)
                {
                    int u = dfsStack.Peek();
                    if (nextEdge[u] < graph[u].Count)
                    {
                        int v = graph[u][nextEdge[u]++];
                        if (disc[v] == 0)
                        {
                            disc[v] = low[v] = ++time;
                            tarjanStack.Push(v);
                            onStack[v] = true;
                            dfsStack.Push(v);
                        }
                        else if (onStack[v] && disc[v] < low[u])
                        {
                            low[u] = disc[v];
                        }
                    }
                    else
                    {
                        dfsStack.Pop();
                        if (dfsStack.Count > 0)
                        {
                            int parent = dfsStack.Peek();
                            if (low[u] < low[parent])
                                low[parent] = low[u];
                        }

                        if (low[u] == disc[u])
                        {
                            int sccId = sccs.Count;
                            List<int> members = [];
                            while (true)
                            {
                                int w = tarjanStack.Pop();
                                onStack[w] = false;
                                comp[w] = sccId;
                                members.Add(w);
                                if (w == u)
                                    break;
                            }
                            sccs.Add(members);
                        }
                    }
                }
            }

            bool[] hasOutEdge = new bool[sccs.Count];
            for (int u = 0; u < totalNodes; u++)
            {
                int cu = comp[u];
                foreach (int v in graph[u])
                {
                    if (comp[v] != cu)
                        hasOutEdge[cu] = true;
                }
            }

            int[] defCount = new int[p];
            int sccStamp = 0;
            int[] defSeenStamp = new int[p];

            for (int ci = 0; ci < sccs.Count; ci++)
            {
                if (hasOutEdge[ci] || ci == comp[0])
                    continue;

                List<int> duelists = [];
                foreach (int u in sccs[ci])
                {
                    if (u < n)
                        duelists.Add(u);
                }

                if (duelists.Count == 0)
                    continue;

                sccStamp++;
                foreach (int u in duelists)
                {
                    foreach (int d in defenses[u])
                    {
                        if (defSeenStamp[d] != sccStamp)
                        {
                            defSeenStamp[d] = sccStamp;
                            defCount[d] = 1;
                        }
                        else
                        {
                            defCount[d]++;
                        }
                    }
                }

                bool allCanCounterEveryInScc = true;
                foreach (int u in duelists)
                {
                    bool hasCommon = false;
                    foreach (int a in attacks[u])
                    {
                        if (defSeenStamp[a] == sccStamp && defCount[a] == duelists.Count)
                        {
                            hasCommon = true;
                            break;
                        }
                    }

                    if (!hasCommon)
                    {
                        allCanCounterEveryInScc = false;
                        break;
                    }
                }

                if (allCanCounterEveryInScc)
                    return "NO";
            }

            return "YES";
        };
    }

    #endregion
}

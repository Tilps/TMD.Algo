using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2019;

[TestFixture]
public class QRTest : BaseGCJTest
{
    #region Q1 Foregone Solution

    [Test]
    public void Q1Sample() => Test(Q1, ValidateForegoneSolution);

    [Test]
    public void Q1Small1() => Test(Q1, ValidateForegoneSolution);

    [Test]
    public void Q1Small2() => Test(Q1, ValidateForegoneSolution);

    [Test]
    public void Q1Large() => Test(Q1, ValidateForegoneSolution);

    private static Func<string> Q1(GCJTestCase testCase)
    {
        testCase.Get(out string n);
        return () =>
        {
            char[] a = new char[n.Length];
            char[] b = new char[n.Length];
            bool hasNonZeroB = false;

            for (int i = 0; i < n.Length; i++)
            {
                if (n[i] == '4')
                {
                    a[i] = '2';
                    b[i] = '2';
                    hasNonZeroB = true;
                }
                else
                {
                    a[i] = n[i];
                    b[i] = '0';
                }
            }

            if (!hasNonZeroB)
            {
                BigInteger nVal = BigInteger.Parse(n, CultureInfo.InvariantCulture);
                return $"1 {nVal - 1}";
            }

            string bStr = new string(b).TrimStart('0');
            return $"{new string(a)} {bStr}";
        };
    }

    private static void ValidateForegoneSolution(string input, string expectedOutput, List<string> actualCases)
    {
        string[] inputLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        int numCases = int.Parse(inputLines[0], CultureInfo.InvariantCulture);
        Assert.AreEqual(numCases, actualCases.Count);

        for (int tc = 0; tc < numCases; tc++)
        {
            BigInteger n = BigInteger.Parse(inputLines[tc + 1].Trim(), CultureInfo.InvariantCulture);
            string[] lines = actualCases[tc].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(1, lines.Length, $"Case #{tc + 1}: expected single line.");
            string prefix = $"Case #{tc + 1}:";
            Assert.IsTrue(lines[0].StartsWith(prefix, StringComparison.Ordinal));

            string[] tokens = lines[0][prefix.Length..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(2, tokens.Length, $"Case #{tc + 1}: expected two integers.");
            Assert.IsFalse(tokens[0].StartsWith('0') || tokens[1].StartsWith('0'));
            Assert.IsFalse(tokens[0].Contains('4') || tokens[1].Contains('4'));

            BigInteger a = BigInteger.Parse(tokens[0], CultureInfo.InvariantCulture);
            BigInteger b = BigInteger.Parse(tokens[1], CultureInfo.InvariantCulture);
            Assert.GreaterOrEqual(a, BigInteger.One);
            Assert.GreaterOrEqual(b, BigInteger.One);
            Assert.AreEqual(n, a + b, $"Case #{tc + 1}: sum mismatch.");
        }
    }

    #endregion

    #region Q2 You Can Go Your Own Way

    [Test]
    public void Q2Sample() => Test(Q2, ValidateOwnWay);

    [Test]
    public void Q2Small1() => Test(Q2, ValidateOwnWay);

    [Test]
    public void Q2Small2() => Test(Q2, ValidateOwnWay);

    [Test]
    public void Q2Large() => Test(Q2, ValidateOwnWay);

    private static Func<string> Q2(GCJTestCase testCase)
    {
        testCase.Get(out int _);
        testCase.Get(out string lydia);
        return () =>
        {
            char[] path = new char[lydia.Length];
            for (int i = 0; i < lydia.Length; i++)
                path[i] = lydia[i] == 'E' ? 'S' : 'E';
            return new string(path);
        };
    }

    private static void ValidateOwnWay(string input, string expectedOutput, List<string> actualCases)
    {
        string[] inputLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        int numCases = int.Parse(inputLines[0], CultureInfo.InvariantCulture);
        Assert.AreEqual(numCases, actualCases.Count);

        for (int tc = 0; tc < numCases; tc++)
        {
            int n = int.Parse(inputLines[1 + 2 * tc].Trim(), CultureInfo.InvariantCulture);
            string theirWay = inputLines[2 + 2 * tc].Trim();

            string[] lines = actualCases[tc].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(1, lines.Length, $"Case #{tc + 1}: expected single line.");
            string prefix = $"Case #{tc + 1}:";
            Assert.IsTrue(lines[0].StartsWith(prefix, StringComparison.Ordinal));
            string[] tokens = lines[0][prefix.Length..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(1, tokens.Length, $"Case #{tc + 1}: expected single path token.");

            string myWay = tokens[0];
            Assert.AreEqual(2 * n - 2, myWay.Length, $"Case #{tc + 1}: invalid length.");

            int myR = 0, myC = 0, theirR = 0, theirC = 0;
            for (int i = 0; i < myWay.Length; i++)
            {
                if (myR == theirR && myC == theirC)
                    Assert.AreNotEqual(theirWay[i], myWay[i], $"Case #{tc + 1}: reused move at step {i}.");

                if (theirWay[i] == 'S')
                    theirR++;
                else
                    theirC++;

                if (myWay[i] == 'S')
                    myR++;
                else if (myWay[i] == 'E')
                    myC++;
                else
                    Assert.Fail($"Case #{tc + 1}: invalid char '{myWay[i]}'.");
            }

            Assert.AreEqual((n - 1, n - 1), (myR, myC), $"Case #{tc + 1}: did not reach destination.");
        }
    }

    #endregion

    #region Q3 Cryptopangrams

    [Test]
    public void Q3Sample() => Test(Q3);

    [Test]
    public void Q3Small() => Test(Q3);

    [Test]
    public void Q3Large() => Test(Q3);

    private static Func<string> Q3(GCJTestCase testCase)
    {
        testCase.Get(out string _, out int l);
        testCase.Get(out string[] tokens);
        return () =>
        {
            BigInteger[] cipher = new BigInteger[l];
            for (int i = 0; i < l; i++)
                cipher[i] = BigInteger.Parse(tokens[i], CultureInfo.InvariantCulture);

            int diffIdx = -1;
            for (int i = 0; i + 1 < l; i++)
            {
                if (cipher[i] != cipher[i + 1])
                {
                    diffIdx = i;
                    break;
                }
            }

            BigInteger[] primes = new BigInteger[l + 1];
            primes[diffIdx + 1] = BigInteger.GreatestCommonDivisor(cipher[diffIdx], cipher[diffIdx + 1]);
            for (int i = diffIdx; i >= 0; i--)
                primes[i] = cipher[i] / primes[i + 1];
            for (int i = diffIdx + 1; i < l; i++)
                primes[i + 1] = cipher[i] / primes[i];

            SortedSet<BigInteger> alphabetPrimes = [];
            foreach (BigInteger p in primes)
                alphabetPrimes.Add(p);

            Dictionary<BigInteger, char> map = new(26);
            int letterIdx = 0;
            foreach (BigInteger p in alphabetPrimes)
                map[p] = (char)('A' + letterIdx++);

            StringBuilder sb = new(l + 1);
            foreach (BigInteger p in primes)
                sb.Append(map[p]);
            return sb.ToString();
        };
    }

    #endregion

    #region Q4 Dat Bae

    [Test]
    public void Q4Small() => InteractiveTest(Q4Solve, io => Q4Judge(io, testNumber: 0));

    [Test]
    public void Q4Large() => InteractiveTest(Q4Solve, io => Q4Judge(io, testNumber: 1));

    private static void Q4Solve(GCJInteractiveChannel io)
    {
        int t = int.Parse(io.ReadRequiredLine(), CultureInfo.InvariantCulture);
        for (int tc = 0; tc < t; tc++)
        {
            string[] header = io.ReadRequiredLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int n = int.Parse(header[0], CultureInfo.InvariantCulture);
            int b = int.Parse(header[1], CultureInfo.InvariantCulture);

            const int queryBits = 5;
            string[] responses = new string[queryBits];
            for (int bit = 0; bit < queryBits; bit++)
            {
                char[] query = new char[n];
                for (int i = 0; i < n; i++)
                    query[i] = ((i >> bit) & 1) == 1 ? '1' : '0';
                io.WriteLine(new string(query));
                string resp = io.ReadRequiredLine();
                if (resp == "-1")
                    throw new InvalidOperationException($"Case #{tc + 1}: judge aborted on bit {bit}.");
                responses[bit] = resp;
            }

            int working = n - b;
            bool[] alive = new bool[n];
            int block = 0;
            int prevMod = -1;
            for (int j = 0; j < working; j++)
            {
                int val = 0;
                for (int bit = 0; bit < queryBits; bit++)
                {
                    if (responses[bit][j] == '1')
                        val |= 1 << bit;
                }

                if (j > 0 && val <= prevMod)
                    block++;
                prevMod = val;
                alive[block * 32 + val] = true;
            }

            List<int> broken = new(b);
            for (int i = 0; i < n; i++)
            {
                if (!alive[i])
                    broken.Add(i);
            }

            io.WriteLine(string.Join(" ", broken));
            string verdict = io.ReadRequiredLine();
            Assert.AreEqual("1", verdict, $"Case #{tc + 1}: expected verdict 1.");
        }
    }

    private static void Q4Judge(GCJInteractiveChannel io, int testNumber)
    {
        const int minN = 2;
        const int maxN = 1024;
        const int maxB = 15;
        int f = testNumber == 0 ? 10 : 5;
        Random rng = new(821397 + testNumber * 17);

        List<(int[] Bad, int N)> cases = [];

        void AddCase(IEnumerable<int> badEnum, int n)
        {
            int[] bad = [.. badEnum];
            Array.Sort(bad);
            Assert.That(n, Is.InRange(minN, maxN));
            Assert.That(bad.Length, Is.InRange(1, maxB));
            for (int i = 0; i < bad.Length; i++)
            {
                Assert.That(bad[i], Is.InRange(0, n - 1));
                if (i > 0)
                    Assert.Greater(bad[i], bad[i - 1]);
            }
            cases.Add((bad, n));
        }

        void AddRandomCase(int b, int n)
        {
            int[] pool = new int[n];
            for (int i = 0; i < n; i++)
                pool[i] = i;
            for (int i = 0; i < b; i++)
            {
                int swapIdx = rng.Next(i, n);
                (pool[i], pool[swapIdx]) = (pool[swapIdx], pool[i]);
            }
            AddCase(pool[..b], n);
        }

        void AddOffset(int b, int n, int start, int jump = 1)
        {
            int[] bad = new int[b];
            for (int i = 0; i < b; i++)
                bad[i] = start + i * jump;
            AddCase(bad, n);
        }

        AddCase([1, 2, 3, 4], 5);
        AddCase([0, 1, 2, 3], 5);
        AddCase([0], 5);
        AddCase([0, 4], 5);
        AddCase([0, 2, 4], 5);

        AddCase([0], 2);
        AddCase([1], 2);

        List<int> all15 = new(maxB);
        for (int i = 0; i < maxB; i++) all15.Add(i);
        AddCase(all15, maxB + 1);
        List<int> upper15 = new(maxB);
        for (int i = 1; i <= maxB; i++) upper15.Add(i);
        AddCase(upper15, maxB + 1);
        List<int> split15 = new(maxB);
        for (int i = 0; i < maxB / 2; i++) split15.Add(i);
        for (int i = maxB / 2 + 1; i < maxB; i++) split15.Add(i);
        AddCase(split15, maxB + 1);

        AddCase([6], 12);
        AddCase([3], 5);
        AddCase([maxN - 1], maxN);
        AddCase([0], maxN);

        AddRandomCase(1, maxN);
        AddRandomCase(1, (int)(maxN * 0.95));
        for (int i = 0; i < 5; i++)
            AddRandomCase(1, rng.Next(2, maxN + 1));

        AddRandomCase(maxB, maxN);
        AddRandomCase(maxB, (int)(maxB * 1.4));
        AddRandomCase(maxB, maxB * 2);
        for (int i = 0; i < 5; i++)
        {
            int b = rng.Next(2, maxB + 1);
            AddRandomCase(b, rng.Next(b + 1, maxN + 1));
        }

        AddOffset(maxB, maxN, 0, 1);
        AddOffset(maxB, maxN, maxN - maxB, 1);
        AddOffset(maxB, maxN, 12, 12);
        AddOffset(6, maxN, 800);
        AddOffset(10, maxN, 101);
        foreach (int x in new[] { 1, 5, 31, 32, 33, 400 })
        {
            AddOffset(maxB, maxN, x, maxB - 1);
            AddOffset(maxB, maxN, x, maxB);
            AddOffset(maxB, maxN, x, maxB + 1);
            AddOffset(maxB, maxN, x, maxB * 2);
        }
        for (int i = 0; i < 5; i++)
            AddOffset(maxB, maxN, rng.Next(0, 201), rng.Next(1, 51));
        for (int i = 0; i < 20; i++)
            AddOffset(maxB, maxN, rng.Next(0, maxN - maxB + 1));

        while (cases.Count < 100)
            AddRandomCase(maxB, maxN);

        io.WriteLine(cases.Count.ToString(CultureInfo.InvariantCulture));
        for (int tc = 0; tc < cases.Count; tc++)
        {
            var (badArr, n) = cases[tc];
            HashSet<int> badSet = [.. badArr];
            io.WriteLine($"{n} {badArr.Length} {f}");

            bool solved = false;
            for (int q = 0; q <= f; q++)
            {
                string line = io.ReadRequiredLine();
                Assert.IsFalse(line.Contains('\n') || line.Contains('\r'));
                string[] tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 1 && tokens[0].Length == n)
                {
                    Assert.Less(q, f, $"Case #{tc + 1}: Query Limit Exceeded.");
                    string bits = tokens[0];
                    StringBuilder ans = new(n - badArr.Length);
                    for (int i = 0; i < n; i++)
                    {
                        char c = bits[i];
                        Assert.IsTrue(c is '0' or '1', $"Case #{tc + 1}: invalid bit '{c}'.");
                        if (!badSet.Contains(i))
                            ans.Append(c);
                    }
                    io.WriteLine(ans.ToString());
                }
                else
                {
                    Assert.AreEqual(badArr.Length, tokens.Length, $"Case #{tc + 1}: wrong answer count.");
                    int[] guess = new int[tokens.Length];
                    for (int i = 0; i < tokens.Length; i++)
                    {
                        guess[i] = int.Parse(tokens[i], CultureInfo.InvariantCulture);
                        Assert.That(guess[i], Is.InRange(0, n - 1));
                        if (i > 0)
                            Assert.Greater(guess[i], guess[i - 1], $"Case #{tc + 1}: answer not strictly sorted.");
                    }
                    CollectionAssert.AreEqual(badArr, guess, $"Case #{tc + 1}: wrong answer.");
                    io.WriteLine("1");
                    solved = true;
                    break;
                }
            }
            Assert.IsTrue(solved, $"Case #{tc + 1}: never solved.");
        }

        Assert.IsNull(io.ReadLine(), "Additional input after all cases finish.");
    }

    #endregion
}

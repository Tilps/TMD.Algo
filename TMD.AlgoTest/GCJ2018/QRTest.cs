using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2018;

[TestFixture]
public class QRTest : BaseGCJTest
{
    #region Q1 Saving The Universe Again

    [Test]
    public void Q1Sample() => Test(Q1);

    [Test]
    public void Q1Small() => Test(Q1);

    [Test]
    public void Q1Large() => Test(Q1);

    private static Func<string> Q1(GCJTestCase testCase)
    {
        testCase.Get(out long d, out string p);
        return () =>
        {
            char[] s = p.ToCharArray();
            int sCount = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == 'S')
                    sCount++;
            }

            if (sCount > d)
                return "IMPOSSIBLE";

            int swaps = 0;
            while (ComputeDamage(s) > d)
            {
                for (int i = s.Length - 2; i >= 0; i--)
                {
                    if (s[i] == 'C' && s[i + 1] == 'S')
                    {
                        s[i] = 'S';
                        s[i + 1] = 'C';
                        swaps++;
                        break;
                    }
                }
            }

            return swaps.ToString();
        };
    }

    private static long ComputeDamage(char[] program)
    {
        long charge = 1;
        long damage = 0;
        for (int i = 0; i < program.Length; i++)
        {
            if (program[i] == 'C')
                charge <<= 1;
            else
                damage += charge;
        }
        return damage;
    }

    #endregion

    #region Q2 Trouble Sort

    [Test]
    public void Q2Sample() => Test(Q2);

    [Test]
    public void Q2Small() => Test(Q2);

    [Test]
    public void Q2Large() => Test(Q2);

    private static Func<string> Q2(GCJTestCase testCase)
    {
        testCase.Get(out int n);
        testCase.Get(out int[] v);
        return () =>
        {
            int evenCount = (n + 1) / 2;
            int oddCount = n / 2;
            int[] evens = new int[evenCount];
            int[] odds = new int[oddCount];
            for (int i = 0; i < n; i++)
            {
                if ((i & 1) == 0)
                    evens[i >> 1] = v[i];
                else
                    odds[i >> 1] = v[i];
            }

            Array.Sort(evens);
            Array.Sort(odds);

            int[] merged = new int[n];
            for (int i = 0; i < n; i++)
                merged[i] = (i & 1) == 0 ? evens[i >> 1] : odds[i >> 1];

            for (int i = 0; i + 1 < n; i++)
            {
                if (merged[i] > merged[i + 1])
                    return i.ToString();
            }

            return "OK";
        };
    }

    #endregion

    #region Q3 Go, Gopher!

    [Test]
    public void Q3Small() => InteractiveTest(Q3Solve, io => Q3Judge(io, testNumber: 0));

    [Test]
    public void Q3Large() => InteractiveTest(Q3Solve, io => Q3Judge(io, testNumber: 1));

    private static void Q3Solve(GCJInteractiveChannel io)
    {
        int t = int.Parse(io.ReadRequiredLine());
        for (int tc = 0; tc < t; tc++)
        {
            int a = int.Parse(io.ReadRequiredLine());
            int w = Math.Max(3, (a + 2) / 3);
            bool[,] prepared = new bool[4, w + 1];

            while (true)
            {
                int bestC = 2;
                int bestUnprepared = -1;
                for (int c = 2; c <= w - 1; c++)
                {
                    int unprep = 0;
                    for (int dr = -1; dr <= 1; dr++)
                    {
                        for (int dc = -1; dc <= 1; dc++)
                        {
                            if (!prepared[2 + dr, c + dc])
                                unprep++;
                        }
                    }

                    if (unprep > bestUnprepared)
                    {
                        bestUnprepared = unprep;
                        bestC = c;
                    }
                }

                io.WriteLine($"2 {bestC}");
                string[] parts = io.ReadRequiredLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int pr = int.Parse(parts[0]);
                int pc = int.Parse(parts[1]);
                if (pr == 0 && pc == 0)
                    break;
                if (pr < 0 || pc < 0)
                    throw new InvalidOperationException($"Judge returned error response: {pr} {pc}");

                if (pr <= 3 && pc <= w)
                    prepared[pr, pc] = true;
            }
        }
    }

    private static void Q3Judge(GCJInteractiveChannel io, int testNumber)
    {
        const int totalCases = 20;
        const int maxTries = 1000;
        const int fieldSize = 1000;
        int minCells = testNumber == 0 ? 20 : 200;

        io.WriteLine(totalCases.ToString());
        for (int caseNum = 1; caseNum <= totalCases; caseNum++)
        {
            io.WriteLine(minCells.ToString());
            Random rng = new(caseNum + testNumber + 101);
            HashSet<(int R, int C)> prepared = [];
            int north = 0, south = 0, west = 0, east = 0;
            bool solved = false;

            for (int attempt = 0; attempt < maxTries; attempt++)
            {
                string line = io.ReadRequiredLine();
                Assert.IsFalse(line.Contains('\n') || line.Contains('\r'));
                string[] tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(2, tokens.Length, $"Case #{caseNum}: expected 2 tokens, got '{line}'.");
                Assert.IsTrue(tokens[0].Length is >= 1 and <= 3 && tokens[1].Length is >= 1 and <= 3);
                Assert.IsTrue(int.TryParse(tokens[0], out int r));
                Assert.IsTrue(int.TryParse(tokens[1], out int c));
                Assert.That(r, Is.InRange(2, fieldSize - 1), $"Case #{caseNum}: row {r} out of range.");
                Assert.That(c, Is.InRange(2, fieldSize - 1), $"Case #{caseNum}: col {c} out of range.");

                int pr = r + rng.Next(-1, 2);
                int pc = c + rng.Next(-1, 2);
                if (prepared.Add((pr, pc)))
                {
                    if (prepared.Count == 1)
                    {
                        north = south = pr;
                        west = east = pc;
                    }
                    else
                    {
                        north = Math.Min(north, pr);
                        south = Math.Max(south, pr);
                        west = Math.Min(west, pc);
                        east = Math.Max(east, pc);
                    }
                }

                if (prepared.Count >= minCells &&
                    prepared.Count == (south - north + 1) * (east - west + 1))
                {
                    io.WriteLine("0 0");
                    solved = true;
                    break;
                }

                io.WriteLine($"{pr} {pc}");
            }

            Assert.IsTrue(solved, $"Case #{caseNum}: failed to prepare {minCells} cells within {maxTries} tries.");
        }

        Assert.IsNull(io.ReadLine(), "Additional input after all cases finish.");
    }

    #endregion

    #region Q4 Cubic UFO

    [Test]
    public void Q4Sample() => Test(Q4, ValidateCubicUfo);

    [Test]
    public void Q4Small() => Test(Q4, ValidateCubicUfo);

    [Test]
    public void Q4Large() => Test(Q4, ValidateCubicUfo);

    private static Func<string[]> Q4(GCJTestCase testCase)
    {
        testCase.Get(out double a);
        return () =>
        {
            double sqrt2 = Math.Sqrt(2.0);
            double x1, y1, z1, x2, y2, z2, x3, y3, z3;

            if (a <= sqrt2)
            {
                double ratio = Math.Min(1.0, a / sqrt2);
                double t = Math.PI / 4.0 - Math.Acos(ratio);
                double c = 0.5 * Math.Cos(t);
                double s = 0.5 * Math.Sin(t);
                x1 = c; y1 = s; z1 = 0.0;
                x2 = -s; y2 = c; z2 = 0.0;
                x3 = 0.0; y3 = 0.0; z3 = 0.5;
            }
            else
            {
                double lo = 0.0;
                double hi = Math.Atan(1.0 / sqrt2);
                for (int iter = 0; iter < 80; iter++)
                {
                    double mid = 0.5 * (lo + hi);
                    double area = sqrt2 * Math.Cos(mid) + Math.Sin(mid);
                    if (area < a)
                        lo = mid;
                    else
                        hi = mid;
                }

                double phi = 0.5 * (lo + hi);
                double s = 0.5 / sqrt2;
                double cosPhi = Math.Cos(phi);
                double sinPhi = Math.Sin(phi);
                x1 = s; y1 = s * cosPhi; z1 = -s * sinPhi;
                x2 = -s; y2 = s * cosPhi; z2 = -s * sinPhi;
                x3 = 0.0; y3 = 0.5 * sinPhi; z3 = 0.5 * cosPhi;
            }

            return
            [
                FormatVec(x1, y1, z1),
                FormatVec(x2, y2, z2),
                FormatVec(x3, y3, z3)
            ];
        };

        static string FormatVec(double x, double y, double z) =>
            string.Create(CultureInfo.InvariantCulture, $"{x:G17} {y:G17} {z:G17}");
    }

    private static void ValidateCubicUfo(string input, string expectedOutput, List<string> actualCases)
    {
        const double tolerance = 1.1e-6;
        string[] inputLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        int numCases = int.Parse(inputLines[0], CultureInfo.InvariantCulture);
        Assert.AreEqual(numCases, actualCases.Count);

        for (int tc = 0; tc < numCases; tc++)
        {
            double targetArea = double.Parse(inputLines[tc + 1], CultureInfo.InvariantCulture);
            string[] lines = actualCases[tc].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(4, lines.Length, $"Case #{tc + 1}: must have 4 lines.");
            Assert.AreEqual($"Case #{tc + 1}:", lines[0].TrimEnd(), $"Case #{tc + 1}: extra info in header.");

            double[][] pts = new double[3][];
            for (int i = 0; i < 3; i++)
            {
                string[] tokens = lines[i + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(3, tokens.Length, $"Case #{tc + 1}: wrong token count.");
                pts[i] = new double[3];
                for (int j = 0; j < 3; j++)
                {
                    Assert.IsTrue(double.TryParse(tokens[j], CultureInfo.InvariantCulture, out double val) &&
                                  double.IsFinite(val));
                    pts[i][j] = val;
                }

                double len = Math.Sqrt(pts[i][0] * pts[i][0] + pts[i][1] * pts[i][1] + pts[i][2] * pts[i][2]);
                Assert.LessOrEqual(Math.Abs(len - 0.5), tolerance, $"Case #{tc + 1}: distance {len} from origin != 0.5.");
            }

            double maxCos = Math.Sin(tolerance);
            for (int i = 0; i < 3; i++)
            {
                double len1 = Math.Sqrt(pts[i][0] * pts[i][0] + pts[i][1] * pts[i][1] + pts[i][2] * pts[i][2]);
                for (int j = 0; j < i; j++)
                {
                    double len2 = Math.Sqrt(pts[j][0] * pts[j][0] + pts[j][1] * pts[j][1] + pts[j][2] * pts[j][2]);
                    double dot = pts[i][0] * pts[j][0] + pts[i][1] * pts[j][1] + pts[i][2] * pts[j][2];
                    double cos = dot / (len1 * len2);
                    Assert.LessOrEqual(Math.Abs(cos), maxCos, $"Case #{tc + 1}: vectors not perpendicular (cos={cos}).");
                }
            }

            List<(double X, double Y)> projections = new(8);
            for (int s0 = -1; s0 <= 1; s0 += 2)
                for (int s1 = -1; s1 <= 1; s1 += 2)
                    for (int s2 = -1; s2 <= 1; s2 += 2)
                        projections.Add((
                            s0 * pts[0][0] + s1 * pts[1][0] + s2 * pts[2][0],
                            s0 * pts[0][2] + s1 * pts[1][2] + s2 * pts[2][2]));

            double area = CubicUfoHullArea(projections);
            Assert.LessOrEqual(Math.Abs(area - targetArea), tolerance,
                $"Case #{tc + 1}: area {area} differs from target {targetArea}.");
        }
    }

    private static double CubicUfoHullArea(List<(double X, double Y)> points)
    {
        List<double> xs = new(points.Count);
        foreach (var (x, _) in points)
            xs.Add(x);
        xs.Sort();

        double area = 0.0;
        for (int idx = 0; idx + 1 < xs.Count; idx++)
        {
            double width = xs[idx + 1] - xs[idx];
            if (width == 0.0)
                continue;
            double x = 0.5 * (xs[idx] + xs[idx + 1]);
            double minY = double.PositiveInfinity;
            double maxY = double.NegativeInfinity;
            foreach (var (x1, y1) in points)
            {
                if (x1 == x)
                {
                    minY = Math.Min(minY, y1);
                    maxY = Math.Max(maxY, y1);
                }
                else if (x1 < x)
                {
                    foreach (var (x2, y2) in points)
                    {
                        if (x2 > x)
                        {
                            double y = ((x - x1) * y2 + (x2 - x) * y1) / (x2 - x1);
                            minY = Math.Min(minY, y);
                            maxY = Math.Max(maxY, y);
                        }
                    }
                }
            }

            area += width * (maxY - minY);
        }

        return area;
    }

    #endregion
}

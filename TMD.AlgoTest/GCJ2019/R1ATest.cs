using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2019;

[TestFixture]
public class R1ATest : BaseGCJTest
{
    #region Q1 Pylons

    [Test]
    public void Q1Sample() => Test(Q1, ValidatePylons);

    [Test]
    public void Q1Small() => Test(Q1, ValidatePylons);

    [Test]
    public void Q1Large() => Test(Q1, ValidatePylons);

    private static Func<string> Q1(GCJTestCase testCase)
    {
        testCase.Get(out int r, out int c);
        return () =>
        {
            int minDim = Math.Min(r, c);
            int maxDim = Math.Max(r, c);
            if (maxDim <= 3 || (minDim == 2 && maxDim == 4))
                return "IMPOSSIBLE";

            int n = r * c;
            int[][] adj = new int[n][];
            List<int> scratch = new(n);
            for (int u = 0; u < n; u++)
            {
                int ur = u / c;
                int uc = u % c;
                scratch.Clear();
                for (int v = 0; v < n; v++)
                {
                    int vr = v / c;
                    int vc = v % c;
                    if (ur != vr && uc != vc && ur - uc != vr - vc && ur + uc != vr + vc)
                        scratch.Add(v);
                }
                adj[u] = [.. scratch];
            }

            int[] path = new int[n];
            int[] deg = new int[n];
            bool[] used = new bool[n];
            Random rng = new(20190101 + r * 31 + c);

            for (int attempt = 0; attempt < 500; attempt++)
            {
                Array.Fill(used, false);
                for (int i = 0; i < n; i++)
                    deg[i] = adj[i].Length;

                int cur = attempt % n;
                bool ok = true;
                for (int step = 0; step < n; step++)
                {
                    path[step] = cur;
                    used[cur] = true;
                    foreach (int nxt in adj[cur])
                        deg[nxt]--;

                    if (step == n - 1)
                        break;

                    int bestNext = -1;
                    int bestDeg = int.MaxValue;
                    int bestTieRand = 0;
                    foreach (int nxt in adj[cur])
                    {
                        if (used[nxt])
                            continue;
                        int d = deg[nxt];
                        int tieRand = rng.Next();
                        if (d < bestDeg || (d == bestDeg && tieRand < bestTieRand))
                        {
                            bestDeg = d;
                            bestTieRand = tieRand;
                            bestNext = nxt;
                        }
                    }

                    if (bestNext < 0)
                    {
                        ok = false;
                        break;
                    }
                    cur = bestNext;
                }

                if (ok)
                {
                    StringBuilder sb = new();
                    sb.Append("POSSIBLE");
                    for (int i = 0; i < n; i++)
                    {
                        int cell = path[i];
                        sb.Append('\n').Append((cell / c) + 1).Append(' ').Append((cell % c) + 1);
                    }
                    return sb.ToString();
                }
            }

            throw new InvalidOperationException($"Failed to construct tour for {r}x{c}.");
        };
    }

    private static void ValidatePylons(string input, string expectedOutput, List<string> actualCases)
    {
        string[] inputLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        int numCases = int.Parse(inputLines[0], CultureInfo.InvariantCulture);
        Assert.AreEqual(numCases, actualCases.Count);

        bool[] expectedPossible = ParsePossibleFlags(expectedOutput, numCases);

        for (int tc = 0; tc < numCases; tc++)
        {
            string[] rcTokens = inputLines[tc + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int r = int.Parse(rcTokens[0], CultureInfo.InvariantCulture);
            int c = int.Parse(rcTokens[1], CultureInfo.InvariantCulture);

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
            Assert.AreEqual(1 + r * c, lines.Length, $"Case #{tc + 1}: wrong line count.");

            bool[,] visited = new bool[r + 1, c + 1];
            int prevR = -1, prevC = -1;
            for (int i = 1; i < lines.Length; i++)
            {
                string[] cell = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(2, cell.Length);
                int cr = int.Parse(cell[0], CultureInfo.InvariantCulture);
                int cc = int.Parse(cell[1], CultureInfo.InvariantCulture);
                Assert.That(cr, Is.InRange(1, r));
                Assert.That(cc, Is.InRange(1, c));
                Assert.IsFalse(visited[cr, cc], $"Case #{tc + 1}: revisited ({cr}, {cc}).");
                visited[cr, cc] = true;

                if (prevR != -1)
                {
                    Assert.AreNotEqual(prevR, cr);
                    Assert.AreNotEqual(prevC, cc);
                    Assert.AreNotEqual(prevR - prevC, cr - cc);
                    Assert.AreNotEqual(prevR + prevC, cr + cc);
                }

                prevR = cr;
                prevC = cc;
            }
        }
    }

    private static bool[] ParsePossibleFlags(string expectedOutput, int numCases)
    {
        bool[] flags = new bool[numCases];
        string[] lines = expectedOutput.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
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

    #region Q2 Golf Gophers

    [Test]
    public void Q2Small() => InteractiveTest(Q2Solve, io => Q2Judge(io, testNumber: 0));

    [Test]
    public void Q2Large() => InteractiveTest(Q2Solve, io => Q2Judge(io, testNumber: 1));

    private static readonly long[] GopherModuli = [5L, 7L, 9L, 11L, 13L, 16L, 17L];

    private static void Q2Solve(GCJInteractiveChannel io)
    {
        string[] header = io.ReadRequiredLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int t = int.Parse(header[0], CultureInfo.InvariantCulture);

        (long Remainder, long Modulus)[] congruences = new (long, long)[GopherModuli.Length];
        for (int tc = 0; tc < t; tc++)
        {
            for (int k = 0; k < GopherModuli.Length; k++)
            {
                long m = GopherModuli[k];
                string query = string.Join(" ", System.Linq.Enumerable.Repeat(m.ToString(CultureInfo.InvariantCulture), 18));
                io.WriteLine(query);
                string resp = io.ReadRequiredLine();
                if (resp == "-1")
                    throw new InvalidOperationException($"Case #{tc + 1}: judge returned -1 on night {k + 1}.");

                string[] parts = resp.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                long sum = 0;
                for (int i = 0; i < 18; i++)
                    sum += long.Parse(parts[i], CultureInfo.InvariantCulture);
                congruences[k] = (sum % m, m);
            }

            Assert.IsTrue(Modulo.TryChineseRemainder(congruences, out long gophers, out long lcm));
            if (gophers == 0)
                gophers = lcm;
            io.WriteLine(gophers.ToString(CultureInfo.InvariantCulture));
            string verdict = io.ReadRequiredLine();
            Assert.AreEqual("1", verdict, $"Case #{tc + 1}: wrong guess {gophers}.");
        }
    }

    private static void Q2Judge(GCJInteractiveChannel io, int testNumber)
    {
        const int maxG = 1_000_000;
        int[][] allCases =
        [
            [1, 2, 3, 4, 5, 7, 10, 11, 15, 50, 51, 55, 64, 81, 83, 88, 91, 98, 99, 100],
            [1, 2, 3, 5, 7, 10, 11, 81, 99, 100, 120, 121, 125, 128, 200, maxG / 2, maxG / 2 + 1, maxG / 2 + 2, maxG - 1, maxG]
        ];
        int[] qsLimits = [365, 7];
        int[] maxGophers = [100, maxG];

        Dictionary<int, Queue<int[]>> hardcoded = new()
        {
            [500000] = new([
                [27884, 27779, 27603, 27688, 27808, 27567, 28034, 27817, 28028, 27843, 27704, 27853, 27604, 27900, 27758, 27710, 27745, 27675],
                [27773, 27758, 27730, 27632, 27796, 27734, 27777, 28002, 27621, 27711, 28028, 27804, 27705, 27718, 27808, 27973, 27722, 27708],
                [27798, 27687, 27574, 27973, 27832, 27741, 27954, 27964, 27554, 27639, 27723, 27765, 27746, 27844, 27862, 27861, 27777, 27706],
                [27735, 27850, 27742, 27620, 27827, 27959, 27913, 27853, 27859, 27949, 27717, 27734, 27635, 27764, 27825, 27516, 27597, 27905],
                [27837, 28037, 27702, 27668, 27874, 27985, 27727, 27780, 27823, 27845, 27823, 27701, 27721, 27804, 27607, 27505, 27740, 27821],
                [27662, 27833, 27832, 27598, 27541, 28021, 27826, 27847, 27573, 27661, 27726, 27682, 27852, 27565, 28081, 27719, 28091, 27890],
                [28016, 27642, 27541, 27866, 27740, 27909, 27743, 27800, 27662, 27838, 27720, 27856, 27839, 28228, 27722, 27632, 27636, 27610]
            ]),
            [500001] = new([
                [27895, 27882, 27928, 27818, 28000, 27792, 27608, 27599, 27948, 27878, 27639, 27895, 27733, 27597, 27699, 27813, 27659, 27618],
                [27813, 28022, 28074, 28101, 27705, 27516, 27779, 27704, 27550, 27599, 27913, 27675, 27780, 27557, 27766, 27894, 27790, 27763],
                [27387, 27666, 27789, 28175, 27902, 27953, 27834, 27757, 27735, 27925, 27780, 27864, 27919, 27566, 27806, 27594, 27924, 27425],
                [27971, 27686, 27821, 27696, 28208, 27957, 27542, 27809, 27849, 27493, 27883, 27836, 27667, 27840, 27712, 27535, 27673, 27823],
                [28063, 27510, 27704, 27783, 27390, 27795, 27567, 27788, 28026, 27854, 27796, 27652, 27803, 28038, 27548, 27800, 28184, 27700],
                [27935, 27832, 27789, 27783, 27888, 28243, 27839, 27835, 27704, 27475, 27761, 27755, 27714, 27464, 27893, 27394, 27947, 27750],
                [27772, 27808, 27591, 27819, 27987, 27983, 27832, 27789, 27856, 27561, 27669, 27709, 27817, 28056, 27703, 27512, 27696, 27841]
            ]),
            [500002] = new([
                [27723, 27796, 27740, 27725, 27843, 27728, 27737, 27540, 28173, 27786, 27609, 27972, 27676, 28074, 27656, 27872, 27724, 27628],
                [27569, 27471, 27855, 27917, 28025, 27654, 27665, 27888, 27728, 28092, 27966, 27737, 27855, 27842, 27540, 27686, 27677, 27835],
                [27881, 27690, 27750, 27711, 27888, 27817, 27780, 27860, 27833, 27882, 27850, 27669, 27893, 27347, 27421, 27905, 27970, 27855],
                [27471, 27710, 27701, 27825, 27750, 27985, 27725, 28093, 27861, 27859, 27599, 27832, 27666, 27882, 27915, 27651, 27829, 27648],
                [27955, 27701, 27857, 28065, 27802, 27672, 27976, 27845, 27723, 27588, 27579, 27735, 27846, 27653, 27776, 27798, 27872, 27559],
                [27654, 27800, 27820, 27817, 27689, 27394, 27941, 27720, 27719, 27658, 27954, 27989, 27735, 27954, 27793, 27970, 27535, 27860],
                [27595, 27951, 27788, 27567, 27411, 27775, 27643, 27847, 28044, 28057, 27721, 27911, 27760, 27737, 27880, 28030, 27641, 27644]
            ]),
            [999999] = new([
                [55656, 55449, 55279, 55737, 55882, 56192, 55816, 55667, 55241, 55309, 55244, 55186, 55044, 55779, 55475, 55913, 55500, 55630],
                [55396, 55855, 55497, 55814, 55625, 55685, 55253, 55552, 55270, 55895, 55258, 55299, 55916, 55834, 55595, 55523, 55328, 55404],
                [55732, 55527, 55636, 55727, 55646, 55620, 55607, 55942, 55408, 55269, 55631, 55717, 55691, 55470, 55322, 55340, 55221, 55493],
                [55600, 55502, 55648, 55753, 55941, 55525, 55586, 55501, 55220, 55165, 55684, 56197, 56011, 55375, 55744, 54919, 55542, 55086],
                [55678, 55768, 55750, 55629, 55529, 55796, 55005, 55570, 55736, 55336, 55400, 55537, 55559, 55547, 55375, 56226, 55059, 55499],
                [55747, 55622, 54957, 55354, 55971, 55687, 55353, 55618, 55851, 55070, 55659, 55732, 55816, 55593, 55663, 55514, 55525, 55267],
                [55738, 55692, 56066, 55141, 55780, 55679, 55309, 55871, 55344, 55299, 55616, 55488, 55481, 55343, 55745, 55306, 55389, 55712]
            ]),
            [1000000] = new([
                [55467, 55478, 55443, 55509, 56050, 55606, 55246, 55284, 55644, 55548, 55895, 55433, 55961, 55452, 55367, 55555, 55588, 55474],
                [55557, 55489, 55346, 55308, 55320, 55309, 55468, 55926, 55682, 55873, 55444, 55715, 55368, 55266, 55737, 55808, 55858, 55526],
                [55557, 55467, 55280, 55724, 55629, 55944, 55773, 55619, 55524, 55176, 55304, 55394, 55523, 55350, 55664, 55207, 55944, 55921],
                [55711, 55999, 55946, 55109, 55888, 55588, 55613, 55473, 55785, 55485, 55577, 55420, 55328, 55583, 55021, 55678, 55606, 55190],
                [55586, 55705, 55692, 55600, 55899, 55346, 55579, 55124, 55694, 55748, 55555, 55641, 55247, 55086, 55467, 55919, 55826, 55286],
                [55272, 55412, 55539, 55615, 55371, 55657, 55534, 55710, 55387, 55611, 55448, 55837, 55370, 55610, 56037, 55661, 55461, 55468],
                [55687, 55605, 55479, 55573, 56097, 55500, 55280, 55777, 55548, 55935, 55274, 55264, 27633 * 2, 55519, 55775, 55372, 55337, 55712]
            ])
        };

        Random rng = new(313122 + testNumber);
        int[] cases = (int[])allCases[testNumber].Clone();
        rng.Shuffle(cases);
        int qs = qsLimits[testNumber];
        int mg = maxGophers[testNumber];

        io.WriteLine($"{cases.Length} {qs} {mg}");
        for (int tc = 0; tc < cases.Length; tc++)
        {
            int g = cases[tc];
            bool solved = false;
            for (int ex = 0; ex <= qs; ex++)
            {
                string line = io.ReadRequiredLine();
                string[] tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                Assert.IsTrue(tokens.Length is 1 or 18, $"Case #{tc + 1}: expected 1 or 18 tokens.");

                if (tokens.Length == 18)
                {
                    Assert.Less(ex, qs, $"Case #{tc + 1}: exceeded {qs} queries.");
                    int[] blades = new int[18];
                    for (int i = 0; i < 18; i++)
                    {
                        blades[i] = int.Parse(tokens[i], CultureInfo.InvariantCulture);
                        Assert.That(blades[i], Is.InRange(2, 18));
                    }

                    int[] choices;
                    if (hardcoded.TryGetValue(g, out var queue) && queue.Count > 0)
                    {
                        choices = (int[])queue.Dequeue().Clone();
                    }
                    else
                    {
                        choices = new int[18];
                        for (int k = 0; k < g; k++)
                            choices[rng.Next(18)]++;
                    }

                    for (int i = 0; i < 18; i++)
                        choices[i] %= blades[i];
                    io.WriteLine(string.Join(" ", choices));
                }
                else
                {
                    int guess = int.Parse(tokens[0], CultureInfo.InvariantCulture);
                    Assert.That(guess, Is.InRange(1, mg));
                    Assert.AreEqual(g, guess, $"Case #{tc + 1}: wrong guess {guess} != {g}.");
                    io.WriteLine("1");
                    solved = true;
                    break;
                }
            }
            Assert.IsTrue(solved);
        }

        Assert.IsNull(io.ReadLine(), "Additional input after all cases finish.");
    }

    #endregion

    #region Q3 Alien Rhyme

    [Test]
    public void Q3Sample() => Test(Q3);

    [Test]
    public void Q3Small() => Test(Q3);

    [Test]
    public void Q3Large() => Test(Q3);

    private static Func<string> Q3(GCJTestCase testCase)
    {
        testCase.Get(out int n);
        testCase.GetLines(n, out string[] words);
        return () =>
        {
            int maxNodes = 1;
            foreach (string w in words)
                maxNodes += w.Length;

            int[,] next = new int[maxNodes, 26];
            for (int i = 0; i < maxNodes; i++)
            {
                for (int c = 0; c < 26; c++)
                    next[i, c] = -1;
            }
            bool[] isEnd = new bool[maxNodes];
            int nodeCount = 1;

            foreach (string w in words)
            {
                int cur = 0;
                for (int i = w.Length - 1; i >= 0; i--)
                {
                    int ch = w[i] - 'A';
                    int nxt = next[cur, ch];
                    if (nxt < 0)
                    {
                        nxt = nodeCount++;
                        next[cur, ch] = nxt;
                    }
                    cur = nxt;
                }
                isEnd[cur] = true;
            }

            int Dfs(int u)
            {
                int rem = isEnd[u] ? 1 : 0;
                for (int c = 0; c < 26; c++)
                {
                    int v = next[u, c];
                    if (v >= 0)
                        rem += Dfs(v);
                }
                if (u != 0 && rem >= 2)
                    rem -= 2;
                return rem;
            }

            int unpaired = Dfs(0);
            return (n - unpaired).ToString(CultureInfo.InvariantCulture);
        };
    }

    #endregion
}

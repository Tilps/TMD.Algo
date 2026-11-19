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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2009
{
    [TestFixture]
    public class FinalsTest : BaseGCJTest
    {
        #region Question 1 - Year of More Code Jam

        private static Func<string> Q1Solver(GCJTestCase test)
        {
            test.Get(out long n, out int t);
            int[][] schedules = new int[t][];
            for (int i = 0; i < t; i++)
            {
                test.Get(out schedules[i]);
            }
            return () => Q1Solver(n, t, schedules);
        }

        private static string Q1Solver(long n, int t, int[][] schedules)
        {
            int maxDay = (int)Math.Min(n, 10000);
            List<int>[] dayEvents = new List<int>[maxDay + 1];

            for (int j = 0; j < t; j++)
            {
                (dayEvents[1] ??= []).Add(j);
                int[] sched = schedules[j];
                for (int k = 1; k < sched.Length; k++)
                {
                    int day = sched[k];
                    if (day <= maxDay)
                    {
                        (dayEvents[day] ??= []).Add(j);
                    }
                }
            }

            int[] c = new int[t];
            long s1 = 0;
            long s2 = 0;
            BigInteger totalNumerator = BigInteger.Zero;

            for (int day = 1; day <= maxDay; day++)
            {
                List<int>? events = dayEvents[day];
                if (events != null)
                {
                    foreach (int j in events)
                    {
                        s1 += 1;
                        s2 += 2L * c[j] + 1;
                        c[j]++;
                    }
                }
                totalNumerator += (BigInteger)n * s1 + (BigInteger)s1 * s1 - s2;
            }

            if (n > maxDay)
            {
                BigInteger tailPerDay = (BigInteger)n * s1 + (BigInteger)s1 * s1 - s2;
                totalNumerator += (n - maxDay) * tailPerDay;
            }

            BigInteger den = (BigInteger)n * n;
            BigInteger kWhole = totalNumerator / den;
            long remainder = (long)(totalNumerator % den);

            if (remainder == 0)
                return $"{kWhole}+0/1";

            var frac = new Fraction(remainder, (long)den);
            return $"{kWhole}+{frac.Numerator}/{frac.Denominator}";
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

        #region Question 2 - Min Perimeter

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out (int X, int Y)[] raw);
            return () => Q2Solver(raw);
        }

        private static string Q2Solver((int X, int Y)[] raw)
        {
            Point2D[] points = new Point2D[raw.Length];
            for (int i = 0; i < raw.Length; i++)
            {
                points[i] = new Point2D(raw[i].X, raw[i].Y);
            }
            return Geometry.MinimumTrianglePerimeter(points).ToString("G15", CultureInfo.InvariantCulture);
        }

        private sealed class ConcatenatedReadStream : Stream
        {
            private readonly Stream[] streams;
            private int index;

            public ConcatenatedReadStream(Stream[] streams)
            {
                this.streams = streams;
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                while (index < streams.Length)
                {
                    int read = streams[index].Read(buffer, offset, count);
                    if (read > 0)
                        return read;
                    index++;
                }
                return 0;
            }

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    foreach (Stream s in streams)
                        s.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        private static string? FindMinPerimeterLargeDir()
        {
            string relPath = Path.Combine(
                "coding-competitions-archive", "codejam", "2009", "world_finals", "min_perimeter", "data", "secret", "subtask2");

            foreach (string start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                DirectoryInfo? dir = new DirectoryInfo(start);
                while (dir != null)
                {
                    string candidate = Path.Combine(dir.FullName, relPath);
                    if (Directory.Exists(candidate))
                        return candidate;
                    dir = dir.Parent;
                }
            }
            return null;
        }

        [Test]
        public void Q2Sample()
        {
            Test(Q2Solver, 1e-5);
        }

        [Test]
        public void Q2Small()
        {
            SlowTest(Q2Solver, 1e-5);
        }

        [Test]
        public void Q2Large()
        {
            string? partDir = FindMinPerimeterLargeDir();
            if (partDir == null)
            {
                Assert.Ignore("Local archive split files not present.");
                return;
            }

            string[] partFiles = Directory.GetFiles(partDir, "1.in.part*");
            Array.Sort(partFiles, StringComparer.Ordinal);
            if (partFiles.Length == 0)
            {
                Assert.Ignore("Local archive split files not present.");
                return;
            }

            Point2D[][] cases;
            FileStream[] fileStreams = new FileStream[partFiles.Length];
            for (int i = 0; i < partFiles.Length; i++)
                fileStreams[i] = File.OpenRead(partFiles[i]);

            using (var concat = new ConcatenatedReadStream(fileStreams))
            using (var reader = new StreamReader(concat))
            {
                string? headerLine = reader.ReadLine();
                while (string.IsNullOrEmpty(headerLine))
                    headerLine = reader.ReadLine();

                int testCount = int.Parse(headerLine.AsSpan().Trim(), CultureInfo.InvariantCulture);
                cases = new Point2D[testCount][];

                for (int tc = 0; tc < testCount; tc++)
                {
                    string? nLine = reader.ReadLine();
                    while (string.IsNullOrEmpty(nLine))
                        nLine = reader.ReadLine();

                    int n = int.Parse(nLine.AsSpan().Trim(), CultureInfo.InvariantCulture);
                    Point2D[] pts = new Point2D[n];
                    for (int i = 0; i < n; i++)
                    {
                        string? ptLine = reader.ReadLine();
                        while (string.IsNullOrEmpty(ptLine))
                            ptLine = reader.ReadLine();

                        ReadOnlySpan<char> span = ptLine.AsSpan().Trim();
                        int sp = span.IndexOf(' ');
                        int x = int.Parse(span[..sp], CultureInfo.InvariantCulture);
                        int y = int.Parse(span[(sp + 1)..], CultureInfo.InvariantCulture);
                        pts[i] = new Point2D(x, y);
                    }
                    cases[tc] = pts;
                }
            }

            string[] outputs = new string[cases.Length];
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Min(4, Environment.ProcessorCount)
            };
            Parallel.For(0, cases.Length, parallelOptions, tc =>
            {
                double perim = Geometry.MinimumTrianglePerimeter(cases[tc]);
                outputs[tc] = $"Case #{tc + 1}: {perim.ToString("G15", CultureInfo.InvariantCulture)}";
            });

            string expected = Load("Q2LargeOutput.txt");
            Validate(expected, [.. outputs], 1e-5);
        }

        #endregion

        #region Question 3 - Doubly-sorted Grid

        private sealed class GridProfileTable
        {
            public required int ProfileCount { get; init; }
            public required byte[] RowLengthsFlat { get; init; }
            public required int[,] Comb { get; init; }
        }

        private static readonly ConcurrentDictionary<(int R, int C), GridProfileTable> ProfileCache = new();

        private static GridProfileTable GetProfileTable(int rows, int cols)
        {
            return ProfileCache.GetOrAdd((rows, cols), static key =>
            {
                var (r, c) = key;
                int maxN = r + c + 1;
                int[,] comb = new int[maxN + 1, maxN + 1];
                for (int i = 0; i <= maxN; i++)
                {
                    comb[i, 0] = 1;
                    comb[i, i] = 1;
                    for (int j = 1; j < i; j++)
                        comb[i, j] = comb[i - 1, j] + comb[i - 1, j - 1];
                }

                int count = comb[r + c, r];
                byte[] flat = new byte[count * r];
                byte[] cur = new byte[r];

                void Generate(int rowIdx, int prevMax)
                {
                    if (rowIdx == r)
                    {
                        int rank = 0;
                        for (int k = 0; k < r; k++)
                        {
                            int len = cur[r - 1 - k];
                            rank += comb[len + k, k + 1];
                        }
                        Buffer.BlockCopy(cur, 0, flat, rank * r, r);
                        return;
                    }

                    for (int val = 0; val <= prevMax; val++)
                    {
                        cur[rowIdx] = (byte)val;
                        Generate(rowIdx + 1, val);
                    }
                }

                Generate(0, c);
                return new GridProfileTable
                {
                    ProfileCount = count,
                    RowLengthsFlat = flat,
                    Comb = comb
                };
            });
        }

        private static Func<int> Q3Solver(GCJTestCase test)
        {
            test.Get(out int r, out int c);
            test.GetLines(r, out string[] grid);
            return () => Q3Solver(r, c, grid);
        }

        private static int Q3Solver(int rows, int cols, string[] grid)
        {
            const int Mod = 10007;
            GridProfileTable table = GetProfileTable(rows, cols);
            int count = table.ProfileCount;
            byte[] rowLens = table.RowLengthsFlat;
            int[,] comb = table.Comb;

            int[] dp = new int[count];
            dp[0] = 1;

            for (char ch = 'a'; ch <= 'z'; ch++)
            {
                for (int r = 0; r < rows; r++)
                {
                    int shiftWeightRow = rows - 1 - r;
                    for (int id = 0; id < count; id++)
                    {
                        int ways = dp[id];
                        if (ways == 0)
                            continue;

                        int baseIdx = id * rows;
                        int len = rowLens[baseIdx + r];
                        int upper = r == 0 ? cols : rowLens[baseIdx + r - 1];
                        if (len >= upper)
                            continue;

                        char cell = grid[r][len];
                        if (cell != '.' && cell != ch)
                            continue;

                        int nextId = id + comb[len + shiftWeightRow, shiftWeightRow];
                        int sum = dp[nextId] + ways;
                        dp[nextId] = sum >= Mod ? sum - Mod : sum;
                    }
                }
            }

            return dp[count - 1];
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
            SlowTest(Q3Solver);
        }

        #endregion

        #region Question 4 - Wi-fi Towers

        private static Func<int> Q4Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out (int X, int Y, int R, int S)[] towers);
            return () => Q4Solver(towers);
        }

        private static int Q4Solver((int X, int Y, int R, int S)[] towers)
        {
            int n = towers.Length;
            int source = n;
            int sink = n + 1;
            var flow = new MaxFlow(n + 2);

            int positiveSum = 0;
            for (int i = 0; i < n; i++)
            {
                if (towers[i].S > 0)
                    positiveSum += towers[i].S;
            }
            int inf = positiveSum + 1;

            for (int i = 0; i < n; i++)
            {
                var (xi, yi, ri, si) = towers[i];
                if (si > 0)
                {
                    flow.AddDirectedEdge(source, i, si);
                }
                else if (si < 0)
                {
                    flow.AddDirectedEdge(i, sink, -si);
                }

                long rSq = (long)ri * ri;
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                        continue;

                    long dx = xi - towers[j].X;
                    long dy = yi - towers[j].Y;
                    if (dx * dx + dy * dy <= rSq)
                    {
                        flow.AddDirectedEdge(i, j, inf);
                    }
                }
            }

            return positiveSum - flow.ComputeMaxFlow(source, sink);
        }

        [Test]
        public void Q4Sample()
        {
            Test(Q4Solver);
        }

        [Test]
        public void Q4Small()
        {
            Test(Q4Solver);
        }

        [Test]
        public void Q4Large()
        {
            Test(Q4Solver);
        }

        #endregion

        #region Question 5 - Marbles

        private static Func<int> Q5Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.Get(out string[] tokens);
            return () => Q5Solver(n, tokens);
        }

        private static int Q5Solver(int n, string[] tokens)
        {
            const int Inf = 1_000_000_000;
            int totalPos = 2 * n;

            Dictionary<string, int> colorIds = [with(capacity: n)];
            int[] marbles = new int[totalPos];
            int[] pos0 = new int[n];
            int[] pos1 = new int[n];
            Array.Fill(pos0, -1);

            for (int i = 0; i < totalPos; i++)
            {
                if (!colorIds.TryGetValue(tokens[i], out int id))
                {
                    id = colorIds.Count;
                    colorIds[tokens[i]] = id;
                }
                marbles[i] = id;
                if (pos0[id] < 0)
                    pos0[id] = i;
                else
                    pos1[id] = i;
            }

            List<int>[] crossAdj = new List<int>[n];
            for (int c = 0; c < n; c++)
                crossAdj[c] = [];

            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if ((pos0[i] < pos0[j] && pos0[j] < pos1[i] && pos1[i] < pos1[j]) ||
                        (pos0[j] < pos0[i] && pos0[i] < pos1[j] && pos1[j] < pos1[i]))
                    {
                        crossAdj[i].Add(j);
                        crossAdj[j].Add(i);
                    }
                }
            }

            (int Pos, int EventType)[]?[] cachedEvents = new (int Pos, int EventType)[totalPos][];
            int[] vis = new int[n];

            bool DfsColor(int u, int sign)
            {
                if (vis[u] == sign)
                    return true;
                if (vis[u] == -sign)
                    return false;

                vis[u] = sign;
                foreach (int v in crossAdj[u])
                {
                    if (!DfsColor(v, -sign))
                        return false;
                }
                return true;
            }

            (int Pos, int EventType)[]? GetComponentEvents(int startX)
            {
                if (cachedEvents[startX] != null)
                    return cachedEvents[startX]!;

                Array.Clear(vis);
                if (!DfsColor(marbles[startX], 1))
                    return null;

                List<(int Pos, int EventType)> events = [];
                for (int x = 0; x < totalPos; x++)
                {
                    int m = marbles[x];
                    if (vis[m] == 0)
                        continue;

                    int endIdx = pos0[m] == x ? 0 : 1;
                    int type = (1 - vis[m]) + endIdx;
                    events.Add((x, type));
                }

                var result = events.ToArray();
                cachedEvents[startX] = result;
                return result;
            }

            // Pre-validate bipartiteness on every component and cache events cleanly.
            for (int x = 0; x < totalPos; x++)
            {
                if (GetComponentEvents(x) == null)
                    return -1;
            }

            int[,] memoH2 = new int[totalPos, n + 1];
            for (int i = 0; i < totalPos; i++)
            {
                for (int h = 0; h <= n; h++)
                    memoH2[i, h] = -1;
            }

            int CalcH2(int a, int b, int h1)
            {
                if (h1 < 0)
                    return Inf;
                if (a == b)
                    return 0;

                int cached = memoH2[a, h1];
                if (cached != -1)
                    return cached;

                var events = cachedEvents[a]!;
                int best = Inf;

                for (int mask = 0; mask <= 2; mask += 2)
                {
                    int top = 0;
                    int bot = 0;
                    int h2 = 0;

                    for (int i = 0; i <= events.Length; i++)
                    {
                        int alpha = i == 0 ? a : events[i - 1].Pos + 1;
                        int beta = i == events.Length ? b : events[i].Pos;
                        int sub = CalcH2(alpha, beta, h1 - top) + bot;
                        if (sub > h2)
                            h2 = sub;

                        if (h2 >= Inf)
                            break;

                        if (i < events.Length)
                        {
                            switch (events[i].EventType ^ mask)
                            {
                                case 0: top++; break;
                                case 1: top--; break;
                                case 2: bot++; break;
                                case 3: bot--; break;
                            }
                        }
                    }

                    if (h2 < best)
                        best = h2;
                }

                memoH2[a, h1] = best;
                return best;
            }

            int minHeight = Inf;
            for (int h1 = 0; h1 <= n; h1++)
            {
                int totalHeight = h1 + CalcH2(0, totalPos, h1);
                if (totalHeight < minHeight)
                    minHeight = totalHeight;
            }

            return minHeight >= Inf ? -1 : minHeight;
        }

        [Test]
        public void Q5Sample()
        {
            Test(Q5Solver);
        }

        [Test]
        public void Q5Small()
        {
            Test(Q5Solver);
        }

        [Test]
        public void Q5Large()
        {
            SlowTest(Q5Solver);
        }

        #endregion

        #region Question 6 - Lights

        private sealed class LightWedge
        {
            public required Point2D[] Triangle { get; init; }
            public required int PillarIdx { get; init; }
            public required double MinX { get; init; }
            public required double MaxX { get; init; }
            public required double MinY { get; init; }
            public required double MaxY { get; init; }
        }

        private static Func<string[]> Q6Solver(GCJTestCase test)
        {
            test.Get(out int rx, out int ry);
            test.Get(out int gx, out int gy);
            test.Get(out int n);
            test.GetLines(n, out (int X, int Y, int R)[] pillars);
            return () => Q6Solver(new Point2D(rx, ry), new Point2D(gx, gy), pillars);
        }

        private static string[] Q6Solver(Point2D redLight, Point2D greenLight, (int X, int Y, int R)[] pillars)
        {
            Point2D[] corners = [new(0, 0), new(100, 0), new(100, 100), new(0, 100)];
            Point2D[] pillarCenters = new Point2D[pillars.Length];
            for (int i = 0; i < pillars.Length; i++)
                pillarCenters[i] = new Point2D(pillars[i].X, pillars[i].Y);

            static double NormalizeAngle(double angle)
            {
                while (angle > Math.PI)
                    angle -= 2.0 * Math.PI;
                while (angle < -Math.PI)
                    angle += 2.0 * Math.PI;
                return angle;
            }

            List<LightWedge> BuildWedges(Point2D light, out double illuminatedArea)
            {
                List<double> angles = [-Math.PI, Math.PI];
                foreach (Point2D corner in corners)
                {
                    angles.Add(Math.Atan2(corner.Y - light.Y, corner.X - light.X));
                }

                for (int i = 0; i < pillars.Length; i++)
                {
                    double dx = pillarCenters[i].X - light.X;
                    double dy = pillarCenters[i].Y - light.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    double centerAngle = Math.Atan2(dy, dx);
                    double ratio = Math.Clamp(pillars[i].R / dist, -1.0, 1.0);
                    double span = Math.Asin(ratio);
                    angles.Add(NormalizeAngle(centerAngle - span));
                    angles.Add(NormalizeAngle(centerAngle + span));
                }

                angles.Sort();
                List<double> uniqueAngles = [with(capacity: angles.Count)];
                foreach (double a in angles)
                {
                    if (uniqueAngles.Count == 0 || a - uniqueAngles[^1] > 1e-12)
                    {
                        uniqueAngles.Add(a);
                    }
                }

                int HitTargetAtAngle(double theta)
                {
                    double dx = Math.Cos(theta);
                    double dy = Math.Sin(theta);
                    double bestT = double.PositiveInfinity;

                    if (dx > 1e-12)
                        bestT = Math.Min(bestT, (100.0 - light.X) / dx);
                    else if (dx < -1e-12)
                        bestT = Math.Min(bestT, -light.X / dx);

                    if (dy > 1e-12)
                        bestT = Math.Min(bestT, (100.0 - light.Y) / dy);
                    else if (dy < -1e-12)
                        bestT = Math.Min(bestT, -light.Y / dy);

                    int bestPillar = -1;
                    for (int i = 0; i < pillars.Length; i++)
                    {
                        double ox = light.X - pillarCenters[i].X;
                        double oy = light.Y - pillarCenters[i].Y;
                        double b = ox * dx + oy * dy;
                        if (b >= 0.0)
                            continue;

                        double c = ox * ox + oy * oy - (double)pillars[i].R * pillars[i].R;
                        double disc = b * b - c;
                        if (disc >= 0.0)
                        {
                            double tHit = -b - Math.Sqrt(disc);
                            if (tHit > 0.0 && tHit < bestT)
                            {
                                bestT = tHit;
                                bestPillar = i;
                            }
                        }
                    }

                    return bestPillar;
                }

                Point2D RayHitWall(double theta)
                {
                    double dx = Math.Cos(theta);
                    double dy = Math.Sin(theta);
                    double bestT = double.PositiveInfinity;

                    if (dx > 1e-12)
                        bestT = Math.Min(bestT, (100.0 - light.X) / dx);
                    else if (dx < -1e-12)
                        bestT = Math.Min(bestT, -light.X / dx);

                    if (dy > 1e-12)
                        bestT = Math.Min(bestT, (100.0 - light.Y) / dy);
                    else if (dy < -1e-12)
                        bestT = Math.Min(bestT, -light.Y / dy);

                    return new Point2D(light.X + bestT * dx, light.Y + bestT * dy);
                }

                Point2D RayHitPillar(double theta, int pillarIdx)
                {
                    double dx = Math.Cos(theta);
                    double dy = Math.Sin(theta);
                    double ox = light.X - pillarCenters[pillarIdx].X;
                    double oy = light.Y - pillarCenters[pillarIdx].Y;
                    double b = ox * dx + oy * dy;
                    double c = ox * ox + oy * oy - (double)pillars[pillarIdx].R * pillars[pillarIdx].R;
                    double disc = Math.Max(0.0, b * b - c);
                    double tHit = -b - Math.Sqrt(disc);
                    return new Point2D(light.X + tHit * dx, light.Y + tHit * dy);
                }

                List<LightWedge> wedges = [with(capacity: uniqueAngles.Count)];
                illuminatedArea = 0.0;

                for (int i = 0; i + 1 < uniqueAngles.Count; i++)
                {
                    double a = uniqueAngles[i];
                    double b = uniqueAngles[i + 1];
                    if (b - a <= 1e-12)
                        continue;

                    double mid = 0.5 * (a + b);
                    int pillarIdx = HitTargetAtAngle(mid);
                    Point2D p1 = pillarIdx >= 0 ? RayHitPillar(a, pillarIdx) : RayHitWall(a);
                    Point2D p2 = pillarIdx >= 0 ? RayHitPillar(b, pillarIdx) : RayHitWall(b);

                    Point2D[] tri = [light, p1, p2];
                    double area = Geometry.PolygonArea(tri);
                    if (area <= 1e-14)
                        continue;

                    if (pillarIdx >= 0)
                    {
                        area -= Geometry.CirclePolygonIntersectionArea(
                            pillarCenters[pillarIdx], pillars[pillarIdx].R, tri);
                    }
                    illuminatedArea += area;

                    wedges.Add(new LightWedge
                    {
                        Triangle = tri,
                        PillarIdx = pillarIdx,
                        MinX = Math.Min(light.X, Math.Min(p1.X, p2.X)),
                        MaxX = Math.Max(light.X, Math.Max(p1.X, p2.X)),
                        MinY = Math.Min(light.Y, Math.Min(p1.Y, p2.Y)),
                        MaxY = Math.Max(light.Y, Math.Max(p1.Y, p2.Y))
                    });
                }

                return wedges;
            }

            List<LightWedge> redWedges = BuildWedges(redLight, out double redTotal);
            List<LightWedge> greenWedges = BuildWedges(greenLight, out double greenTotal);

            double yellowArea = 0.0;
            foreach (LightWedge rw in redWedges)
            {
                foreach (LightWedge gw in greenWedges)
                {
                    if (rw.MaxX < gw.MinX - 1e-9 || gw.MaxX < rw.MinX - 1e-9 ||
                        rw.MaxY < gw.MinY - 1e-9 || gw.MaxY < rw.MinY - 1e-9)
                    {
                        continue;
                    }

                    List<Point2D> inter = Geometry.ConvexPolygonIntersection(rw.Triangle, gw.Triangle);
                    if (inter.Count < 3)
                        continue;

                    double subArea = Geometry.PolygonArea(inter);
                    if (rw.PillarIdx >= 0)
                    {
                        subArea -= Geometry.CirclePolygonIntersectionArea(
                            pillarCenters[rw.PillarIdx], pillars[rw.PillarIdx].R, inter);
                    }
                    if (gw.PillarIdx >= 0 && gw.PillarIdx != rw.PillarIdx)
                    {
                        subArea -= Geometry.CirclePolygonIntersectionArea(
                            pillarCenters[gw.PillarIdx], pillars[gw.PillarIdx].R, inter);
                    }
                    if (subArea > 0.0)
                        yellowArea += subArea;
                }
            }

            double pillarArea = 0.0;
            foreach (var p in pillars)
            {
                pillarArea += Math.PI * p.R * p.R;
            }

            double freeArea = 10000.0 - pillarArea;
            double redOnly = Math.Max(0.0, redTotal - yellowArea);
            double greenOnly = Math.Max(0.0, greenTotal - yellowArea);
            double blackArea = Math.Max(0.0, freeArea - redTotal - greenTotal + yellowArea);

            return
            [
                blackArea.ToString("G12", CultureInfo.InvariantCulture),
                redOnly.ToString("G12", CultureInfo.InvariantCulture),
                greenOnly.ToString("G12", CultureInfo.InvariantCulture),
                yellowArea.ToString("G12", CultureInfo.InvariantCulture)
            ];
        }

        [Test]
        public void Q6Sample()
        {
            Test(Q6Solver, 1e-5);
        }

        [Test]
        public void Q6Small()
        {
            Test(Q6Solver, 1e-5);
        }

        [Test]
        public void Q6Large()
        {
            SlowTest(Q6Solver, 1e-5);
        }

        #endregion
    }
}

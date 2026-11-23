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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2017
{
    [TestFixture]
    public class R2Test : BaseGCJTest
    {
        #region Question 1 - Fresh Chocolate

        private static Func<int> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n, out int p);
            test.Get(out int[] groups);
            return () => Q1Solver(n, p, groups);
        }

        private static int Q1Solver(int n, int p, int[] groups)
        {
            int[] rem = new int[p];
            for (int i = 0; i < n; i++)
                rem[groups[i] % p]++;

            int ans = rem[0];
            if (p == 2)
            {
                ans += (rem[1] + 1) / 2;
            }
            else if (p == 3)
            {
                int pairs = Math.Min(rem[1], rem[2]);
                ans += pairs + (Math.Abs(rem[1] - rem[2]) + 2) / 3;
            }
            else if (p == 4)
            {
                int pairs13 = Math.Min(rem[1], rem[3]);
                int pairs22 = rem[2] / 2;
                int left2 = rem[2] & 1;
                int left13 = Math.Abs(rem[1] - rem[3]);
                int extra = 0;
                if (left2 == 1)
                {
                    extra = 1;
                    left13 = Math.Max(0, left13 - 2);
                }
                ans += pairs13 + pairs22 + extra + (left13 + 3) / 4;
            }
            return ans;
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

        #region Question 2 - Roller Coaster Scheduling

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int n, out int c, out int m);
            test.GetLines(m, out int[] pos, out int[] buyer);
            return () => Q2Solver(n, c, m, pos, buyer);
        }

        private static string Q2Solver(int n, int c, int m, int[] pos, int[] buyer)
        {
            int[] posCnt = new int[n + 1];
            int[] buyerCnt = new int[c + 1];

            for (int i = 0; i < m; i++)
            {
                posCnt[pos[i]]++;
                buyerCnt[buyer[i]]++;
            }

            int rides = 0;
            for (int b = 1; b <= c; b++)
            {
                if (buyerCnt[b] > rides)
                    rides = buyerCnt[b];
            }

            int prefSum = 0;
            for (int p = 1; p <= n; p++)
            {
                prefSum += posCnt[p];
                int ceilDiv = (prefSum + p - 1) / p;
                if (ceilDiv > rides)
                    rides = ceilDiv;
            }

            int promos = 0;
            for (int p = 1; p <= n; p++)
            {
                if (posCnt[p] > rides)
                    promos += posCnt[p] - rides;
            }

            return $"{rides} {promos}";
        }

        [Test]
        public void Q2Sample()
        {
            Test(Q2Solver);
        }

        [Test]
        public void Q2Small()
        {
            Test(Q2Solver);
        }

        [Test]
        public void Q2Large()
        {
            Test(Q2Solver);
        }

        #endregion

        #region Question 3 - Beaming With Joy

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int r, out int c);
            test.GetLines(r, out string[] grid);
            return () => Q3Solver(r, c, grid);
        }

        private static readonly int[] BeamDr = [-1, 0, 1, 0]; // U, R, D, L
        private static readonly int[] BeamDc = [0, 1, 0, -1];

        private static string Q3Solver(int r, int c, string[] grid)
        {
            List<(int R, int C)> shooters = [];
            int[,] shooterId = new int[r, c];
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    if (grid[i][j] == '-' || grid[i][j] == '|')
                    {
                        shooterId[i, j] = shooters.Count;
                        shooters.Add((i, j));
                    }
                    else
                    {
                        shooterId[i, j] = -1;
                    }
                }
            }

            int k = shooters.Count;
            TwoSat sat = new(k);
            bool[,] validOrient = new bool[k, 2]; // 0 = '-', 1 = '|'
            int[,,] rayVis = new int[r, c, 4];
            int[,] cellVis = new int[r, c];
            int stamp = 0;
            int cellStamp = 0;

            bool TraceRay(int startR, int startC, int startDir, int sIdx, List<(int R, int C)> hitEmpty, bool record)
            {
                stamp++;
                int rr = startR + BeamDr[startDir];
                int cc = startC + BeamDc[startDir];
                int dir = startDir;

                while (rr >= 0 && rr < r && cc >= 0 && cc < c)
                {
                    char ch = grid[rr][cc];
                    if (ch == '#')
                        return true;
                    if (ch == '-' || ch == '|')
                        return false;
                    if (rayVis[rr, cc, dir] == stamp)
                        return true;
                    rayVis[rr, cc, dir] = stamp;

                    if (ch == '/')
                    {
                        dir ^= 1;
                    }
                    else if (ch == '\\')
                    {
                        dir = 3 - dir;
                    }
                    else if (record && cellVis[rr, cc] != cellStamp)
                    {
                        cellVis[rr, cc] = cellStamp;
                        hitEmpty.Add((rr, cc));
                    }

                    rr += BeamDr[dir];
                    cc += BeamDc[dir];
                }
                return true;
            }

            List<(int Shooter, bool IsVert)>[,] coverers = new List<(int, bool)>[r, c];
            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    if (grid[i][j] == '.')
                        coverers[i, j] = [];

            List<(int R, int C)> tempEmpty = [];
            for (int s = 0; s < k; s++)
            {
                var (sr, sc) = shooters[s];
                for (int orient = 0; orient < 2; orient++)
                {
                    int d1 = orient == 0 ? 3 : 0;
                    int d2 = orient == 0 ? 1 : 2;
                    bool ok = TraceRay(sr, sc, d1, s, tempEmpty, record: false) &&
                              TraceRay(sr, sc, d2, s, tempEmpty, record: false);
                    validOrient[s, orient] = ok;
                    if (!ok)
                        sat.SetValue(s, orient == 0);
                    else
                    {
                        tempEmpty.Clear();
                        cellStamp++;
                        TraceRay(sr, sc, d1, s, tempEmpty, record: true);
                        TraceRay(sr, sc, d2, s, tempEmpty, record: true);
                        foreach (var (er, ec) in tempEmpty)
                            coverers[er, ec].Add((s, orient == 1));
                    }
                }
                if (!validOrient[s, 0] && !validOrient[s, 1])
                    return "IMPOSSIBLE";
            }

            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    if (grid[i][j] != '.')
                        continue;
                    var list = coverers[i, j];
                    if (list.Count == 0)
                        return "IMPOSSIBLE";
                    if (list.Count == 1)
                        sat.SetValue(list[0].Shooter, list[0].IsVert);
                    else if (list.Count == 2)
                        sat.AddClause(list[0].Shooter, list[0].IsVert, list[1].Shooter, list[1].IsVert);
                    else
                        return "IMPOSSIBLE";
                }
            }

            if (!sat.TrySolve(out bool[] assign))
                return "IMPOSSIBLE";

            char[][] outGrid = new char[r][];
            for (int i = 0; i < r; i++)
                outGrid[i] = grid[i].ToCharArray();
            for (int s = 0; s < k; s++)
            {
                var (sr, sc) = shooters[s];
                outGrid[sr][sc] = assign[s] ? '|' : '-';
            }

            StringBuilder sb = new();
            sb.Append("POSSIBLE");
            for (int i = 0; i < r; i++)
                sb.AppendLine().Append(outGrid[i]);
            return sb.ToString();
        }

        private static void ValidateBeamingWithJoy(string input, string expectedOutput, List<string> result)
        {
            char[] sep = ['\r', '\n'];
            string[] inLines = input.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            string[] expLines = expectedOutput.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            string[] actLines = string.Join(Environment.NewLine, result)
                .Split(sep, StringSplitOptions.RemoveEmptyEntries);

            int t = int.Parse(inLines[0], CultureInfo.InvariantCulture);
            int inIdx = 1, expIdx = 0, actIdx = 0;

            for (int caseNum = 1; caseNum <= t; caseNum++)
            {
                string[] rc = inLines[inIdx++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int r = int.Parse(rc[0], CultureInfo.InvariantCulture);
                int c = int.Parse(rc[1], CultureInfo.InvariantCulture);
                string[] orig = new string[r];
                for (int i = 0; i < r; i++)
                    orig[i] = inLines[inIdx++];

                string prefix = $"Case #{caseNum}: ";
                string expHeader = expLines[expIdx++].Substring(prefix.Length).Trim();
                if (expHeader == "POSSIBLE")
                    expIdx += r;

                Assert.IsTrue(actLines[actIdx].StartsWith(prefix, StringComparison.Ordinal));
                string actHeader = actLines[actIdx++].Substring(prefix.Length).Trim();
                Assert.AreEqual(expHeader, actHeader, $"Case #{caseNum} feasibility mismatch");
                if (actHeader == "IMPOSSIBLE")
                    continue;

                string[] grid = new string[r];
                for (int i = 0; i < r; i++)
                {
                    grid[i] = actLines[actIdx++];
                    Assert.AreEqual(c, grid[i].Length, $"Case #{caseNum} row {i} length mismatch");
                    for (int j = 0; j < c; j++)
                    {
                        if (orig[i][j] != grid[i][j])
                        {
                            Assert.IsTrue((orig[i][j] == '-' && grid[i][j] == '|') ||
                                          (orig[i][j] == '|' && grid[i][j] == '-'),
                                $"Case #{caseNum} illegal grid change at ({i},{j})");
                        }
                    }
                }

                bool[,] covered = new bool[r, c];
                bool[,,] vis = new bool[r, c, 4];
                for (int i = 0; i < r; i++)
                {
                    for (int j = 0; j < c; j++)
                    {
                        if (grid[i][j] != '-' && grid[i][j] != '|')
                            continue;
                        int[] dirs = grid[i][j] == '-' ? [3, 1] : [0, 2];
                        foreach (int startDir in dirs)
                        {
                            int dir = startDir;
                            int rr = i + BeamDr[dir];
                            int cc = j + BeamDc[dir];
                            Array.Clear(vis);
                            while (rr >= 0 && rr < r && cc >= 0 && cc < c)
                            {
                                char ch = grid[rr][cc];
                                if (ch == '#') break;
                                Assert.IsFalse(ch == '-' || ch == '|',
                                    $"Case #{caseNum} beam from ({i},{j}) hit shooter at ({rr},{cc})");
                                if (vis[rr, cc, dir]) break;
                                vis[rr, cc, dir] = true;
                                if (ch == '/') dir ^= 1;
                                else if (ch == '\\') dir = 3 - dir;
                                else covered[rr, cc] = true;
                                rr += BeamDr[dir];
                                cc += BeamDc[dir];
                            }
                        }
                    }
                }

                for (int i = 0; i < r; i++)
                {
                    for (int j = 0; j < c; j++)
                    {
                        if (grid[i][j] == '.')
                            Assert.IsTrue(covered[i, j], $"Case #{caseNum} empty cell ({i},{j}) uncovered");
                    }
                }
            }
        }

        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver, ValidateBeamingWithJoy);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Solver, ValidateBeamingWithJoy);
        }

        [Test]
        public void Q3Large()
        {
            Test(Q3Solver, ValidateBeamingWithJoy);
        }

        #endregion

        #region Question 4 - Shoot the Turrets

        private static Func<string> Q4Solver(GCJTestCase test)
        {
            test.Get(out int c, out int r, out int m);
            test.GetLines(r, out string[] grid);
            return () => Q4Solver(c, r, m, grid);
        }

        private static string Q4Solver(int c, int r, int m, string[] grid)
        {
            int nCells = r * c;
            List<int> soldiers = [];
            List<int> turrets = [];
            int[,] turretIdxGrid = new int[r, c];

            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    turretIdxGrid[i, j] = -1;
                    if (grid[i][j] == 'S')
                        soldiers.Add(i * c + j);
                    else if (grid[i][j] == 'T')
                    {
                        turretIdxGrid[i, j] = turrets.Count;
                        turrets.Add(i * c + j);
                    }
                }
            }

            int ns = soldiers.Count;
            int nt = turrets.Count;
            if (ns == 0 || nt == 0)
                return "0";

            List<int>[] cellTurrets = new List<int>[nCells];
            for (int i = 0; i < nCells; i++)
                cellTurrets[i] = [];

            int[] aliveCover = new int[nCells];
            void MarkTurretCoverage(int t, int delta)
            {
                int tr = turrets[t] / c;
                int tc = turrets[t] % c;
                aliveCover[tr * c + tc] += delta;
                if (delta > 0)
                    cellTurrets[tr * c + tc].Add(t);
                for (int d = 0; d < 4; d++)
                {
                    int nr = tr + BeamDr[d];
                    int nc = tc + BeamDc[d];
                    while (nr >= 0 && nr < r && nc >= 0 && nc < c && grid[nr][nc] != '#')
                    {
                        int idx = nr * c + nc;
                        aliveCover[idx] += delta;
                        if (delta > 0)
                            cellTurrets[idx].Add(t);
                        nr += BeamDr[d];
                        nc += BeamDc[d];
                    }
                }
            }

            for (int t = 0; t < nt; t++)
                MarkTurretCoverage(t, 1);

            bool[] turretAlive = new bool[nt];
            Array.Fill(turretAlive, true);

            // Build initial bipartite graph G via relaxed BFS per soldier (no turret obstacles).
            int[] bfsQueue = new int[nCells];
            int[] bfsDist = new int[nCells];
            int[] bfsStamp = new int[nCells];
            int[] turretStamp = new int[nt];
            int curStamp = 0;

            List<int>[] adjG = new List<int>[ns];
            for (int s = 0; s < ns; s++)
            {
                curStamp++;
                int start = soldiers[s];
                bfsStamp[start] = curStamp;
                bfsDist[start] = 0;
                int head = 0, tail = 0;
                bfsQueue[tail++] = start;
                List<int> reach = [];

                while (head < tail)
                {
                    int cell = bfsQueue[head++];
                    foreach (int t in cellTurrets[cell])
                    {
                        if (turretStamp[t] != curStamp)
                        {
                            turretStamp[t] = curStamp;
                            reach.Add(t);
                        }
                    }

                    int dNext = bfsDist[cell] + 1;
                    if (dNext > m)
                        continue;

                    int cr = cell / c;
                    int cc = cell % c;
                    for (int d = 0; d < 4; d++)
                    {
                        int nr = cr + BeamDr[d];
                        int nc = cc + BeamDc[d];
                        if (nr >= 0 && nr < r && nc >= 0 && nc < c && grid[nr][nc] != '#')
                        {
                            int nCell = nr * c + nc;
                            if (bfsStamp[nCell] != curStamp)
                            {
                                bfsStamp[nCell] = curStamp;
                                bfsDist[nCell] = dNext;
                                bfsQueue[tail++] = nCell;
                            }
                        }
                    }
                }

                adjG[s] = reach;
            }

            BipartiteMatchingResult matchRes = BipartiteMatching.FindMaximumMatching(ns, nt, adjG);
            int[] matchLeft = [.. matchRes.MatchLeft];
            int[] matchRight = [.. matchRes.MatchRight];
            bool[] soldierDone = new bool[ns];

            int FindOneActiveTurret(int s)
            {
                curStamp++;
                int start = soldiers[s];
                bfsStamp[start] = curStamp;
                bfsDist[start] = 0;
                int head = 0, tail = 0;
                bfsQueue[tail++] = start;

                int preferred = matchLeft[s];
                int unmatchedCandidate = -1;
                int anyCandidate = -1;

                while (head < tail)
                {
                    int cell = bfsQueue[head++];
                    if (aliveCover[cell] > 0)
                    {
                        foreach (int t in cellTurrets[cell])
                        {
                            if (!turretAlive[t])
                                continue;
                            if (t == preferred)
                                return t;
                            if (matchRight[t] == -1)
                                unmatchedCandidate = t;
                            else if (anyCandidate == -1)
                                anyCandidate = t;
                        }
                        if (unmatchedCandidate != -1)
                            return unmatchedCandidate;
                        // Cannot move past an active turret's line of sight.
                        continue;
                    }

                    int dNext = bfsDist[cell] + 1;
                    if (dNext > m)
                        continue;

                    int cr = cell / c;
                    int cc = cell % c;
                    for (int d = 0; d < 4; d++)
                    {
                        int nr = cr + BeamDr[d];
                        int nc = cc + BeamDc[d];
                        if (nr >= 0 && nr < r && nc >= 0 && nc < c && grid[nr][nc] != '#')
                        {
                            int nCell = nr * c + nc;
                            if (bfsStamp[nCell] != curStamp)
                            {
                                bfsStamp[nCell] = curStamp;
                                bfsDist[nCell] = dNext;
                                bfsQueue[tail++] = nCell;
                            }
                        }
                    }
                }

                return unmatchedCandidate != -1 ? unmatchedCandidate : anyCandidate;
            }

            List<(int Soldier, int Turret)> order = [];
            List<int> chain = [];
            int[] posInChain = new int[ns];
            Array.Fill(posInChain, -1);

            void DestroyTurret(int t)
            {
                if (!turretAlive[t])
                    return;
                turretAlive[t] = false;
                MarkTurretCoverage(t, -1);
            }

            int nextSoldier = 0;
            while (true)
            {
                if (chain.Count == 0)
                {
                    while (nextSoldier < ns && (soldierDone[nextSoldier] || matchLeft[nextSoldier] == -1))
                        nextSoldier++;
                    if (nextSoldier >= ns)
                        break;
                    posInChain[nextSoldier] = 0;
                    chain.Add(nextSoldier);
                }

                int s = chain[^1];
                int t = FindOneActiveTurret(s);

                if (t == matchLeft[s])
                {
                    order.Add((s, t));
                    soldierDone[s] = true;
                    matchLeft[s] = -1;
                    matchRight[t] = -1;
                    DestroyTurret(t);
                    posInChain[s] = -1;
                    chain.RemoveAt(chain.Count - 1);
                }
                else if (matchRight[t] == -1)
                {
                    int oldT = matchLeft[s];
                    matchRight[oldT] = -1;
                    matchLeft[s] = t;
                    matchRight[t] = s;

                    order.Add((s, t));
                    soldierDone[s] = true;
                    matchLeft[s] = -1;
                    matchRight[t] = -1;
                    DestroyTurret(t);
                    posInChain[s] = -1;
                    chain.RemoveAt(chain.Count - 1);
                }
                else
                {
                    int sNext = matchRight[t];
                    if (posInChain[sNext] != -1)
                    {
                        // Cycle detected from cyclePos .. chain.Count - 1: rotate along cycle and execute all.
                        int cyclePos = posInChain[sNext];
                        for (int i = cyclePos; i < chain.Count; i++)
                        {
                            int curS = chain[i];
                            int targetT = (i + 1 < chain.Count) ? matchLeft[chain[i + 1]] : t;
                            order.Add((curS, targetT));
                            soldierDone[curS] = true;
                            matchLeft[curS] = -1;
                            matchRight[targetT] = -1;
                            DestroyTurret(targetT);
                            posInChain[curS] = -1;
                        }
                        chain.RemoveRange(cyclePos, chain.Count - cyclePos);
                    }
                    else
                    {
                        posInChain[sNext] = chain.Count;
                        chain.Add(sNext);
                    }
                }
            }

            StringBuilder sb = new();
            sb.Append(order.Count);
            foreach (var (s, t) in order)
                sb.AppendLine().Append(s + 1).Append(' ').Append(t + 1);
            return sb.ToString();
        }

        private static void ValidateShootTheTurrets(string input, string expectedOutput, List<string> result)
        {
            char[] sep = ['\r', '\n'];
            string[] inLines = input.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            string[] expLines = expectedOutput.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            string[] actLines = string.Join(Environment.NewLine, result)
                .Split(sep, StringSplitOptions.RemoveEmptyEntries);

            int tCases = int.Parse(inLines[0], CultureInfo.InvariantCulture);
            int inIdx = 1, expIdx = 0, actIdx = 0;

            for (int caseNum = 1; caseNum <= tCases; caseNum++)
            {
                string[] crm = inLines[inIdx++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int c = int.Parse(crm[0], CultureInfo.InvariantCulture);
                int r = int.Parse(crm[1], CultureInfo.InvariantCulture);
                int m = int.Parse(crm[2], CultureInfo.InvariantCulture);
                string[] board = new string[r];
                List<(int R, int C)> sold = [];
                List<(int R, int C)> outp = [];
                for (int i = 0; i < r; i++)
                {
                    board[i] = inLines[inIdx++];
                    for (int j = 0; j < c; j++)
                    {
                        if (board[i][j] == 'S') sold.Add((i, j));
                        else if (board[i][j] == 'T') outp.Add((i, j));
                    }
                }

                string prefix = $"Case #{caseNum}: ";
                int expAns = int.Parse(expLines[expIdx++].Substring(prefix.Length).Trim(), CultureInfo.InvariantCulture);
                expIdx += expAns;

                Assert.IsTrue(actLines[actIdx].StartsWith(prefix, StringComparison.Ordinal));
                int actAns = int.Parse(actLines[actIdx++].Substring(prefix.Length).Trim(), CultureInfo.InvariantCulture);
                Assert.AreEqual(expAns, actAns, $"Case #{caseNum} turret count mismatch");

                bool[] usedSold = new bool[sold.Count];
                bool[] usedOutp = new bool[outp.Count];
                (int S, int T)[] ops = new (int, int)[actAns];
                for (int i = 0; i < actAns; i++)
                {
                    string[] st = actLines[actIdx++].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    int s = int.Parse(st[0], CultureInfo.InvariantCulture) - 1;
                    int o = int.Parse(st[1], CultureInfo.InvariantCulture) - 1;
                    Assert.IsTrue(s >= 0 && s < sold.Count && o >= 0 && o < outp.Count);
                    Assert.IsFalse(usedSold[s], $"Case #{caseNum} duplicate soldier {s + 1}");
                    Assert.IsFalse(usedOutp[o], $"Case #{caseNum} duplicate turret {o + 1}");
                    usedSold[s] = true;
                    usedOutp[o] = true;
                    ops[i] = (s, o);
                }

                int[,] hitCount = new int[r, c];
                int[,] destCover = new int[r, c];
                void FillSides(int[,] gridArr, int ti, int tj, int delta)
                {
                    gridArr[ti, tj] += delta;
                    for (int d = 0; d < 4; d++)
                    {
                        int ni = ti + BeamDr[d], nj = tj + BeamDc[d];
                        while (ni >= 0 && ni < r && nj >= 0 && nj < c && board[ni][nj] != '#')
                        {
                            gridArr[ni, nj] += delta;
                            ni += BeamDr[d];
                            nj += BeamDc[d];
                        }
                    }
                }

                for (int o = 0; o < outp.Count; o++)
                    FillSides(hitCount, outp[o].R, outp[o].C, 1);

                Queue<(int R, int C, int D)> q = new();
                int[,] vis = new int[r, c];
                int ts = 0;

                foreach (var (s, o) in ops)
                {
                    var (ti, tj) = outp[o];
                    FillSides(destCover, ti, tj, 1);
                    ts++;
                    q.Clear();
                    var (si, sj) = sold[s];
                    vis[si, sj] = ts;
                    q.Enqueue((si, sj, 0));
                    bool found = false;
                    while (q.Count > 0)
                    {
                        var (ci, cj, dist) = q.Dequeue();
                        if (destCover[ci, cj] == 1)
                        {
                            found = true;
                            break;
                        }
                        if (hitCount[ci, cj] > 0 || dist >= m)
                            continue;
                        for (int d = 0; d < 4; d++)
                        {
                            int ni = ci + BeamDr[d], nj = cj + BeamDc[d];
                            if (ni >= 0 && ni < r && nj >= 0 && nj < c && board[ni][nj] != '#' && vis[ni, nj] != ts)
                            {
                                vis[ni, nj] = ts;
                                q.Enqueue((ni, nj, dist + 1));
                            }
                        }
                    }

                    Assert.IsTrue(found, $"Case #{caseNum} soldier {s + 1} cannot reach turret {o + 1}");
                    FillSides(destCover, ti, tj, -1);
                    FillSides(hitCount, ti, tj, -1);
                }
            }
        }

        [Test]
        public void Q4Sample()
        {
            Test(Q4Solver, ValidateShootTheTurrets);
        }

        [Test]
        public void Q4Small()
        {
            Test(Q4Solver, ValidateShootTheTurrets);
        }

        [Test]
        public void Q4Large()
        {
            SlowTest(Q4Solver, ValidateShootTheTurrets);
        }

        #endregion
    }
}

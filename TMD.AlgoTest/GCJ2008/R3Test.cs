#region License

/*
Copyright (c) 2015, the TMD.Algo authors.
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
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Algorithms.Generic;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2008
{
    [TestFixture]
    public class R3Test : BaseGCJTest
    {
        private Func<int> Q1Solver(GCJTestCase test)
        {
            int L;
            test.Get(out L);
            List<string> sList = new List<string>();
            List<int> tList = new List<int>();
            bool s = true;
            while (tList.Count() < L)
            {
                string[] bits;
                test.Get(out bits);
                for (int i = 0; i < bits.Length; i++)
                {
                    if (s)
                    {
                        sList.Add(bits[i]);
                    }
                    else
                    {
                        tList.Add(int.Parse(bits[i]));
                    }
                    s = !s;
                }
            }
            return () => Q1Solver(sList, tList);
        }

        private int Q1Solver(List<string> sList, List<int> tList)
        {
            bool[,] vEdges = new bool[6002, 6002];
            bool[,] hEdges = new bool[6002, 6002];
            bool[,] pockets = new bool[6002, 6002];
            int x = 3000;
            int y = 3000;
            int direction = 0;
            int[][] deltas = {new[] {0, -1}, new[] {1, 0}, new[] {0, 1}, new[] {-1, 0}};
            for (int i = 0; i < sList.Count; i++)
            {
                for (int j = 0; j < tList[i]; j++)
                {
                    for (int k = 0; k < sList[i].Length; k++)
                    {
                        char instruction = sList[i][k];
                        if (instruction == 'F')
                        {
                            int nextX = x + deltas[direction][0];
                            int nextY = y + deltas[direction][1];
                            if (direction%2 == 0)
                            {
                                vEdges[x, Math.Min(nextY, y)] = true;
                            }
                            else
                            {
                                hEdges[Math.Min(nextX, x), y] = true;
                            }
                            x = nextX;
                            y = nextY;
                        }
                        else if (instruction == 'R')
                        {
                            direction = (direction + 1)%4;
                        }
                        else if (instruction == 'L')
                        {
                            direction = (direction + 3)%4;
                        }
                    }
                }
            }
            int total = 0;
            int lastInsidex = -1;
            bool insideX = false;
            int lastInsidey = -1;
            bool insideY = false;
            for (int i = 0; i < 6002; i++)
            {
                for (int j = 0; j < 6002; j++)
                {
                    if (vEdges[j, i])
                    {
                        insideX = !insideX;
                        if (insideX && lastInsidex != -1)
                        {
                            for (int k = lastInsidex; k < j; k++)
                            {
                                if (!pockets[k, i])
                                {
                                    total++;
                                    pockets[k, i] = true;
                                }
                            }
                        }
                        else
                        {
                            lastInsidex = j;
                        }
                    }
                    if (hEdges[i, j])
                    {
                        insideY = !insideY;
                        if (insideY && lastInsidey != -1)
                        {
                            for (int k = lastInsidey; k < j; k++)
                            {
                                if (!pockets[i, k])
                                {
                                    total++;
                                    pockets[i, k] = true;
                                }
                            }
                        }
                        else
                        {
                            lastInsidey = j;
                        }
                    }
                }
            }
            return total;
        }

        [Test]
        public void Q1Sample()
        {
            Test(Q1Solver);
        }

        [Test]
        public void Q1Small()
        {
            // Even small is slow, due to effectively fixed runtime regardless of input.
            // Note this uses a lot of memory on many core systems.
            SlowTest(Q1Solver);
        }

        [Test]
        public void Q1Large()
        {
            SlowTest(Q1Solver);
        }

        private Func<object> Q2Solver(GCJTestCase test)
        {
            int R, C;
            test.Get(out R, out C);
            string[] lines;
            test.GetLines(R, out lines);
            return () => Q2Solver(R, C, lines);
        }

        private struct MapState : IEquatable<MapState>
        {
            public int x;
            public int y;
            public int portalx;
            public int portaly;

            // Improves performance. Not essential, but reduces unit test runtime significantly for this problem.
            public bool Equals(MapState other)
            {
                return x == other.x && y == other.y && portalx == other.portalx && portaly == other.portaly;
            }
        }

        // TODO generalize directions in 2d array to simplify all this logic.
        private bool IsLeftWall(int x, int y, string[] lines)
        {
            return x == 0 || lines[y][x - 1] == '#';
        }

        private bool IsUpWall(int x, int y, string[] lines)
        {
            return y == 0 || lines[y - 1][x] == '#';
        }

        private bool IsRightWall(int x, int y, string[] lines)
        {
            return x == lines[y].Length - 1 || lines[y][x + 1] == '#';
        }

        private bool IsDownWall(int x, int y, string[] lines)
        {
            return y == lines.Length - 1 || lines[y + 1][x] == '#';
        }

        private object Q2Solver(int R, int C, string[] lines)
        {
            List<KeyValuePair<int, int>>[,] portalOptions = new List<KeyValuePair<int, int>>[R, C];
            int sy = 0;
            int sx = 0;
            int cy = 0;
            int cx = 0;
            for (int i = 0; i < R; i++)
            {
                for (int j = 0; j < C; j++)
                {
                    if (lines[i][j] == 'X')
                    {
                        cy = i;
                        cx = j;
                    }
                    if (lines[i][j] == 'O')
                    {
                        sy = i;
                        sx = j;
                    }
                    if (lines[i][j] == '#') continue;
                    if (IsLeftWall(j, i, lines))
                    {
                        for (int k = j; k < C; k++)
                        {
                            if (lines[i][k] == '#') break;
                            if (portalOptions[i, k] == null) portalOptions[i, k] = new List<KeyValuePair<int, int>>();
                            portalOptions[i, k].Add(new KeyValuePair<int, int>(i, j));
                        }
                    }
                    if (IsUpWall(j, i, lines))
                    {
                        for (int k = i; k < R; k++)
                        {
                            if (lines[k][j] == '#') break;
                            if (portalOptions[k, j] == null) portalOptions[k, j] = new List<KeyValuePair<int, int>>();
                            portalOptions[k, j].Add(new KeyValuePair<int, int>(i, j));
                        }
                    }
                    if (IsRightWall(j, i, lines))
                    {
                        for (int k = j; k >= 0; k--)
                        {
                            if (lines[i][k] == '#') break;
                            if (portalOptions[i, k] == null) portalOptions[i, k] = new List<KeyValuePair<int, int>>();
                            portalOptions[i, k].Add(new KeyValuePair<int, int>(i, j));
                        }
                    }
                    if (IsDownWall(j, i, lines))
                    {
                        for (int k = i; k >= 0; k--)
                        {
                            if (lines[k][j] == '#') break;
                            if (portalOptions[k, j] == null) portalOptions[k, j] = new List<KeyValuePair<int, int>>();
                            portalOptions[k, j].Add(new KeyValuePair<int, int>(i, j));
                        }
                    }
                }
            }
            MapState start = new MapState()
            {
                x = sx,
                y = sy,
                portalx = -1,
                portaly = -1,
            };
            int steps = Search.Pfs(start, cur => cur.x == cx && cur.y == cy, cur =>
            {
                List<KeyValuePair<MapState, int>> options = new List<KeyValuePair<MapState, int>>();
                var portals = portalOptions[cur.y, cur.x];
                if (portals != null)
                {
                    // Can change portal.
                    foreach (var portal in portals)
                    {
                        MapState next = new MapState()
                        {
                            x = cur.x,
                            y = cur.y,
                            portalx = portal.Value,
                            portaly = portal.Key
                        };
                        options.Add(new KeyValuePair<MapState, int>(next, 0));
                    }
                    // Can walk through an opened portal if next to a wall.
                    if (cur.portalx != -1 &&
                        (IsDownWall(cur.x, cur.y, lines) || IsLeftWall(cur.x, cur.y, lines) ||
                         IsUpWall(cur.x, cur.y, lines) || IsRightWall(cur.x, cur.y, lines)))
                    {
                        // Close portal behind us, no reason to come back.
                        MapState next = new MapState() {x = cur.portalx, y = cur.portaly, portalx = -1, portaly = -1};
                        options.Add(new KeyValuePair<MapState, int>(next, 1));
                    }
                    // Can walk.
                    if (!IsLeftWall(cur.x, cur.y, lines))
                    {
                        MapState next = new MapState()
                        {
                            x = cur.x - 1,
                            y = cur.y,
                            portalx = cur.portalx,
                            portaly = cur.portaly
                        };
                        options.Add(new KeyValuePair<MapState, int>(next, 1));
                    }
                    if (!IsUpWall(cur.x, cur.y, lines))
                    {
                        MapState next = new MapState()
                        {
                            x = cur.x,
                            y = cur.y - 1,
                            portalx = cur.portalx,
                            portaly = cur.portaly
                        };
                        options.Add(new KeyValuePair<MapState, int>(next, 1));
                    }
                    if (!IsRightWall(cur.x, cur.y, lines))
                    {
                        MapState next = new MapState()
                        {
                            x = cur.x + 1,
                            y = cur.y,
                            portalx = cur.portalx,
                            portaly = cur.portaly
                        };
                        options.Add(new KeyValuePair<MapState, int>(next, 1));
                    }
                    if (!IsDownWall(cur.x, cur.y, lines))
                    {
                        MapState next = new MapState()
                        {
                            x = cur.x,
                            y = cur.y + 1,
                            portalx = cur.portalx,
                            portaly = cur.portaly
                        };
                        options.Add(new KeyValuePair<MapState, int>(next, 1));
                    }
                }
                return options;
            }, null, null, null, new IntAdder());
            if (steps < 0) return "THE CAKE IS A LIE";
            return steps;
        }

        // The validator for these tests does not match real validator, as such failures are expected if implementation changes.
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
            SlowTest(Q2Solver);
        }

        private Func<int> Q3Solver(GCJTestCase test)
        {
            int N, M;
            test.Get(out M, out N);
            string[] brokens;
            test.GetLines(M, out brokens);
            return () => Q3Solver(M, N, brokens);
        }

        private int Q3Solver(int M, int N, string[] brokens)
        {
            int[,] verticies = new int[M, N];
            Graph<int, int> graph = new Graph<int, int>();
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    if (brokens[i][j] == '.')
                    {
                        verticies[i, j] = graph.AddVertex(i*N + j);
                    }
                }
            }
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < N - 1; j++)
                {
                    if (brokens[i][j] == '.')
                    {
                        if (i > 0)
                        {
                            if (brokens[i - 1][j + 1] == '.')
                            {
                                graph.AddUndirectedEdge(verticies[i, j], verticies[i - 1, j + 1], 0);
                            }
                        }
                        if (brokens[i][j + 1] == '.')
                        {
                            graph.AddUndirectedEdge(verticies[i, j], verticies[i, j + 1], 0);
                        }
                        if (i < M - 1)
                        {
                            if (brokens[i + 1][j + 1] == '.')
                            {
                                graph.AddUndirectedEdge(verticies[i, j], verticies[i + 1, j + 1], 0);
                            }
                        }
                    }
                }
            }
            return graph.MaximumBipartiteIndependentSet();
        }


        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver, 1e-6);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Solver, 1e-6);
        }

        [Test]
        public void Q3Large()
        {
            SlowTest(Q3Solver, 1e-6);
        }

        private Func<int> Q4Solver(GCJTestCase test)
        {
            int H, W, R;
            test.Get(out H, out W, out R);
            int[][] stones;
            test.GetMatrix(R, out stones);
            return () => Q4Solver(H, W, stones);
        }

        private bool TryConvertKnightPos(int x, int y, out int a, out int b)
        {
            // for x,y based from zero.
            // 2a + b = x, 2b + a = y
            // 2y - 4b + b = x
            // b = (2y-x)/3;
            // a = y - (4y-2x)/3 = (2x-y)/3;
            int b3 = (2*(y - 1) - (x - 1));
            int a3 = (2*(x - 1) - (y - 1));
            if (b3 >= 0 && a3 >= 0 && b3%3 == 0 && a3%3 == 0)
            {
                a = a3/3;
                b = b3/3;
                return true;
            }
            a = 0;
            b = 0;
            return false;
        }

        private int Q4Solver(int H, int W, int[][] stones)
        {
            int ta, tb;
            if (!TryConvertKnightPos(W, H, out ta, out tb))
                return 0;
            if (ta == 0 && tb == 0) return 1;
            List<KeyValuePair<int, int>> stonesPos = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < stones.Length; i++)
            {
                int sa, sb;
                if (TryConvertKnightPos(stones[i][1], stones[i][0], out sa, out sb))
                {
                    stonesPos.Add(new KeyValuePair<int, int>(sa, sb));
                }
            }
            long smallTotal = Q4SmallSolver(ta, tb, stonesPos);
            long total = 0;
            Modulo mod = new Modulo(10007);
            stonesPos.Sort(KvpComparer<int, int>.Default);
            KeyValuePair<int, int> start = new KeyValuePair<int, int>(0, 0);
            KeyValuePair<int, int> end = new KeyValuePair<int, int>(ta, tb);
            for (int i = 0; i < (1 << stonesPos.Count); i++)
            {
                long subtotal = 1;
                KeyValuePair<int, int> prev = start;
                for (int j = 0; j < stonesPos.Count; j++)
                {
                    if ((i & (1 << j)) == 0) continue;
                    if (prev.Key <= stonesPos[j].Key && prev.Value <= stonesPos[j].Value)
                    {
                        int n = stonesPos[j].Key - prev.Key;
                        int m = stonesPos[j].Value - prev.Value;
                        subtotal = mod.Multiply(subtotal, mod.Choose(n + m, n));
                    }
                    else
                    {
                        subtotal = 0;
                        break;
                    }
                    prev = stonesPos[j];
                }
                if (prev.Key <= end.Key && prev.Value <= end.Value)
                {
                    int n = end.Key - prev.Key;
                    int m = end.Value - prev.Value;
                    subtotal = mod.Multiply(subtotal, mod.Choose(n + m, n));
                }
                else
                {
                    subtotal = 0;
                }
                if (BitTricks.PopulationCount(i)%2 == 0)
                {
                    total = mod.Add(total, subtotal);
                }
                else
                {
                    total = mod.Subtract(total, subtotal);
                }
            }
            if (smallTotal >= 0 && smallTotal != total) Debugger.Break();
            return (int)total;
        }

        private long Q4SmallSolver(int ta, int tb, List<KeyValuePair<int, int>> stonesPos)
        {
            Modulo mod = new Modulo(10007);
            if (((long)ta + 1)*((long)tb + 1) > 10000) return -1;
            long[,] dp = new long[ta + 1, tb + 1];
            dp[0, 0] = 1;
            for (int i = 0; i <= ta; i++)
            {
                for (int j = 0; j <= tb; j++)
                {
                    if (i == 0 && j == 0) continue;
                    bool stone = false;
                    for (int k = 0; k < stonesPos.Count; k++)
                    {
                        if (stonesPos[k].Key == i && stonesPos[k].Value == j)
                        {
                            stone = true;
                            break;
                        }
                    }
                    if (stone) continue;
                    if (i == 0) dp[i, j] = dp[i, j - 1];
                    else if (j == 0) dp[i, j] = dp[i - 1, j];
                    else dp[i, j] = mod.Add(dp[i, j - 1], dp[i - 1, j]);
                }
            }
            return dp[ta, tb];
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
            SlowTest(Q4Solver);
        }
    }
}
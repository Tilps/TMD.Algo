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
using System.Text;
using NUnit.Framework;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2010
{
    [TestFixture]
    public class R1CTest : BaseGCJTest
    {
        #region Question 1 - Rope Intranet

        private static Func<long> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out (int A, int B)[] wires);
            return () => Q1Solver(wires);
        }

        private static long Q1Solver((int A, int B)[] wires)
        {
            Array.Sort(wires, static (x, y) => x.A.CompareTo(y.A));
            int[] bValues = new int[wires.Length];
            for (int i = 0; i < wires.Length; i++)
            {
                bValues[i] = wires[i].B;
            }
            return SpecialtySort.CountInversions<int>(bValues);
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

        #region Question 2 - Load Testing

        private static Func<int> Q2Solver(GCJTestCase test)
        {
            test.Get(out long l, out long p, out long c);
            return () => Q2Solver(l, p, c);
        }

        private static int Q2Solver(long l, long p, long c)
        {
            int steps = 0;
            long cur = l;
            while (cur < p)
            {
                cur *= c;
                steps++;
            }

            if (steps <= 1)
                return 0;

            int tests = 0;
            long span = 1;
            while (span < steps)
            {
                span <<= 1;
                tests++;
            }
            return tests;
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

        #region Question 3 - Making Chess Boards

        private static int HexValue(char ch) => ch switch
        {
            >= '0' and <= '9' => ch - '0',
            >= 'A' and <= 'F' => ch - 'A' + 10,
            _ => ch - 'a' + 10
        };

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int rows, out int cols);
            test.GetLines(rows, out string[] hexRows);
            return () => Q3Solver(rows, cols, hexRows);
        }

        private static string Q3Solver(int rows, int cols, string[] hexRows)
        {
            bool[,] grid = new bool[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                string hex = hexRows[r];
                for (int h = 0; h < hex.Length; h++)
                {
                    int nibble = HexValue(hex[h]);
                    for (int bit = 0; bit < 4; bit++)
                    {
                        grid[r, h * 4 + bit] = ((nibble >> (3 - bit)) & 1) != 0;
                    }
                }
            }

            bool[,] cut = new bool[rows, cols];
            int[,] larg = new int[rows, cols];

            int ComputeLarg(int r, int c)
            {
                if (cut[r, c])
                    return 0;
                if (r == 0 || c == 0 || cut[r - 1, c] || cut[r, c - 1] || cut[r - 1, c - 1])
                    return 1;
                bool cell = grid[r, c];
                if (grid[r - 1, c] != cell && grid[r, c - 1] != cell && grid[r - 1, c - 1] == cell)
                {
                    int minPrev = Math.Min(larg[r - 1, c], Math.Min(larg[r, c - 1], larg[r - 1, c - 1]));
                    return 1 + minPrev;
                }
                return 1;
            }

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    larg[r, c] = ComputeLarg(r, c);
                }
            }

            // LookupHeap is a max-heap: higher size, then smaller row, then smaller column.
            var comparer = Comparer<int>.Create((aId, bId) =>
            {
                int ra = aId / cols;
                int ca = aId % cols;
                int rb = bId / cols;
                int cb = bId % cols;
                int cmp = larg[ra, ca].CompareTo(larg[rb, cb]);
                if (cmp != 0)
                    return cmp;
                cmp = rb.CompareTo(ra);
                if (cmp != 0)
                    return cmp;
                return cb.CompareTo(ca);
            });

            int[] allIds = new int[rows * cols];
            for (int i = 0; i < allIds.Length; i++)
                allIds[i] = i;

            LookupHeap<int> heap = new(allIds, comparer);
            int maxSize = Math.Min(rows, cols);
            int[] sizeCounts = new int[maxSize + 1];
            int distinctSizes = 0;
            int remaining = rows * cols;

            while (remaining > 0)
            {
                int topId = heap.Front;
                int br = topId / cols;
                int bc = topId % cols;
                int size = larg[br, bc];

                if (sizeCounts[size] == 0)
                    distinctSizes++;
                sizeCounts[size]++;
                remaining -= size * size;

                int rStart = br - size + 1;
                int cStart = bc - size + 1;

                for (int r = rStart; r <= br; r++)
                {
                    for (int c = cStart; c <= bc; c++)
                    {
                        heap.Remove(r * cols + c);
                        cut[r, c] = true;
                        larg[r, c] = 0;
                    }
                }

                int rEnd = Math.Min(rows - 1, br + size - 1);
                int cEnd = Math.Min(cols - 1, bc + size - 1);
                for (int r = rStart; r <= rEnd; r++)
                {
                    for (int c = cStart; c <= cEnd; c++)
                    {
                        if (cut[r, c])
                            continue;
                        int newVal = ComputeLarg(r, c);
                        if (newVal != larg[r, c])
                        {
                            int id = r * cols + c;
                            heap.Remove(id);
                            larg[r, c] = newVal;
                            heap.Add(id);
                        }
                    }
                }
            }

            StringBuilder sb = new();
            sb.Append(distinctSizes);
            for (int s = maxSize; s >= 1; s--)
            {
                if (sizeCounts[s] > 0)
                {
                    sb.AppendLine();
                    sb.Append(s).Append(' ').Append(sizeCounts[s]);
                }
            }
            return sb.ToString();
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
    }
}

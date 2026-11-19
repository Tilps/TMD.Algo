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
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2009
{
    [TestFixture]
    public class R1BTest : BaseGCJTest
    {
        #region Question 1 - Decision Tree

        private sealed class DecisionNode
        {
            public required double Weight { get; init; }
            public string? Feature { get; init; }
            public DecisionNode? TrueBranch { get; init; }
            public DecisionNode? FalseBranch { get; init; }
        }

        private static DecisionNode ParseDecisionTree(string text)
        {
            string spaced = text.Replace("(", " ( ").Replace(")", " ) ");
            string[] tokens = spaced.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            int idx = 0;

            DecisionNode ParseNode()
            {
                idx++; // '('
                double weight = double.Parse(tokens[idx++], CultureInfo.InvariantCulture);
                if (tokens[idx] == ")")
                {
                    idx++; // ')'
                    return new DecisionNode { Weight = weight };
                }

                string feature = tokens[idx++];
                DecisionNode left = ParseNode();
                DecisionNode right = ParseNode();
                idx++; // ')'
                return new DecisionNode
                {
                    Weight = weight,
                    Feature = feature,
                    TrueBranch = left,
                    FalseBranch = right
                };
            }

            return ParseNode();
        }

        private static Func<string[]> Q1Solver(GCJTestCase test)
        {
            test.Get(out int l);
            test.GetLines(l, out string[] treeLines);
            test.Get(out int a);
            test.GetLines(a, out string[] animalLines);
            return () => Q1Solver(treeLines, animalLines);
        }

        private static string[] Q1Solver(string[] treeLines, string[] animalLines)
        {
            DecisionNode root = ParseDecisionTree(string.Join(' ', treeLines));
            string[] outputs = new string[animalLines.Length];

            for (int i = 0; i < animalLines.Length; i++)
            {
                string[] parts = animalLines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                HashSet<string> features = [];
                for (int j = 2; j < parts.Length; j++)
                {
                    features.Add(parts[j]);
                }

                double prob = 1.0;
                DecisionNode? node = root;
                while (node != null)
                {
                    prob *= node.Weight;
                    if (node.Feature == null)
                        break;
                    node = features.Contains(node.Feature) ? node.TrueBranch : node.FalseBranch;
                }

                outputs[i] = $"{prob:F7}";
            }

            return outputs;
        }

        [Test]
        public void Q1Sample()
        {
            Test(Q1Solver, 1e-4);
        }

        [Test]
        public void Q1Small()
        {
            Test(Q1Solver, 1e-4);
        }

        [Test]
        public void Q1Large()
        {
            Test(Q1Solver, 1e-4);
        }

        #endregion

        #region Question 2 - The Next Number

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out string number);
            return () => Q2Solver(number);
        }

        private static string Q2Solver(string number)
        {
            char[] digits = ['0', .. number];
            Combinatorics.NextPermutation(digits.AsSpan());
            return digits[0] == '0' ? new string(digits.AsSpan(1)) : new string(digits);
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

        #region Question 3 - Square Math

        private static Func<string[]> Q3Solver(GCJTestCase test)
        {
            test.Get(out int w, out int q);
            test.GetLines(w, out string[] grid);
            test.Get(out int[] queries);
            return () => Q3Solver(w, grid, queries);
        }

        private static string[] Q3Solver(int w, string[] grid, int[] queries)
        {
            const int MinVal = -600;
            const int MaxVal = 850;
            const int Span = MaxVal - MinVal + 1;
            int[] dr = [-1, 1, 0, 0];
            int[] dc = [0, 0, -1, 1];

            int[,] cellToId = new int[w, w];
            List<(int R, int C)> digitCells = [];
            for (int r = 0; r < w; r++)
            {
                for (int c = 0; c < w; c++)
                {
                    if (char.IsDigit(grid[r][c]))
                    {
                        cellToId[r, c] = digitCells.Count;
                        digitCells.Add((r, c));
                    }
                    else
                    {
                        cellToId[r, c] = -1;
                    }
                }
            }

            List<(int NextCell, char Op, int SignedDigit)>[] transitions = new List<(int, char, int)>[digitCells.Count];
            for (int id = 0; id < digitCells.Count; id++)
            {
                var (r, c) = digitCells[id];
                transitions[id] = [];
                for (int d1 = 0; d1 < 4; d1++)
                {
                    int or = r + dr[d1];
                    int oc = c + dc[d1];
                    if (or < 0 || or >= w || oc < 0 || oc >= w)
                        continue;

                    char op = grid[or][oc];
                    if (op != '+' && op != '-')
                        continue;

                    int sign = op == '+' ? 1 : -1;
                    for (int d2 = 0; d2 < 4; d2++)
                    {
                        int nr = or + dr[d2];
                        int nc = oc + dc[d2];
                        if (nr < 0 || nr >= w || nc < 0 || nc >= w)
                            continue;

                        int nextId = cellToId[nr, nc];
                        if (nextId >= 0)
                        {
                            int nextDigit = grid[nr][nc] - '0';
                            transitions[id].Add((nextId, op, sign * nextDigit));
                        }
                    }
                }
            }

            Dictionary<int, string?> queryAnswers = [];
            foreach (int query in queries)
            {
                queryAnswers.TryAdd(query, null);
            }
            int remainingQueries = queryAnswers.Count;

            bool[] visited = new bool[digitCells.Count * Span];
            Dictionary<int, string> currentLayer = [];

            for (int id = 0; id < digitCells.Count; id++)
            {
                var (r, c) = digitCells[id];
                int val = grid[r][c] - '0';
                int stateId = id * Span + (val - MinVal);
                string expr = grid[r][c].ToString();

                visited[stateId] = true;
                currentLayer[stateId] = expr;

                if (queryAnswers.TryGetValue(val, out string? existing))
                {
                    if (existing == null)
                    {
                        queryAnswers[val] = expr;
                        remainingQueries--;
                    }
                    else if (string.CompareOrdinal(expr, existing) < 0)
                    {
                        queryAnswers[val] = expr;
                    }
                }
            }

            Dictionary<int, string> nextLayer = [];
            while (remainingQueries > 0 && currentLayer.Count > 0)
            {
                nextLayer.Clear();
                foreach (var (stateId, expr) in currentLayer)
                {
                    int cellId = stateId / Span;
                    int val = (stateId % Span) + MinVal;

                    foreach (var (nextCell, op, signedDigit) in transitions[cellId])
                    {
                        int nextVal = val + signedDigit;
                        if (nextVal < MinVal || nextVal > MaxVal)
                            continue;

                        int nextStateId = nextCell * Span + (nextVal - MinVal);
                        if (visited[nextStateId])
                            continue;

                        char digitChar = (char)('0' + Math.Abs(signedDigit));
                        string nextExpr = $"{expr}{op}{digitChar}";

                        if (!nextLayer.TryGetValue(nextStateId, out string? prevCand) ||
                            string.CompareOrdinal(nextExpr, prevCand) < 0)
                        {
                            nextLayer[nextStateId] = nextExpr;
                        }
                    }
                }

                int newlySolved = 0;
                foreach (var (nextStateId, nextExpr) in nextLayer)
                {
                    visited[nextStateId] = true;
                    int val = (nextStateId % Span) + MinVal;
                    if (queryAnswers.TryGetValue(val, out string? existing))
                    {
                        if (existing == null)
                        {
                            queryAnswers[val] = nextExpr;
                            newlySolved++;
                        }
                        else if (nextExpr.Length == existing.Length && string.CompareOrdinal(nextExpr, existing) < 0)
                        {
                            queryAnswers[val] = nextExpr;
                        }
                    }
                }
                remainingQueries -= newlySolved;
                (currentLayer, nextLayer) = (nextLayer, currentLayer);
            }

            string[] result = new string[queries.Length];
            for (int i = 0; i < queries.Length; i++)
            {
                result[i] = queryAnswers[queries[i]]!;
            }
            return result;
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

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
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2011
{
    [TestFixture]
    public class R1BTest : BaseGCJTest
    {
        #region Question 1 - RPI

        private static Func<string[]> Q1Solver(GCJTestCase test)
        {
            test.Get(out int n);
            test.GetLines(n, out string[] schedule);
            return () => Q1Solver(schedule);
        }

        private static string[] Q1Solver(string[] schedule)
        {
            int n = schedule.Length;
            int[] wins = new int[n];
            int[] total = new int[n];
            double[] wp = new double[n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (schedule[i][j] == '1')
                    {
                        wins[i]++;
                        total[i]++;
                    }
                    else if (schedule[i][j] == '0')
                    {
                        total[i]++;
                    }
                }
                wp[i] = (double)wins[i] / total[i];
            }

            double[] owp = new double[n];
            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int j = 0; j < n; j++)
                {
                    if (schedule[i][j] != '.')
                    {
                        int oppWins = wins[j] - (schedule[j][i] == '1' ? 1 : 0);
                        sum += (double)oppWins / (total[j] - 1);
                    }
                }
                owp[i] = sum / total[i];
            }

            string[] output = new string[n];
            for (int i = 0; i < n; i++)
            {
                double sumOwp = 0;
                for (int j = 0; j < n; j++)
                {
                    if (schedule[i][j] != '.')
                        sumOwp += owp[j];
                }
                double oowp = sumOwp / total[i];
                double rpi = 0.25 * wp[i] + 0.50 * owp[i] + 0.25 * oowp;
                output[i] = rpi.ToString("F12", CultureInfo.InvariantCulture);
            }

            return output;
        }

        [Test]
        public void Q1Sample()
        {
            Test(Q1Solver, 1e-6);
        }

        [Test]
        public void Q1Small()
        {
            Test(Q1Solver, 1e-6);
        }

        [Test]
        public void Q1Large()
        {
            Test(Q1Solver, 1e-6);
        }

        #endregion

        #region Question 2 - Revenge of the Hot Dogs

        private static Func<string> Q2Solver(GCJTestCase test)
        {
            test.Get(out int c, out long d);
            test.GetLines(c, out (long Pos, long Count)[] clusters);
            return () => Q2Solver(d, clusters);
        }

        private static string Q2Solver(long d, (long Pos, long Count)[] clusters)
        {
            long minVal = long.MaxValue;
            long bestX = 0;
            long cnt = 0;

            foreach (var (pos, v) in clusters)
            {
                long first = d * cnt - pos;
                long last = d * (cnt + v - 1) - pos;
                if (first < minVal)
                    minVal = first;
                if (last - minVal > bestX)
                    bestX = last - minVal;
                cnt += v;
            }

            return $"{bestX / 2}.{(bestX % 2) * 5}";
        }

        [Test]
        public void Q2Sample()
        {
            Test(Q2Solver, 1e-6);
        }

        [Test]
        public void Q2Small()
        {
            Test(Q2Solver, 1e-6);
        }

        [Test]
        public void Q2Large()
        {
            Test(Q2Solver, 1e-6);
        }

        #endregion

        #region Question 3 - House of Kittens

        private static Func<string> Q3Solver(GCJTestCase test)
        {
            test.Get(out int n, out int _);
            test.Get(out int[] starts);
            test.Get(out int[] ends);
            return () => Q3Solver(n, starts, ends);
        }

        private static List<List<int>> ExtractFaces(int n, int[] starts, int[] ends, out List<int>[] children)
        {
            List<int>[] adj = new List<int>[n];
            for (int i = 0; i < n; i++)
                adj[i] = [];

            for (int i = 0; i < starts.Length; i++)
                adj[starts[i] - 1].Add(ends[i] - 1);

            for (int i = 0; i < n; i++)
                adj[i].Sort();

            List<List<int>> faces = [];
            List<List<int>> childList = [];

            int Extract(int startPos, int endPos)
            {
                int faceId = faces.Count;
                List<int> face = [startPos];
                faces.Add(face);
                childList.Add([]);

                int pos = startPos;
                while (pos != endPos)
                {
                    if (adj[pos].Count > 0)
                    {
                        int newEndPos = adj[pos][^1];
                        adj[pos].RemoveAt(adj[pos].Count - 1);
                        int childId = Extract(pos, newEndPos);
                        childList[faceId].Add(childId);
                        pos = newEndPos;
                    }
                    else
                    {
                        pos++;
                    }
                    face.Add(pos);
                }

                return faceId;
            }

            Extract(0, n - 1);
            children = [.. childList];
            return faces;
        }

        private static string Q3Solver(int n, int[] starts, int[] ends)
        {
            List<List<int>> faces = ExtractFaces(n, starts, ends, out List<int>[] children);
            int numColors = int.MaxValue;
            foreach (var f in faces)
            {
                if (f.Count < numColors)
                    numColors = f.Count;
            }

            int[] colors = new int[n];
            List<int> root = faces[0];
            for (int i = 0; i < numColors; i++)
                colors[root[i]] = i + 1;
            for (int i = numColors; i < root.Count; i++)
            {
                for (int col = 1; col <= numColors; col++)
                {
                    if (col != colors[root[i - 1]] && (i < root.Count - 1 || col != colors[root[0]]))
                    {
                        colors[root[i]] = col;
                        break;
                    }
                }
            }

            Queue<int> queue = new();
            foreach (int child in children[0])
                queue.Enqueue(child);

            int[] unused = new int[numColors];
            while (queue.Count > 0)
            {
                int fid = queue.Dequeue();
                List<int> face = faces[fid];
                int cStart = colors[face[0]];
                int cEnd = colors[face[^1]];
                int uCount = 0;
                for (int col = 1; col <= numColors; col++)
                {
                    if (col != cStart && col != cEnd)
                        unused[uCount++] = col;
                }

                for (int i = 1; i < face.Count - 1; i++)
                {
                    if (i - 1 < uCount)
                    {
                        colors[face[i]] = unused[i - 1];
                    }
                    else
                    {
                        for (int col = 1; col <= numColors; col++)
                        {
                            if (col != colors[face[i - 1]] && (i < face.Count - 2 || col != cEnd))
                            {
                                colors[face[i]] = col;
                                break;
                            }
                        }
                    }
                }

                foreach (int child in children[fid])
                    queue.Enqueue(child);
            }

            return $"{numColors}\n{string.Join(" ", colors)}";
        }

        private static bool TryParseJudgeInt(string token, out int value)
        {
            value = 0;
            if (string.IsNullOrEmpty(token))
                return false;
            if ((token.Length > 1 && token[0] == '0') || token.StartsWith("-0", StringComparison.Ordinal))
                return false;
            return int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static void ValidateQ3(string input, string _, List<string> result)
        {
            string[] inLines = input.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            int numCases = int.Parse(inLines[0].Trim(), CultureInfo.InvariantCulture);
            Assert.AreEqual(numCases, result.Count);

            for (int caseIdx = 0; caseIdx < numCases; caseIdx++)
            {
                string[] headerToks = inLines[3 * caseIdx + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int n = int.Parse(headerToks[0], CultureInfo.InvariantCulture);
                string[] sToks = inLines[3 * caseIdx + 2].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string[] eToks = inLines[3 * caseIdx + 3].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int[] starts = Array.ConvertAll(sToks, s => int.Parse(s, CultureInfo.InvariantCulture));
                int[] ends = Array.ConvertAll(eToks, s => int.Parse(s, CultureInfo.InvariantCulture));

                List<List<int>> faces = ExtractFaces(n, starts, ends, out List<int>[] _);
                int expectedColors = int.MaxValue;
                foreach (var f in faces)
                {
                    if (f.Count < expectedColors)
                        expectedColors = f.Count;
                }

                string[] outLines = result[caseIdx].Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(2, outLines.Length, $"Case #{caseIdx + 1}: expected 2 lines.");

                string[] line1 = outLines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(3, line1.Length, $"Case #{caseIdx + 1}: broken header.");
                Assert.AreEqual("Case", line1[0]);
                Assert.AreEqual($"#{caseIdx + 1}:", line1[1]);
                Assert.IsTrue(TryParseJudgeInt(line1[2], out int actualColors));
                Assert.AreEqual(expectedColors, actualColors, $"Case #{caseIdx + 1}: wrong color count.");

                string[] line2 = outLines[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Assert.AreEqual(n, line2.Length, $"Case #{caseIdx + 1}: wrong vertex count.");
                int[] colors = new int[n];
                for (int i = 0; i < n; i++)
                {
                    Assert.IsTrue(TryParseJudgeInt(line2[i], out int c) && c >= 1 && c <= actualColors,
                        $"Case #{caseIdx + 1}: invalid color '{line2[i]}'.");
                    colors[i] = c;
                }

                HashSet<int> seen = [];
                foreach (var face in faces)
                {
                    seen.Clear();
                    foreach (int v in face)
                        seen.Add(colors[v]);
                    Assert.AreEqual(expectedColors, seen.Count, $"Case #{caseIdx + 1}: missing color on a face.");
                }
            }
        }

        [Test]
        public void Q3Sample()
        {
            Test(Q3Solver, ValidateQ3);
        }

        [Test]
        public void Q3Small()
        {
            Test(Q3Solver, ValidateQ3);
        }

        [Test]
        public void Q3Large()
        {
            Test(Q3Solver, ValidateQ3);
        }

        #endregion
    }
}

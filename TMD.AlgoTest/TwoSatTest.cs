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
using NUnit.Framework;
using TMD.Algo.Algorithms;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class TwoSatTest
    {
        [Test]
        public void EmptyAndTrivialCases()
        {
            TwoSat empty = new(0);
            Assert.IsTrue(empty.TrySolve(out bool[] emptyAssign));
            Assert.AreEqual(0, emptyAssign.Length);

            TwoSat forced = new(2);
            forced.SetValue(0, true);
            forced.SetValue(1, false);
            Assert.IsTrue(forced.TrySolve(out bool[] forcedAssign));
            Assert.AreEqual(new[] { true, false }, forcedAssign);

            Assert.Throws<ArgumentOutOfRangeException>(() => _ = new TwoSat(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => forced.SetValue(2, true));
            Assert.Throws<ArgumentOutOfRangeException>(() => forced.AddClause(-1, true, 0, false));
        }

        [Test]
        public void XorEqualImplicationAndContradiction()
        {
            TwoSat sat = new(4);
            sat.AddEqual(0, 1);
            sat.AddXor(1, 2);
            sat.AddImplication(2, true, 3, true);
            sat.SetValue(0, false);

            Assert.IsTrue(sat.TrySolve(out bool[] assign));
            Assert.IsFalse(assign[0]);
            Assert.IsFalse(assign[1]);
            Assert.IsTrue(assign[2]);
            Assert.IsTrue(assign[3]);

            TwoSat unsat = new(3);
            unsat.AddXor(0, 1);
            unsat.AddXor(1, 2);
            unsat.AddXor(0, 2);
            Assert.IsFalse(unsat.TrySolve(out bool[] failAssign));
            Assert.AreEqual(0, failAssign.Length);
        }

        [Test]
        public void DeepChainDoesNotStackOverflow()
        {
            const int n = 20_000;
            TwoSat chain = new(n);
            for (int i = 0; i + 1 < n; i++)
                chain.AddImplication(i, true, i + 1, true);
            chain.SetValue(0, true);

            Assert.IsTrue(chain.TrySolve(out bool[] assign));
            for (int i = 0; i < n; i++)
                Assert.IsTrue(assign[i]);

            chain.SetValue(n - 1, false);
            Assert.IsFalse(chain.TrySolve(out _));
        }

        [Test]
        public void RandomizedComparisonAgainstBruteForce()
        {
            Random rng = new(12345);
            for (int trial = 0; trial < 200; trial++)
            {
                int n = rng.Next(2, 9);
                int m = rng.Next(1, 20);
                TwoSat sat = new(n);
                List<(int A, bool Ta, int B, bool Tb)> clauses = [];

                for (int i = 0; i < m; i++)
                {
                    int a = rng.Next(n);
                    int b = rng.Next(n);
                    bool ta = rng.Next(2) == 1;
                    bool tb = rng.Next(2) == 1;
                    sat.AddClause(a, ta, b, tb);
                    clauses.Add((a, ta, b, tb));
                }

                bool expectedSat = false;
                for (int mask = 0; mask < (1 << n); mask++)
                {
                    bool ok = true;
                    foreach (var (a, ta, b, tb) in clauses)
                    {
                        bool va = ((mask >> a) & 1) != 0;
                        bool vb = ((mask >> b) & 1) != 0;
                        if (va != ta && vb != tb)
                        {
                            ok = false;
                            break;
                        }
                    }
                    if (ok)
                    {
                        expectedSat = true;
                        break;
                    }
                }

                bool actualSat = sat.TrySolve(out bool[] assign);
                Assert.AreEqual(expectedSat, actualSat, $"Trial {trial} satisfiability mismatch.");
                if (actualSat)
                {
                    foreach (var (a, ta, b, tb) in clauses)
                        Assert.IsTrue(assign[a] == ta || assign[b] == tb);
                }
            }
        }
    }
}

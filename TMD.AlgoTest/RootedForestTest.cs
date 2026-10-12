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
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using TMD.Algo.Collections.Generic;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class RootedForestTest
    {
        [Test]
        public void TestSingleTree()
        {
            // Tree structure:
            //       0
            //      / \
            //     1   2
            //    /
            //   3
            int[] parents = { -1, 0, 0, 1 };
            RootedForest forest = new RootedForest(parents);

            Assert.AreEqual(4, forest.NodeCount);
            Assert.AreEqual(1, forest.Roots.Count);
            Assert.AreEqual(0, forest.Roots[0]);

            Assert.AreEqual(4, forest.SubtreeSizes[0]);
            Assert.AreEqual(2, forest.SubtreeSizes[1]);
            Assert.AreEqual(1, forest.SubtreeSizes[2]);
            Assert.AreEqual(1, forest.SubtreeSizes[3]);

            Assert.AreEqual(0, forest.Depths[0]);
            Assert.AreEqual(1, forest.Depths[1]);
            Assert.AreEqual(1, forest.Depths[2]);
            Assert.AreEqual(2, forest.Depths[3]);

            Assert.AreEqual(0, forest.GetLowestCommonAncestor(2, 3));
            Assert.AreEqual(1, forest.GetLowestCommonAncestor(1, 3));
            Assert.AreEqual(3, forest.GetLowestCommonAncestor(3, 3));

            Assert.AreEqual(3, forest.GetDistance(2, 3));
            Assert.AreEqual(1, forest.GetDistance(1, 3));
            Assert.AreEqual(0, forest.GetDistance(2, 2));

            Assert.IsTrue(forest.IsAncestor(0, 3));
            Assert.IsTrue(forest.IsAncestor(1, 3));
            Assert.IsFalse(forest.IsAncestor(2, 3));
            Assert.IsFalse(forest.IsAncestor(3, 1));

            // Topological sorts count: 4! / (4 * 2 * 1 * 1) = 24 / 8 = 3.
            // Valid topological sorts: (0, 1, 3, 2), (0, 1, 2, 3), (0, 2, 1, 3).
            Assert.AreEqual(new BigInteger(3), forest.CountTopologicalSorts());
        }

        [Test]
        public void TestMultipleTrees()
        {
            // Forest with two trees:
            // Tree 1: 0 -> 1
            // Tree 2: 2 -> 3
            int[] parents = { -1, 0, -1, 2 };
            RootedForest forest = new RootedForest(parents);

            Assert.AreEqual(4, forest.NodeCount);
            Assert.AreEqual(2, forest.Roots.Count);

            Assert.AreEqual(-1, forest.GetLowestCommonAncestor(1, 3));
            Assert.AreEqual(-1, forest.GetDistance(1, 3));
            Assert.IsFalse(forest.IsAncestor(0, 3));

            // Topological sorts count: 4! / (2 * 1 * 2 * 1) = 24 / 4 = 6.
            Assert.AreEqual(new BigInteger(6), forest.CountTopologicalSorts());
        }

        [Test]
        public void TestRandomTopologicalSort()
        {
            int[] parents = { -1, 0, 0, 1 };
            RootedForest forest = new RootedForest(parents);
            Random rng = new Random(12345);

            for (int i = 0; i < 50; i++)
            {
                int[] sort = forest.GenerateRandomTopologicalSort(rng);
                Assert.AreEqual(4, sort.Length);
                Assert.AreEqual(4, sort.Distinct().Count());

                int[] pos = new int[4];
                for (int p = 0; p < 4; p++) pos[sort[p]] = p;

                // Parent must appear before child
                Assert.IsTrue(pos[0] < pos[1]);
                Assert.IsTrue(pos[0] < pos[2]);
                Assert.IsTrue(pos[1] < pos[3]);
            }
        }

        [Test]
        public void TestCycleThrowsException()
        {
            int[] cyclic = { 1, 2, 0 };
            Assert.Throws<ArgumentException>(() => new RootedForest(cyclic));
        }
    }
}


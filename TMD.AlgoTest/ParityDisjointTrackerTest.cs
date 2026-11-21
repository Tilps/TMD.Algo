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
using NUnit.Framework;
using TMD.Algo.Collections.Generic;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class ParityDisjointTrackerTest
    {
        [Test]
        public void DynamicAddAndBipartiteConstraints()
        {
            ParityDisjointTracker dsu = new(2);
            Assert.AreEqual(2, dsu.Count);
            Assert.IsTrue(dsu.UnionOpposite(0, 1));
            Assert.IsTrue(dsu.AreConnected(0, 1));
            Assert.IsTrue(dsu.TryGetRelation(0, 1, out bool same01));
            Assert.IsFalse(same01);

            int a = dsu.Add();
            int b = dsu.Add();
            int c = dsu.Add();
            Assert.AreEqual(5, dsu.Count);
            Assert.IsFalse(dsu.TryGetRelation(0, a, out _));

            // 0 != a, a == b, b != c => 0 != a == b != c, so 0 == c and 1 != c.
            Assert.IsTrue(dsu.UnionOpposite(0, a));
            Assert.IsTrue(dsu.UnionSame(a, b));
            Assert.IsTrue(dsu.UnionOpposite(b, c));

            Assert.IsTrue(dsu.TryGetRelation(0, c, out bool same0C));
            Assert.IsTrue(same0C);
            Assert.IsTrue(dsu.TryGetRelation(1, c, out bool same1C));
            Assert.IsFalse(same1C);

            // Redundant consistent constraints succeed
            Assert.IsTrue(dsu.UnionSame(0, c));
            Assert.IsTrue(dsu.UnionOpposite(1, c));

            // Odd-cycle contradiction (0 == c yet trying 0 != c) fails without corrupting state
            Assert.IsFalse(dsu.UnionOpposite(0, c));
            Assert.IsFalse(dsu.UnionSame(1, c));
            Assert.IsTrue(dsu.TryGetRelation(0, c, out bool stillSame));
            Assert.IsTrue(stillSame);
        }

        [Test]
        public void ValidationAndBounds()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParityDisjointTracker(-1));
            ParityDisjointTracker dsu = new(3);
            Assert.Throws<ArgumentOutOfRangeException>(() => dsu.Find(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => dsu.Find(3));
            Assert.Throws<ArgumentOutOfRangeException>(() => dsu.Union(0, 5, true));
            Assert.Throws<ArgumentOutOfRangeException>(() => dsu.AreConnected(-1, 0));
        }
    }
}

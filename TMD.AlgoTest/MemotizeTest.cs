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
using TMD.Algo.Algorithms;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class MemotizeTest
    {
        [Test]
        public void Basic1ArgDictionary()
        {
            int evalCount = 0;
            Func<int, int> square = x =>
            {
                evalCount++;
                return x * x;
            };
            var memoSquare = square.CreateMemotized();

            Assert.AreEqual(25, memoSquare(5));
            Assert.AreEqual(1, evalCount);
            Assert.AreEqual(25, memoSquare(5));
            Assert.AreEqual(1, evalCount);
            Assert.AreEqual(49, memoSquare(7));
            Assert.AreEqual(2, evalCount);
        }

        [Test]
        public void Basic2ArgDictionary()
        {
            int evalCount = 0;
            Func<string, int, string> repeat = (s, count) =>
            {
                evalCount++;
                return string.Concat(System.Linq.Enumerable.Repeat(s, count));
            };
            var memoRepeat = repeat.CreateMemotized();

            Assert.AreEqual("abcabc", memoRepeat("abc", 2));
            Assert.AreEqual(1, evalCount);
            Assert.AreEqual("abcabc", memoRepeat("abc", 2));
            Assert.AreEqual(1, evalCount);
            Assert.AreEqual("xyz", memoRepeat("xyz", 1));
            Assert.AreEqual(2, evalCount);
        }

        [Test]
        public void Basic1ArgArray()
        {
            int evalCount = 0;
            Func<long, long> cube = x =>
            {
                evalCount++;
                return x * x * x;
            };
            var memoCube = cube.CreateMemotized(-10, 10);

            Assert.AreEqual(-8, memoCube(-2));
            Assert.AreEqual(1, evalCount);
            Assert.AreEqual(-8, memoCube(-2));
            Assert.AreEqual(1, evalCount);
            Assert.AreEqual(27, memoCube(3));
            Assert.AreEqual(2, evalCount);
        }

        [Test]
        public void Basic2ArgArray()
        {
            int evalCount = 0;
            Func<int, int, int> add = (a, b) =>
            {
                evalCount++;
                return a + b;
            };
            var memoAdd = add.CreateMemotized(0, 10, 0, 10);

            Assert.AreEqual(7, memoAdd(3, 4));
            Assert.AreEqual(1, evalCount);
            Assert.AreEqual(7, memoAdd(3, 4));
            Assert.AreEqual(1, evalCount);
        }

        [Test]
        public void Recursive1ArgDictionary()
        {
            var fib = Memotize.CreateMemotizedRecursive<int, long>((recurse, n) =>
            {
                if (n <= 0) return 0;
                if (n == 1) return 1;
                return recurse(n - 1) + recurse(n - 2);
            });

            Assert.AreEqual(0, fib(0));
            Assert.AreEqual(1, fib(1));
            Assert.AreEqual(55, fib(10));
            Assert.AreEqual(12586269025L, fib(50));
        }

        [Test]
        public void Recursive1ArgArray()
        {
            var fib = Memotize.CreateMemotizedRecursive<long>(0, 60, (recurse, n) =>
            {
                if (n <= 0) return 0;
                if (n == 1) return 1;
                return recurse(n - 1) + recurse(n - 2);
            });

            Assert.AreEqual(55, fib(10));
            Assert.AreEqual(12586269025L, fib(50));
        }

        [Test]
        public void Recursive2ArgDictionary()
        {
            var choose = Memotize.CreateMemotizedRecursive<int, int, long>((recurse, n, k) =>
            {
                if (k < 0 || k > n) return 0;
                if (k == 0 || k == n) return 1;
                return recurse(n - 1, k - 1) + recurse(n - 1, k);
            });

            Assert.AreEqual(1, choose(5, 0));
            Assert.AreEqual(10, choose(5, 2));
            Assert.AreEqual(252, choose(10, 5));
        }

        [Test]
        public void Recursive2ArgArray()
        {
            var gridPaths = Memotize.CreateMemotizedRecursive<long>(
                0, 20,
                0, 20,
                (recurse, r, c) =>
                {
                    if (r == 0 || c == 0) return 1;
                    return recurse(r - 1, c) + recurse(r, c - 1);
                });

            Assert.AreEqual(1, gridPaths(0, 5));
            Assert.AreEqual(6, gridPaths(2, 2));
            Assert.AreEqual(184756, gridPaths(10, 10));
        }
    }
}


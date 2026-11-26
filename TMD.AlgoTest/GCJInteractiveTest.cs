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
using System.IO;
using NUnit.Framework;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class GCJInteractiveTest
    {
        [Test]
        public void NormalMultiRoundSessionWithEofCheck()
        {
            GCJInteractive.Run(
                solver: io =>
                {
                    int n = int.Parse(io.ReadRequiredLine());
                    for (int i = 0; i < n; i++)
                    {
                        int x = int.Parse(io.ReadRequiredLine());
                        io.WriteLine((x * 2).ToString());
                    }
                },
                judge: io =>
                {
                    io.WriteLine("3");
                    for (int i = 1; i <= 3; i++)
                    {
                        io.WriteLine((i * 10).ToString());
                        Assert.AreEqual((i * 20).ToString(), io.ReadRequiredLine());
                    }

                    Assert.IsNull(io.ReadLine(), "Expected EOF after final response.");
                });
        }

        [Test]
        public void JudgeFailureAbortsBlockedSolverImmediately()
        {
            AssertionException ex = Assert.Throws<AssertionException>(() =>
                GCJInteractive.Run(
                    solver: io =>
                    {
                        io.WriteLine("wrong");
                        // Blocked reading a response the judge will never send.
                        _ = io.ReadRequiredLine();
                    },
                    judge: io =>
                    {
                        string got = io.ReadRequiredLine();
                        Assert.AreEqual("right", got, "Judge caught wrong answer");
                    },
                    timeout: TimeSpan.FromSeconds(5)))!;

            StringAssert.Contains("Judge caught wrong answer", ex.Message);
        }

        [Test]
        public void SolverFailureAbortsBlockedJudgeImmediately()
        {
            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
                GCJInteractive.Run(
                    solver: io =>
                    {
                        _ = io.ReadRequiredLine();
                        throw new InvalidOperationException("Solver crashed");
                    },
                    judge: io =>
                    {
                        io.WriteLine("1");
                        _ = io.ReadRequiredLine();
                    },
                    timeout: TimeSpan.FromSeconds(5)))!;

            Assert.AreEqual("Solver crashed", ex.Message);
        }

        [Test]
        public void DeliberateDeadlockThrowsTimeoutException()
        {
            Assert.Throws<TimeoutException>(() =>
                GCJInteractive.Run(
                    solver: io => _ = io.ReadRequiredLine(),
                    judge: io => _ = io.ReadRequiredLine(),
                    timeout: TimeSpan.FromMilliseconds(150)));
        }

        [Test]
        public void FromConsoleAndUnexpectedEof()
        {
            using GCJInteractiveChannel consoleCh = GCJInteractiveChannel.FromConsole();
            Assert.IsNotNull(consoleCh);

            Assert.Throws<EndOfStreamException>(() =>
                GCJInteractive.Run(
                    solver: io => _ = io.ReadRequiredLine(),
                    judge: _ => { },
                    timeout: TimeSpan.FromSeconds(5)));
        }
    }
}

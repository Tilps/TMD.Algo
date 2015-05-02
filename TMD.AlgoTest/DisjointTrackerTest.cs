#region License

/*
Copyright (c) 2008, the TMD.Algo authors.
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of TMD.Algo nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#endregion

using NUnit.Framework;
using TMD.Algo.Collections.Generic;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class DisjointTrackerTest
    {
        [Test]
        public void Basic()
        {
            DisjointTracker<int> tracker = new DisjointTracker<int>();
            MappedDisjointTracker<int> tracker2 = new MappedDisjointTracker<int>(i => i, 0, 10000);
            RevertibleDisjointTracker<int> tracker3 = new RevertibleDisjointTracker<int>();
            for (int i = 0; i < 10000; i++)
            {
                tracker.Add(i);
                tracker2.Add(i);
                tracker3.Add(i);
            }
            for (int i = 2; i <= 100; i++)
            {
                for (int j = 2*i; j < 10000; j += i)
                {
                    tracker.Union(2*i, j);
                    tracker2.Union(2*i, j);
                    tracker3.Union(2*i, j);
                }
            }
            int allComps1 = -1;
            int allComps2 = -1;
            int allComps3 = -1;
            for (int i = 0; i < 10000; i++)
            {
                int rep1 = tracker.GetRepresentative(i);
                if (rep1 != i)
                {
                    if (allComps1 == -1)
                        allComps1 = rep1;
                    else
                        Assert.AreEqual(allComps1, rep1);
                    Assert.IsFalse(IsPrime(i));
                }
                else if (i > 1 && i != allComps1 && allComps1 != -1)
                    Assert.IsTrue(IsPrime(i));
                int rep2 = tracker2.GetRepresentative(i);
                if (rep2 != i)
                {
                    if (allComps2 == -1)
                        allComps2 = rep2;
                    else
                        Assert.AreEqual(allComps2, rep2);
                    Assert.IsFalse(IsPrime(i));
                }
                else if (i > 1 && i != allComps2 && allComps2 != -1)
                    Assert.IsTrue(IsPrime(i));
                int rep3 = tracker3.GetRepresentative(i);
                if (rep3 != i)
                {
                    if (allComps3 == -1)
                        allComps3 = rep3;
                    else
                        Assert.AreEqual(allComps3, rep3);
                    Assert.IsFalse(IsPrime(i));
                }
                else if (i > 1 && i != allComps3 && allComps3 != -1)
                    Assert.IsTrue(IsPrime(i));
            }
        }

        private bool IsPrime(int i)
        {
            for (int j = 2; j*j <= i; j++)
                if (i%j == 0)
                    return false;

            return true;
        }

        [Test]
        public void RevertibleTracking()
        {
            RevertibleDisjointTracker<int> tracker = new RevertibleDisjointTracker<int>();
            tracker.Add(1);
            tracker.Add(2);
            tracker.Add(3);
            tracker.Add(4);
            tracker.Add(5);
            tracker.Add(6);
            tracker.Union(2, 3);
            tracker.Union(4, 5);
            tracker.Union(4, 6);
            tracker.Union(6, 3);
            Assert.IsTrue(tracker.GetRepresentative(2) == tracker.GetRepresentative(5));
            tracker.Revert();
            Assert.IsFalse(tracker.GetRepresentative(2) == tracker.GetRepresentative(5));
            Assert.IsTrue(tracker.GetRepresentative(6) == tracker.GetRepresentative(5));
            Assert.IsTrue(tracker.GetRepresentative(2) == tracker.GetRepresentative(3));
            tracker.Revert();
            Assert.IsFalse(tracker.GetRepresentative(2) == tracker.GetRepresentative(5));
            Assert.IsFalse(tracker.GetRepresentative(6) == tracker.GetRepresentative(5));
            Assert.IsTrue(tracker.GetRepresentative(4) == tracker.GetRepresentative(5));
            Assert.IsTrue(tracker.GetRepresentative(2) == tracker.GetRepresentative(3));
            tracker.Revert();
            tracker.Revert();
            for (int i = 1; i < 6; i++)
            {
                for (int j = i + 1; j <= 6; j++)
                {
                    Assert.IsFalse(tracker.GetRepresentative(i) == tracker.GetRepresentative(j));
                }
            }
        }
    }
}
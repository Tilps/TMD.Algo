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

using System.Collections.Generic;
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

        [Test]
        public void UnionOutputsAndMetadataMerging()
        {
            DisjointTracker<int> dictTracker = new();
            MappedDisjointTracker<int> mappedTracker = new(static i => i, 0, 5);
            RevertibleDisjointTracker<int> revertibleTracker = new();

            Dictionary<int, int> dictSums = [];
            int[] mappedMasks = [1, 2, 4, 8, 16, 32];
            int[] revSums = [10, 20, 30, 40, 50, 60];

            for (int i = 0; i < 6; i++)
            {
                dictTracker.Add(i);
                dictSums[i] = (i + 1) * 10;
                mappedTracker.Add(i);
                revertibleTracker.Add(i);
            }

            Assert.IsFalse(dictTracker.AreConnected(0, 1));
            Assert.IsFalse(mappedTracker.AreConnected(0, 1));
            Assert.IsFalse(revertibleTracker.AreConnected(0, 1));

            // Merging distinct sets returns true and outputs surviving/absorbed roots.
            Assert.IsTrue(dictTracker.Union(0, 1, out int dRoot, out int dAbsorbed));
            Assert.AreNotEqual(dRoot, dAbsorbed);
            Assert.AreEqual(dRoot, dictTracker.GetRepresentative(0));
            Assert.AreEqual(dRoot, dictTracker.GetRepresentative(1));
            Assert.IsTrue(dictTracker.AreConnected(0, 1));
            dictSums[dRoot] += dictSums[dAbsorbed];
            Assert.AreEqual(30, dictSums[dictTracker.GetRepresentative(0)]);

            Assert.IsTrue(dictTracker.Union(2, 3, out dRoot, out dAbsorbed));
            dictSums[dRoot] += dictSums[dAbsorbed];
            Assert.IsTrue(dictTracker.Union(1, 3, out dRoot, out dAbsorbed));
            dictSums[dRoot] += dictSums[dAbsorbed];
            Assert.AreEqual(100, dictSums[dictTracker.GetRepresentative(2)]);

            // No-op union returns false with both out parameters set to the existing representative.
            Assert.IsFalse(dictTracker.Union(0, 2, out int dSameRoot, out int dSameAbsorbed));
            Assert.AreEqual(dictTracker.GetRepresentative(0), dSameRoot);
            Assert.AreEqual(dSameRoot, dSameAbsorbed);
            Assert.AreEqual(100, dictSums[dictTracker.GetRepresentative(0)]);

            // MappedDisjointTracker with bitmask merging.
            Assert.IsTrue(mappedTracker.Union(0, 2, out int mRoot, out int mAbsorbed));
            mappedMasks[mRoot] |= mappedMasks[mAbsorbed];
            Assert.AreEqual(5, mappedMasks[mappedTracker.GetRepresentative(0)]);
            Assert.IsTrue(mappedTracker.AreConnected(0, 2));
            Assert.IsFalse(mappedTracker.Union(0, 2, out int mSameRoot, out int mSameAbsorbed));
            Assert.AreEqual(mappedTracker.GetRepresentative(2), mSameRoot);
            Assert.AreEqual(mSameRoot, mSameAbsorbed);
            Assert.IsTrue(mappedTracker.Union(2, 4, out mRoot, out mAbsorbed));
            mappedMasks[mRoot] |= mappedMasks[mAbsorbed];
            Assert.AreEqual(21, mappedMasks[mappedTracker.GetRepresentative(4)]);

            // RevertibleDisjointTracker including no-op Union followed by Revert().
            Assert.IsTrue(revertibleTracker.Union(0, 1, out int rRoot, out int rAbsorbed));
            revSums[rRoot] += revSums[rAbsorbed];
            Assert.AreEqual(30, revSums[revertibleTracker.GetRepresentative(1)]);
            Assert.IsTrue(revertibleTracker.AreConnected(0, 1));

            // No-op Union still pushes an undo frame balanced by Revert().
            Assert.IsFalse(revertibleTracker.Union(0, 1, out int rSameRoot, out int rSameAbsorbed));
            Assert.AreEqual(revertibleTracker.GetRepresentative(0), rSameRoot);
            Assert.AreEqual(rSameRoot, rSameAbsorbed);
            Assert.IsTrue(revertibleTracker.Union(1, 2));
            Assert.IsTrue(revertibleTracker.AreConnected(0, 2));

            revertibleTracker.Revert(); // Undoes Union(1, 2)
            Assert.IsFalse(revertibleTracker.AreConnected(0, 2));
            Assert.IsTrue(revertibleTracker.AreConnected(0, 1));

            revertibleTracker.Revert(); // Undoes no-op Union(0, 1)
            Assert.IsTrue(revertibleTracker.AreConnected(0, 1));

            revertibleTracker.Revert(); // Undoes Union(0, 1)
            Assert.IsFalse(revertibleTracker.AreConnected(0, 1));
        }
    }
}
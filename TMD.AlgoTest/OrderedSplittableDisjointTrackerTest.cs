#region License
/*
Copyright (c) 2012, Gareth Pearce (www.themissingdocs.net)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of the www.themissingdocs.net nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Collections.Generic;
using System.Diagnostics;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class OrderedSplittableDisjointTrackerTest
    {

        [Test]
        public void Basic()
        {
            OrderedSplittableDisjointTracker<int, object> tracker = new OrderedSplittableDisjointTracker<int, object>();
            tracker.Add(1, "A");
            tracker.Add(2, "B");
            tracker.Add(3, "C");
            tracker.Add(4, "D");
            tracker.Union(1, 2, (a, b) => "E");
            tracker.___TESTCheckTreesTEST();
            tracker.Union(3, 4, (a, b) => "F");
            tracker.___TESTCheckTreesTEST();
            tracker.Union(2, 4, (a, b) => "G");
            tracker.___TESTCheckTreesTEST();
            Assert.IsTrue(tracker.GetRepresentative(1) == tracker.GetRepresentative(3));
            tracker.Split(2, a => "H");
            tracker.___TESTCheckTreesTEST();
            Assert.IsTrue(tracker.GetRepresentative(1) == tracker.GetRepresentative(2));
            Assert.IsTrue(tracker.GetRepresentative(3) == tracker.GetRepresentative(4));
            Assert.IsFalse(tracker.GetRepresentative(2) == tracker.GetRepresentative(3));
            tracker.Union(2, 3, (a, b) => "I");
            tracker.___TESTCheckTreesTEST();
            tracker.Split(3, a => "J");
            tracker.___TESTCheckTreesTEST();
            Assert.IsTrue(tracker.GetRepresentative(1) == tracker.GetRepresentative(2));
            Assert.IsFalse(tracker.GetRepresentative(3) == tracker.GetRepresentative(4));
            Assert.IsTrue(tracker.GetRepresentative(2) == tracker.GetRepresentative(3));
        }

        [Test]
        public void Extended()
        {
            OrderedSplittableDisjointTracker<int, object> tracker = new OrderedSplittableDisjointTracker<int, object>();
            Random rnd = new Random(143523);
            int nodeCount = 130;
            for (int i = 0; i < nodeCount; i++)
            {
                tracker.Add(i, i.ToString());
            }
            tracker.___TESTCheckTreesTEST();
            int counter = nodeCount * 2;
            for (int i = 0; i < 100000; i++)
            {
                //if (i == 15832)
                //    Debugger.Break();
                int nextAction = rnd.Next(2);
                if (nextAction == 0)
                {
                    int nextSpot = rnd.Next(nodeCount-1);
                    tracker.Union(nextSpot, nextSpot + 1, (a, b) => { counter++; return counter.ToString(); });
                    Assert.IsTrue(tracker.GetRepresentative(nextSpot) == tracker.GetRepresentative(nextSpot + 1));
                }
                else
                {
                    int nextSpot = rnd.Next(nodeCount);
                    tracker.Split(nextSpot, (a) => { counter++; return counter.ToString(); });
                    if (nextSpot < nodeCount - 1)
                        Assert.IsTrue(tracker.GetRepresentative(nextSpot) != tracker.GetRepresentative(nextSpot + 1));
                }
                tracker.___TESTCheckTreesTEST();
            }
        }

    }
}

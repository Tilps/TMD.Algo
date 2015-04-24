﻿#region License
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Collections.Generic;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class DequeueTest
    {

        [Test]
        public void Simple()
        {
            List<int> baseline = new List<int>();
            Dequeue<int> dequeue = new Dequeue<int>();
            Random rnd = new Random();
            for (int i = 0; i < 10000; i++)
            {
                switch (rnd.Next(5))
                {
                    case 0:
                        {
                            int val = rnd.Next(10000);
                            baseline.Add(val);
                            dequeue.PushBack(val);
                        }
                        break;
                    case 1:
                        {
                            int val = rnd.Next(10000);
                            baseline.Insert(0, val);
                            dequeue.PushFront(val);
                        }
                        break;
                    case 2:
                        if (baseline.Count > 0)
                        {
                            Assert.AreEqual(baseline[0], dequeue.Front);
                            Assert.AreEqual(baseline[0], dequeue.PopFront());
                            baseline.RemoveAt(0);
                        }
                        break;
                    case 3:
                        if (baseline.Count > 0)
                        {
                            Assert.AreEqual(baseline[baseline.Count - 1], dequeue.Back);
                            Assert.AreEqual(baseline[baseline.Count - 1], dequeue.PopBack());
                            baseline.RemoveAt(baseline.Count-1);
                        }
                        break;
                    case 4:
                        baseline.Clear();
                        dequeue.Clear();
                        break;
                }
                Verify(baseline, dequeue);
            }
        }

        private void Verify(List<int> baseline, Dequeue<int> dequeue)
        {
            Assert.AreEqual(baseline.Count, dequeue.Count);
            int index = 0;
            foreach (int i in dequeue)
            {
                Assert.AreEqual(baseline[index], i);
                index++;
            }
        }
    }
}

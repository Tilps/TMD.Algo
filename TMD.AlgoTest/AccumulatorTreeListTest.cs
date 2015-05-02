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

using System;
using System.Collections.Generic;
using NUnit.Framework;
using TMD.Algo.Collections.Generic;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class AccumulatorTreeListTest
    {
        [Test]
        public void BasicTests()
        {
            // Tree list must return same results as List<T>
            List<long> control = new List<long>();
            AccumulatorTreeList<long> test = new AccumulatorTreeList<long>(new LongAdder());
            Random rnd = new Random();
            for (int i = 0; i < 1000; i++)
            {
                switch (rnd.Next(4))
                {
                    case 0:
                    {
                        long value = rnd.Next();
                        control.Add(value);
                        test.Add(value);
                        Compare(control, test);
                    }
                        break;
                    case 1:
                    {
                        long value = rnd.Next();
                        int index = rnd.Next(control.Count);
                        control.Insert(index, value);
                        test.Insert(index, value);
                        Compare(control, test);
                    }
                        break;
                    case 2:
                    {
                        long target;
                        if (control.Count > 0 && rnd.Next(2) == 1)
                            target = control[rnd.Next(control.Count)];
                        else
                            target = rnd.Next();
                        Assert.AreEqual(control.Contains(target), test.Contains(target), "Contains test failed.");
                        Assert.AreEqual(control.IndexOf(target), test.IndexOf(target), "Contains test failed.");
                    }
                        break;
                    case 3:
                    {
                        long target;
                        if (control.Count > 0 && rnd.Next(2) == 1)
                            target = control[rnd.Next(control.Count)];
                        else
                            target = rnd.Next();
                        Assert.AreEqual(control.Remove(target), test.Remove(target), "Target removal not the same.");
                        Compare(control, test);
                    }
                        break;
                    case 4:
                    {
                        if (control.Count > 0)
                        {
                            int index = rnd.Next(control.Count);
                            control.RemoveAt(index);
                            test.RemoveAt(index);
                            Compare(control, test);
                        }
                    }
                        break;
                }
            }
        }

        private void Compare(List<long> control, AccumulatorTreeList<long> test)
        {
            Assert.AreEqual(control.Count, test.Count, "Counts fail to match after modification.");
            long total = 0;
            for (int i = 0; i < control.Count; i++)
            {
                Assert.AreEqual(control[i], test[i], "Contents fails to match after modification.");
                total += control[i];
                Assert.AreEqual(total, test.GetSum(i), "Accumulations fail to match after modification.");
            }
            IEnumerator<long> en = test.GetEnumerator();
            for (int i = 0; i < control.Count; i++)
            {
                en.MoveNext();
                Assert.AreEqual(control[i], en.Current, "Contents fails to match after modification.");
            }
        }
    }
}
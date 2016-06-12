#region License

/*
Copyright (c) 2016, the TMD.Algo authors.
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
    public class DictionaryTest
    {
        [Test]
        public void BasicTests()
        {
            // Special Dictionaries must match Dictionary in behavior.
            Dictionary<int, int> control = new Dictionary<int, int>();
            RangeSentinalDictionary<int, int> test = new RangeSentinalDictionary<int, int>(1000, a=>a, a=>(int)a, -1);
            Random rnd = new Random();
            for (int i = 0; i < 10000; i++)
            {
                switch (rnd.Next(2))
                {
                    case 0:
                    {
                        int key = rnd.Next(1000);
                        int value = rnd.Next();
                        if (!control.ContainsKey(key))
                        {
                            control.Add(key, value);
                            test.Add(key, value);
                        }
                        else
                        {
                            control[key] = value;
                            test[key] = value;
                        }
                        Compare(control, test);
                    }
                        break;
                    case 1:
                    {
                        if (rnd.Next(100) == 0)
                        {
                            control.Clear();
                            test.Clear();
                        }
                        else
                        {
                            int key = rnd.Next(1000);
                            Assert.AreEqual(control.Remove(key), test.Remove(key), "Target removal not the same.");
                        }
                        Compare(control, test);
                    }
                        break;
                }
            }
        }

        private void Compare(IDictionary<int, int> control, IDictionary<int, int> test)
        {
            Assert.AreEqual(control.Count, test.Count, "Counts fail to match after modification.");
            foreach (var item in control)
            {
                Assert.IsTrue(test.ContainsKey(item.Key));
                int result;
                Assert.IsTrue(test.TryGetValue(item.Key, out result));
                Assert.AreEqual(item.Value, result);
                Assert.AreEqual(item.Value, test[item.Key]);
            }
            foreach (var item in test)
            {
                Assert.IsTrue(test.ContainsKey(item.Key));
                int result;
                Assert.IsTrue(test.TryGetValue(item.Key, out result));
                Assert.AreEqual(item.Value, result);
                Assert.AreEqual(item.Value, test[item.Key]);
                Assert.IsTrue(control.ContainsKey(item.Key));
            }
        }
    }
}
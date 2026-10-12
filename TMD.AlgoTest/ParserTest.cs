#region License

/*
Copyright (c) 2014, the TMD.Algo authors.
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of TMD.Algo nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#endregion

using System;
using System.Numerics;
using NUnit.Framework;
using TMD.Algo.Text;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class ParserTest
    {
        [Test]
        public void ParseSingleValues()
        {
            "42".Parse(out int i);
            Assert.AreEqual(42, i);

            "1234567890123".Parse(out long l);
            Assert.AreEqual(1234567890123L, l);

            "3.14159".Parse(out double d);
            Assert.AreEqual(3.14159, d, 0.000001);

            "2.718".Parse(out float f);
            Assert.AreEqual(2.718f, f, 0.0001f);

            "hello".Parse(out string s);
            Assert.AreEqual("hello", s);

            "99999999999999999999999999999999".Parse(out BigInteger bi);
            Assert.AreEqual(BigInteger.Parse("99999999999999999999999999999999"), bi);

            "100".Parse(out short sh);
            Assert.AreEqual((short)100, sh);

            "200".Parse(out byte b);
            Assert.AreEqual((byte)200, b);

            "12345.67".Parse(out decimal dec);
            Assert.AreEqual(12345.67m, dec);

            "01:02:03".Parse(out TimeSpan ts);
            Assert.AreEqual(new TimeSpan(1, 2, 3), ts);
        }

        [Test]
        public void ParseTuples()
        {
            "10 20".Parse(out int a, out int b);
            Assert.AreEqual(10, a);
            Assert.AreEqual(20, b);

            "1 2 3".Parse(out int t1, out int t2, out int t3);
            Assert.AreEqual(1, t1);
            Assert.AreEqual(2, t2);
            Assert.AreEqual(3, t3);

            "1 2 3 4".Parse(out int q1, out int q2, out int q3, out int q4);
            Assert.AreEqual(1, q1);
            Assert.AreEqual(2, q2);
            Assert.AreEqual(3, q3);
            Assert.AreEqual(4, q4);

            "1 2 3 4 5".Parse(out int p1, out int p2, out int p3, out int p4, out int p5);
            Assert.AreEqual(1, p1);
            Assert.AreEqual(5, p5);

            "1 2 3 4 5 6".Parse(out int h1, out int h2, out int h3, out int h4, out int h5, out int h6);
            Assert.AreEqual(1, h1);
            Assert.AreEqual(6, h6);

            "1 2 3 4 5 6 7".Parse(out int s1, out int s2, out int s3, out int s4, out int s5, out int s6, out int s7);
            Assert.AreEqual(1, s1);
            Assert.AreEqual(7, s7);

            "1 2 3 4 5 6 7 8".Parse(out int o1, out int o2, out int o3, out int o4, out int o5, out int o6, out int o7, out int o8);
            Assert.AreEqual(1, o1);
            Assert.AreEqual(8, o8);
        }

        [Test]
        public void ParseSpanDirectly()
        {
            ReadOnlySpan<char> span = "42 99".AsSpan();
            span.Parse(out int a, out int b);
            Assert.AreEqual(42, a);
            Assert.AreEqual(99, b);
        }

        [Test]
        public void ParseArray()
        {
            "10 20 30 40 50".Parse(out int[] arr);
            Assert.AreEqual(5, arr.Length);
            Assert.AreEqual(10, arr[0]);
            Assert.AreEqual(20, arr[1]);
            Assert.AreEqual(30, arr[2]);
            Assert.AreEqual(40, arr[3]);
            Assert.AreEqual(50, arr[4]);

            "one two three".Parse(out string[] words);
            Assert.AreEqual(3, words.Length);
            Assert.AreEqual("one", words[0]);
            Assert.AreEqual("two", words[1]);
            Assert.AreEqual("three", words[2]);
        }

        [Test]
        public void ParseValidation()
        {
            Assert.Throws<ArgumentNullException>(() => ((string)null).Parse(out int _));
            Assert.Throws<ArgumentNullException>(() => string.Empty.Parse(out int _, out int _));
            Assert.Throws<ArgumentException>(() => "10".Parse(out int _, out int _));
            Assert.Throws<ArgumentException>(() => "10 20 30".Parse(out int _, out int _));
        }
    }
}

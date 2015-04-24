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
using System.Linq;
using System.Text;
using NUnit.Framework;
using TMD.Algo.Algorithms;
using TMD.Algo.Collections.Generic;

namespace TMD.AlgoTest
{
    [TestFixture]
    public class GeometryTest
    {

        [Test]
        public void Basic()
        {
            List<KeyValuePair<long, long>> points = new List<KeyValuePair<long,long>>();
            points.Add(new KeyValuePair<long,long>(0, 0));
            points.Add(new KeyValuePair<long,long>(int.MaxValue, 0));
            points.Add(new KeyValuePair<long,long>(0, int.MaxValue));
            points.Add(new KeyValuePair<long,long>(int.MaxValue, int.MaxValue));
            Random rnd = new Random();
            for (int i = 0; i < 1000; i++)
            {
                points.Add(new KeyValuePair<long, long>(rnd.Next(1, int.MaxValue - 1), rnd.Next(1, int.MaxValue - 1)));
            }
            for (int i = 0; i < 1000; i++)
            {
                int a = rnd.Next(points.Count);
                int b = rnd.Next(points.Count);
                KeyValuePair<long, long> tmp = points[a];
                points[a] = points[b];
                points[b] = tmp;
            }
            List<KeyValuePair<long, long>> hull = Geometry.ConvexHull(points);
            Assert.AreEqual(4, hull.Count);
            Assert.AreEqual(0, hull[0].Key);
            Assert.AreEqual(0, hull[0].Value);
            Assert.AreEqual(int.MaxValue, hull[1].Key);
            Assert.AreEqual(0, hull[1].Value);
            Assert.AreEqual(int.MaxValue, hull[2].Key);
            Assert.AreEqual(int.MaxValue, hull[2].Value);
            Assert.AreEqual(0, hull[3].Key);
            Assert.AreEqual(int.MaxValue, hull[3].Value);
        }
    }
}

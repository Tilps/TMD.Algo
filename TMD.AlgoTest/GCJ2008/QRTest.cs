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
using System.Collections.Generic;
using NUnit.Framework;
using TMD.Algo.Algorithms.Generic;
using TMD.Algo.Collections.Generic;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest.GCJ2008
{
    [TestFixture]
    public class QRTest : BaseGCJTest
    {
        private Func<int> Q1Solver(GCJTestCase test)
        {
            int q;
            test.Get(out q);
            string[] searchEngines;
            test.GetLines(q, out searchEngines);
            int s;
            test.Get(out s);
            string[] searchTerms;
            test.GetLines(s, out searchTerms);
            return () => Q1Solver(searchEngines, searchTerms);
        }

        private int Q1Solver(string[] searchEngines, string[] searchTerms)
        {
            HashSet<string> engines = new HashSet<string>(searchEngines);
            HashSet<string> can = new HashSet<string>(engines);
            int switchCount = 0;
            foreach (string searchTerm in searchTerms)
            {
                can.Remove(searchTerm);
                if (can.Count == 0)
                {
                    switchCount++;
                    can = new HashSet<string>(engines);
                    can.Remove(searchTerm);
                }
            }
            return switchCount;
        }

        [Test]
        public void Q1Sample()
        {
            string input =
                #region SampleInput 

                @"2
5
Yeehaw
NSM
Dont Ask
B9
Googol
10
Yeehaw
Yeehaw
Googol
B9
Googol
NSM
B9
NSM
Dont Ask
Googol
5
Yeehaw
NSM
Dont Ask
B9
Googol
7
Googol
Dont Ask
NSM
NSM
Yeehaw
Yeehaw
Googol
";

            #endregion
            string expectedOutput = @"Case #1: 1
Case #2: 0";

            var result = GCJ.___TESTRunTestsTEST(input, Q1Solver);
            Validate(expectedOutput, result);
        }

        [Test]
        public void Q1Small()
        {
            string input = Load("Q1Small.txt");
            string expectedOutput =
                #region SmallOutputFile

                @"Case #1: 0
Case #2: 0
Case #3: 1
Case #4: 1
Case #5: 3
Case #6: 3
Case #7: 99
Case #8: 48
Case #9: 10
Case #10: 3
Case #11: 0
Case #12: 1
Case #13: 5
Case #14: 2
Case #15: 0
Case #16: 1
Case #17: 2
Case #18: 3
Case #19: 1
Case #20: 2";

            #endregion

            var result = GCJ.___TESTRunTestsTEST(input, Q1Solver);
            Validate(expectedOutput, result);
        }

        [Test]
        public void Q1Large()
        {
            SlowTest(Q1Solver);
        }

        private Func<int[]> Q2Solver(GCJTestCase test)
        {
            int t;
            test.Get(out t);
            int na, nb;
            test.Get(out na, out nb);
            TimeSpan[][] atob;
            test.GetMatrix(na, out atob);
            TimeSpan[][] btoa;
            test.GetMatrix(nb, out btoa);
            return () => Q2Solver(t, atob, btoa);
        }

        private int[] Q2Solver(int t, TimeSpan[][] atob, TimeSpan[][] btoa)
        {
            int neededAtA = 0;
            int neededAtB = 0;
            int atA = 0;
            int atB = 0;
            TimeSpan turn = new TimeSpan(0, t, 0);
            var rc = ReverseComparer<TimeSpan>.Default;
            Heap<TimeSpan> aArrivals = new Heap<TimeSpan>(rc);
            Heap<TimeSpan> bArrivals = new Heap<TimeSpan>(rc);
            Heap<TimeSpan> aDepartures = new Heap<TimeSpan>(rc);
            Heap<TimeSpan> bDepartures = new Heap<TimeSpan>(rc);
            foreach (TimeSpan[] trip in atob)
            {
                aDepartures.Add(trip[0]);
                bArrivals.Add(trip[1] + turn);
            }
            foreach (TimeSpan[] trip in btoa)
            {
                bDepartures.Add(trip[0]);
                aArrivals.Add(trip[1] + turn);
            }
            TimeSpan departure;
            while (aDepartures.TryPopFront(out departure))
            {
                TimeSpan arrival;
                while (aArrivals.TryGetFront(out arrival) && arrival <= departure)
                {
                    atA++;
                    aArrivals.PopFront();
                }
                if (atA == 0)
                {
                    neededAtA++;
                }
                else
                {
                    atA--;
                }
            }
            while (bDepartures.TryPopFront(out departure))
            {
                TimeSpan arrival;
                while (bArrivals.TryGetFront(out arrival) && arrival <= departure)
                {
                    atB++;
                    bArrivals.PopFront();
                }
                if (atB == 0)
                {
                    neededAtB++;
                }
                else
                {
                    atB--;
                }
            }

            return new[] {neededAtA, neededAtB};
        }

        [Test]
        public void Q2Sample()
        {
            string input =
            #region SampleInput

 @"2
5
3 2
09:00 12:00
10:00 13:00
11:00 12:30
12:02 15:00
09:00 10:30
2
2 0
09:00 09:01
12:00 12:02";

            #endregion
            string expectedOutput = @"Case #1: 2 2
Case #2: 2 0";

            var result = GCJ.___TESTRunTestsTEST(input, Q2Solver);
            Validate(expectedOutput, result);
        }

        [Test]
        public void Q2Small()
        {
            string input = Load("Q2Small.txt");
            string expectedOutput =
            #region SmallOutputFile

 @"Case #1: 0 2
Case #2: 1 0
Case #3: 2 0
Case #4: 4 2
Case #5: 3 1
Case #6: 3 2
Case #7: 3 5
Case #8: 1 10
Case #9: 8 7
Case #10: 7 7
Case #11: 7 11
Case #12: 1 1
Case #13: 2 0
Case #14: 3 6
Case #15: 13 0
Case #16: 0 8
Case #17: 6 8
Case #18: 2 2
Case #19: 6 3
Case #20: 8 4";

            #endregion

            var result = GCJ.___TESTRunTestsTEST(input, Q2Solver);
            Validate(expectedOutput, result);
        }
        [Test]
        public void Q2Large()
        {
            SlowTest(Q2Solver);
        }

        private Func<double> Q3Solver(GCJTestCase test)
        {
            double f, R, t, r, g;
            test.Get(out f, out R, out t, out r, out g);
            return () => Q3Solver(f, R, t, r, g);
        }

        private static double Q3Solver(double f, double R, double t, double r, double g)
        {
            if (2 * f >= g || R - t - f <= 0)
            {
                return 1.0;
            }
            double basis = Math.PI * R * R / 4.0;
            double accumulator = 0.0;
            // we do have holes.
            // fudge numbers to make it simpler.
            t += f;
            r += f;
            g -= 2 * f;
            double innerRadius = R - t;
            double innerRadiusSq = innerRadius * innerRadius;
            double step = 2 * r + g;
            double gSq = g * g;
            for (double lx = r; lx <= innerRadius; lx += step)
            {
                double rx = lx + g;
                double rxIntercept = Math.Sqrt(innerRadiusSq - rx*rx);
                // Accelerate the loop by jumping ahead a clearly safe amount.
                int yStepsBeforeRxIntercept = Math.Max(0, (int) (Math.Floor((rxIntercept - g - r)/step) - 1));
                accumulator += gSq*yStepsBeforeRxIntercept;
                for (double by = r + step*yStepsBeforeRxIntercept; by <= innerRadius; by += step)
                {
                    if (lx * lx + by * by > innerRadiusSq)
                        break;
                    double ty = by + g;
                    if (rx * rx + ty * ty <= innerRadiusSq)
                    {
                        accumulator += gSq;
                        continue;
                    }
                    bool tlIn = lx * lx + ty * ty <= innerRadiusSq;
                    bool brIn = rx * rx + by * by <= innerRadiusSq;
                    double xc1;
                    double xc2;
                    double yc1;
                    double yc2;
                    // calculate the triangle truncated square area.
                    // 4 cases to handle.
                    // first calc intercepts.
                    if (tlIn)
                    {
                        yc1 = ty;
                        xc1 = Math.Sqrt(innerRadiusSq - yc1 * yc1);
                    }
                    else
                    {
                        xc1 = lx;
                        yc1 = Math.Sqrt(innerRadiusSq - xc1 * xc1);
                    }
                    if (brIn)
                    {
                        xc2 = rx;
                        yc2 = rxIntercept;
                    }
                    else
                    {
                        yc2 = by;
                        xc2 = Math.Sqrt(innerRadiusSq - yc2 * yc2);
                    }
                    if (tlIn)
                    {
                        if (brIn)
                        {
                            accumulator += gSq - (rx - xc1) * (ty - yc2) / 2.0;
                        }
                        else
                        {
                            accumulator += g * (xc1 - lx) + (xc2 - xc1) * g / 2.0;
                        }
                    }
                    else
                    {
                        if (brIn)
                        {
                            accumulator += g * (yc2 - by) + (yc1 - yc2) * g / 2.0;
                        }
                        else
                        {
                            accumulator += (yc1 - by) * (xc2 - lx) / 2.0;
                        }
                    }
                    // add area of arc to cord of the triangle.
                    // arc to cord is r2[theta-sin(theta)]/2 where theta = 2 arcsin(c/[2r]) where c is the cord length
                    double a = xc2 - xc1;
                    double b = yc1 - yc2;
                    double c = Math.Sqrt(a * a + b * b);
                    double theta = 2.0 * Math.Asin(c / 2.0 / innerRadius);
                    double area = innerRadiusSq * (theta - Math.Sin(theta)) / 2.0;
                    accumulator += area;
                }
            }
            return (basis - accumulator) / basis;
        }

        [Test]
        public void Q3Sample()
        {
            string input =
            #region SampleInput

 @"5
0.25 1.0 0.1 0.01 0.5
0.25 1.0 0.1 0.01 0.9
0.00001 10000 0.00001 0.00001 1000
0.4 10000 0.00001 0.00001 700
1 100 1 1 10";

            #endregion
            string expectedOutput = @"Case #1: 1.000000
Case #2: 0.910015
Case #3: 0.000000
Case #4: 0.002371
Case #5: 0.573972";

            var result = GCJ.___TESTRunTestsTEST(input, Q3Solver);
            Validate(expectedOutput, result, 1e-6);
        }

        [Test]
        public void Q3Small()
        {
            string input = Load("Q3Small.txt");
            string expectedOutput =
            #region SmallOutputFile

 @"Case #1: 0.0150736670270347
Case #2: 0.00594685009102022
Case #3: 0.0357107768138673
Case #4: 0.00299429113384212
Case #5: 0.000366181185225535
Case #6: 0.113442764142921
Case #7: 0.00462766015298413
Case #8: 0.0102942217756396
Case #9: 0.000180327786086383";

            #endregion

            var result = GCJ.___TESTRunTestsTEST(input, Q3Solver);
            Validate(expectedOutput, result, 1e-6);
        }

        [Test]
        public void Q3Large()
        {
            Test(Q3Solver, 1e-6);
        }
    }
}

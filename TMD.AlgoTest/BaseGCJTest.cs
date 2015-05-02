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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using TMD.Algo.Competitions;

namespace TMD.AlgoTest
{
    public class BaseGCJTest
    {
        protected void Test<T>(Func<GCJTestCase, Func<T>> target, [CallerMemberName] string caller = null)
        {
            string input = Load(caller + ".txt");
            string expectedOutput = Load(caller + "Output.txt");

            var result = GCJ.___TESTRunTestsTEST(input, target);
            Validate(expectedOutput, result);
        }

        protected void SlowTest<T>(Func<GCJTestCase, Func<T>> target, [CallerMemberName] string caller = null)
        {
            string input = Load(caller + ".txt");
            string expectedOutput = Load(caller + "Output.txt");

            var result = GCJ.___TESTRunParallelTestsTEST(input, target);
            Validate(expectedOutput, result);
        }

        protected void Test<T>(Func<GCJTestCase, Func<T>> target, double error, [CallerMemberName] string caller = null)
        {
            string input = Load(caller + ".txt");
            string expectedOutput = Load(caller + "Output.txt");

            var result = GCJ.___TESTRunTestsTEST(input, target);
            Validate(expectedOutput, result, error);
        }

        protected void SlowTest<T>(Func<GCJTestCase, Func<T>> target, double error,
            [CallerMemberName] string caller = null)
        {
            string input = Load(caller + ".txt");
            string expectedOutput = Load(caller + "Output.txt");

            var result = GCJ.___TESTRunParallelTestsTEST(input, target);
            Validate(expectedOutput, result, error);
        }

        protected string Load(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = GetType().Namespace + "." + GetType().Name.Replace("Test", "") + "." + fileName;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        protected static void Validate(string expectedOutput, List<string> result)
        {
            DebugPoint(expectedOutput, result);
            string[] bits = expectedOutput.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
            Validate(bits, result);
        }

        protected static void Validate(string expectedOutput, List<string> result, double error)
        {
            DebugPoint(expectedOutput, result);
            string[] bits = expectedOutput.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
            Validate(bits, result, error);
        }

        private static void DebugPoint(string expectedOutput, List<string> result)
        {
            if (string.IsNullOrEmpty(expectedOutput))
            {
                string newfile = string.Join(Environment.NewLine, result);
                Debugger.Break();
                Console.Out.WriteLine(newfile);
            }
        }

        private static void Validate(string[] expected, List<string> result)
        {
            Assert.AreEqual(expected.Length, result.Count);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], result[i]);
            }
        }

        private static void Validate(string[] expected, List<string> result, double error)
        {
            Assert.AreEqual(expected.Length, result.Count);
            for (int i = 0; i < expected.Length; i++)
            {
                string[] bitsExpected = expected[i].Split(' ');
                string[] bitsResult = result[i].Split(' ');
                Assert.AreEqual(bitsExpected.Length, bitsResult.Length);
                for (int j = 0; j < bitsExpected.Length - 1; j++)
                    Assert.AreEqual(bitsExpected[j], bitsResult[j]);
                double expNum;
                bool expDouble = double.TryParse(bitsExpected[bitsExpected.Length - 1], out expNum);
                double resNum;
                bool resDouble = double.TryParse(bitsResult[bitsResult.Length - 1], out resNum);
                Assert.AreEqual(expDouble, resDouble);
                if (expDouble)
                {
                    Assert.IsTrue(IsApproximatelyEqual(expNum, resNum, error));
                }
                else
                {
                    Assert.AreEqual(bitsExpected.Last(), bitsResult.Last());
                }
            }
        }

        private static bool IsApproximatelyEqual(double expNum, double resNum, double error)
        {
            double diff = Math.Abs(expNum - resNum);
            if (diff <= error) return true;
            if (Math.Abs(expNum) <= error || Math.Abs(resNum) <= error) return false;
            return Math.Abs(diff/expNum) <= error || Math.Abs(diff/resNum) <= error;
        }
    }
}
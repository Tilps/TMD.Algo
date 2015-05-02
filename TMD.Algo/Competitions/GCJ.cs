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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace TMD.Algo.Competitions
{
    /// <summary>
    /// Provides methods to process a GCJ style problem input and produce a GCJ style problem output.
    /// </summary>
    public static class GCJ
    {
        /// <summary>
        /// Processes an input into test cases which are each handled by test case handler to produce an output.
        /// </summary>
        /// <param name="args">
        /// Optional arguments {input file name} {output file name}.
        /// Default value of input.txt and output.txt respectively.
        /// </param>
        /// <param name="testCaseHandler">
        /// Callback invoked once for each test case.
        /// </param>
        /// <typeparam name="T">
        /// Return type of the test case handler.
        /// </typeparam>
        public static void Run<T>(string[] args, Func<GCJTestCase, T> testCaseHandler)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            string inputFile = args.Length < 1 ? "input.txt" : args[0];
            string outputFile = args.Length < 2 ? "output.txt" : args[1];
            string[] lines = File.ReadAllLines(inputFile);
            var output = RunTests(lines, testCaseHandler);
            File.WriteAllLines(outputFile, output);
        }

        /// <summary>
        /// Processes an input into test cases which are each handled by test case handler to produce an output.
        /// Return values from testCaseHandler are invoked concurrently, so must be thread safe with respect to each other.
        /// </summary>
        /// <param name="args">
        /// Optional arguments {input file name} {output file name}.
        /// Default value of input.txt and output.txt respectively.
        /// </param>
        /// <param name="threads">
        /// Maximum number of threads to use. If less than 1 the value will be selected automatically.
        /// </param>
        /// <param name="testCaseHandler">
        /// Callback invoked once for each test case.  Should return a closure which can be executed to determine the test case result.
        /// </param>
        /// <typeparam name="T">
        /// Return type of the test case handler.
        /// </typeparam>
        public static void Run<T>(string[] args, int threads, Func<GCJTestCase, Func<T>> testCaseHandler)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            string inputFile = args.Length < 1 ? "input.txt" : args[0];
            string outputFile = args.Length < 2 ? "output.txt" : args[1];
            if (threads < 1) threads = Environment.ProcessorCount;
            string[] lines = File.ReadAllLines(inputFile);
            var output = RunTestsParallel(threads, lines, testCaseHandler);
            File.WriteAllLines(outputFile, output);
        }

        internal static List<string> ___TESTRunTestsTEST<T>(string lines, Func<GCJTestCase, T> testCaseHandler)
        {
            string[] bits = lines.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
            return RunTests(bits, testCaseHandler);
        }

        internal static List<string> ___TESTRunTestsTEST<T>(string lines, Func<GCJTestCase, Func<T>> testCaseHandler)
        {
            string[] bits = lines.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
            return RunTestsParallel(1, bits, testCaseHandler);
        }

        internal static List<string> ___TESTRunParallelTestsTEST<T>(string lines,
            Func<GCJTestCase, Func<T>> testCaseHandler)
        {
            string[] bits = lines.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);
            return RunTestsParallel(Environment.ProcessorCount, bits, testCaseHandler);
        }

        private static List<string> RunTests<T>(string[] lines, Func<GCJTestCase, T> testCaseHandler)
        {
            List<string> output = new List<string>();
            int testCount = int.Parse(lines[0]);
            GCJTestCase test = new GCJTestCase(1, lines);
            for (int i = 0; i < testCount; i++)
            {
                T result = testCaseHandler(test);
                output.Add($"Case #{i + 1}: {Format(result)}");
            }
            return output;
        }

        private static List<string> RunTestsParallel<T>(int threads, string[] lines,
            Func<GCJTestCase, Func<T>> testCaseHandler)
        {
            int testCount = int.Parse(lines[0]);
            string[] output = new string[testCount];
            GCJTestCase test = new GCJTestCase(1, lines);
            object counterLock = new object();
            int counter = 0;
            object dispatchLock = new object();
            int dispatched = 0;
            for (int i = 0; i < testCount; i++)
            {
                int localI = i;
                Func<T> toRun = testCaseHandler(test);
                while (true)
                {
                    lock (dispatchLock)
                    {
                        if (dispatched < threads)
                        {
                            dispatched++;
                            break;
                        }
                        Monitor.Wait(dispatchLock);
                    }
                }
                ThreadPool.QueueUserWorkItem(a =>
                {
                    T result = toRun();
                    output[localI] = $"Case #{localI + 1}: {Format(result)}";
                    lock (dispatchLock)
                    {
                        dispatched--;
                        Monitor.PulseAll(dispatchLock);
                    }
                    lock (counterLock)
                    {
                        counter++;
                        Monitor.PulseAll(counterLock);
                    }
                });
            }
            while (true)
            {
                lock (counterLock)
                {
                    if (counter == testCount) break;
                    Monitor.Wait(counterLock);
                }
            }
            return new List<string>(output);
        }

        private static string Format(object input)
        {
            // TODO: further specializations for char[] and char[][] and Tuples.
            var stringInput = input as string;
            if (stringInput != null)
            {
                return stringInput;
            }
            var stringEnumerable = input as IEnumerable<string>;
            if (stringEnumerable != null)
            {
                StringBuilder result = new StringBuilder();
                foreach (string i in stringEnumerable)
                {
                    result.AppendLine();
                    result.Append(i);
                }
                return result.ToString();
            }
            var enumerable = input as IEnumerable;
            if (enumerable != null)
            {
                return string.Join(" ", enumerable.Cast<object>().Select(Format));
            }
            return input.ToString();
        }
    }
}
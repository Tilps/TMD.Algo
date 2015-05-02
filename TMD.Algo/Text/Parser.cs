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
using System.Globalization;
using System.Numerics;

namespace TMD.Algo.Text
{
    /// <summary>
    /// Provides helper extension methods for splitting strings into typed items.
    /// Types supported include int/long/float/double/string/BigInteger/TimeSpan/DateTime.
    /// All types are parsed with their default parser for invariant culture.
    /// </summary>
    public static class Parser
    {
        /// <summary>
        /// Converts the value into the desired type.
        /// </summary>
        /// <param name="value">
        /// Value to parse.
        /// </param>
        /// <param name="result">
        /// Result to populate.
        /// </param>
        /// <typeparam name="T">
        /// Type of the result.
        /// </typeparam>
        public static void Parse<T>(this string value, out T result)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            result = Parse<T>(value);
        }

        /// <summary>
        /// Converts the value into an array of the desired type, assuming each value is separated by spaces.
        /// </summary>
        /// <param name="value">
        /// Value to parse.
        /// </param>
        /// <param name="results">
        /// Results to populate.
        /// </param>
        /// <typeparam name="T">
        /// Type of each element in the results.
        /// </typeparam>
        public static void Parse<T>(this string value, out T[] results)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            string[] bits = value.Split(' ');
            T[] conversion = new T[bits.Length];
            for (int i = 0; i < bits.Length; i++)
            {
                conversion[i] = Parse<T>(bits[i]);
            }
            results = conversion;
        }

        /// <summary>
        /// Converts a string containing a space separated pair into 2 typed results.
        /// </summary>
        /// <param name="value">
        /// Value to parse.
        /// </param>
        /// <param name="res1">
        /// First result.
        /// </param>
        /// <param name="res2">
        /// Second result.
        /// </param>
        /// <typeparam name="T1">
        /// Type of the first result.
        /// </typeparam>
        /// <typeparam name="T2">
        /// Type of the second result.
        /// </typeparam>
        public static void Parse<T1, T2>(this string value, out T1 res1, out T2 res2)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
            string[] bits = value.Split(' ');
            if (bits.Length != 2) throw new ArgumentException("Value has wrong number of components.", nameof(value));
            Parse(bits, 0, out res1, out res2);
        }

        /// <summary>
        /// Converts a string containing a space separated triple into 3 typed results.
        /// </summary>
        /// <param name="value">
        /// Value to parse.
        /// </param>
        /// <param name="res1">
        /// First result.
        /// </param>
        /// <param name="res2">
        /// Second result.
        /// </param>
        /// <param name="res3">
        /// Third result.
        /// </param>
        /// <typeparam name="T1">
        /// Type of the first result.
        /// </typeparam>
        /// <typeparam name="T2">
        /// Type of the second result.
        /// </typeparam>
        /// <typeparam name="T3">
        /// Type of the third result.
        /// </typeparam>
        public static void Parse<T1, T2, T3>(this string value, out T1 res1, out T2 res2, out T3 res3)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
            string[] bits = value.Split(' ');
            if (bits.Length != 3) throw new ArgumentException("Value has wrong number of components.", nameof(value));
            Parse(bits, 0, out res1, out res2, out res3);
        }

        /// <summary>
        /// Converts a string containing a space separated quadruple into 4 typed results.
        /// </summary>
        /// <param name="value">
        /// Value to parse.
        /// </param>
        /// <param name="res1">
        /// First result.
        /// </param>
        /// <param name="res2">
        /// Second result.
        /// </param>
        /// <param name="res3">
        /// Third result.
        /// </param>
        /// <param name="res4">
        /// Fourth result.
        /// </param>
        /// <typeparam name="T1">
        /// Type of the first result.
        /// </typeparam>
        /// <typeparam name="T2">
        /// Type of the second result.
        /// </typeparam>
        /// <typeparam name="T3">
        /// Type of the third result.
        /// </typeparam>
        /// <typeparam name="T4">
        /// Type of the fourth result.
        /// </typeparam>
        public static void Parse<T1, T2, T3, T4>(this string value, out T1 res1, out T2 res2, out T3 res3, out T4 res4)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
            string[] bits = value.Split(' ');
            if (bits.Length != 4) throw new ArgumentException("Value has wrong number of components.", nameof(value));
            Parse(bits, 0, out res1, out res2, out res3, out res4);
        }

        /// <summary>
        /// Converts a string containing a space separated quintuple into 5 typed results.
        /// </summary>
        /// <param name="value">
        /// Value to parse.
        /// </param>
        /// <param name="res1">
        /// First result.
        /// </param>
        /// <param name="res2">
        /// Second result.
        /// </param>
        /// <param name="res3">
        /// Third result.
        /// </param>
        /// <param name="res4">
        /// Fourth result.
        /// </param>
        /// <param name="res5">
        /// Fifth result.
        /// </param>
        /// <typeparam name="T1">
        /// Type of the first result.
        /// </typeparam>
        /// <typeparam name="T2">
        /// Type of the second result.
        /// </typeparam>
        /// <typeparam name="T3">
        /// Type of the third result.
        /// </typeparam>
        /// <typeparam name="T4">
        /// Type of the fourth result.
        /// </typeparam>
        /// <typeparam name="T5">
        /// Type of the fourth result.
        /// </typeparam>
        public static void Parse<T1, T2, T3, T4, T5>(this string value, out T1 res1, out T2 res2, out T3 res3,
            out T4 res4, out T5 res5)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
            string[] bits = value.Split(' ');
            if (bits.Length != 5) throw new ArgumentException("Value has wrong number of components.", nameof(value));
            Parse(bits, 0, out res1, out res2, out res3, out res4, out res5);
        }

        /// <summary>
        /// Converts a string containing a space separated hextuple into 6 typed results.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="res1"></param>
        /// <param name="res2"></param>
        /// <param name="res3"></param>
        /// <param name="res4"></param>
        /// <param name="res5"></param>
        /// <param name="res6"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        public static void Parse<T1, T2, T3, T4, T5, T6>(this string value, out T1 res1, out T2 res2, out T3 res3,
            out T4 res4, out T5 res5, out T6 res6)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
            string[] bits = value.Split(' ');
            if (bits.Length != 6) throw new ArgumentException("Value has wrong number of components.", nameof(value));
            Parse(bits, 0, out res1, out res2, out res3, out res4, out res5, out res6);
        }

        /// <summary>
        /// Converts a string containing a space separated septuple into 7 typed results.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="res1"></param>
        /// <param name="res2"></param>
        /// <param name="res3"></param>
        /// <param name="res4"></param>
        /// <param name="res5"></param>
        /// <param name="res6"></param>
        /// <param name="res7"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        public static void Parse<T1, T2, T3, T4, T5, T6, T7>(this string value, out T1 res1, out T2 res2, out T3 res3,
            out T4 res4, out T5 res5, out T6 res6, out T7 res7)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
            string[] bits = value.Split(' ');
            if (bits.Length != 7) throw new ArgumentException("Value has wrong number of components.", nameof(value));
            Parse(bits, 0, out res1, out res2, out res3, out res4, out res5, out res6, out res7);
        }

        /// <summary>
        /// Converts a string containing a space separated octuple into 8 typed results.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="res1"></param>
        /// <param name="res2"></param>
        /// <param name="res3"></param>
        /// <param name="res4"></param>
        /// <param name="res5"></param>
        /// <param name="res6"></param>
        /// <param name="res7"></param>
        /// <param name="res8"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        public static void Parse<T1, T2, T3, T4, T5, T6, T7, T8>(this string value, out T1 res1, out T2 res2,
            out T3 res3, out T4 res4, out T5 res5, out T6 res6, out T7 res7, out T8 res8)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
            string[] bits = value.Split(' ');
            if (bits.Length != 8) throw new ArgumentException("Value has wrong number of components.", nameof(value));
            Parse(bits, 0, out res1, out res2, out res3, out res4, out res5, out res6, out res7, out res8);
        }

        private static void Parse<T1, T2, T3, T4, T5, T6, T7, T8>(string[] bits, int index, out T1 res1, out T2 res2,
            out T3 res3, out T4 res4, out T5 res5, out T6 res6, out T7 res7, out T8 res8)
        {
            res1 = Parse<T1>(bits[index]);
            Parse(bits, index + 1, out res2, out res3, out res4, out res5, out res6, out res7, out res8);
        }

        private static void Parse<T1, T2, T3, T4, T5, T6, T7>(string[] bits, int index, out T1 res1, out T2 res2,
            out T3 res3, out T4 res4, out T5 res5, out T6 res6, out T7 res7)
        {
            res1 = Parse<T1>(bits[index]);
            Parse(bits, index + 1, out res2, out res3, out res4, out res5, out res6, out res7);
        }

        private static void Parse<T1, T2, T3, T4, T5, T6>(string[] bits, int index, out T1 res1, out T2 res2,
            out T3 res3, out T4 res4, out T5 res5, out T6 res6)
        {
            res1 = Parse<T1>(bits[index]);
            Parse(bits, index + 1, out res2, out res3, out res4, out res5, out res6);
        }

        private static void Parse<T1, T2, T3, T4, T5>(string[] bits, int index, out T1 res1, out T2 res2, out T3 res3,
            out T4 res4, out T5 res5)
        {
            res1 = Parse<T1>(bits[index]);
            Parse(bits, index + 1, out res2, out res3, out res4, out res5);
        }

        private static void Parse<T1, T2, T3, T4>(string[] bits, int index, out T1 res1, out T2 res2, out T3 res3,
            out T4 res4)
        {
            res1 = Parse<T1>(bits[index]);
            Parse(bits, index + 1, out res2, out res3, out res4);
        }

        private static void Parse<T1, T2, T3>(string[] bits, int index, out T1 res1, out T2 res2, out T3 res3)
        {
            res1 = Parse<T1>(bits[index]);
            Parse(bits, index + 1, out res2, out res3);
        }

        private static void Parse<T1, T2>(string[] bits, int index, out T1 res1, out T2 res2)
        {
            res1 = Parse<T1>(bits[index]);
            Parse(bits, index + 1, out res2);
        }

        private static void Parse<T>(string[] bits, int index, out T res1)
        {
            res1 = Parse<T>(bits[index]);
        }

        private static T Parse<T>(string value)
        {
            Type t = typeof (T);
            if (t == typeof (int))
            {
                return (T)(object)int.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (t == typeof (long))
            {
                return (T)(object)long.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (t == typeof (double))
            {
                return (T)(object)double.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (t == typeof (float))
            {
                return (T)(object)float.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (t == typeof (string))
            {
                return (T)(object)value;
            }
            else if (t == typeof (BigInteger))
            {
                return (T)(object)BigInteger.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (t == typeof (TimeSpan))
            {
                return (T)(object)TimeSpan.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (t == typeof (DateTime))
            {
                return (T)(object)DateTime.Parse(value, CultureInfo.InvariantCulture);
            }
            throw new NotSupportedException("Parse type requested not supported.");
        }
    }
}
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
    /// Provides helper extension methods for splitting strings and spans into typed items.
    /// Types supported include int/long/float/double/string/BigInteger/TimeSpan/DateTime/short/uint/ulong/byte/decimal.
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
            result = Parse<T>(value.AsSpan());
        }

        /// <summary>
        /// Converts the span into the desired type.
        /// </summary>
        /// <param name="span">
        /// Span to parse.
        /// </param>
        /// <param name="result">
        /// Result to populate.
        /// </param>
        /// <typeparam name="T">
        /// Type of the result.
        /// </typeparam>
        public static void Parse<T>(this ReadOnlySpan<char> span, out T result)
        {
            result = Parse<T>(span);
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
            Parse(value.AsSpan(), out results);
        }

        /// <summary>
        /// Converts the span into an array of the desired type, assuming each value is separated by spaces.
        /// </summary>
        /// <param name="value">
        /// Span to parse.
        /// </param>
        /// <param name="results">
        /// Results to populate.
        /// </param>
        /// <typeparam name="T">
        /// Type of each element in the results.
        /// </typeparam>
        public static void Parse<T>(this ReadOnlySpan<char> value, out T[] results)
        {
            if (value.IsEmpty)
            {
                results = Array.Empty<T>();
                return;
            }
            int count = 1;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == ' ') count++;
            }
            T[] conversion = new T[count];
            int current = 0;
            ReadOnlySpan<char> remaining = value;
            while (true)
            {
                int idx = remaining.IndexOf(' ');
                if (idx < 0)
                {
                    conversion[current] = Parse<T>(remaining);
                    break;
                }
                conversion[current++] = Parse<T>(remaining.Slice(0, idx));
                remaining = remaining.Slice(idx + 1);
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
            Parse(value.AsSpan(), out res1, out res2);
        }

        /// <summary>
        /// Converts a span containing a space separated pair into 2 typed results.
        /// </summary>
        /// <param name="value">
        /// Span to parse.
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
        public static void Parse<T1, T2>(this ReadOnlySpan<char> value, out T1 res1, out T2 res2)
        {
            res1 = Parse<T1>(NextToken(ref value, false));
            res2 = Parse<T2>(NextToken(ref value, true));
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
            Parse(value.AsSpan(), out res1, out res2, out res3);
        }

        /// <summary>
        /// Converts a span containing a space separated triple into 3 typed results.
        /// </summary>
        /// <param name="value">
        /// Span to parse.
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
        public static void Parse<T1, T2, T3>(this ReadOnlySpan<char> value, out T1 res1, out T2 res2, out T3 res3)
        {
            res1 = Parse<T1>(NextToken(ref value, false));
            res2 = Parse<T2>(NextToken(ref value, false));
            res3 = Parse<T3>(NextToken(ref value, true));
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
            Parse(value.AsSpan(), out res1, out res2, out res3, out res4);
        }

        /// <summary>
        /// Converts a span containing a space separated quadruple into 4 typed results.
        /// </summary>
        /// <param name="value">
        /// Span to parse.
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
        public static void Parse<T1, T2, T3, T4>(this ReadOnlySpan<char> value, out T1 res1, out T2 res2, out T3 res3, out T4 res4)
        {
            res1 = Parse<T1>(NextToken(ref value, false));
            res2 = Parse<T2>(NextToken(ref value, false));
            res3 = Parse<T3>(NextToken(ref value, false));
            res4 = Parse<T4>(NextToken(ref value, true));
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
        /// Type of the fifth result.
        /// </typeparam>
        public static void Parse<T1, T2, T3, T4, T5>(this string value, out T1 res1, out T2 res2, out T3 res3,
            out T4 res4, out T5 res5)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
            Parse(value.AsSpan(), out res1, out res2, out res3, out res4, out res5);
        }

        /// <summary>
        /// Converts a span containing a space separated quintuple into 5 typed results.
        /// </summary>
        /// <param name="value">
        /// Span to parse.
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
        /// Type of the fifth result.
        /// </typeparam>
        public static void Parse<T1, T2, T3, T4, T5>(this ReadOnlySpan<char> value, out T1 res1, out T2 res2, out T3 res3,
            out T4 res4, out T5 res5)
        {
            res1 = Parse<T1>(NextToken(ref value, false));
            res2 = Parse<T2>(NextToken(ref value, false));
            res3 = Parse<T3>(NextToken(ref value, false));
            res4 = Parse<T4>(NextToken(ref value, false));
            res5 = Parse<T5>(NextToken(ref value, true));
        }

        /// <summary>
        /// Converts a string containing a space separated hextuple into 6 typed results.
        /// </summary>
        /// <param name="value">Value to parse.</param>
        /// <param name="res1">First result.</param>
        /// <param name="res2">Second result.</param>
        /// <param name="res3">Third result.</param>
        /// <param name="res4">Fourth result.</param>
        /// <param name="res5">Fifth result.</param>
        /// <param name="res6">Sixth result.</param>
        /// <typeparam name="T1">Type of the first result.</typeparam>
        /// <typeparam name="T2">Type of the second result.</typeparam>
        /// <typeparam name="T3">Type of the third result.</typeparam>
        /// <typeparam name="T4">Type of the fourth result.</typeparam>
        /// <typeparam name="T5">Type of the fifth result.</typeparam>
        /// <typeparam name="T6">Type of the sixth result.</typeparam>
        public static void Parse<T1, T2, T3, T4, T5, T6>(this string value, out T1 res1, out T2 res2, out T3 res3,
            out T4 res4, out T5 res5, out T6 res6)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
            Parse(value.AsSpan(), out res1, out res2, out res3, out res4, out res5, out res6);
        }

        /// <summary>
        /// Converts a span containing a space separated hextuple into 6 typed results.
        /// </summary>
        /// <param name="value">Span to parse.</param>
        /// <param name="res1">First result.</param>
        /// <param name="res2">Second result.</param>
        /// <param name="res3">Third result.</param>
        /// <param name="res4">Fourth result.</param>
        /// <param name="res5">Fifth result.</param>
        /// <param name="res6">Sixth result.</param>
        /// <typeparam name="T1">Type of the first result.</typeparam>
        /// <typeparam name="T2">Type of the second result.</typeparam>
        /// <typeparam name="T3">Type of the third result.</typeparam>
        /// <typeparam name="T4">Type of the fourth result.</typeparam>
        /// <typeparam name="T5">Type of the fifth result.</typeparam>
        /// <typeparam name="T6">Type of the sixth result.</typeparam>
        public static void Parse<T1, T2, T3, T4, T5, T6>(this ReadOnlySpan<char> value, out T1 res1, out T2 res2, out T3 res3,
            out T4 res4, out T5 res5, out T6 res6)
        {
            res1 = Parse<T1>(NextToken(ref value, false));
            res2 = Parse<T2>(NextToken(ref value, false));
            res3 = Parse<T3>(NextToken(ref value, false));
            res4 = Parse<T4>(NextToken(ref value, false));
            res5 = Parse<T5>(NextToken(ref value, false));
            res6 = Parse<T6>(NextToken(ref value, true));
        }

        /// <summary>
        /// Converts a string containing a space separated septuple into 7 typed results.
        /// </summary>
        /// <param name="value">Value to parse.</param>
        /// <param name="res1">First result.</param>
        /// <param name="res2">Second result.</param>
        /// <param name="res3">Third result.</param>
        /// <param name="res4">Fourth result.</param>
        /// <param name="res5">Fifth result.</param>
        /// <param name="res6">Sixth result.</param>
        /// <param name="res7">Seventh result.</param>
        /// <typeparam name="T1">Type of the first result.</typeparam>
        /// <typeparam name="T2">Type of the second result.</typeparam>
        /// <typeparam name="T3">Type of the third result.</typeparam>
        /// <typeparam name="T4">Type of the fourth result.</typeparam>
        /// <typeparam name="T5">Type of the fifth result.</typeparam>
        /// <typeparam name="T6">Type of the sixth result.</typeparam>
        /// <typeparam name="T7">Type of the seventh result.</typeparam>
        public static void Parse<T1, T2, T3, T4, T5, T6, T7>(this string value, out T1 res1, out T2 res2, out T3 res3,
            out T4 res4, out T5 res5, out T6 res6, out T7 res7)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
            Parse(value.AsSpan(), out res1, out res2, out res3, out res4, out res5, out res6, out res7);
        }

        /// <summary>
        /// Converts a span containing a space separated septuple into 7 typed results.
        /// </summary>
        /// <param name="value">Span to parse.</param>
        /// <param name="res1">First result.</param>
        /// <param name="res2">Second result.</param>
        /// <param name="res3">Third result.</param>
        /// <param name="res4">Fourth result.</param>
        /// <param name="res5">Fifth result.</param>
        /// <param name="res6">Sixth result.</param>
        /// <param name="res7">Seventh result.</param>
        /// <typeparam name="T1">Type of the first result.</typeparam>
        /// <typeparam name="T2">Type of the second result.</typeparam>
        /// <typeparam name="T3">Type of the third result.</typeparam>
        /// <typeparam name="T4">Type of the fourth result.</typeparam>
        /// <typeparam name="T5">Type of the fifth result.</typeparam>
        /// <typeparam name="T6">Type of the sixth result.</typeparam>
        /// <typeparam name="T7">Type of the seventh result.</typeparam>
        public static void Parse<T1, T2, T3, T4, T5, T6, T7>(this ReadOnlySpan<char> value, out T1 res1, out T2 res2, out T3 res3,
            out T4 res4, out T5 res5, out T6 res6, out T7 res7)
        {
            res1 = Parse<T1>(NextToken(ref value, false));
            res2 = Parse<T2>(NextToken(ref value, false));
            res3 = Parse<T3>(NextToken(ref value, false));
            res4 = Parse<T4>(NextToken(ref value, false));
            res5 = Parse<T5>(NextToken(ref value, false));
            res6 = Parse<T6>(NextToken(ref value, false));
            res7 = Parse<T7>(NextToken(ref value, true));
        }

        /// <summary>
        /// Converts a string containing a space separated octuple into 8 typed results.
        /// </summary>
        /// <param name="value">Value to parse.</param>
        /// <param name="res1">First result.</param>
        /// <param name="res2">Second result.</param>
        /// <param name="res3">Third result.</param>
        /// <param name="res4">Fourth result.</param>
        /// <param name="res5">Fifth result.</param>
        /// <param name="res6">Sixth result.</param>
        /// <param name="res7">Seventh result.</param>
        /// <param name="res8">Eighth result.</param>
        /// <typeparam name="T1">Type of the first result.</typeparam>
        /// <typeparam name="T2">Type of the second result.</typeparam>
        /// <typeparam name="T3">Type of the third result.</typeparam>
        /// <typeparam name="T4">Type of the fourth result.</typeparam>
        /// <typeparam name="T5">Type of the fifth result.</typeparam>
        /// <typeparam name="T6">Type of the sixth result.</typeparam>
        /// <typeparam name="T7">Type of the seventh result.</typeparam>
        /// <typeparam name="T8">Type of the eighth result.</typeparam>
        public static void Parse<T1, T2, T3, T4, T5, T6, T7, T8>(this string value, out T1 res1, out T2 res2,
            out T3 res3, out T4 res4, out T5 res5, out T6 res6, out T7 res7, out T8 res8)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
            Parse(value.AsSpan(), out res1, out res2, out res3, out res4, out res5, out res6, out res7, out res8);
        }

        /// <summary>
        /// Converts a span containing a space separated octuple into 8 typed results.
        /// </summary>
        /// <param name="value">Span to parse.</param>
        /// <param name="res1">First result.</param>
        /// <param name="res2">Second result.</param>
        /// <param name="res3">Third result.</param>
        /// <param name="res4">Fourth result.</param>
        /// <param name="res5">Fifth result.</param>
        /// <param name="res6">Sixth result.</param>
        /// <param name="res7">Seventh result.</param>
        /// <param name="res8">Eighth result.</param>
        /// <typeparam name="T1">Type of the first result.</typeparam>
        /// <typeparam name="T2">Type of the second result.</typeparam>
        /// <typeparam name="T3">Type of the third result.</typeparam>
        /// <typeparam name="T4">Type of the fourth result.</typeparam>
        /// <typeparam name="T5">Type of the fifth result.</typeparam>
        /// <typeparam name="T6">Type of the sixth result.</typeparam>
        /// <typeparam name="T7">Type of the seventh result.</typeparam>
        /// <typeparam name="T8">Type of the eighth result.</typeparam>
        public static void Parse<T1, T2, T3, T4, T5, T6, T7, T8>(this ReadOnlySpan<char> value, out T1 res1, out T2 res2,
            out T3 res3, out T4 res4, out T5 res5, out T6 res6, out T7 res7, out T8 res8)
        {
            res1 = Parse<T1>(NextToken(ref value, false));
            res2 = Parse<T2>(NextToken(ref value, false));
            res3 = Parse<T3>(NextToken(ref value, false));
            res4 = Parse<T4>(NextToken(ref value, false));
            res5 = Parse<T5>(NextToken(ref value, false));
            res6 = Parse<T6>(NextToken(ref value, false));
            res7 = Parse<T7>(NextToken(ref value, false));
            res8 = Parse<T8>(NextToken(ref value, true));
        }

        private static ReadOnlySpan<char> NextToken(ref ReadOnlySpan<char> span, bool isLast)
        {
            int idx = span.IndexOf(' ');
            if (isLast)
            {
                if (idx >= 0) throw new ArgumentException("Value has wrong number of components.");
                ReadOnlySpan<char> token = span;
                span = default;
                return token;
            }
            if (idx < 0) throw new ArgumentException("Value has wrong number of components.");
            ReadOnlySpan<char> res = span.Slice(0, idx);
            span = span.Slice(idx + 1);
            return res;
        }

        /// <summary>
        /// Converts a character span into the specified type using invariant culture.
        /// </summary>
        /// <param name="value">Character span to parse.</param>
        /// <typeparam name="T">Type of the result.</typeparam>
        /// <returns>Parsed value of type T.</returns>
        public static T Parse<T>(ReadOnlySpan<char> value)
        {
            Type t = typeof(T);
            if (t == typeof(int))
            {
                return (T)(object)int.Parse(value, CultureInfo.InvariantCulture);
            }
            if (t == typeof(long))
            {
                return (T)(object)long.Parse(value, CultureInfo.InvariantCulture);
            }
            if (t == typeof(double))
            {
                return (T)(object)double.Parse(value, CultureInfo.InvariantCulture);
            }
            if (t == typeof(float))
            {
                return (T)(object)float.Parse(value, CultureInfo.InvariantCulture);
            }
            if (t == typeof(string))
            {
                return (T)(object)value.ToString();
            }
            if (t == typeof(BigInteger))
            {
                return (T)(object)BigInteger.Parse(value, CultureInfo.InvariantCulture);
            }
            if (t == typeof(TimeSpan))
            {
                return (T)(object)TimeSpan.Parse(value, CultureInfo.InvariantCulture);
            }
            if (t == typeof(DateTime))
            {
                return (T)(object)DateTime.Parse(value, CultureInfo.InvariantCulture);
            }
            if (t == typeof(short))
            {
                return (T)(object)short.Parse(value, CultureInfo.InvariantCulture);
            }
            if (t == typeof(uint))
            {
                return (T)(object)uint.Parse(value, CultureInfo.InvariantCulture);
            }
            if (t == typeof(ulong))
            {
                return (T)(object)ulong.Parse(value, CultureInfo.InvariantCulture);
            }
            if (t == typeof(byte))
            {
                return (T)(object)byte.Parse(value, CultureInfo.InvariantCulture);
            }
            if (t == typeof(decimal))
            {
                return (T)(object)decimal.Parse(value, CultureInfo.InvariantCulture);
            }
            throw new NotSupportedException("Parse type requested not supported.");
        }
    }
}
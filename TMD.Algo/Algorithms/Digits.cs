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
using System.Linq;
using System.Numerics;

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Holds a set of digits in a specified base.
    /// </summary>
    public class DigitCollection
    {
        /// <summary>
        /// The digits of the stored value, from least significant to most significant.
        /// </summary>
        public int[] Digits { get; }

        /// <summary>
        /// The base of the digit representation of the stored value.
        /// </summary>
        public int DigitBase { get; }

        /// <summary>
        /// Constructor.  Assumes base 10 and no padding.
        /// </summary>
        /// <param name="digits">
        /// Enumeration of digits to be stored.
        /// </param>
        public DigitCollection(IEnumerable<int> digits) : this(digits, 10)
        {
        }

        /// <summary>
        /// Constructor.
        ///  </summary>
        /// <param name="digits">
        /// Enumeration of digits to be stored.
        /// </param>
        /// <param name="digitBase">
        /// Base of the digits.
        /// </param>
        public DigitCollection(IEnumerable<int> digits, int digitBase)
        {
            Digits = digits.ToArray();
            DigitBase = digitBase;
            foreach (int digit in Digits)
            {
                if (digit < 0 || digit >= DigitBase) throw new ArgumentException("Digits must be between 0 and base - 1.", nameof(digits));
            }
        }

        /// <summary>
        /// Creates a new digit collection with the same digits, but a different base.
        /// </summary>
        /// <param name="newBase">
        /// The newBase to interpret the digits as.  The current digits must have values valid for the new base.
        /// </param>
        /// <returns>
        /// A new digit collection with the same digits.
        /// </returns>
        public DigitCollection SwitchBase(int newBase)
        {
            return new DigitCollection((int[])Digits.Clone(), newBase);
        }

        /// <summary>
        /// Converts from digit representation back to an int value.
        /// </summary>
        /// <returns>
        /// A value the same as the represented digits, unless it overflows.
        /// </returns>
        public int ToInt()
        {
            // TODO - check for overflow.
            return (int)ToBigInteger();
        }

        /// <summary>
        /// Converts from digit representation back to a long value.
        /// </summary>
        /// <returns>
        /// A value the same as the represented digits, unless it overflows.
        /// </returns>
        public long ToLong()
        {
            // TODO - check for overflow.
            return (long)ToBigInteger();
        }

        /// <summary>
        /// Converts from digit representation back to a value.
        /// </summary>
        /// <returns>
        /// A value the same as the represented digits.
        /// </returns>
        public BigInteger ToBigInteger()
        {
            return Digits.ToValueAsDigits(DigitBase);
        }
    }

    /// <summary>
    /// Extension methods to break values into digits.
    /// </summary>
    public static class DigitExtensions
    {
        /// <summary>
        /// Returns an enumeration of the digits of the value from least to most significant.  In base 10.
        /// </summary>
        /// <param name="value">
        /// Value to be decomposed into digits.  Must be non-negative.
        /// </param>
        /// <returns>
        /// An enumeration of the digits in base 10 in value from least to most significant.
        /// </returns>
        public static IEnumerable<int> ToDigits(this int value)
        {
            return ToDigits(value, 10);
        }

        /// <summary>
        /// Returns an enumeration of the digits of the value from least to most significant.  In specified base.
        /// </summary>
        /// <param name="value">
        /// Value to be decomposed into digits.  Must be non-negative.
        /// </param>
        /// <param name="digitBase">
        /// The base to decompose the digits.
        /// </param>
        /// <returns>
        /// An enumeration of the digits in the specified base in value from least to most significant.
        /// </returns>
        public static IEnumerable<int> ToDigits(this int value, int digitBase)
        {
            return ((BigInteger)value).ToDigits(digitBase);
        }

        /// <summary>
        /// Returns an enumeration of the digits of the value from least to most significant.  In base 10.
        /// </summary>
        /// <param name="value">
        /// Value to be decomposed into digits.  Must be non-negative.
        /// </param>
        /// <returns>
        /// An enumeration of the digits in base 10 in value from least to most significant.
        /// </returns>
        public static IEnumerable<int> ToDigits(this long value)
        {
            return ToDigits(value, 10);
        }

        /// <summary>
        /// Returns an enumeration of the digits of the value from least to most significant.  In specified base.
        /// </summary>
        /// <param name="value">
        /// Value to be decomposed into digits.  Must be non-negative.
        /// </param>
        /// <param name="digitBase">
        /// The base to decompose the digits.
        /// </param>
        /// <returns>
        /// An enumeration of the digits in the specified base in value from least to most significant.
        /// </returns>
        public static IEnumerable<int> ToDigits(this long value, int digitBase)
        {
            return ((BigInteger)value).ToDigits(digitBase);
        }

        /// <summary>
        /// Returns an enumeration of the digits of the value from least to most significant.  In base 10.
        /// </summary>
        /// <param name="value">
        /// Value to be decomposed into digits.  Must be non-negative.
        /// </param>
        /// <returns>
        /// An enumeration of the digits in base 10 in value from least to most significant.
        /// </returns>
        public static IEnumerable<int> ToDigits(this BigInteger value)
        {
            return ToDigits(value, 10);
        }

        /// <summary>
        /// Returns an enumeration of the digits of the value from least to most significant.  In specified base.
        /// </summary>
        /// <param name="value">
        /// Value to be decomposed into digits.  Must be non-negative.
        /// </param>
        /// <param name="digitBase">
        /// The base to decompose the digits.
        /// </param>
        /// <returns>
        /// An enumeration of the digits in the specified base in value from least to most significant.
        /// </returns>
        public static IEnumerable<int> ToDigits(this BigInteger value, int digitBase)
        {
            if (value < 0) throw new ArgumentException("Negative values can't be turned into digits.", nameof(value));
            if (value == 0)
            {
                yield return 0;
                yield break;
            }
            while (value != 0)
            {
                yield return (int)(value % digitBase);
                value /= digitBase;
            }
        }

        /// <summary>
        /// Creates a value by interpreting the enumeration as a sequence of base 10 digits from least to most significant.
        /// </summary>
        /// <param name="digits">
        /// Squence of digits to interpretet as a value.
        /// </param>
        /// <returns>
        /// The effective value of the sequence of digits from least to most significant.
        /// </returns>
        public static BigInteger ToValueAsDigits(this IEnumerable<int> digits)
        {
            return ToValueAsDigits(digits, 10);
        }

        /// <summary>
        /// Creates a value by interpreting the enumeration as a sequence of digits in a specified base from least to most significant.
        /// </summary>
        /// <param name="digits">
        /// Squence of digits to interpretet as a value.
        /// </param>
        /// <param name="digitBase">
        /// Base to interpret the digits as.
        /// </param>
        /// <returns>
        /// The effective value of the sequence of digits from least to most significant.
        /// </returns>
        public static BigInteger ToValueAsDigits(this IEnumerable<int> digits, int digitBase)
        {
            BigInteger exponent = 1;
            BigInteger result = 0;
            foreach (int digit in digits)
            {
                result += digit * exponent;
                exponent *= digitBase;
            }
            return result;
        }
    }

}

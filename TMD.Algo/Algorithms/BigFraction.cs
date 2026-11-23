#region License

/*
Copyright (c) 2026, the TMD.Algo authors.
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

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Represents an arbitrary-precision reduced rational number with a positive denominator.
    /// </summary>
    public readonly struct BigFraction : IComparable<BigFraction>, IEquatable<BigFraction>
    {
        private readonly BigInteger denominatorMinusOne;

        /// <summary>
        /// Gets the additive identity <c>0 / 1</c>.
        /// </summary>
        public static BigFraction Zero => default;

        /// <summary>
        /// Gets the multiplicative identity <c>1 / 1</c>.
        /// </summary>
        public static BigFraction One => new(BigInteger.One);

        /// <summary>
        /// Initializes a new instance of the <see cref="BigFraction"/> struct from a numerator and denominator,
        /// reducing to lowest terms with a strictly positive denominator.
        /// </summary>
        /// <param name="numerator">Fraction numerator.</param>
        /// <param name="denominator">Fraction denominator (must be non-zero).</param>
        /// <exception cref="DivideByZeroException">Thrown when <paramref name="denominator"/> is zero.</exception>
        public BigFraction(BigInteger numerator, BigInteger denominator)
        {
            if (denominator.IsZero)
                throw new DivideByZeroException("Denominator cannot be zero.");

            if (numerator.IsZero)
            {
                Numerator = BigInteger.Zero;
                denominatorMinusOne = BigInteger.Zero;
                return;
            }

            if (denominator.Sign < 0)
            {
                numerator = -numerator;
                denominator = -denominator;
            }

            BigInteger gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);
            if (!gcd.IsOne)
            {
                numerator /= gcd;
                denominator /= gcd;
            }

            Numerator = numerator;
            denominatorMinusOne = denominator - BigInteger.One;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigFraction"/> struct equal to an integer value.
        /// </summary>
        /// <param name="value">Integer value.</param>
        public BigFraction(BigInteger value)
        {
            Numerator = value;
            denominatorMinusOne = BigInteger.Zero;
        }

        private BigFraction(BigInteger reducedNum, BigInteger reducedDen, bool _)
        {
            Numerator = reducedNum;
            denominatorMinusOne = reducedDen - BigInteger.One;
        }

        /// <summary>
        /// Gets the reduced numerator of the fraction.
        /// </summary>
        public BigInteger Numerator { get; }

        /// <summary>
        /// Gets the strictly positive reduced denominator of the fraction.
        /// </summary>
        public BigInteger Denominator => denominatorMinusOne + BigInteger.One;

        /// <summary>
        /// Gets a value indicating whether this fraction equals zero.
        /// </summary>
        public bool IsZero => Numerator.IsZero;

        /// <summary>
        /// Gets the sign of the fraction (-1, 0, or 1).
        /// </summary>
        public int Sign => Numerator.Sign;

        /// <summary>
        /// Returns the absolute value of this fraction.
        /// </summary>
        /// <returns>The non-negative magnitude as a <see cref="BigFraction"/>.</returns>
        public BigFraction Abs() => Numerator.Sign < 0 ? new BigFraction(-Numerator, Denominator, true) : this;

        /// <summary>
        /// Negates the fraction.
        /// </summary>
        public static BigFraction operator -(BigFraction value) => new(-value.Numerator, value.Denominator, true);

        /// <summary>
        /// Adds two fractions.
        /// </summary>
        public static BigFraction operator +(BigFraction left, BigFraction right)
        {
            if (left.Numerator.IsZero) return right;
            if (right.Numerator.IsZero) return left;
            BigInteger ld = left.Denominator;
            BigInteger rd = right.Denominator;
            return new BigFraction(left.Numerator * rd + right.Numerator * ld, ld * rd);
        }

        /// <summary>
        /// Subtracts <paramref name="right"/> from <paramref name="left"/>.
        /// </summary>
        public static BigFraction operator -(BigFraction left, BigFraction right)
        {
            if (right.Numerator.IsZero) return left;
            if (left.Numerator.IsZero) return -right;
            BigInteger ld = left.Denominator;
            BigInteger rd = right.Denominator;
            return new BigFraction(left.Numerator * rd - right.Numerator * ld, ld * rd);
        }

        /// <summary>
        /// Multiplies two fractions.
        /// </summary>
        public static BigFraction operator *(BigFraction left, BigFraction right)
        {
            if (left.Numerator.IsZero || right.Numerator.IsZero) return Zero;
            return new BigFraction(left.Numerator * right.Numerator, left.Denominator * right.Denominator);
        }

        /// <summary>
        /// Divides <paramref name="left"/> by <paramref name="right"/>.
        /// </summary>
        public static BigFraction operator /(BigFraction left, BigFraction right)
        {
            if (right.Numerator.IsZero)
                throw new DivideByZeroException("Cannot divide by zero.");
            if (left.Numerator.IsZero) return Zero;
            return new BigFraction(left.Numerator * right.Denominator, left.Denominator * right.Numerator);
        }

        /// <inheritdoc />
        public int CompareTo(BigFraction other) =>
            (Numerator * other.Denominator).CompareTo(other.Numerator * Denominator);

        /// <summary>
        /// Returns whether <paramref name="left"/> is strictly less than <paramref name="right"/>.
        /// </summary>
        public static bool operator <(BigFraction left, BigFraction right) => left.CompareTo(right) < 0;

        /// <summary>
        /// Returns whether <paramref name="left"/> is less than or equal to <paramref name="right"/>.
        /// </summary>
        public static bool operator <=(BigFraction left, BigFraction right) => left.CompareTo(right) <= 0;

        /// <summary>
        /// Returns whether <paramref name="left"/> is strictly greater than <paramref name="right"/>.
        /// </summary>
        public static bool operator >(BigFraction left, BigFraction right) => left.CompareTo(right) > 0;

        /// <summary>
        /// Returns whether <paramref name="left"/> is greater than or equal to <paramref name="right"/>.
        /// </summary>
        public static bool operator >=(BigFraction left, BigFraction right) => left.CompareTo(right) >= 0;

        /// <inheritdoc />
        public bool Equals(BigFraction other) =>
            Numerator == other.Numerator && denominatorMinusOne == other.denominatorMinusOne;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is BigFraction other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Numerator, denominatorMinusOne);

        /// <summary>
        /// Returns whether two fractions are equal.
        /// </summary>
        public static bool operator ==(BigFraction left, BigFraction right) => left.Equals(right);

        /// <summary>
        /// Returns whether two fractions are not equal.
        /// </summary>
        public static bool operator !=(BigFraction left, BigFraction right) => !left.Equals(right);

        /// <summary>
        /// Explicitly converts a <see cref="BigInteger"/> to a <see cref="BigFraction"/>.
        /// </summary>
        public static explicit operator BigFraction(BigInteger value) => new(value);

        /// <summary>
        /// Implicitly converts a <see cref="long"/> to a <see cref="BigFraction"/>.
        /// </summary>
        public static implicit operator BigFraction(long value) => new(new BigInteger(value));

        /// <inheritdoc />
        public override string ToString() => $"{Numerator}/{Denominator}";
    }
}

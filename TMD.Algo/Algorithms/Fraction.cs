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

namespace TMD.Algo.Algorithms
{
    /// <summary>
    /// Represents a fraction.
    /// </summary>
    public struct Fraction : IComparable<Fraction>, IEquatable<Fraction>
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="numerator">
        /// Numerator.
        /// </param>
        /// <param name="denominator">
        /// Denominator.
        /// </param>
        public Fraction(long numerator, long denominator)
        {
            this.numerator = numerator;
            this.denominator = denominator;
            Reduce();
        }

        /// <summary>
        /// Gets or sets the numerator of the fraction.
        /// </summary>
        public long Numerator
        {
            get
            {
                return numerator;
            }
        }
        private long numerator;


        /// <summary>
        /// Gets or sets the denominator of the function.
        /// </summary>
        public long Denominator
        {
            get
            {
                return denominator;
            }
        }
        private long denominator;

        /// <summary>
        /// Calculates the least common multiple of several fractions.
        /// </summary>
        /// <param name="fractions">
        /// Fractions to combine.
        /// </param>
        /// <returns>
        /// The least common multiple of the fractions.
        /// </returns>
        public static Fraction LeastCommonMultiple(IEnumerable<Fraction> fractions)
        {
            // return LCM numerators / GCD denominators.
            // TODO:
            return new Fraction(0,0);
        }

        /// <summary>
        /// Reduces the fraction to minimal form.
        /// </summary>
        private void Reduce()
        {
            if (denominator == 0)
                throw new DivideByZeroException("Denominator cannot be zero.");
            if (denominator < 0)
            {
                denominator = -denominator;
                numerator = -numerator;
            }
            if (numerator == 0)
                denominator = 1;
            else
            {
                long gcd = Gcd(Math.Abs(numerator), denominator);
                numerator /= gcd;
                denominator /= gcd;
            }
        }

        private long Gcd(long p, long q)
        {
            if (q == 0)
                return p;
            return Gcd(q, p % q);
        }

        /// <summary>
        /// Adds two fractions.
        /// </summary>
        /// <param name="other">
        /// Other fraction.
        /// </param>
        /// <returns>
        /// The sum of this fraction and the other.
        /// </returns>
        public Fraction Add(Fraction other)
        {
            return new Fraction(numerator * other.denominator + other.numerator * denominator, denominator * other.denominator);
        }

        /// <summary>
        /// Adds two fractions.
        /// </summary>
        /// <param name="a">
        /// First fraction.
        /// </param>
        /// <param name="b">
        /// Second fraction.
        /// </param>
        /// <returns>
        /// The sum of a and b.
        /// </returns>
        public static Fraction operator +(Fraction a, Fraction b)
        {
            return a.Add(b);
        }

        /// <summary>
        /// Subtracts two fractions.
        /// </summary>
        /// <param name="other">
        /// Other fraction.
        /// </param>
        /// <returns>
        /// The difference between this fraction and the other.
        /// </returns>
        public Fraction Subtract(Fraction other)
        {
            return new Fraction(numerator * other.denominator - other.numerator * denominator, denominator * other.denominator);
        }

        /// <summary>
        /// Subtracts two fractions.
        /// </summary>
        /// <param name="a">
        /// First fraction.
        /// </param>
        /// <param name="b">
        /// Second fraction.
        /// </param>
        /// <returns>
        /// The difference between a and b.
        /// </returns>
        public static Fraction operator -(Fraction a, Fraction b)
        {
            return a.Subtract(b);
        }

        /// <summary>
        /// Multiplys two fractions.
        /// </summary>
        /// <param name="other">
        /// Other fraction.
        /// </param>
        /// <returns>
        /// The multiple of this fraction with other.
        /// </returns>
        public Fraction Multiply(Fraction other)
        {
            return new Fraction(numerator * other.numerator, denominator * other.denominator);
        }

        /// <summary>
        /// Multiplies two fractions.
        /// </summary>
        /// <param name="a">
        /// First fraction.
        /// </param>
        /// <param name="b">
        /// Second fraction.
        /// </param>
        /// <returns>
        /// The multiple of the two fractions.
        /// </returns>
        public static Fraction operator *(Fraction a, Fraction b)
        {
            return a.Multiply(b);
        }

        /// <summary>
        /// Divices this fraction by another.
        /// </summary>
        /// <param name="other">
        /// The other fraction.
        /// </param>
        /// <returns>
        /// The result of dividing this fraction by the other.
        /// </returns>
        public Fraction Divide(Fraction other)
        {
            return new Fraction(numerator * other.denominator, denominator * other.numerator);
        }

        /// <summary>
        /// Divides two fractions.
        /// </summary>
        /// <param name="a">
        /// First fraction.
        /// </param>
        /// <param name="b">
        /// Second fraction.
        /// </param>
        /// <returns>
        /// Fraction a divided by fraction b.
        /// </returns>
        public static Fraction operator /(Fraction a, Fraction b)
        {
            return a.Divide(b);
        }

        /// <summary>
        /// Calculates the absolute value of the fraction.
        /// </summary>
        /// <returns>
        /// A non-negative number equal to the distance of the original fraction from 0.
        /// </returns>
        public Fraction Abs()
        {
            // Normalization means denominator is never negative.
            return new Fraction(Math.Abs(numerator), denominator);
        }

        /// <summary>
        /// Returns the closest integer with an absolute value which is not larger than the original fraction.
        /// </summary>
        public long Truncate()
        {
            return numerator / denominator;
        }

        /// <summary>k
        /// Explicit cast operator to convert fraction to double.
        /// </summary>
        /// <param name="a">
        /// Fraction to convert to its equivelent double.
        /// </param>
        /// <returns>
        /// A double value equivelent to the fraction.
        /// </returns>
        public static explicit operator double(Fraction a)
        {
            return (double)a.numerator / (double)a.denominator;
        }

        /// <summary>
        /// Explicit cast operator to convert an integer into a fraction.
        /// </summary>
        /// <param name="a">
        /// The value for the fraction.
        /// </param>
        /// <returns>
        /// A fraction equal to the integral constant.
        /// </returns>
        public static explicit operator Fraction(long a)
        {
            return new Fraction(a, 1);
        }

        /// <summary>
        /// Converts this fraction to a double.
        /// </summary>
        /// <returns>
        /// A double value equivelent to the fraction.
        /// </returns>
        public double ToDouble()
        {
            return (double)numerator / (double)denominator;
        }

        #region IComparable<Fraction> Members

        /// <summary>
        /// Compares this fraction with another.
        /// </summary>
        /// <param name="other">
        /// The other fraction to compare to.
        /// </param>
        /// <returns>
        /// A value indicating the relative order of the fractions.
        /// </returns>
        public int CompareTo(Fraction other)
        {
            long res = numerator * other.denominator - other.numerator * denominator;
            if (res > 0)
                return 1; 
            if(res < 0)
                return -1;
            return 0;
        }

        /// <summary>
        /// Determines if a is less than b.
        /// </summary>
        /// <param name="a">
        /// First fraction.
        /// </param>
        /// <param name="b">
        /// Second fraction.
        /// </param>
        /// <returns>
        /// True if a compares before b, false otherwise.
        /// </returns>
        public static bool operator <(Fraction a, Fraction b)
        {
            return a.CompareTo(b) < 0;
        }

        /// <summary>
        /// Determines if a is less than or equal to b.
        /// </summary>
        /// <param name="a">
        /// First fraction.
        /// </param>
        /// <param name="b">
        /// Second fraction.
        /// </param>
        /// <returns>
        /// True if a compares before or equal with b, false otherwise.
        /// </returns>
        public static bool operator <=(Fraction a, Fraction b)
        {
            return a.CompareTo(b) <= 0;
        }

        /// <summary>
        /// Determines if a is greater than b.
        /// </summary>
        /// <param name="a">
        /// First fraction.
        /// </param>
        /// <param name="b">
        /// Second fraction.
        /// </param>
        /// <returns>
        /// True if a compares after b, false otherwise.
        /// </returns>
        public static bool operator >(Fraction a, Fraction b)
        {
            return a.CompareTo(b) > 0;
        }

        /// <summary>
        /// Determines if a is greater than or equal to b.
        /// </summary>
        /// <param name="a">
        /// First fraction.
        /// </param>
        /// <param name="b">
        /// Second fraction.
        /// </param>
        /// <returns>
        /// True if a compares after or equal with b, false otherwise.
        /// </returns>
        public static bool operator >=(Fraction a, Fraction b)
        {
            return a.CompareTo(b) >= 0;
        }

        #endregion

        #region IEquatable<Fraction> Members

        /// <summary>
        /// Determines if this fraction is the same as another.
        /// </summary>
        /// <param name="other">
        /// Fraction to compare to.
        /// </param>
        /// <returns>
        /// True if the other fraction has the same value as this, false otherwise.
        /// </returns>
        public bool Equals(Fraction other)
        {
            // Since fractions are always minimal, we can do this.
            return numerator == other.numerator && denominator == other.denominator;
        }

        #endregion

        /// <summary>
        /// Determines if this fraction is the same as another object.
        /// </summary>
        /// <param name="obj">
        /// Object to compare to.
        /// </param>
        /// <returns>
        /// True if the other object is a fraction and it has the same value.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is Fraction))
                return false;
            return Equals((Fraction)obj);

        }

        /// <summary>
        /// Gets this objects hash code.
        /// </summary>
        /// <returns>
        /// A number which is the same for all equal objects, but also potentially the same for some non-equal objects.
        /// </returns>
        public override int GetHashCode()
        {
            // The base hash codes should be sufficient here, until we add another property.
            return base.GetHashCode();
        }

        /// <summary>
        /// Provides an equality test operator for fractions.
        /// </summary>
        /// <param name="a">
        /// First fraction to compare.
        /// </param>
        /// <param name="b">
        /// Second fraction to compare.
        /// </param>
        /// <returns>
        /// True if the fractions are equal, false otherwise.
        /// </returns>
        public static bool operator ==(Fraction a, Fraction b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Provides an inequality test operator for fractions.
        /// </summary>
        /// <param name="a">
        /// First fraction to compare.
        /// </param>
        /// <param name="b">
        /// Second fraction to compare.
        /// </param>
        /// <returns>
        /// True if the fractions are not equal, false otherwise.
        /// </returns>
        public static bool operator !=(Fraction a, Fraction b)
        {
            return !a.Equals(b);
        }
    }
}

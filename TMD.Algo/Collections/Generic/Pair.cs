#region License

/*
Copyright (c) 2015, the TMD.Algo authors.
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

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Provides a simple struct pair type.
    /// Unlike KeyValuePair, is equatable.  Uses default equality for each member.
    /// </summary>
    /// <typeparam name="T1">
    /// Type of the first item in the pair.
    /// </typeparam>
    /// <typeparam name="T2">
    /// Type of the second item in the pair.
    /// </typeparam>
    public struct Pair<T1, T2> : IEquatable<Pair<T1, T2>>
    {
        /// <summary>
        /// First item in the pair.
        /// </summary>
        public readonly T1 Item1;

        /// <summary>
        /// Second item in the pair.
        /// </summary>
        public readonly T2 Item2;

        private static readonly EqualityComparer<T1> t1Comparer = EqualityComparer<T1>.Default;
        private static readonly EqualityComparer<T2> t2Comparer = EqualityComparer<T2>.Default;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item1">
        /// Value for item1.
        /// </param>
        /// <param name="item2">
        /// Value for item2.
        /// </param>
        public Pair(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        /// <summary>
        /// Calculates equality with another pair using default member equality.
        /// </summary>
        /// <param name="other">
        /// Other pair to compare with.
        /// </param>
        /// <returns>
        /// True if memberwise equal with other.
        /// </returns>
        public bool Equals(Pair<T1, T2> other)
        {
            return t1Comparer.Equals(Item1, other.Item1) && t2Comparer.Equals(Item2, other.Item2);
        }

        /// <summary>
        /// Calculate equality with obj.
        /// </summary>
        /// <param name="obj">
        /// Other object to compare to.
        /// </param>
        /// <returns>
        /// True if obj is a Pair of same type and memberwise equal.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;
            return Equals((Pair<T1, T2>)obj);
        }

        /// <summary>
        /// Gets hash code by mixing member default hash codes.
        /// </summary>
        /// <returns>
        /// A mixture of the member hashcodes.
        /// </returns>
        public override int GetHashCode()
        {
            return t1Comparer.GetHashCode(Item1)*33 ^ t2Comparer.GetHashCode(Item2);
        }
    }

    /// <summary>
    /// Compares pairs by their first item only.
    /// </summary>
    /// <typeparam name="T1">
    /// Type of Item1 in the pair.
    /// </typeparam>
    /// <typeparam name="T2">
    /// Type of Item2 in the pair.
    /// </typeparam>
    public class Item1Comparer<T1, T2> : IComparer<Pair<T1, T2>>
    {
        /// <summary>
        /// Gets a static instance of the default KvpComparer.
        /// </summary>
        public static Item1Comparer<T1, T2> Default { get; } = new Item1Comparer<T1, T2>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public Item1Comparer()
        {
            item1Comparer = Comparer<T1>.Default;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="item1Comparer">
        /// Comparer to compare the Item1 values.
        /// </param>
        public Item1Comparer(IComparer<T1> item1Comparer)
        {
            this.item1Comparer = item1Comparer;
        }

        private readonly IComparer<T1> item1Comparer;

        #region IComparer<KeyValuePair<K,V>> Members

        /// <summary>
        /// Compares two key value pairs.
        /// </summary>
        /// <param name="x">
        /// First key value pair.
        /// </param>
        /// <param name="y">
        /// Second key value pair.
        /// </param>
        /// <returns>
        /// A number indicating the relative order of the two key value pairs.
        /// </returns>
        public int Compare(Pair<T1, T2> x, Pair<T1, T2> y)
        {
            return item1Comparer.Compare(x.Item1, y.Item1);
        }

        #endregion
    }
}
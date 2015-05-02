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

using System.Collections.Generic;

namespace TMD.Algo.Algorithms.Generic
{
    /// <summary>
    /// Compares key value pair objects.
    /// </summary>
    /// <typeparam name="TKey">
    /// Type of key in the key value pair.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// Type of value in the key value pair.
    /// </typeparam>
    public class KvpComparer<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// Gets a static instance of the default KvpComparer.
        /// </summary>
        public static KvpComparer<TKey, TValue> Default { get; } = new KvpComparer<TKey, TValue>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public KvpComparer()
        {
            kComparer = Comparer<TKey>.Default;
            vComparer = Comparer<TValue>.Default;
        }

        private readonly IComparer<TKey> kComparer;
        private readonly IComparer<TValue> vComparer;

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
        public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
        {
            int firstRes = kComparer.Compare(x.Key, y.Key);
            if (firstRes == 0)
                return vComparer.Compare(x.Value, y.Value);
            return firstRes;
        }

        #endregion
    }
}
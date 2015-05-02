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
    /// Reverses a comparer.
    /// </summary>
    /// <typeparam name="T">
    /// Type of value being compared by the underlying comparer.
    /// </typeparam>
    public class ReverseComparer<T> : IComparer<T>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="comparer">
        /// Comparer to compare the values.
        /// </param>
        public ReverseComparer(IComparer<T> comparer)
        {
            this.comparer = comparer;
        }


        /// <summary>
        /// Returns a reverse comparer for the default comparer.
        /// </summary>
        public static ReverseComparer<T> Default { get; } = new ReverseComparer<T>(Comparer<T>.Default);

        private readonly IComparer<T> comparer;

        #region IComparer<T> Members

        /// <summary>
        /// Reverse compares two values.
        /// </summary>
        /// <param name="x">
        /// First value.
        /// </param>
        /// <param name="y">
        /// Second value.
        /// </param>
        /// <returns>
        /// A number indicating the relative order of the two values.
        /// </returns>
        public int Compare(T x, T y)
        {
            return comparer.Compare(y, x);
        }

        #endregion
    }
}
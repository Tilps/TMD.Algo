#region License
/*
Copyright (c) 2008, Gareth Pearce (www.themissingdocs.net)
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
    * Neither the name of the www.themissingdocs.net nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Provides a common interface for heap implementations.
    /// A heap is a semistructured data source such that the most important item is easily available at any time.
    /// Heaps are useful as a priority queue.
    /// </summary>
    /// <typeparam name="T">
    /// Type of element stored in the heap.
    /// </typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1710")]
    public interface IHeap<T> : ICollection<T>
    {

        /// <summary>
        /// Gets the first item in the heap, the most important item.
        /// </summary>
        T Front { get; }

        /// <summary>
        /// Gets the first item in the heap, if the heap isn't empty.
        /// </summary>
        /// <param name="var">
        /// Receives the first item in the heap, if there is one.
        /// </param>
        /// <returns>
        /// True if the heap was non-empty, false otherwise.
        /// </returns>
        bool TryGetFront(out T var);

        /// <summary>
        /// Gets and removes the first element in the heap.
        /// </summary>
        /// <returns>
        /// The first element in the heap, if the heap was not empty.
        /// </returns>
        T PopFront();
    }
}

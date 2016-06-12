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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices.WindowsRuntime;

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Implements a dictionary using a flat array which has sentinal values to indicate no data.
    /// </summary>
    /// <remarks>
    /// In some circumstances, provides better memory locality than a normal Dictionary, which may give better performance.
    /// 1) The keys must have an ordering within a given range to map to the array.
    /// 2) There must be a sentinel value to avoid the need to have a separate array of booleans for what entries are filled and which are not.
    /// 3) Due to the sentinal, values must be comparable for equality. (Currently by default comparer.)
    /// 4) In order to implement IEnumerable, it must be possible to map back from indexes to keys.
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1710")]
    public class RangeSentinalDictionary<K, V> : IDictionary<K, V>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mapSize">
        /// The mapFunc must always return values between 0 and mapSize - 1 inclusive.
        /// </param>
        /// <param name="mapFunc">
        /// A function which maps keys to array indexes.
        /// </param>
        /// <param name="unmapFunc">
        /// A function which maps array indexes back to keys.  Will only ever be called for a value which was output by a previous call to mapFunc.
        /// </param>
        /// <param name="sentinal">
        /// A sentinal value.
        /// </param>
        public RangeSentinalDictionary(long mapSize, Func<K, long> mapFunc, Func<long, K> unmapFunc, V sentinal)
        {
            comparer = EqualityComparer<V>.Default;
            data = new V[mapSize];
            this.sentinal = sentinal;
            this.mapFunc = mapFunc;
            this.unmapFunc = unmapFunc;
            Clear();
        }

        private readonly V[] data;
        private int version;
        private readonly Func<K, long> mapFunc;
        private readonly Func<long, K> unmapFunc;
        private readonly V sentinal;
        private int count;
        private readonly IEqualityComparer<V> comparer;

        #region ICollection<T> Members

        /// <summary>
        /// Clears the list.
        /// </summary>
        public void Clear()
        {
            if (!comparer.Equals(sentinal, default(V)))
            {
                for (long i = 0; i < data.LongLength; i++) data[i] = sentinal;
            }
            else
            {
                Array.Clear(data, 0, data.Length);
            }
            count = 0;
            version++;
        }


        /// <summary>
        /// Gets the count of the number of items in the list.
        /// </summary>
        public int Count => count;

        /// <summary>
        /// Returns false.
        /// </summary>
        public bool IsReadOnly => false;

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Gets the untyped enumerator for this list.
        /// </summary>
        /// <returns>
        /// An enumerator to enumerate the objects stored in this list.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new RangeSentinalDictionaryEnumerator(this);
        }

        #endregion

        /// <summary>
        /// Enumerator for tree lists.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1815")]
        public struct RangeSentinalDictionaryEnumerator : IEnumerator<KeyValuePair<K, V>>
        {
            /// <summary>
            /// Internal constructor.
            /// </summary>
            /// <param name="parent">
            /// Parent tree list to enumerate.
            /// </param>
            internal RangeSentinalDictionaryEnumerator(RangeSentinalDictionary<K, V> parent)
            {
                this.parent = parent;
                origVersion = parent.version;
                currentIndex = -1;
            }

            private readonly RangeSentinalDictionary<K, V> parent;
            private readonly int origVersion;
            private long currentIndex;

            #region IEnumerator<T> Members

            /// <summary>
            /// Gets the current item this enumerator points to.
            /// </summary>
            public KeyValuePair<K, V> Current
            {
                get
                {
                    if (currentIndex < 0 || currentIndex >= parent.data.LongLength)
                        return new KeyValuePair<K, V>();
                    else
                        return new KeyValuePair<K, V>(parent.unmapFunc(currentIndex), parent.data[currentIndex]);
                }
            }

            #endregion

            #region IDisposable Members

            /// <summary>
            /// Does nothing.
            /// </summary>
            public void Dispose()
            {
            }

            #endregion

            #region IEnumerator Members

            /// <summary>
            /// Gets the current object the enumerator points to.
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (currentIndex < 0 || currentIndex >= parent.data.LongLength)
                        throw new InvalidOperationException("Current is not pointing to a valid node.");
                    return this.Current;
                }
            }

            /// <summary>
            /// Moves to the next node if possible.
            /// </summary>
            /// <returns>
            /// True if moved to the next node, false otherwise.
            /// </returns>
            public bool MoveNext()
            {
                if (origVersion != parent.version)
                    throw new InvalidOperationException(
                        "The collection being enumerated has been modified since enumerator was acquired.");
                if (currentIndex >= parent.data.LongLength) return false;
                currentIndex++;
                while (currentIndex < parent.data.LongLength)
                {
                    if (!parent.comparer.Equals(parent.data[currentIndex], parent.sentinal))
                    {
                        return true;
                    }
                    currentIndex++;
                }
                return false;
            }

            /// <summary>
            /// Resets the enumerator.
            /// </summary>
            void System.Collections.IEnumerator.Reset()
            {
                if (origVersion != parent.version)
                    throw new InvalidOperationException(
                        "The collection being enumerated has been modified since enumerator was acquired.");
                currentIndex = -1;
            }

            #endregion
        }


        /// <summary>
        /// Adds the specifiedkey and value to the tree.
        /// </summary>
        /// <param name="key">
        /// Key to add the value under.
        /// </param>
        /// <param name="value">
        /// Value to add.
        /// </param>
        public void Add(K key, V value)
        {
            if (ContainsKey(key)) throw new Exception("Duplicate key");
            if (comparer.Equals(value, sentinal)) throw new Exception("Can't insert sentinal.");
            data[mapFunc(key)] = value;
            count++;
            version++;
        }

        /// <summary>
        /// Returns whether the specfied key is stored in this dictionary.
        /// </summary>
        /// <param name="key">
        /// Key to check for.
        /// </param>
        /// <returns>
        /// True if the specified key is stored, otherwise false.
        /// </returns>
        public bool ContainsKey(K key)
        {
            return !comparer.Equals(data[mapFunc(key)], sentinal);
        }

        /// <summary>
        /// Gets the collection of keys in the dictionary.
        /// </summary>
        public ICollection<K> Keys
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Removes the specified key from the dictionary if it is present.
        /// </summary>
        /// <param name="key">
        /// Key to remove.
        /// </param>
        /// <returns>
        /// True if the key was present and removed.
        /// </returns>
        public bool Remove(K key)
        {
            if (!ContainsKey(key)) return false;
            data[mapFunc(key)] = sentinal;
            count--;
            version++;
            return true;
        }

        /// <summary>
        /// Attempts to obtain the value for the specified key, if it is present.
        /// </summary>
        /// <param name="key">
        /// Key to check for.
        /// </param>
        /// <param name="value">
        /// Receives value if found.
        /// </param>
        /// <returns>
        /// True if key is found and value returned, false otherwise.
        /// </returns>
        public bool TryGetValue(K key, out V value)
        {
            if (!ContainsKey(key))
            {
                value = default(V);
                return false;
            }
            value = data[mapFunc(key)];
            return true;
        }

        /// <summary>
        /// Gets the collection of values.
        /// </summary>
        public ICollection<V> Values
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Indexer.  Gets or sets values by keys.
        /// </summary>
        /// <param name="key">
        /// Key to access or update.
        /// </param>
        /// <returns>
        /// The value stored at the key if the key is present in the dictionary.
        /// </returns>
        public V this[K key]
        {
            get
            {
                V result;
                if (TryGetValue(key, out result))
                    return result;
                throw new KeyNotFoundException();
            }
            set
            {
                Remove(key);
                Add(key, value);
            }
        }

        /// <summary>
        /// Adds the specified key value pair into the dictionary.
        /// </summary>
        /// <param name="item">
        /// The key value pair to add.
        /// </param>
        public void Add(KeyValuePair<K, V> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Checks if the specified key value pair is already in the dictionary.
        /// </summary>
        /// <param name="item">
        /// Key value pair to check the dictionary for.
        /// </param>
        /// <returns>
        /// True if the key value pair is in the dictionary, false otherwise.
        /// </returns>
        public bool Contains(KeyValuePair<K, V> item)
        {
            V otherValue;
            if (!TryGetValue(item.Key, out otherValue))
                return false;
            return Comparer<V>.Default.Compare(item.Value, otherValue) == 0;
        }

        /// <summary>
        /// Copies the contents of the dictionary into the array.
        /// </summary>
        /// <param name="array">
        /// Array to receive the key value pairs stored in the dictionary.
        /// </param>
        /// <param name="arrayIndex">
        /// Starting index to copy to.
        /// </param>
        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            if (arrayIndex < 0 || arrayIndex + count > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            foreach (KeyValuePair<K, V> item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        /// <summary>
        /// Removes the specified key value pair if it is in the dictionary.
        /// </summary>
        /// <param name="item">
        /// Key value pair to remove.
        /// </param>
        /// <returns>
        /// True if the key value pair was in the dictionary and removed.
        /// </returns>
        public bool Remove(KeyValuePair<K, V> item)
        {
            if (!ContainsKey(item.Key)) return false;
            if (Comparer<V>.Default.Compare(item.Value, data[mapFunc(item.Key)]) != 0)
                return false;
            Remove(item.Key);
            return true;
        }

        IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
        {
            return new RangeSentinalDictionaryEnumerator(this);
        }
    }
}
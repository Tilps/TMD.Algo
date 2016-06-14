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
using System.Text;
using System.Threading.Tasks;

namespace TMD.Algo.Collections.Generic
{
    /// <summary>
    /// Helper methods to assist with working with dictionaries.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Update value for key.  Transforms and inserts the default value if key is not already present.
        /// </summary>
        /// <param name="dictionary">
        /// Dictionary to update.
        /// </param>
        /// <param name="key">
        /// Key in the dictionary which is to have its value updated or added.
        /// </param>
        /// <param name="updater">
        /// Function which receives the existing value (or default) and returns the new value to associate with the key.
        /// </param>
        /// <typeparam name="TKey">
        /// Type of Key.
        /// </typeparam>
        /// <typeparam name="TValue">
        /// Type of Value.
        /// </typeparam>
        public static void UpdateValueForKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            Func<TValue, TValue> updater)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            dictionary[key] = updater(value);
        }

        /// <summary>
        /// Gets the value stored in a dictionary for a key, or creates, stores and returns a new value using the populater.
        /// </summary>
        /// <param name="dictionary">
        /// Dictionary to lookup or populate.
        /// </param>
        /// <param name="key">
        /// Key to lookup in the dictionary.
        /// </param>
        /// <param name="populater">
        /// Function to populate the dictionary with if it has no value.
        /// </param>
        /// <typeparam name="TKey">
        /// Type of Key.
        /// </typeparam>
        /// <typeparam name="TValue">
        /// Type of Value.
        /// </typeparam>
        /// <returns>
        /// The value stored under the key, or the value added by the populater.
        /// </returns>
        public static TValue GetOrPopulate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            Func<TValue> populater)
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = populater();
                dictionary[key] = value;
            }
            return value;
        }

        /// <summary>
        /// Gets the value stored in a dictionary for a key, or returns the default value if no value is available.
        /// </summary>
        /// <param name="dictionary">
        /// Dictionary to lookup.
        /// </param>
        /// <param name="key">
        /// Key to lookup in the dictionary.
        /// </param>
        /// <param name="def">
        /// Value to use if the key is not found.
        /// </param>
        /// <typeparam name="TKey">
        /// Type of Key.
        /// </typeparam>
        /// <typeparam name="TValue">
        /// Type of value.
        /// </typeparam>
        /// <returns>
        /// The value stored by the key, or the default value if no value was found.
        /// </returns>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue def)
        {
            TValue value;
            if (dictionary.TryGetValue(key, out value))
                return value;
            return def;
        }
    }
}

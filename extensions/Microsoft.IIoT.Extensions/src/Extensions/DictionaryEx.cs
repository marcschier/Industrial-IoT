// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace System.Collections.Generic {
    using System.Linq;

    /// <summary>
    /// Dictionary extensions
    /// </summary>
    public static class DictionaryEx {

        /// <summary>
        /// Safe dictionary equals
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="that"></param>
        /// <param name="equality"></param>
        /// <returns></returns>
        public static bool DictionaryEqualsSafe<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> dict,
            IReadOnlyDictionary<TKey, TValue> that, Func<TValue, TValue, bool> equality) {
            if (dict == that) {
                return true;
            }
            if (dict == null || that == null) {
                return false;
            }
            if (dict.Count != that.Count) {
                return false;
            }
            return that.All(kv => dict.TryGetValue(kv.Key, out var v) &&
                equality(kv.Value, v));
        }

        /// <summary>
        /// Safe dictionary equals
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="that"></param>
        /// <param name="equality"></param>
        /// <returns></returns>
        public static bool DictionaryEqualsSafe<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            IDictionary<TKey, TValue> that, Func<TValue, TValue, bool> equality) {
            if (dict == that) {
                return true;
            }
            if (dict == null || that == null) {
                return false;
            }
            if (dict.Count != that.Count) {
                return false;
            }
            return that.All(kv => dict.TryGetValue(kv.Key, out var v) &&
                equality(kv.Value, v));
        }

        /// <summary>
        /// Returns the contents of a dictionary as KeyValuePairs
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<TKey, TValue>> ToKeyValuePairs<TKey, TValue>(
            this IDictionary dictionary) {
            if (dictionary is null) {
                throw new ArgumentNullException(nameof(dictionary));
            }
            foreach (var key in dictionary.Keys) {
                yield return new KeyValuePair<TKey, TValue>((TKey)key, (TValue)dictionary[key]);
            }
        }

        /// <summary>
        /// Returns the contents of a dictionary as typed dictionary
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IDictionary dictionary) {
            return dictionary
                .ToKeyValuePairs<TKey, TValue>()
                .ToDictionary(k => k.Key, v => v.Value);
        }

        /// <summary>
        /// Create a copy
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(
            this IReadOnlyDictionary<TKey, TValue> dictionary) {
            if (dictionary == null) {
                return new Dictionary<TKey, TValue>();
            }
            return dictionary.ToDictionary(k => k.Key, v => v.Value);
        }

        /// <summary>
        /// Create a copy
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary) {
            if (dictionary == null) {
                return new Dictionary<TKey, TValue>();
            }
            return dictionary.ToDictionary(k => k.Key, v => v.Value);
        }

        /// <summary>
        /// Safe dictionary equals
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool DictionaryEqualsSafe<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict,
            IReadOnlyDictionary<TKey, TValue> that) {
            return DictionaryEqualsSafe(dict, that, (x, y) => x.EqualsSafe(y));
        }

        /// <summary>
        /// Safe dictionary equals
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool DictionaryEqualsSafe<TKey, TValue>(this IDictionary<TKey, TValue> dict,
            IDictionary<TKey, TValue> that) {
            return DictionaryEqualsSafe(dict, that, (x, y) => x.EqualsSafe(y));
        }

        /// <summary>
        /// Add or update item
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dict,
            TKey key, TValue value) {
            if (dict is null) {
                throw new ArgumentNullException(nameof(dict));
            }
            if (dict.ContainsKey(key)) {
                dict[key] = value;
            }
            else {
                dict.Add(key, value);
            }
        }

        /// <summary>
        /// Get or add item
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict,
            TKey key, TValue value) {
            if (dict is null) {
                throw new ArgumentNullException(nameof(dict));
            }
            if (dict.TryGetValue(key, out var result)) {
                return result;
            }
            dict.Add(key, value);
            return value;
        }


        /// <summary>
        /// Get or create new item
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
            where TValue : new() {
            if (dict is null) {
                throw new ArgumentNullException(nameof(dict));
            }
            if (dict.TryGetValue(key, out var result)) {
                return result;
            }
            result = new TValue();
            dict[key] = result;
            return result;
        }
    }
}

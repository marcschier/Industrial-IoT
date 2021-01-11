// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Messaging {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Event properties
    /// </summary>
    public static class EventPropertiesEx {

        /// <summary>
        /// Convert to event properties
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static IEventProperties ToEventProperties<T>(this IEnumerable<KeyValuePair<string, T>> dictionary) {
            if (dictionary == null) {
                throw new ArgumentNullException(nameof(dictionary));
            }
            return new EventProperties<T>(dictionary);
        }

        /// <summary>
        /// Convert to event properties
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsSameAs(this IEventProperties dictionary, IEventProperties other) {
            if (dictionary == null) {
                throw new ArgumentNullException(nameof(dictionary));
            }
            if (other == null) {
                throw new ArgumentNullException(nameof(other));
            }
            var x = (IDictionary<string, string>)dictionary.ToDictionary(kv => kv.Key, kv => kv.Value);
            var y = other.ToDictionary(kv => kv.Key, kv => kv.Value);
            return x.DictionaryEqualsSafe(y);
        }

        /// <summary>
        /// Event properties
        /// </summary>
        private sealed class EventProperties<T> : IEventProperties {

            /// <inheritdoc/>
            public string this[string key] => _dictionary[key];

            /// <inheritdoc/>
            public EventProperties(IEnumerable<KeyValuePair<string, T>> dictionary) {
                if (dictionary is IDictionary<string, string> dict) {
                    _dictionary = dict;
                }
                else {
                    _dictionary = new Dictionary<string, string>();
                    foreach (var item in dictionary) {
                        _dictionary.AddOrUpdate(item.Key, item.Value?.ToString());
                    }
                }
            }

            /// <inheritdoc/>
            public bool TryGetValue(string key, out string value) {
                return _dictionary.TryGetValue(key, out value);
            }

            /// <inheritdoc/>
            public IEnumerator<KeyValuePair<string, string>> GetEnumerator() {
                return _dictionary.GetEnumerator();
            }

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator() {
                return _dictionary.GetEnumerator();
            }

            private readonly IDictionary<string, string> _dictionary;
        }
    }
}

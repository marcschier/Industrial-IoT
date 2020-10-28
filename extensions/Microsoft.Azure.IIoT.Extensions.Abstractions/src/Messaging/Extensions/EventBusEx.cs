// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Messaging {
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Event bus extensions
    /// </summary>
    public static class EventBusEx {

        /// <summary>
        /// Convert type name to event name - the namespace of the type should
        /// include versioning information.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetMoniker(this Type type) {
            if (type is null) {
                throw new ArgumentNullException(nameof(type));
            }
            var name = type.FullName
                .Replace("Microsoft.Azure.IIoT.", "", StringComparison.InvariantCultureIgnoreCase)
                .Replace(".", "-", StringComparison.InvariantCultureIgnoreCase)
                .Replace("Model", "", StringComparison.InvariantCultureIgnoreCase)
                .ToUpperInvariant();
            if (name.Length >= 50) {
                name = name.Substring(0, 50);
            }
            return name;
        }

        /// <summary>
        /// Register callback
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bus"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static Task<string> RegisterAsync<T>(this IEventBus bus,
            Func<T, Task> handler) {
            if (bus is null) {
                throw new ArgumentNullException(nameof(bus));
            }
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }
            return bus.RegisterAsync(new DelegateHandler<T>(handler));
        }

        /// <summary>
        /// Helper wrapper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class DelegateHandler<T> : IEventHandler<T> {

            internal DelegateHandler(Func<T, Task> handler) {
                _handler = handler;
            }

            /// <inheritdoc/>
            public Task HandleAsync(T eventData) {
                return _handler(eventData);
            }

            private readonly Func<T, Task> _handler;
        }
    }
}

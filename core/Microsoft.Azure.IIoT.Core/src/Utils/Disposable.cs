// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Utils {
    using System;

    /// <summary>
    /// Disposable wrapper
    /// </summary>
    public class Disposable : IDisposable {

        /// <summary>
        /// Create disposable
        /// </summary>
        /// <param name="disposable"></param>
        public Disposable(Action disposable) {
            _disposable = disposable;
        }

        /// <summary>
        /// Create disposable from disposables
        /// </summary>
        /// <param name="disposables"></param>
        public Disposable(params IDisposable[] disposables) {
            _disposable = () => {
                foreach (var disposable in disposables) {
                    Try.Op(() => disposable.Dispose());
                }
            };
        }

        /// <inheritdoc/>
        public void Dispose() {
            _disposable?.Invoke();
        }

        private readonly Action _disposable;
    }
}

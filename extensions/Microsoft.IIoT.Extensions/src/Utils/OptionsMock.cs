// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Utils {
    using System;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Options mock helper
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class OptionsMock<TOptions> : IOptions<TOptions>, IOptionsSnapshot<TOptions>,
        IOptionsMonitor<TOptions> where TOptions : class {

        /// <inheritdoc/>
        public OptionsMock(TOptions options = null) {
            Value = options ?? (TOptions)Activator.CreateInstance(typeof(TOptions));
        }

        /// <inheritdoc/>
        public TOptions Value { get; }

        /// <inheritdoc/>
        public TOptions CurrentValue => Value;

        /// <inheritdoc/>
        public TOptions Get(string name) {
            return Value;
        }

        /// <inheritdoc/>
        public IDisposable OnChange(Action<TOptions, string> listener) {
            return new Disposable();
        }
    }
}


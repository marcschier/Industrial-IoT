// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Configuration {
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Configuration base helper class
    /// </summary>
    public abstract class ConfigureOptionBase<T> : ConfigureOptionBase,
        IConfigureOptions<T>, IConfigureNamedOptions<T> where T : class {

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        protected ConfigureOptionBase(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public abstract void Configure(string name, T options);

        /// <inheritdoc/>
        public void Configure(T options) {
            Configure(string.Empty, options);
        }

        /// <summary>
        /// Helper to get options
        /// </summary>
        /// <returns></returns>
        public IOptions<T> ToOptions() {
            var t = Configuration.Get<T>();
            Configure(t);
            return Options.Create(t);
        }
    }
}

// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Extensions.Configuration {
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using System;

    /// <summary>
    /// Post configuration base helper class
    /// </summary>
    public abstract class PostConfigureOptionBase<T> : ConfigureOptionBase,
        IPostConfigureOptions<T> where T : class {

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        protected PostConfigureOptionBase(IConfiguration configuration = null) :
            base(configuration) {
        }

        /// <inheritdoc/>
        public abstract void PostConfigure(string name, T options);

        /// <summary>
        /// Helper to get options
        /// </summary>
        /// <returns></returns>
        public IOptions<T> ToOptions() {
            var t = Configuration.Get<T>() ?? (T)Activator.CreateInstance(typeof(T));
            PostConfigure(Options.DefaultName, t);
            return Options.Create(t);
        }
    }
}

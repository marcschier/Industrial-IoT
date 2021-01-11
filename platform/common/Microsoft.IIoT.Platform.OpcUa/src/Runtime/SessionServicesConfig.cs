// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.OpcUa.Runtime {
    using Microsoft.IIoT.Extensions.Configuration;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// Gateway configuration
    /// </summary>
    public class SessionServicesConfig : ConfigureOptionBase, ISessionServicesConfig {

        private const string kMaxSessionCountKey = "MaxSessionCount";
        private const string kMaxSessionTimeoutKey = "MaxSessionTimeout";
        private const string kMinSessionTimeoutKey = "MinSessionTimeout";
        private const string kMaxRequestAgeKey = "MaxRequestAge";
        private const string kNonceLengthKey = "NonceLength";

        /// <inheritdoc/>
        public int MaxSessionCount =>
            GetIntOrDefault(kMaxSessionCountKey, 1000);
        /// <inheritdoc/>
        public TimeSpan MaxSessionTimeout =>
            GetDurationOrDefault(kMaxSessionTimeoutKey, TimeSpan.FromHours(1));
        /// <inheritdoc/>
        public TimeSpan MinSessionTimeout =>
            GetDurationOrDefault(kMinSessionTimeoutKey, TimeSpan.FromSeconds(10));
        /// <inheritdoc/>
        public TimeSpan MaxRequestAge =>
            GetDurationOrDefault(kMaxRequestAgeKey, TimeSpan.FromMinutes(5));
        /// <inheritdoc/>
        public int NonceLength =>
            GetIntOrDefault(kNonceLengthKey, 32);

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public SessionServicesConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}

// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Core.Models {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Endpoint model extensions
    /// </summary>
    public static class EndpointModelEx {

        /// <summary>
        /// Convert to connection model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="user"></param>
        /// <param name="diagnostics"></param>
        /// <param name="operationTimeout"></param>
        /// <returns></returns>
        public static ConnectionModel ToConnectionModel(this EndpointModel model,
            CredentialModel user = null, DiagnosticsModel diagnostics = null, 
            TimeSpan? operationTimeout = null) {
            return new ConnectionModel {
                Endpoint = model.Clone(),
                User = user.Clone(),
                OperationTimeout = operationTimeout,
                Diagnostics = diagnostics.Clone(),
            };
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="model"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool IsSameAs(this EndpointModel model, EndpointModel that) {
            if (model == that) {
                return true;
            }
            if (model == null || that == null) {
                return false;
            }
            if (!that.HasSameSecurityProperties(model)) {
                return false;
            }
            if (!that.GetAllUrls().SequenceEqualsSafe(model.GetAllUrls())) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        /// <param name="model"></param>
        /// <param name="that"></param>
        /// <returns></returns>
        public static bool HasSameSecurityProperties(this EndpointModel model, 
            EndpointModel that) {
            if (model == that) {
                return true;
            }
            if (model == null || that == null) {
                return false;
            }
            if (!that.Certificate.SequenceEqualsSafe(model.Certificate)) {
                return false;
            }
            if (that.SecurityPolicy != model.SecurityPolicy && 
                that.SecurityPolicy != null && model.SecurityPolicy != null) {
                return false;
            }
            if ((that.SecurityMode ?? SecurityMode.Best) !=
                    (model.SecurityMode ?? SecurityMode.Best)) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Create unique hash
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public static int CreateConsistentHash(this EndpointModel endpoint) {
            if (endpoint is null) {
                return 0;
            }

            var hashCode = -1971667340;
            hashCode = (hashCode * -1521134295) +
                endpoint.GetAllUrls().SequenceGetHashSafe();
            hashCode = (hashCode * -1521134295) +
                EqualityComparer<string>.Default.GetHashCode(endpoint.SecurityPolicy);
            hashCode = (hashCode * -1521134295) +
                EqualityComparer<SecurityMode?>.Default.GetHashCode(
                    endpoint.SecurityMode ?? SecurityMode.Best);
            return hashCode;
        }

        /// <summary>
        /// Get all urls
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllUrls(this EndpointModel model) {
            if (model == null) {
                return null;
            }
            var all = model.Url.YieldReturn();
            if (model.AlternativeUrls != null) {
                all = all.Concat(model.AlternativeUrls);
            }
            return all;
        }

        /// <summary>
        /// Create Union with endpoint
        /// </summary>
        /// <param name="model"></param>
        /// <param name="endpoint"></param>
        public static void UnionWith(this EndpointModel model,
            EndpointModel endpoint) {
            if (model is null) {
                throw new System.ArgumentNullException(nameof(model));
            }

            if (endpoint == null) {
                return;
            }
            HashSet<string> urls;
            if (model.AlternativeUrls == null) {
                if (endpoint.AlternativeUrls != null) {
                    urls = new HashSet<string>(endpoint.AlternativeUrls);
                }
                else {
                    urls = new HashSet<string>();
                }
            }
            else {
                urls = model.AlternativeUrls.MergeWith(
                    endpoint.AlternativeUrls);
            }
            if (model.Url == null) {
                model.Url = endpoint.Url;
            }
            else {
                urls.Add(endpoint.Url);
            }
            urls.Remove(model.Url);
            model.AlternativeUrls = urls;
        }

        /// <summary>
        /// Deep clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EndpointModel Clone(this EndpointModel model) {
            if (model == null) {
                return null;
            }
            return new EndpointModel {
                Certificate = model.Certificate,
                AlternativeUrls = model.AlternativeUrls.ToHashSetSafe(),
                SecurityMode = model.SecurityMode,
                SecurityPolicy = model.SecurityPolicy,
                Url = model.Url
            };
        }
    }
}

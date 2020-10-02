// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.Twin.Models;
    using Microsoft.Azure.IIoT.Utils;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Browse services extensions
    /// </summary>
    public static class BrowseServicesEx {

        /// <summary>
        /// Browse all references if max references is null and user
        /// wants all. If user has requested maximum to return use
        /// <see cref="IBrowseServices{T}.NodeBrowseFirstAsync"/>
        /// </summary>
        /// <param name="service"></param>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<BrowseResultModel> NodeBrowseAsync<T>(
            this IBrowseServices<T> service, T endpoint, BrowseRequestModel request) {
            if (service is null) {
                throw new ArgumentNullException(nameof(service));
            }
            if (request is null) {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.MaxReferencesToReturn != null) {
                return await service.NodeBrowseFirstAsync(endpoint,
                    request).ConfigureAwait(false);
            }
            while (true) {
                var result = await service.NodeBrowseFirstAsync(endpoint,
                    request).ConfigureAwait(false);
                var references = new List<NodeReferenceModel>(result.References);
                while (result.ContinuationToken != null) {
                    try {
                        var next = await service.NodeBrowseNextAsync(endpoint,
                            new BrowseNextRequestModel {
                                ContinuationToken = result.ContinuationToken,
                                Header = request.Header,
                                NodeIdsOnly = request.NodeIdsOnly,
                                ReadVariableValues = request.ReadVariableValues,
                                TargetNodesOnly = request.TargetNodesOnly
                            }).ConfigureAwait(false);
                        references.AddRange(next.References);
                        result.ContinuationToken = next.ContinuationToken;
                    }
                    catch (Exception) {
                        await Try.Async(() => service.NodeBrowseNextAsync(endpoint,
                            new BrowseNextRequestModel {
                                ContinuationToken = result.ContinuationToken,
                                Abort = true
                            })).ConfigureAwait(false);
                        throw;
                    }
                }
                result.References = references;
                return result;
            }
        }
    }
}

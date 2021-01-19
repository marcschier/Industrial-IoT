// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Api.Models {
    using Microsoft.IIoT.Platform.Discovery.Events.v2.Models;
    using Microsoft.IIoT.Platform.Discovery.Models;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Event extensions
    /// </summary>
    public static class EventExtensions {

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static DiscoveryProgressApiModel ToApiModel(
            this DiscoveryProgressModel model) {
            return new DiscoveryProgressApiModel {
                Discovered = model.Discovered,
                EventType = (DiscoveryProgressType)model.EventType,
                Progress = model.Progress,
                Total = model.Total,
                RequestDetails = model.RequestDetails?
                    .ToDictionary(k => k.Key, v => v.Value),
                RequestId = model.Request?.Id,
                Result = model.Result,
                ResultDetails = model.ResultDetails?
                    .ToDictionary(k => k.Key, v => v.Value),
                DiscovererId = model.DiscovererId,
                TimeStamp = model.TimeStamp,
                Workers = model.Workers
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ApplicationEventApiModel ToApiModel(
            this ApplicationEventModel model) {
            return new ApplicationEventApiModel {
                EventType = (ApplicationEventType)model.EventType,
                Application = model.Application.ToApiModel()
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static EndpointEventApiModel ToApiModel(
            this EndpointEventModel model) {
            return new EndpointEventApiModel {
                EventType = (EndpointEventType)model.EventType,
                Endpoint = model.Endpoint.ToApiModel()
            };
        }
    }
}
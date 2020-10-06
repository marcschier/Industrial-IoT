// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Directory.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Directory.Events.v2.Models;
    using Microsoft.Azure.IIoT.Platform.Directory.Models;
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
        public static DiscovererEventApiModel ToApiModel(
            this DiscovererEventModel model) {
            return new DiscovererEventApiModel {
                EventType = (DiscovererEventType)model.EventType,
                Id = model.Id,
                Discoverer = model.Discoverer.ToApiModel()
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static SupervisorEventApiModel ToApiModel(
            this SupervisorEventModel model) {
            return new SupervisorEventApiModel {
                EventType = (SupervisorEventType)model.EventType,
                Id = model.Id,
                Supervisor = model.Supervisor.ToApiModel()
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static GatewayEventApiModel ToApiModel(
            this GatewayEventModel model) {
            return new GatewayEventApiModel {
                EventType = (GatewayEventType)model.EventType,
                Id = model.Id,
                Gateway = model.Gateway.ToApiModel()
            };
        }

        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static PublisherEventApiModel ToApiModel(
            this PublisherEventModel model) {
            return new PublisherEventApiModel {
                EventType = (PublisherEventType)model.EventType,
                Id = model.Id,
                Publisher = model.Publisher.ToApiModel()
            };
        }
    }
}
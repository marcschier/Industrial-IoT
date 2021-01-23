// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models {
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Events.v2.Models;

    /// <summary>
    /// Event extensions
    /// </summary>
    public static class EventExtensions {


        /// <summary>
        /// Convert to api model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static TwinEventApiModel ToApiModel(
            this TwinEventModel model) {
            return new TwinEventApiModel {
                EventType = (TwinEventType)model.EventType,
                Twin = model.Twin.ToApiModel()
            };
        }
    }
}
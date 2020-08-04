// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Activation state of the entity twin
    /// </summary>
    [DataContract]
    public enum EntityActivationState {

        /// <summary>
        /// Entity twin is deactivated (default)
        /// </summary>
        [EnumMember]
        Deactivated,

        /// <summary>
        /// Entity twin is activated but not connected
        /// </summary>
        [EnumMember]
        Activated,

        /// <summary>
        /// Entity twin is activated and connected to hub
        /// </summary>
        [EnumMember]
        ActivatedAndConnected
    }
}

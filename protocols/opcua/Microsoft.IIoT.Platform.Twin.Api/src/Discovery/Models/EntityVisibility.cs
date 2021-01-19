// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Discovery.Api.Models {
    using System.Runtime.Serialization;

    /// <summary>
    /// Visibility of the entity 
    /// </summary>
    [DataContract]
    public enum EntityVisibility {

        /// <summary>
        /// Unknown visibilty
        /// </summary>
        [EnumMember]
        Unknown,

        /// <summary>
        /// The entity is currently not visible
        /// </summary>
        [EnumMember]
        Lost,

        /// <summary>
        /// The entity was found in the network
        /// </summary>
        [EnumMember]
        Found
    }
}


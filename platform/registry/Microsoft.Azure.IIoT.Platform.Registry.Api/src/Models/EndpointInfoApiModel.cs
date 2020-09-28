// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Registry.Api.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using System.Runtime.Serialization;
    using System.Collections.Generic;
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Endpoint registration model
    /// </summary>
    [DataContract]
    public class EndpointInfoApiModel {

        /// <summary>
        /// Registered identifier of the endpoint
        /// </summary>
        [DataMember(Name = "id", Order = 0)]
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// Original endpoint url of the endpoint
        /// </summary>
        [DataMember(Name = "endpointUrl", Order = 1,
            EmitDefaultValue = false)]
        public string EndpointUrl { get; set; }

        /// <summary>
        /// Registered site of the endpoint
        /// </summary>
        [DataMember(Name = "siteId", Order = 2,
            EmitDefaultValue = false)]
        public string SiteId { get; set; }

        /// <summary>
        /// Supervisor that can manage the endpoint.
        /// </summary>
        [DataMember(Name = "supervisorId", Order = 3,
            EmitDefaultValue = false)]
        public string SupervisorId { get; set; }

        /// <summary>
        /// Discoverer that registered the endpoint
        /// </summary>
        [DataMember(Name = "discovererId", Order = 4,
            EmitDefaultValue = false)]
        public string DiscovererId { get; set; }

        /// <summary>
        /// Endpoint information of the registration
        /// </summary>
        [DataMember(Name = "endpoint", Order = 5)]
        public EndpointApiModel Endpoint { get; set; }

        /// <summary>
        /// Security level of the endpoint
        /// </summary>
        [DataMember(Name = "securityLevel", Order = 6,
            EmitDefaultValue = false)]
        public int? SecurityLevel { get; set; }

        /// <summary>
        /// Supported authentication methods that can be selected to
        /// obtain a credential and used to interact with the endpoint.
        /// </summary>
        [DataMember(Name = "authenticationMethods", Order = 7,
            EmitDefaultValue = false)]
        public List<AuthenticationMethodApiModel> AuthenticationMethods { get; set; }

        /// <summary>
        /// Application id endpoint is registered under.
        /// </summary>
        [DataMember(Name = "applicationId", Order = 8)]
        [Required]
        public string ApplicationId { get; set; }

        /// <summary>
        /// Activation state of endpoint
        /// </summary>
        [DataMember(Name = "activationState", Order = 9,
            EmitDefaultValue = false)]
        public EntityActivationState? ActivationState { get; set; }

        /// <summary>
        /// Last state of the activated endpoint
        /// </summary>
        [DataMember(Name = "endpointState", Order = 10,
            EmitDefaultValue = false)]
        public EndpointConnectivityState? EndpointState { get; set; }

        /// <summary>
        /// Last time endpoint was seen
        /// </summary>
        [DataMember(Name = "notSeenSince", Order = 11,
            EmitDefaultValue = false)]
        public DateTime? NotSeenSince { get; set; }

        /// <summary>
        /// Generation id
        /// </summary>
        [DataMember(Name = "generationId", Order = 12)]
        [Required]
        public string GenerationId { get; set; }
    }
}

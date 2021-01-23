// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service.Controllers {
    using Microsoft.IIoT.Protocols.OpcUa.Service.Filters;
    using Microsoft.IIoT.Protocols.OpcUa.Twin;
    using Microsoft.IIoT.Protocols.OpcUa.Twin.Api.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Transfer services
    /// </summary>
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/transfer")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanRead)]
    [ApiController]
    public class TransferController : ControllerBase {

        /// <summary>
        /// Create controller with service
        /// </summary>
        /// <param name="transfer"></param>
        public TransferController(ITransferServices<string> transfer) {
            _transfer = transfer;
        }

        /// <summary>
        /// Start model upload
        /// </summary>
        /// <remarks>
        /// Upload model from endpoint to a service.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="endpointId">The identifier of the activated endpoint.</param>
        /// <param name="request">The model upload request</param>
        /// <returns>The start upload response</returns>
        [HttpPost("{endpointId}")]
        public async Task<ModelUploadStartResponseApiModel> ModelUploadStartAsync(
            string endpointId, [FromBody][Required] ModelUploadStartRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var browseresult = await _transfer.ModelUploadStartAsync(endpointId,
                request.ToServiceModel()).ConfigureAwait(false);
            return browseresult.ToApiModel();
        }

        private readonly ITransferServices<string> _transfer;
    }
}

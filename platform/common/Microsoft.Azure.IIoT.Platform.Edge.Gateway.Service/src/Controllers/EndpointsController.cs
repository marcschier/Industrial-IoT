// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Edge.Gateway.Service.Controllers {
    using Microsoft.Azure.IIoT.Platform.Edge.Gateway.Service.Filters;
    using Microsoft.Azure.IIoT.Platform.Publisher;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    /// <summary>
    /// Publish from model
    /// </summary>
    [ApiVersion("2")][Route("v{version:apiVersion}/endpoints")]
    [ExceptionsFilter]
    [ApiController]
    public class EndpointsController : ControllerBase {

        /// <summary>
        /// Create controller with service
        /// </summary>
        /// <param name="processor"></param>
        public EndpointsController(IBulkPublishHandler processor) {
            _processor = processor;
        }

        /// <summary>
        /// Publish everything in the contained nodeset
        /// </summary>
        /// <remarks>
        /// Allows twin module to upload model information to
        /// bulk import into publisher.
        /// </remarks>
        /// <param name="endpointId"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpPut("{endpointId}/{fileName}")]
        public async Task ProcessAsync(string endpointId, string fileName) {
            await _processor.PublishFromNodesetAsync(endpointId, fileName,
                Request.Body, Request.ContentType);
        }

        private readonly IBulkPublishHandler _processor;
    }
}

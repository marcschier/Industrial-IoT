// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Service.Controllers {
    using Microsoft.Azure.IIoT.Platform.Publisher.Service.Auth;
    using Microsoft.Azure.IIoT.Platform.Publisher.Service.Filters;
    using Microsoft.Azure.IIoT.Platform.Publisher.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// Bulk manage writer and variable resources
    /// </summary>
    [ApiVersion("2")]
    [Route("v{version:apiVersion}/bulk")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanWrite)]
    [ApiController]
    public class BulkController : ControllerBase {

        /// <summary>
        /// Create controller
        /// </summary>
        /// <param name="bulk"></param>
        public BulkController(IDataSetBatchOperations bulk) {
            _bulk = bulk;
        }

        /// <summary>
        /// Add variables to writer
        /// </summary>
        /// <remarks>
        /// Adds variables in bulk or in other words a single operation to an
        /// existing dataset writer.
        /// </remarks>
        /// <param name="dataSetWriterId">The dataset writer identifier</param>
        /// <param name="request">Bulk add operations</param>
        /// <returns>Operation results</returns>
        [HttpPost("writers/{dataSetWriterId}")]
        public async Task<DataSetAddVariableBatchResponseApiModel> AddVariablesToDataSetWriterAsync(
            string dataSetWriterId, [FromBody] DataSetAddVariableBatchRequestApiModel request) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await _bulk.AddVariablesToDataSetWriterAsync(dataSetWriterId,
                request.ToServiceModel(), new PublisherOperationContextModel {
                    Time = DateTime.UtcNow,
                    AuthorityId = HttpContext.User.Identity.Name
                });
            return result.ToApiModel();
        }

        /// <summary>
        /// Add variables to default writer for endpoint
        /// </summary>
        /// <remarks>
        /// Adds variables in bulk to a default dataset writer for a particular endpoint.
        /// The dataset writer is created if it does not yet exists.  The dataset writer
        /// identifier is the endpoint id, which then can be used interchangeably.
        /// </remarks>
        /// <param name="endpointId">The endpoint identifier</param>
        /// <param name="request">Bulk add operations</param>
        /// <returns>Operation results</returns>
        [HttpPost("endpoints/{endpointId}")]
        public async Task<DataSetAddVariableBatchResponseApiModel> AddVariablesToDefaultDataSetWriterAsync(
            string endpointId, [FromBody] DataSetAddVariableBatchRequestApiModel request) {
            if (string.IsNullOrEmpty(endpointId)) {
                throw new ArgumentNullException(nameof(endpointId));
            }
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await _bulk.AddVariablesToDefaultDataSetWriterAsync(endpointId,
                request.ToServiceModel(), new PublisherOperationContextModel {
                    Time = DateTime.UtcNow,
                    AuthorityId = HttpContext.User.Identity.Name
                });
            return result.ToApiModel();
        }

        /// <summary>
        /// Remove variables from writer
        /// </summary>
        /// <remarks>
        /// Remove variables from a specified writer using bulk remove operations.
        /// All variables are removed regardless of generation id which means it
        /// will force delete the variable regardless of competing changes.
        /// </remarks>
        /// <param name="dataSetWriterId">The dataset writer identifier</param>
        /// <param name="request">Bulk remove operations</param>
        /// <returns>Operation results</returns>
        [HttpPost("writers/{dataSetWriterId}/remove")]
        public async Task<DataSetRemoveVariableBatchResponseApiModel> RemoveVariablesFromDataSetWriterAsync(
            string dataSetWriterId, [FromBody] DataSetRemoveVariableBatchRequestApiModel request) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await _bulk.RemoveVariablesFromDataSetWriterAsync(dataSetWriterId,
                request.ToServiceModel(), new PublisherOperationContextModel {
                    Time = DateTime.UtcNow,
                    AuthorityId = HttpContext.User.Identity.Name
                });
            return result.ToApiModel();
        }

        private readonly IDataSetBatchOperations _bulk;
    }
}

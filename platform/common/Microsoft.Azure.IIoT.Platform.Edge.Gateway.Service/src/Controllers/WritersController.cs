// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Edge.Gateway.Service.Controllers {
    using Microsoft.Azure.IIoT.Platform.Edge.Gateway.Service.Filters;
    using Microsoft.Azure.IIoT.Platform.Publisher.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Publisher;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Writer controller
    /// </summary>
    [ApiVersion("2")][Route("v{version:apiVersion}/writers")]
    [ExceptionsFilter]
    [ApiController]
    public class WritersController : ControllerBase {

        /// <summary>
        /// Create controller
        /// </summary>
        /// <param name="writers"></param>
        public WritersController(IDataSetWriterRegistry writers) {
            _writers = writers;
        }

        /// <summary>
        /// Get dataset writer
        /// </summary>
        /// <remarks>
        /// Returns a dataset writer with the provided identifier. The dataset
        /// writer has all its fields expanded.
        /// </remarks>
        /// <param name="dataSetWriterId">The datset writer identifier</param>
        /// <returns>A dataset writer with all fields expanded</returns>
        [HttpGet("{dataSetWriterId}")]
        public async Task<DataSetWriterApiModel> GetDataSetWriterAsync(
            string dataSetWriterId) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            var group = await _writers.GetDataSetWriterAsync(dataSetWriterId);
            return group.ToApiModel();
        }

        private readonly IDataSetWriterRegistry _writers;
    }
}
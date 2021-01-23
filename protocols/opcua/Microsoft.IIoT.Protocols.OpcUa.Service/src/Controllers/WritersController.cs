// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.OpcUa.Service.Controllers {
    using Microsoft.IIoT.Protocols.OpcUa.Service.Filters;
    using Microsoft.IIoT.Protocols.OpcUa.Publisher.Api.Models;
    using Microsoft.IIoT.Protocols.OpcUa.Publisher;
    using Microsoft.IIoT.Protocols.OpcUa.Core.Models;
    using Microsoft.IIoT.Extensions.AspNetCore.OpenApi;
	using Microsoft.IIoT.Extensions.Rpc;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using System;

    /// <summary>
    /// CRUD and Query data set writer and definition resources
    /// </summary>
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/writers")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanRead)]
    [ApiController]
    public class WritersController : ControllerBase {

        /// <summary>
        /// Create controller
        /// </summary>
        /// <param name="writers"></param>
        /// <param name="events"></param>
        public WritersController(IDataSetWriterRegistry writers,
			IGroupRegistrationT<WritersHub> events) {
            _writers = writers;
			_events = events;
        }

        /// <summary>
        /// Adds a new and empty dataset writer
        /// </summary>
        /// <remarks>
        /// Creates a new dataset writer and returns the assigned
        /// dataset writer identifier and generation.
        /// </remarks>
        /// <param name="request">The writer properties</param>
        /// <returns>Assigned identifier and generation</returns>
        [HttpPut]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task<DataSetWriterAddResponseApiModel> CreateDataSetWriterAsync(
            [FromBody] DataSetWriterAddRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await _writers.AddDataSetWriterAsync(
                request.ToServiceModel(), new OperationContextModel {
                    Time = DateTime.UtcNow,
                    AuthorityId = HttpContext.User.Identity.Name
                }).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Get dataset writer
        /// </summary>
        /// <remarks>
        /// Returns a dataset writer with the provided identifier. The dataset
        /// writer has all its fields expanded.  Depending on the size of the
        /// datset this can be a very expensive operation.  Instead a writer
        /// query followed by a paged listing of variables is many times a
        /// better solution.
        /// </remarks>
        /// <param name="dataSetWriterId">The datset writer identifier</param>
        /// <returns>A dataset writer with all fields expanded</returns>
        [HttpGet("{dataSetWriterId}")]
        public async Task<DataSetWriterApiModel> GetDataSetWriterAsync(
            string dataSetWriterId) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            var group = await _writers.GetDataSetWriterAsync(dataSetWriterId).ConfigureAwait(false);
            return group.ToApiModel();
        }

        /// <summary>
        /// Updates a datset writer
        /// </summary>
        /// <remarks>
        /// Patches or updates properties of a datset writer. A generation
        /// identifier must be provided.  If resource is out of date error
        /// is returned patching must be retried with the current generation.
        /// </remarks>
        /// <param name="dataSetWriterId">The datset writer identifier</param>
        /// <param name="request">Updated properties</param>
        /// <returns></returns>
        [HttpPatch("{dataSetWriterId}")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task UpdateDataSetWriterAsync(string dataSetWriterId,
            [FromBody] DataSetWriterUpdateRequestApiModel request) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            await _writers.UpdateDataSetWriterAsync(dataSetWriterId,
                request.ToServiceModel(), new OperationContextModel {
                    Time = DateTime.UtcNow,
                    AuthorityId = HttpContext.User.Identity.Name
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a event filter definition
        /// </summary>
        /// <remarks>
        /// Creates an event filter definition for a newly created dataset
        /// writer.  The writer must not have any existing variables or
        /// event filter definition set.
        /// </remarks>
        /// <param name="dataSetWriterId">The datset writer identifier</param>
        /// <param name="request">Event filter definition</param>
        /// <returns>Generation id</returns>
        [HttpPut("{dataSetWriterId}/event")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task<DataSetAddEventResponseApiModel> CreateDataSetEventFilterAsync(
            string dataSetWriterId, [FromBody] DataSetAddEventRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await _writers.AddEventDataSetAsync(dataSetWriterId,
                request.ToServiceModel(), new OperationContextModel {
                    Time = DateTime.UtcNow,
                    AuthorityId = HttpContext.User.Identity.Name
                }).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Returns event filter definition
        /// </summary>
        /// <remarks>
        /// Returns an event filter definition for event dataset writer.
        /// </remarks>
        /// <param name="dataSetWriterId">The datset writer identifier</param>
        /// <returns>Event filter definition</returns>
        [HttpGet("{dataSetWriterId}/event")]
        public async Task<PublishedDataSetEventsApiModel> GetDataSetEventFilterAsync(
            string dataSetWriterId) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            var result = await _writers.GetEventDataSetAsync(dataSetWriterId).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Update event filter definition
        /// </summary>
        /// <remarks>
        /// Patches or updates properties of a datset event defintion. A generation
        /// identifier must be provided.  If resource is out of date error
        /// is returned patching must be retried with the current generation.
        /// </remarks>
        /// <param name="dataSetWriterId">The datset writer identifier</param>
        /// <param name="request">Updated event filter properties</param>
        /// <returns></returns>
        [HttpPatch("{dataSetWriterId}/event")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task UpdateDataSetEventFilterAsync(string dataSetWriterId,
            [FromBody] DataSetUpdateEventRequestApiModel request) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            await _writers.UpdateEventDataSetAsync(dataSetWriterId,
                request.ToServiceModel(), new OperationContextModel {
                    Time = DateTime.UtcNow,
                    AuthorityId = HttpContext.User.Identity.Name
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove event filter definition
        /// </summary>
        /// <remarks>
        /// Removes a event definition with the specified generation identifier.
        /// If resource is out of date error is returned patching should be
        /// retried with the current generation.
        /// </remarks>
        /// <param name="dataSetWriterId">The dataset writer identifier</param>
        /// <param name="generationId">Generation id of the event definition</param>
        /// <returns></returns>
        [HttpDelete("{dataSetWriterId}/event/{generationId}")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task DeleteDataSetEventFilterAsync(string dataSetWriterId,
            string generationId) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(generationId)) {
                throw new ArgumentNullException(nameof(generationId));
            }
            await _writers.RemoveEventDataSetAsync(dataSetWriterId,
                generationId, new OperationContextModel {
                    Time = DateTime.UtcNow,
                    AuthorityId = HttpContext.User.Identity.Name
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new dataset variable
        /// </summary>
        /// <remarks>
        /// Adds a new variable definition in the created dataset writer.
        /// The writer must not have any event filter definition set -
        /// both are mutually exclusive.
        /// </remarks>
        /// <param name="dataSetWriterId">The datset writer identifier</param>
        /// <param name="request">Variable definition</param>
        /// <returns>Generation and variable id</returns>
        [HttpPut("{dataSetWriterId}/variables")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task<DataSetAddVariableResponseApiModel> AddDataSetVariableAsync(
            string dataSetWriterId, [FromBody] DataSetAddVariableRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await _writers.AddDataSetVariableAsync(dataSetWriterId,
                request.ToServiceModel(), new OperationContextModel {
                    Time = DateTime.UtcNow,
                    AuthorityId = HttpContext.User.Identity.Name
                }).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Update dataset variable
        /// </summary>
        /// <remarks>
        /// Patches or updates properties of a datset variable. A generation
        /// identifier must be provided.  If resource is out of date error
        /// is returned patching must be retried with the current generation.
        /// </remarks>
        /// <param name="dataSetWriterId">The datset writer identifier</param>
        /// <param name="variableId">Variable id</param>
        /// <param name="request">Variable updates</param>
        /// <returns></returns>
        [HttpPatch("{dataSetWriterId}/variables/{variableId}")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task UpdateDataSetVariableAsync(string dataSetWriterId, string variableId,
            [FromBody] DataSetUpdateVariableRequestApiModel request) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(variableId)) {
                throw new ArgumentNullException(nameof(variableId));
            }
            await _writers.UpdateDataSetVariableAsync(dataSetWriterId,
                variableId, request.ToServiceModel(), new OperationContextModel {
                    Time = DateTime.UtcNow,
                    AuthorityId = HttpContext.User.Identity.Name
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Lists dataset variables
        /// </summary>
        /// <remarks>
        /// List all dataset variables for a particular writer or continues a query.
        /// </remarks>
        /// <param name="dataSetWriterId">The dataset writer identifier</param>
        /// <param name="continuationToken">Optional Continuation token</param>
        /// <param name="pageSize">Optional number of results to return</param>
        /// <returns>Dataset writers</returns>
        [HttpGet("{dataSetWriterId}/variables")]
        public async Task<PublishedDataSetVariableListApiModel> GetListOfDataSetVariablesAsync(
            string dataSetWriterId, string continuationToken, int? pageSize) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            continuationToken = Request.GetContinuationToken(continuationToken);
            pageSize = Request.GetPageSize(pageSize);
            var result = await _writers.ListDataSetVariablesAsync(dataSetWriterId,
                continuationToken, pageSize).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Query dataset variables
        /// </summary>
        /// <remarks>
        /// Query dataset variables in the writer that match a query model.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call the GetListOfDataSetVariables operation using the token to retrieve
        /// more results.
        /// </remarks>
        /// <param name="dataSetWriterId">The dataset writer identifier</param>
        /// <param name="query">Dataset writer query</param>
        /// <param name="pageSize">Optional number of results to return</param>
        /// <returns>Dataset writers</returns>
        [HttpPost("{dataSetWriterId}/variables/query")]
        public async Task<PublishedDataSetVariableListApiModel> QueryDataSetVariablesAsync(
            string dataSetWriterId, PublishedDataSetVariableQueryApiModel query, int? pageSize) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (query == null) {
                throw new ArgumentNullException(nameof(query));
            }
            pageSize = Request.GetPageSize(pageSize);
            var result = await _writers.QueryDataSetVariablesAsync(dataSetWriterId,
                query.ToServiceModel(), pageSize).ConfigureAwait(false);

            return result.ToApiModel();
        }

        /// <summary>
        /// Remove variable from dataset
        /// </summary>
        /// <remarks>
        /// Removes a variable with the specified generation identifier from a dataset .
        /// If resource is out of date error is returned patching should be
        /// retried with the current generation.
        /// </remarks>
        /// <param name="dataSetWriterId">The dataset writer identifier</param>
        /// <param name="variableId">Identifier of the variable</param>
        /// <param name="generationId">Generation of the variable</param>
        /// <returns></returns>
        [HttpDelete("{dataSetWriterId}/variables/{variableId}/{generationId}")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task RemoveDataSetVariableAsync(string dataSetWriterId,
            string variableId, string generationId) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(variableId)) {
                throw new ArgumentNullException(nameof(variableId));
            }
            if (string.IsNullOrEmpty(generationId)) {
                throw new ArgumentNullException(nameof(generationId));
            }
            await _writers.RemoveDataSetVariableAsync(dataSetWriterId, variableId,
                generationId, new OperationContextModel {
                    Time = DateTime.UtcNow,
                    AuthorityId = HttpContext.User.Identity.Name
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Get list of dataset writers
        /// </summary>
        /// <remarks>
        /// List all dataset writers that are registered or continues a query.
        /// </remarks>
        /// <param name="continuationToken">Optional Continuation token</param>
        /// <param name="pageSize">Optional number of results to return</param>
        /// <returns>Dataset writers</returns>
        [HttpGet]
        [AutoRestExtension(NextPageLinkName = "continuationToken")]
        public async Task<DataSetWriterInfoListApiModel> GetListOfDataSetWritersAsync(
            [FromQuery] string continuationToken, [FromQuery] int? pageSize) {
            continuationToken = Request.GetContinuationToken(continuationToken);
            pageSize = Request.GetPageSize(pageSize);
            var result = await _writers.ListDataSetWritersAsync(
                continuationToken, pageSize).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Query dataset writers
        /// </summary>
        /// <remarks>
        /// List dataset writers that match a query model.
        /// The returned model can contain a continuation token if more results are
        /// available.
        /// Call the GetListOfDataSetWriters operation using the token to retrieve
        /// more results.
        /// </remarks>
        /// <param name="query">Dataset writer query</param>
        /// <param name="pageSize">Optional number of results to return</param>
        /// <returns>Dataset writers</returns>
        [HttpPost("query")]
        public async Task<DataSetWriterInfoListApiModel> QueryDataSetWritersAsync(
            [FromBody] DataSetWriterInfoQueryApiModel query, [FromQuery] int? pageSize) {
            if (query == null) {
                throw new ArgumentNullException(nameof(query));
            }
            pageSize = Request.GetPageSize(pageSize);
            var result = await _writers.QueryDataSetWritersAsync(
                query.ToServiceModel(), pageSize).ConfigureAwait(false);

            return result.ToApiModel();
        }

        /// <summary>
        /// Delete dataset writer
        /// </summary>
        /// <remarks>
        /// Removes a dataset writer with the specified generation identifier.
        /// If resource is out of date error is returned patching should be
        /// retried with the current generation.
        /// </remarks>
        /// <param name="dataSetWriterId">The dataset writer identifier</param>
        /// <param name="generationId">Generation id of the instance</param>
        /// <returns></returns>
        [HttpDelete("{dataSetWriterId}/{generationId}")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task DeleteDataSetWriterAsync(string dataSetWriterId,
            string generationId) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(generationId)) {
                throw new ArgumentNullException(nameof(generationId));
            }
            await _writers.RemoveDataSetWriterAsync(dataSetWriterId, generationId,
                new OperationContextModel {
                    Time = DateTime.UtcNow,
                    AuthorityId = HttpContext.User.Identity.Name
                }).ConfigureAwait(false);
        }


        /// <summary>
        /// Subscribe to receive dataset writer item status updates
        /// </summary>
        /// <remarks>
        /// Register a client to receive status updates for variables and events
        /// in the dataset through SignalR.
        /// </remarks>
        /// <param name="dataSetWriterId">The dataset writer to subscribe to</param>
        /// <param name="connectionId">The connection that will receive status
        /// updates.</param>
        /// <returns></returns>
        [HttpPut("{dataSetWriterId}/status")]
        public async Task SubscribeAsync(string dataSetWriterId,
            [FromBody] string connectionId) {
            await _events.SubscribeAsync(dataSetWriterId,
                connectionId).ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe from receiving dataset writer item status updates.
        /// </summary>
        /// <remarks>
        /// Unregister a client and stop it from receiving status updates
        /// for variables and events in the dataset.
        /// </remarks>
        /// <param name="dataSetWriterId">The writer to unsubscribe from </param>
        /// <param name="connectionId">The connection that will not receive
        /// any more status updates.</param>
        /// <returns></returns>
        [HttpDelete("{dataSetWriterId}/status/{connectionId}")]
        public async Task UnsubscribeAsync(string dataSetWriterId,
            string connectionId) {
            await _events.UnsubscribeAsync(dataSetWriterId,
                connectionId).ConfigureAwait(false);
        }

        /// <summary>
        /// Subscribe to receive samples
        /// </summary>
        /// <remarks>
        /// Register a client to receive publisher samples through SignalR.
        /// </remarks>
        /// <param name="dataSetWriterId">The writer to subscribe to</param>
        /// <param name="variableId">The variable to subscribe to</param>
        /// <param name="connectionId">The connection that will receive publisher
        /// samples.</param>
        /// <returns></returns>
        [HttpPut("{dataSetWriterId}/variables/{variableId}")]
        public async Task SubscribeVariableAsync(string dataSetWriterId, string variableId,
            [FromBody] string connectionId) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(variableId)) {
                throw new ArgumentNullException(nameof(variableId));
            }
            await _events.SubscribeAsync($"{dataSetWriterId}_{variableId}",
                connectionId).ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe from receiving samples.
        /// </summary>
        /// <remarks>
        /// Unregister a client and stop it from receiving samples.
        /// </remarks>
        /// <param name="dataSetWriterId">The writer to unsubscribe from </param>
        /// <param name="variableId">The variable to unsubscribe from</param>
        /// <param name="connectionId">The connection that will not receive
        /// any more published samples</param>
        /// <returns></returns>
        [HttpDelete("{dataSetWriterId}/variables/{variableId}/{connectionId}")]
        public async Task UnsubscribeVariableAsync(string dataSetWriterId, string variableId,
            string connectionId) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            if (string.IsNullOrEmpty(variableId)) {
                throw new ArgumentNullException(nameof(variableId));
            }
            await _events.UnsubscribeAsync($"{dataSetWriterId}_{variableId}",
                connectionId).ConfigureAwait(false);
        }

        /// <summary>
        /// Subscribe to receive dataset event messages
        /// </summary>
        /// <remarks>
        /// Register a client to receive publisher samples through SignalR.
        /// </remarks>
        /// <param name="dataSetWriterId">The writer to subscribe to</param>
        /// <param name="connectionId">The connection that will receive publisher
        /// samples.</param>
        /// <returns></returns>
        [HttpPut("{dataSetWriterId}/event")]
        public async Task SubscribeEventsAsync(string dataSetWriterId,
            [FromBody] string connectionId) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            await _events.SubscribeAsync($"{dataSetWriterId}_event",
                connectionId).ConfigureAwait(false);
        }

        /// <summary>
        /// Unsubscribe from receiving dataset event messages.
        /// </summary>
        /// <remarks>
        /// Unregister a client and stop it from receiving samples.
        /// </remarks>
        /// <param name="dataSetWriterId">The writer to unsubscribe from </param>
        /// <param name="connectionId">The connection that will not receive
        /// any more published samples</param>
        /// <returns></returns>
        [HttpDelete("{dataSetWriterId}/event/{connectionId}")]
        public async Task UnsubscribeEventsAsync(string dataSetWriterId,
            string connectionId) {
            if (string.IsNullOrEmpty(dataSetWriterId)) {
                throw new ArgumentNullException(nameof(dataSetWriterId));
            }
            await _events.UnsubscribeAsync($"{dataSetWriterId}_event",
                connectionId).ConfigureAwait(false);
        }

        private readonly IGroupRegistrationT<WritersHub> _events;
        private readonly IDataSetWriterRegistry _writers;
    }
}

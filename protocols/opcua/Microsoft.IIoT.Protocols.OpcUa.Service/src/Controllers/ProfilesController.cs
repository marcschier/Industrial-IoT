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
    using System.Linq;

    /// <summary>
    /// Historian services
    /// </summary>
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/profiles")]
    [ExceptionsFilter]
    [Authorize(Policy = Policies.CanWrite)]
    [ApiController]
    public class ProfilesController : ControllerBase {

        /// <summary>
        /// Create controller with service
        /// </summary>
        /// <param name="historian"></param>
        public ProfilesController(IHistorianServices<string> historian) {
            _historian = historian ?? throw new ArgumentNullException(nameof(historian));
        }

        /// <summary>
        /// Read historic processed values at specified times
        /// </summary>
        /// <remarks>
        /// Read processed history values of a node if available using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history read request</param>
        /// <returns>The historic values</returns>
        [HttpPost("hda/{twinId}/values/read")]
        [Authorize(Policy = Policies.CanRead)]
        public async Task<HistoryReadResponseApiModel<HistoricValueApiModel[]>> HistoryReadValuesAsync(
            string twinId,
            [FromBody][Required] HistoryReadRequestApiModel<ReadValuesDetailsApiModel> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var readresult = await _historian.HistoryReadValuesAsync(
                twinId, request.ToServiceModel(d => d.ToServiceModel())).ConfigureAwait(false);
            return readresult.ToApiModel(d => d?.Select(v => v.ToApiModel()).ToArray());
        }

        /// <summary>
        /// Read historic values at specified times
        /// </summary>
        /// <remarks>
        /// Read historic values of a node if available using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history read request</param>
        /// <returns>The historic values</returns>
        [HttpPost("hda/{twinId}/values/read/attimes")]
        [Authorize(Policy = Policies.CanRead)]
        public async Task<HistoryReadResponseApiModel<HistoricValueApiModel[]>> HistoryReadValuesAtTimesAsync(
            string twinId,
            [FromBody][Required] HistoryReadRequestApiModel<ReadValuesAtTimesDetailsApiModel> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var readresult = await _historian.HistoryReadValuesAtTimesAsync(
                twinId, request.ToServiceModel(d => d.ToServiceModel())).ConfigureAwait(false);
            return readresult.ToApiModel(d => d?.Select(v => v.ToApiModel()).ToArray());
        }

        /// <summary>
        /// Read historic processed values at specified times
        /// </summary>
        /// <remarks>
        /// Read processed history values of a node if available using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history read request</param>
        /// <returns>The historic values</returns>
        [HttpPost("hda/{twinId}/values/read/processed")]
        [Authorize(Policy = Policies.CanRead)]
        public async Task<HistoryReadResponseApiModel<HistoricValueApiModel[]>> HistoryReadProcessedValuesAsync(
            string twinId,
            [FromBody][Required] HistoryReadRequestApiModel<ReadProcessedValuesDetailsApiModel> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var readresult = await _historian.HistoryReadProcessedValuesAsync(
                twinId, request.ToServiceModel(d => d.ToServiceModel())).ConfigureAwait(false);
            return readresult.ToApiModel(d => d?.Select(v => v.ToApiModel()).ToArray());
        }

        /// <summary>
        /// Read historic modified values at specified times
        /// </summary>
        /// <remarks>
        /// Read processed history values of a node if available using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history read request</param>
        /// <returns>The historic values</returns>
        [HttpPost("hda/{twinId}/values/read/modified")]
        [Authorize(Policy = Policies.CanRead)]
        public async Task<HistoryReadResponseApiModel<HistoricValueApiModel[]>> HistoryReadModifiedValuesAsync(
            string twinId,
            [FromBody][Required] HistoryReadRequestApiModel<ReadModifiedValuesDetailsApiModel> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var readresult = await _historian.HistoryReadModifiedValuesAsync(
                twinId, request.ToServiceModel(d => d.ToServiceModel())).ConfigureAwait(false);
            return readresult.ToApiModel(d => d?.Select(v => v.ToApiModel()).ToArray());
        }

        /// <summary>
        /// Read next batch of historic values
        /// </summary>
        /// <remarks>
        /// Read next batch of historic values of a node using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history read next request</param>
        /// <returns>The historic values</returns>
        [HttpPost("hda/{twinId}/values/read/next")]
        [Authorize(Policy = Policies.CanRead)]
        public async Task<HistoryReadNextResponseApiModel<HistoricValueApiModel[]>> HistoryReadValueNextAsync(
            string twinId,
            [FromBody][Required] HistoryReadNextRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var readresult = await _historian.HistoryReadValuesNextAsync(
                twinId, request.ToServiceModel()).ConfigureAwait(false);
            return readresult.ToApiModel(d => d?.Select(v => v.ToApiModel()).ToArray());
        }

        /// <summary>
        /// Insert historic values
        /// </summary>
        /// <remarks>
        /// Insert historic values using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history insert request</param>
        /// <returns>The history insert result</returns>
        [HttpPost("hda/{twinId}/values/insert")]
        public async Task<HistoryUpdateResponseApiModel> HistoryInsertValuesAsync(
            string twinId,
            [FromBody][Required] HistoryUpdateRequestApiModel<InsertValuesDetailsApiModel> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var writeResult = await _historian.HistoryInsertValuesAsync(
                twinId, request.ToServiceModel(d => d.ToServiceModel())).ConfigureAwait(false);
            return writeResult.ToApiModel();
        }

        /// <summary>
        /// Replace historic values
        /// </summary>
        /// <remarks>
        /// Replace historic values using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history replace request</param>
        /// <returns>The history replace result</returns>
        [HttpPost("hda/{twinId}/values/replace")]
        public async Task<HistoryUpdateResponseApiModel> HistoryReplaceValuesAsync(
            string twinId,
            [FromBody][Required] HistoryUpdateRequestApiModel<ReplaceValuesDetailsApiModel> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var writeResult = await _historian.HistoryReplaceValuesAsync(
                twinId, request.ToServiceModel(d => d.ToServiceModel())).ConfigureAwait(false);
            return writeResult.ToApiModel();
        }

        /// <summary>
        /// Delete value history at specified times
        /// </summary>
        /// <remarks>
        /// Delete value history using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history update request</param>
        /// <returns>The history delete result</returns>
        [HttpPost("hda/{twinId}/values/delete/attimes")]
        public async Task<HistoryUpdateResponseApiModel> HistoryDeleteValuesAtTimesAsync(
            string twinId,
            [FromBody][Required] HistoryUpdateRequestApiModel<DeleteValuesAtTimesDetailsApiModel> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var writeResult = await _historian.HistoryDeleteValuesAtTimesAsync(
                twinId, request.ToServiceModel(d => d.ToServiceModel())).ConfigureAwait(false);
            return writeResult.ToApiModel();
        }

        /// <summary>
        /// Delete historic values
        /// </summary>
        /// <remarks>
        /// Delete historic values using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history update request</param>
        /// <returns>The history delete result</returns>
        [HttpPost("hda/{twinId}/values/delete")]
        public async Task<HistoryUpdateResponseApiModel> HistoryDeleteValuesAsync(
            string twinId,
            [FromBody][Required] HistoryUpdateRequestApiModel<DeleteValuesDetailsApiModel> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var writeResult = await _historian.HistoryDeleteValuesAsync(
                twinId, request.ToServiceModel(d => d.ToServiceModel())).ConfigureAwait(false);
            return writeResult.ToApiModel();
        }

        /// <summary>
        /// Delete historic values
        /// </summary>
        /// <remarks>
        /// Delete historic values using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history update request</param>
        /// <returns>The history delete result</returns>
        [HttpPost("hda/{twinId}/values/delete/modified")]
        public async Task<HistoryUpdateResponseApiModel> HistoryDeleteModifiedValuesAsync(
            string twinId,
            [FromBody][Required] HistoryUpdateRequestApiModel<DeleteModifiedValuesDetailsApiModel> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var writeResult = await _historian.HistoryDeleteModifiedValuesAsync(
                twinId, request.ToServiceModel(d => d.ToServiceModel())).ConfigureAwait(false);
            return writeResult.ToApiModel();
        }

        /// <summary>
        /// Read historic events
        /// </summary>
        /// <remarks>
        /// Read historic events of a node if available using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history read request</param>
        /// <returns>The historic events</returns>
        [HttpPost("hda/{twinId}/events/read")]
        [Authorize(Policy = Policies.CanRead)]
        public async Task<HistoryReadResponseApiModel<HistoricEventApiModel[]>> HistoryReadEventsAsync(
            string twinId,
            [FromBody][Required] HistoryReadRequestApiModel<ReadEventsDetailsApiModel> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var readresult = await _historian.HistoryReadEventsAsync(
                twinId, request.ToServiceModel(d => d.ToServiceModel())).ConfigureAwait(false);
            return readresult.ToApiModel(d => d?.Select(v => v.ToApiModel()).ToArray());
        }

        /// <summary>
        /// Read next batch of historic events
        /// </summary>
        /// <remarks>
        /// Read next batch of historic events of a node using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history read next request</param>
        /// <returns>The historic events</returns>
        [HttpPost("hda/{twinId}/events/read/next")]
        [Authorize(Policy = Policies.CanRead)]
        public async Task<HistoryReadNextResponseApiModel<HistoricEventApiModel[]>> HistoryReadEventsNextAsync(
            string twinId,
            [FromBody][Required] HistoryReadNextRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var readresult = await _historian.HistoryReadEventsNextAsync(
                twinId, request.ToServiceModel()).ConfigureAwait(false);
            return readresult.ToApiModel(d => d?.Select(v => v.ToApiModel()).ToArray());
        }

        /// <summary>
        /// Insert historic events
        /// </summary>
        /// <remarks>
        /// Insert historic events using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history insert request</param>
        /// <returns>The history insert result</returns>
        [HttpPost("hda/{twinId}/events/insert")]
        public async Task<HistoryUpdateResponseApiModel> HistoryInsertEventsAsync(
            string twinId,
            [FromBody][Required] HistoryUpdateRequestApiModel<InsertEventsDetailsApiModel> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var writeResult = await _historian.HistoryInsertEventsAsync(
                twinId, request.ToServiceModel(d => d.ToServiceModel())).ConfigureAwait(false);
            return writeResult.ToApiModel();
        }

        /// <summary>
        /// Replace historic events
        /// </summary>
        /// <remarks>
        /// Replace historic events using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history replace request</param>
        /// <returns>The history replace result</returns>
        [HttpPost("hda/{twinId}/events/replace")]
        public async Task<HistoryUpdateResponseApiModel> HistoryReplaceEventsAsync(
            string twinId,
            [FromBody][Required] HistoryUpdateRequestApiModel<ReplaceEventsDetailsApiModel> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var writeResult = await _historian.HistoryReplaceEventsAsync(
                twinId, request.ToServiceModel(d => d.ToServiceModel())).ConfigureAwait(false);
            return writeResult.ToApiModel();
        }

        /// <summary>
        /// Delete historic events
        /// </summary>
        /// <remarks>
        /// Delete historic events using historic access.
        /// The endpoint must be activated and connected and the module client
        /// and server must trust each other.
        /// </remarks>
        /// <param name="twinId">The identifier of the activated endpoint.</param>
        /// <param name="request">The history update request</param>
        /// <returns>The history delete result</returns>
        [HttpPost("hda/{twinId}/events/delete")]
        public async Task<HistoryUpdateResponseApiModel> HistoryDeleteEventsAsync(
            string twinId,
            [FromBody][Required] HistoryUpdateRequestApiModel<DeleteEventsDetailsApiModel> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var writeResult = await _historian.HistoryDeleteEventsAsync(
                twinId, request.ToServiceModel(d => d.ToServiceModel())).ConfigureAwait(false);
            return writeResult.ToApiModel();
        }

        private readonly IHistorianServices<string> _historian;
    }
}

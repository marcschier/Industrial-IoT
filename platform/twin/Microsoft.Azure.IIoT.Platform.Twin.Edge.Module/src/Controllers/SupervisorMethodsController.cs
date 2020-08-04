// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Twin.Edge.Module.Controllers {
    using Microsoft.Azure.IIoT.Platform.Twin.Edge.Module.Filters;
    using Microsoft.Azure.IIoT.Platform.Twin.Edge.Module.Models;
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Twin;
    using Microsoft.Azure.IIoT.Platform.History.Api.Models;
    using Microsoft.Azure.IIoT.Platform.History;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Registry;
    using Microsoft.Azure.IIoT.Platform.Core.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using Microsoft.Azure.IIoT.Platform.Edge;
    using Microsoft.Azure.IIoT.Serializers;
    using Microsoft.Azure.IIoT.Hosting;
    using Microsoft.Azure.IIoT.Rpc;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Supervisor method controller
    /// </summary>
    [Version(1)]
    [Version(2)]
    [ExceptionsFilter]
    public class SupervisorMethodsController : IMethodController {

        /// <summary>
        /// Create controller with service
        /// </summary>
        /// <param name="supervisor"></param>
        /// <param name="activator"></param>
        /// <param name="discovery"></param>
        /// <param name="nodes"></param>
        /// <param name="historian"></param>
        /// <param name="browse"></param>
        /// <param name="upload"></param>
        public SupervisorMethodsController(ISupervisorServices supervisor,
            IActivationServices<string> activator, ICertificateServices<EndpointModel> discovery,
            INodeServices<EndpointModel> nodes, IHistoricAccessServices<EndpointModel> historian,
            IBrowseServices<EndpointModel> browse, ITransferServices<EndpointModel> upload) {
            _discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
            _supervisor = supervisor ?? throw new ArgumentNullException(nameof(supervisor));
            _upload = upload ?? throw new ArgumentNullException(nameof(upload));
            _browse = browse ?? throw new ArgumentNullException(nameof(browse));
            _historian = historian ?? throw new ArgumentNullException(nameof(historian));
            _nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            _activator = activator ?? throw new ArgumentNullException(nameof(activator));
        }

        /// <summary>
        /// Reset supervisor
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ResetAsync() {
            await _supervisor.ResetAsync();
            return true;
        }

        /// <summary>
        /// Get status
        /// </summary>
        /// <returns></returns>
        public async Task<SupervisorStatusApiModel> GetStatusAsync() {
            var result = await _supervisor.GetStatusAsync();
            return result.ToApiModel();
        }

        /// <summary>
        /// Browse
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BrowseResponseApiModel> BrowseAsync(
            EndpointApiModel endpoint, BrowseRequestInternalApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (endpoint == null) {
                throw new ArgumentNullException(nameof(endpoint));
            }
            var result = await _browse.NodeBrowseFirstAsync(
                endpoint.ToServiceModel(), request.ToServiceModel());
            return result.ToApiModel();
        }

        /// <summary>
        /// Browse next
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BrowseNextResponseApiModel> BrowseNextAsync(
            EndpointApiModel endpoint, BrowseNextRequestInternalApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (endpoint == null) {
                throw new ArgumentNullException(nameof(endpoint));
            }
            var result = await _browse.NodeBrowseNextAsync(
                endpoint.ToServiceModel(), request.ToServiceModel());
            return result.ToApiModel();
        }

        /// <summary>
        /// Browse by path
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<BrowsePathResponseApiModel> BrowsePathAsync(
            EndpointApiModel endpoint, BrowsePathRequestInternalApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (endpoint == null) {
                throw new ArgumentNullException(nameof(endpoint));
            }
            var result = await _browse.NodeBrowsePathAsync(
                endpoint.ToServiceModel(), request.ToServiceModel());
            return result.ToApiModel();
        }

        /// <summary>
        /// Read value
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ValueReadResponseApiModel> ValueReadAsync(
            EndpointApiModel endpoint, ValueReadRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (endpoint == null) {
                throw new ArgumentNullException(nameof(endpoint));
            }
            var result = await _nodes.NodeValueReadAsync(
                endpoint.ToServiceModel(), request.ToServiceModel());
            return result.ToApiModel();
        }

        /// <summary>
        /// Write value
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ValueWriteResponseApiModel> ValueWriteAsync(
            EndpointApiModel endpoint, ValueWriteRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (endpoint == null) {
                throw new ArgumentNullException(nameof(endpoint));
            }
            var result = await _nodes.NodeValueWriteAsync(
                endpoint.ToServiceModel(), request.ToServiceModel());
            return result.ToApiModel();
        }

        /// <summary>
        /// Get method meta data
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<MethodMetadataResponseApiModel> MethodMetadataAsync(
            EndpointApiModel endpoint, MethodMetadataRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (endpoint == null) {
                throw new ArgumentNullException(nameof(endpoint));
            }
            var result = await _nodes.NodeMethodGetMetadataAsync(
                endpoint.ToServiceModel(), request.ToServiceModel());
            return result.ToApiModel();
        }

        /// <summary>
        /// Call method
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<MethodCallResponseApiModel> MethodCallAsync(
            EndpointApiModel endpoint, MethodCallRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (endpoint == null) {
                throw new ArgumentNullException(nameof(endpoint));
            }
            var result = await _nodes.NodeMethodCallAsync(
                endpoint.ToServiceModel(), request.ToServiceModel());
            return result.ToApiModel();
        }

        /// <summary>
        /// Read attributes
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ReadResponseApiModel> NodeReadAsync(
            EndpointApiModel endpoint, ReadRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await _nodes.NodeReadAsync(
                endpoint.ToServiceModel(), request.ToServiceModel());
            return result.ToApiModel();
        }

        /// <summary>
        /// Write attributes
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<WriteResponseApiModel> NodeWriteAsync(
            EndpointApiModel endpoint, WriteRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await _nodes.NodeWriteAsync(
                endpoint.ToServiceModel(), request.ToServiceModel());
            return result.ToApiModel();
        }

        /// <summary>
        /// Read history
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<HistoryReadResponseApiModel<VariantValue>> HistoryReadAsync(
            EndpointApiModel endpoint, HistoryReadRequestApiModel<VariantValue> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await _historian.HistoryReadAsync(
               endpoint.ToServiceModel(), request.ToServiceModel());
            return result.ToApiModel();
        }

        /// <summary>
        /// Read next history
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<HistoryReadNextResponseApiModel<VariantValue>> HistoryReadNextAsync(
            EndpointApiModel endpoint, HistoryReadNextRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await _historian.HistoryReadNextAsync(
               endpoint.ToServiceModel(), request.ToServiceModel());
            return result.ToApiModel();
        }

        /// <summary>
        /// Update history
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<HistoryUpdateResponseApiModel> HistoryUpdateAsync(
            EndpointApiModel endpoint, HistoryUpdateRequestApiModel<VariantValue> request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await _historian.HistoryUpdateAsync(
               endpoint.ToServiceModel(), request.ToServiceModel());
            return result.ToApiModel();
        }

        /// <summary>
        /// Start model upload
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<ModelUploadStartResponseApiModel> UploadModelAsync(
            EndpointApiModel endpoint, ModelUploadStartRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            if (endpoint == null) {
                throw new ArgumentNullException(nameof(endpoint));
            }
            var result = await _upload.ModelUploadStartAsync(
                endpoint.ToServiceModel(), request.ToServiceModel());
            return result.ToApiModel();
        }

        /// <summary>
        /// Get endpoint certificate
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<byte[]> GetEndpointCertificateAsync(
            EndpointApiModel endpoint) {
            if (endpoint == null) {
                throw new ArgumentNullException(nameof(endpoint));
            }
            var result = await _discovery.GetEndpointCertificateAsync(
                endpoint.ToServiceModel());
            return result;
        }

        /// <summary>
        /// Activate endpoint
        /// </summary>
        /// <param name="id"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public async Task<bool> ActivateEndpointAsync(string id, string secret) {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException(nameof(id));
            }
            if (string.IsNullOrEmpty(secret)) {
                throw new ArgumentNullException(nameof(secret));
            }
            if (!secret.IsBase64()) {
                throw new ArgumentException("not base64", nameof(secret));
            }
            await _activator.ActivateAsync(id, secret);
            return true;
        }

        /// <summary>
        /// Deactivate endpoint
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeactivateEndpointAsync(string id) {
            if (string.IsNullOrEmpty(id)) {
                throw new ArgumentNullException(nameof(id));
            }
            await _activator.DeactivateAsync(id);
            return true;
        }

        private readonly IActivationServices<string> _activator;
        private readonly ICertificateServices<EndpointModel> _discovery;
        private readonly ISupervisorServices _supervisor;
        private readonly ITransferServices<EndpointModel> _upload;
        private readonly IBrowseServices<EndpointModel> _browse;
        private readonly IHistoricAccessServices<EndpointModel> _historian;
        private readonly INodeServices<EndpointModel> _nodes;
    }
}

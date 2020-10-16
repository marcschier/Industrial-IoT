// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Vault.Service.Controllers {
    using Microsoft.Azure.IIoT.Platform.Vault.Service.Auth;
    using Microsoft.Azure.IIoT.Platform.Vault.Service.Filters;
    using Microsoft.Azure.IIoT.Platform.Vault.Service.Models;
    using Microsoft.Azure.IIoT.Platform.Vault.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Vault;
    using Microsoft.Azure.IIoT.AspNetCore.OpenApi;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Trust group services.
    /// </summary>
    [ExceptionsFilter]
    [ApiVersion("3")]
    [Route("v{version:apiVersion}/groups")]
    [Authorize(Policy = Policies.CanRead)]
    [ApiController]
    public sealed class TrustGroupsController : ControllerBase {

        /// <summary>
        /// Create the controller.
        /// </summary>
        /// <param name="groups">Policy store client</param>
        /// <param name="services"></param>
        public TrustGroupsController(ITrustGroupStore groups, ITrustGroupServices services) {
            _groups = groups;
            _services = services;
        }

        /// <summary>
        /// Get information about all groups.
        /// </summary>
        /// <remarks>
        /// A trust group has a root certificate which issues certificates
        /// to entities.  Entities can be part of a trust group and thus
        /// trust the root certificate and all entities that the root has
        /// issued certificates for.
        /// </remarks>
        /// <param name="continuationToken">optional, continuation token</param>
        /// <param name="pageSize">optional, the maximum number of result per page</param>
        /// <returns>The configurations</returns>
        [HttpGet]
        [AutoRestExtension(NextPageLinkName = "continuationToken")]
        public async Task<TrustGroupRegistrationListApiModel> ListGroupsAsync(
            [FromQuery] string continuationToken, [FromQuery] int? pageSize) {
            continuationToken = Request.GetContinuationToken(continuationToken);
            pageSize = Request.GetPageSize(pageSize);
            // Use service principal
            HttpContext.User = null; // TODO Set sp
            var config = await _groups.ListGroupsAsync(continuationToken, pageSize).ConfigureAwait(false);
            return config.ToApiModel();
        }

        /// <summary>
        /// Get group information.
        /// </summary>
        /// <remarks>
        /// A trust group has a root certificate which issues certificates
        /// to entities.  Entities can be part of a trust group and thus
        /// trust the root certificate and all entities that the root has
        /// issued certificates for.
        /// </remarks>
        /// <param name="groupId">The group id</param>
        /// <returns>The configuration</returns>
        [HttpGet("{groupId}")]
        public async Task<TrustGroupRegistrationApiModel> GetGroupAsync(
            string groupId) {
            var group = await _groups.GetGroupAsync(groupId).ConfigureAwait(false);
            return group.ToApiModel();
        }

        /// <summary>
        /// Update group registration.
        /// </summary>
        /// <remarks>
        /// Use this function with care and only if you are aware of
        /// the security implications.
        /// Requires manager role.
        /// </remarks>
        /// <param name="groupId">The group id</param>
        /// <param name="request">The group configuration</param>
        /// <returns>The configuration</returns>
        [HttpPost("{groupId}")]
        [Authorize(Policy = Policies.CanManage)]
        public async Task UpdateGroupAsync(string groupId,
            [FromBody] [Required] TrustGroupUpdateRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            await _groups.UpdateGroupAsync(groupId, request.ToServiceModel()).ConfigureAwait(false);
        }

        /// <summary>
        /// Create new root group.
        /// </summary>
        /// <remarks>
        /// Requires manager role.
        /// </remarks>
        /// <param name="request">The create request</param>
        /// <returns>The group registration response</returns>
        [HttpPut("root")]
        [Authorize(Policy = Policies.CanManage)]
        public async Task<TrustGroupRegistrationResponseApiModel> CreateRootAsync(
            [FromBody] [Required] TrustGroupRootCreateRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await _groups.CreateRootAsync(request.ToServiceModel()).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Create new sub-group of an existing group.
        /// </summary>
        /// <remarks>
        /// Requires manager role.
        /// </remarks>
        /// <param name="request">The create request</param>
        /// <returns>The group registration response</returns>
        [HttpPut]
        [Authorize(Policy = Policies.CanManage)]
        public async Task<TrustGroupRegistrationResponseApiModel> CreateGroupAsync(
            [FromBody] [Required] TrustGroupRegistrationRequestApiModel request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }
            var result = await _groups.CreateGroupAsync(request.ToServiceModel()).ConfigureAwait(false);
            return result.ToApiModel();
        }

        /// <summary>
        /// Renew a group CA Certificate.
        /// </summary>
        /// <remark>
        /// A new key and CA cert is created for the group.
        /// The new issuer cert and CRL become active immediately
        /// for signing.
        /// All newly approved certificates are signed with the new key.
        /// </remark>
        /// <param name="groupId"></param>
        /// <returns>The new Issuer CA certificate</returns>
        [HttpPost("{groupId}/renew")]
        [Authorize(Policy = Policies.CanManage)]
        public async Task RenewIssuerCertificateAsync(string groupId) {
            await _services.RenewCertificateAsync(groupId).ConfigureAwait(false);
        }

        /// <summary>
        /// Delete a group.
        /// </summary>
        /// <remarks>
        /// After this operation the Issuer CA, CRLs and keys become inaccessible.
        /// Use this function with extreme caution.
        /// Requires manager role.
        /// </remarks>
        /// <param name="groupId">The group id</param>
        /// <returns></returns>
        [HttpDelete("{groupId}")]
        [Authorize(Policy = Policies.CanManage)]
        public async Task DeleteGroupAsync(string groupId) {
            await _groups.DeleteGroupAsync(groupId).ConfigureAwait(false);
        }

        private readonly ITrustGroupStore _groups;
        private readonly ITrustGroupServices _services;
    }
}

// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace Microsoft.IIoT.Platform.Identity.Provider.Controllers {
    using Microsoft.IIoT.Platform.Identity.Provider.Filters;
    using Microsoft.IIoT.Platform.Identity.Provider.Models;
    using Microsoft.IIoT.AspNetCore.Authentication;
    using IdentityServer4.Events;
    using IdentityServer4.Extensions;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// This sample controller allows a user to revoke grants given to clients
    /// </summary>
    [SecurityHeaders]
    [ExceptionsFilter]
    [Authorize]
    public class GrantsController : Controller {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clients;
        private readonly IResourceStore _resources;
        private readonly IEventService _events;

        /// <summary>
        /// Create controller
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="clients"></param>
        /// <param name="resources"></param>
        /// <param name="events"></param>
        public GrantsController(IIdentityServerInteractionService interaction,
            IClientStore clients,
            IResourceStore resources,
            IEventService events) {
            _interaction = interaction;
            _clients = clients;
            _resources = resources;
            _events = events;
        }

        /// <summary>
        /// Show list of grants
        /// </summary>
        [HttpGet]
#pragma warning disable IDE1006 // Naming Styles
        public async Task<IActionResult> Index() {
#pragma warning restore IDE1006 // Naming Styles
            return View("Index", await BuildViewModelAsync());
        }

        /// <summary>
        /// Handle postback to revoke a client
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
#pragma warning disable IDE1006 // Naming Styles
        public async Task<IActionResult> Revoke(string clientId) {
#pragma warning restore IDE1006 // Naming Styles
            await _interaction.RevokeUserConsentAsync(clientId).ConfigureAwait(false);
            await _events.RaiseAsync(new GrantsRevokedEvent(User.GetSubjectId(), clientId)).ConfigureAwait(false);

            return RedirectToAction("Index");
        }

        private async Task<GrantsViewModel> BuildViewModelAsync() {
            var grants = await _interaction.GetAllUserGrantsAsync().ConfigureAwait(false);

            var list = new List<GrantViewModel>();
            foreach (var grant in grants) {
                var client = await _clients.FindClientByIdAsync(grant.ClientId).ConfigureAwait(false);
                if (client != null) {
                    var resources = await _resources.FindResourcesByScopeAsync(grant.Scopes).ConfigureAwait(false);

                    var item = new GrantViewModel() {
                        ClientId = client.ClientId,
                        ClientName = client.ClientName ?? client.ClientId,
                        ClientLogoUrl = client.LogoUri,
                        ClientUrl = client.ClientUri,
                        Description = grant.Description,
                        Created = grant.CreationTime,
                        Expires = grant.Expiration,
                        IdentityGrantNames = resources.IdentityResources.Select(x => x.DisplayName ?? x.Name).ToArray(),
                        ApiGrantNames = resources.ApiScopes.Select(x => x.DisplayName ?? x.Name).ToArray()
                    };

                    list.Add(item);
                }
            }

            return new GrantsViewModel {
                Grants = list
            };
        }
    }
}
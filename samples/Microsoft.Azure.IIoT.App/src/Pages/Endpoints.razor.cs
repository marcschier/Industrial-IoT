// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Pages {
    using Microsoft.Azure.IIoT.App.Data;
    using Microsoft.Azure.IIoT.App.Models;
    using Microsoft.Azure.IIoT.App.Services;
    using Microsoft.Azure.IIoT.Platform.Registry.Api;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using Microsoft.AspNetCore.Components;
    using System;
    using System.Threading.Tasks;

    public partial class Endpoints {

        [Parameter]
        public string DiscovererId { get; set; } = string.Empty;

        [Parameter]
        public string ApplicationId { get; set; } = string.Empty;

        public EndpointInfo EndpointData { get; set; }

        protected override async Task LoadPageContentAsync(bool getNextPage) {
            Items = await RegistryHelper.GetEndpointListAsync(DiscovererId, ApplicationId, Items, getNextPage).ConfigureAwait(false);
        }

        protected override async Task SubscribeContentEventsAsync() {
            _events = await RegistryServiceEvents.SubscribeEndpointEventsAsync(
                    ev => InvokeAsync(() => EndpointEvent(ev))).ConfigureAwait(false);
        }

        private Task EndpointEvent(EndpointEventApiModel ev) {
            Items.Results.Update(ev);
            StateHasChanged();
            return Task.CompletedTask;
        }

        private static bool IsEndpointSeen(EndpointInfo endpoint) {
            return endpoint.EndpointModel?.NotSeenSince == null;
        }

        private static bool IsIdGiven(string id) {
            return !string.IsNullOrEmpty(id) && id != Registry.PathAll;
        }

        /// <summary>
        /// Checks whether the endpoint is activated
        /// </summary>
        /// <param name="endpoint">The endpoint info</param>
        /// <returns>True if the endpoint is activated, false otherwise</returns>
        private static bool IsEndpointActivated(EndpointInfo endpoint) {
            return endpoint.EndpointModel.ActivationState == EntityActivationState.Activated ||
                 endpoint.EndpointModel.ActivationState == EntityActivationState.Activated;
        }

        /// <summary>
        /// Creates a css string for an endpoint row based on activity and availability of the endpoint
        /// </summary>
        /// <param name="endpoint">The endpoint info</param>
        /// <returns>The css string</returns>
        private static string GetEndpointVisibilityString(EndpointInfo endpoint) {
            if (!IsEndpointSeen(endpoint)) {
                return "enabled-false";
            }
            else if (IsEndpointActivated(endpoint)) {
                return "enabled-true activated-true";
            }
            else {
                return "enabled-true";
            }
        }

        /// <summary>
        /// Activate or deactivate an endpoint
        /// </summary>
        /// <param name="endpointId"></param>
        /// <returns></returns>
        internal async Task SetActivationAsync(EndpointInfo endpoint) {
            try {
                await RegistryService.UpdateEndpointAsync(endpoint.EndpointModel.Id,
                    new EndpointInfoUpdateApiModel {
                        GenerationId = endpoint.EndpointModel.GenerationId,
                        ActivationState = IsEndpointActivated(endpoint) ?
                            EntityActivationState.Activated :
                            EntityActivationState.Deactivated
                    }).ConfigureAwait(false);
            }
            catch (Exception e) {
                Status = e.Message;
            }
        }

        // <summary>
        /// Open then Drawer
        /// </summary>
        /// <param name="OpenDrawer"></param>
        private void OpenDrawer(EndpointInfo endpoint) {
            IsOpen = true;
            EndpointData = endpoint;
        }
    }
}
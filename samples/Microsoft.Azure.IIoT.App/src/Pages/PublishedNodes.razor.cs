// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Pages {
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Azure.IIoT.Platform.Publisher.Api.Models;

    public partial class PublishedNodes {
        [Parameter]
        public string EndpointId { get; set; } = string.Empty;

        [Parameter]
        public string DiscovererId { get; set; } = string.Empty;

        [Parameter]
        public string ApplicationId { get; set; } = string.Empty;

        [Parameter]
        public string SupervisorId { get; set; } = string.Empty;

        private const string kValueGood = "Good";

        protected override async Task LoadPageContentAsync(bool getNextPage) {
            Items = await PublisherHelper.PublishedAsync(EndpointId, true).ConfigureAwait(false);
        }

        protected override async Task SubscribeContentEventsAsync() {
            _events = await PublisherServiceEvents.NodePublishSubscribeByEndpointAsync(EndpointId,
                samples => InvokeAsync(() => GetPublishedNodeData(samples))).ConfigureAwait(false);
        }

        private static bool IsIdGiven(string id) {
            return !string.IsNullOrEmpty(id);
        }

        /// <summary>
        /// GetPublishedNodeData
        /// </summary>
        /// <param name="samples"></param>
        private Task GetPublishedNodeData(MonitoredItemMessageApiModel samples) {
            foreach (var node in Items.Results) {
                if (node.PublishedItem.NodeId == samples.NodeId) {
                    node.Value = samples.Value?.ToJson()?.TrimQuotes();
                    node.Status = string.IsNullOrEmpty(samples.Status) ? kValueGood : samples.Status;
                    node.Timestamp = samples.Timestamp.Value.ToLocalTime().ToString();
                    StateHasChanged();
                }
            }
            return Task.CompletedTask;
        }
    }
}
// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Pages {
    using System.Threading.Tasks;
    using Microsoft.Azure.IIoT.App.Data;
    using Microsoft.Azure.IIoT.Platform.Directory.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Directory.Api;
    using Microsoft.Azure.IIoT.App.Models;

    public partial class Publishers {

        public PublisherInfo Publisher { get; set; }

        protected override async Task LoadPageContentAsync(bool getNextPage) {
            Items = await RegistryHelper.GetPublisherListAsync(Items, getNextPage).ConfigureAwait(false);
        }

        protected override async Task SubscribeContentEventsAsync() {
            _events = await DirectoryServiceEvents.SubscribePublisherEventsAsync(
                    ev => InvokeAsync(() => PublisherEvent(ev))).ConfigureAwait(false);
        }

        private Task PublisherEvent(PublisherEventApiModel ev) {
            Items.Results.Update(ev);
            StateHasChanged();
            return Task.CompletedTask;
        }
    }
}
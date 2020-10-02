// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Pages {
    using System.Threading.Tasks;
    using Microsoft.Azure.IIoT.App.Data;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Api;

    public partial class Gateways {
        protected override async Task LoadPageContentAsync(bool getNextPage) {
            Items = await RegistryHelper.GetGatewayListAsync(Items, getNextPage).ConfigureAwait(false);
        }

        protected override async Task SubscribeContentEventsAsync() {
            _events = await RegistryServiceEvents.SubscribeGatewayEventsAsync(
                    ev => InvokeAsync(() => GatewayEvent(ev))).ConfigureAwait(false);
        }

        private Task GatewayEvent(GatewayEventApiModel ev) {
            Items.Results.Update(ev);
            StateHasChanged();
            return Task.CompletedTask;
        }
    }
}
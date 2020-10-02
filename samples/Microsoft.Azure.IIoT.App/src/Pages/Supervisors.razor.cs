// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Pages {
    using System.Threading.Tasks;
    using Microsoft.Azure.IIoT.App.Data;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Api;

    public partial class Supervisors {
        public string SupervisorId { get; set; }

        protected override async Task LoadPageContentAsync(bool getNextPage) {
            Items = await RegistryHelper.GetSupervisorListAsync(Items, getNextPage).ConfigureAwait(false);
        }

        protected override async Task SubscribeContentEventsAsync() {
            _events = await RegistryServiceEvents.SubscribeSupervisorEventsAsync(
                    async data => {
                        await InvokeAsync(() => SupervisorEvent(data)).ConfigureAwait(false);
                    }).ConfigureAwait(false);
        }

        // <summary>
        /// Open then Drawer
        /// </summary>
        /// <param name="OpenDrawer"></param>
        private void OpenDrawer(string supervisorId) {
            IsOpen = true;
            SupervisorId = supervisorId;
        }

        /// <summary>
        /// Reset Supervisor
        /// </summary>
        /// <param name="supervisorId"></param>
        private async Task ResetSupervisorUIAsync(string supervisorId) {
            Status = await RegistryHelper.ResetSupervisorAsync(supervisorId).ConfigureAwait(false);
        }

        /// <summary>
        /// action on Supervisor Event
        /// </summary>
        /// <param name="ev"></param>
        private Task SupervisorEvent(SupervisorEventApiModel ev) {
            Items.Results.Update(ev);
            StateHasChanged();
            return Task.CompletedTask;
        }
    }
}
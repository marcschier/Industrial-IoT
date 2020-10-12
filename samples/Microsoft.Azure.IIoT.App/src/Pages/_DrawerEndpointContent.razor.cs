// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Pages {
    using Microsoft.AspNetCore.Components;
    using Microsoft.Azure.IIoT.App.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using System.Threading.Tasks;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE1006 // Naming Styles
    public partial class _DrawerEndpointContent {
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0079 // Remove unnecessary suppression
        [Parameter]
        public EndpointInfo EndpointData { get; set; }
        public ApplicationRegistrationApiModel Application{ get; set; }

        public bool IsLoading { get; set; }

        /// <summary>
        /// OnInitialized
        /// </summary>
        protected override void OnInitialized() {
            IsLoading = true;
        }

        /// <summary>
        /// OnAfterRenderAsync
        /// </summary>
        /// <param name="firstRender"></param>
        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                Application = await RegistryService.GetApplicationAsync(EndpointData.EndpointModel.ApplicationId).ConfigureAwait(false);
                IsLoading = false;
                StateHasChanged();
            }
        }
    }
}

// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Pages {
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE1006 // Naming Styles
    public partial class _DrawerSupervisorContent {
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0079 // Remove unnecessary suppression
        [Parameter]
        public string SupervisorId { get; set; }

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
        protected override Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                IsLoading = false;
                StateHasChanged();
            }
            return Task.CompletedTask;
        }
    }
}
﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Pages {
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;

    public partial class _DrawerSupervisorContent {
        [Parameter]
        public string SupervisorId { get; set; }

        public SupervisorStatusApiModel SupervisorStatus { get; set; }

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
                SupervisorStatus = await RegistryHelper.GetSupervisorStatusAsync(SupervisorId);
                IsLoading = false;
                StateHasChanged();
            }
        }
    }
}
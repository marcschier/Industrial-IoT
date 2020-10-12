﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Pages {
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Azure.IIoT.App.Models;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE1006 // Naming Styles
    public partial class _DrawerDiscoverer {
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0079 // Remove unnecessary suppression
        [Parameter]
        public DiscovererInfo DiscovererData { get; set; }

        [Parameter]
        public EventCallback Onclick { get; set; }

        private DiscovererInfoRequested InputData { get; set; }
        private string DiscoveryUrl { get; set; }
        private string Status { get; set; }
        private string ButtonLabel { get; set; }
        private string IdleTimeView { get; set; }
        private bool UrlListVisible { get; set; }

        /// <summary>
        /// OnInitialized
        /// </summary>
        protected override void OnInitialized() {
            if (DiscovererData.IsAdHocDiscovery) {
                ButtonLabel = "Apply & Scan";
                IdleTimeView = "displayNone";

            }
            else {
                ButtonLabel = "Apply";
                IdleTimeView = "displayBlock";
            }
            InputData = DiscovererData.ToDiscovererInfoRequested();
        }

        /// <summary>
        /// Close Drawer and update discovery
        /// </summary>
        private async Task UpdateDiscovererConfigAsync() {
            DiscovererData.TryUpdateData(InputData);
            await Onclick.InvokeAsync(DiscovererData).ConfigureAwait(false);
        }

        private void UrlListButtonClick() {
            UrlListVisible = !UrlListVisible;
        }

        private void DiscoveryAddUrlButtonClick() {
            InputData.AddDiscoveryUrl(DiscoveryUrl);
            DiscoveryUrl = string.Empty;
        }
    }
}
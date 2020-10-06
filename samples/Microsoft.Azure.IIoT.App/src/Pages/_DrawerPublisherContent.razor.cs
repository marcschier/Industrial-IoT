// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Pages {
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Azure.IIoT.Platform.Twin.Api.Models;
    using Microsoft.Azure.IIoT.App.Models;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE1006 // Naming Styles
    public partial class _DrawerPublisherContent {
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0079 // Remove unnecessary suppression
        [Parameter]
        public ListNode NodeData { get; set; }

        [Parameter]
        public EventCallback Onclick { get; set; }

        private ListNodeRequested InputData { get; set; }

        /// <summary>
        /// OnInitialized
        /// </summary>
        protected override void OnInitialized() {
            if (NodeData.PublishedItem == null) {
                NodeData.PublishedItem = new PublishedItemApiModel();
            }

            InputData = new ListNodeRequested(NodeData.PublishedItem);
        }


        /// <summary>
        /// Close Drawer and update discovery
        /// </summary>
        /// <param name="discoverer"></param>
        private async Task UpdatePublishedNodeConfigAsync() {
            NodeData.TryUpdateData(InputData);
            await Onclick.InvokeAsync(NodeData).ConfigureAwait(false);
        }
    }
}
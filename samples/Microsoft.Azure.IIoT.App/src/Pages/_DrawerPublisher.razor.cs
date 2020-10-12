// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Pages {
    using Microsoft.AspNetCore.Components;
    using Microsoft.Azure.IIoT.App.Models;
    using System.Threading.Tasks;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE1006 // Naming Styles
    public partial class _DrawerPublisher {
#pragma warning restore IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning restore IDE1006 // Naming Styles
        [Parameter]
#pragma warning restore IDE0079 // Remove unnecessary suppression
        public PublisherInfo Publisher { get; set; }

        private PublisherInfoRequested InputData { get; set; }

        /// <summary>
        /// OnInitialized
        /// </summary>
        protected override void OnInitialized() {
            InputData = new PublisherInfoRequested(Publisher);
        }

        /// <summary>
        /// Close Drawer and update publisher
        /// </summary>
        private async Task UpdatePublisherConfigAsync() {
            Publisher.TryUpdateData(InputData);
            await RegistryHelper.UpdatePublisherAsync(Publisher).ConfigureAwait(false);
        }
    }
}

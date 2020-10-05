// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Pages {
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.IIoT.App.Models;
    using Microsoft.Azure.IIoT.App.Data;
    using Microsoft.Azure.IIoT.Platform.Registry.Api;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;

    public sealed partial class Discoverers {
        public bool IsSearching { get; set; }
        public DiscovererInfo DiscovererData { get; set; }
        private string EventResult { get; set; }
        private string ScanResult { get; set; } = "displayNone";

        private IAsyncDisposable Discovery { get; set; }
        private bool IsDiscoveryEventSubscribed { get; set; }

        protected override async Task LoadPageContentAsync(bool getNextPage) {
            Items = await RegistryHelper.GetDiscovererListAsync(Items, getNextPage).ConfigureAwait(false);
        }

        protected override async Task SubscribeContentEventsAsync() {
           _events = await RegistryServiceEvents.SubscribeDiscovererEventsAsync(
                   ev => InvokeAsync(() => DiscovererEvent(ev))).ConfigureAwait(false);
        }

        /// <summary>
        /// Enable discoverer scan
        /// </summary>
        /// <param name="discoverer"></param>
        private async Task SetScanAsync(DiscovererInfo discoverer, bool checkStatus) {
           try {
               discoverer.ScanStatus = checkStatus;
               EventResult = string.Empty;

               if (discoverer.ScanStatus == true) {
                   if (!IsDiscoveryEventSubscribed) {
                       Discovery = await RegistryServiceEvents.SubscribeDiscoveryProgressByDiscovererIdAsync(
                           discoverer.DiscovererModel.Id, async data => {
                               await InvokeAsync(() => ScanProgress(data)).ConfigureAwait(false);
                           }).ConfigureAwait(false);
                   }

                   IsDiscoveryEventSubscribed = true;
                   discoverer.IsSearching = true;
                   ScanResult = "displayBlock";
                   DiscovererData = discoverer;
               }
               else {
                   discoverer.IsSearching = false;
                   ScanResult = "displayNone";
                   if (Discovery != null) {
                       await Discovery.DisposeAsync();
                   }
                   IsDiscoveryEventSubscribed = false;
               }
               Status = await RegistryHelper.SetDiscoveryAsync(discoverer).ConfigureAwait(false);
           }
           catch {
               if (Discovery != null) {
                   await Discovery.DisposeAsync();
               }
               IsDiscoveryEventSubscribed = false;
           }
        }

        /// <summary>
        /// Start ad-hoc scan
        /// </summary>
        /// <param name="discoverer"></param>
        private async Task SetAdHocScanAsync(DiscovererInfo discoverer) {
           if (!IsDiscoveryEventSubscribed) {
               discoverer.DiscoveryRequestId = Guid.NewGuid().ToString();
               Discovery = await RegistryServiceEvents.SubscribeDiscoveryProgressByRequestIdAsync(
               discoverer.DiscoveryRequestId, async data => {
                   await InvokeAsync(() => ScanProgress(data)).ConfigureAwait(false);
               }).ConfigureAwait(false);
               IsDiscoveryEventSubscribed = true;
           }

           try {
               EventResult = string.Empty;

               discoverer.IsSearching = true;
               ScanResult = "displayBlock";
               DiscovererData = discoverer;
               Status = await RegistryHelper.DiscoverServersAsync(discoverer).ConfigureAwait(false);
           }
           catch {
               if (Discovery != null) {
                   await Discovery.DisposeAsync();
               }
               IsDiscoveryEventSubscribed = false;
           }
        }

        /// <summary>
        /// Open then Drawer
        /// </summary>
        /// <param name="OpenDrawer"></param>
        private void OpenDrawer(DiscovererInfo discoverer) {
           IsOpen = true;
           DiscovererData = discoverer;
        }

        /// <summary>
        /// display discoverers scan events
        /// </summary>
        /// <param name="ev"></param>
        private void ScanProgress(DiscoveryProgressApiModel ev) {
           var ts = ev.TimeStamp.ToLocalTime();
           switch (ev.EventType) {
               case DiscoveryProgressType.Pending:
                   EventResult += $"[{ts}] {ev.DiscovererId}: {ev.Total} waiting..." + System.Environment.NewLine;
                   break;
               case DiscoveryProgressType.Started:
                   EventResult += $"[{ts}] {ev.DiscovererId}: Started." + System.Environment.NewLine;
                   break;
               case DiscoveryProgressType.NetworkScanStarted:
                   EventResult += $"[{ts}] {ev.DiscovererId}: Scanning network..." + System.Environment.NewLine;
                   break;
               case DiscoveryProgressType.NetworkScanResult:
                   EventResult += $"[{ts}] {ev.DiscovererId}: {ev.Progress}/{ev.Total}: {ev.Discovered} addresses found - NEW: {ev.Result}..." + System.Environment.NewLine;
                   break;
               case DiscoveryProgressType.NetworkScanProgress:
                   EventResult += $"[{ts}] {ev.DiscovererId}: {ev.Progress}/{ev.Total}: {ev.Discovered} addresses found" + System.Environment.NewLine;
                   break;
               case DiscoveryProgressType.NetworkScanFinished:
                   EventResult += $"[{ts}] {ev.DiscovererId}: {ev.Progress}/{ev.Total}: {ev.Discovered} addresses found - complete!" + System.Environment.NewLine;
                   break;
               case DiscoveryProgressType.PortScanStarted:
                   EventResult += $"[{ts}] {ev.DiscovererId}: Scanning ports..." + System.Environment.NewLine;
                   break;
               case DiscoveryProgressType.PortScanResult:
                   EventResult += $"[{ts}] {ev.DiscovererId}: {ev.Progress}/{ev.Total}: {ev.Discovered} ports found - NEW: {ev.Result}" + System.Environment.NewLine;
                   break;
               case DiscoveryProgressType.PortScanProgress:
                   EventResult += $"[{ts}] {ev.DiscovererId}: {ev.Progress}/{ev.Total}: {ev.Discovered} ports found" + System.Environment.NewLine;
                   break;
               case DiscoveryProgressType.PortScanFinished:
                   EventResult += $"[{ts}] {ev.DiscovererId}: {ev.Progress}/{ev.Total}: {ev.Discovered} ports found - complete!" + System.Environment.NewLine;
                   break;
               case DiscoveryProgressType.ServerDiscoveryStarted:
                   EventResult += "==========================================" + System.Environment.NewLine;
                   EventResult += $"[{ts}] {ev.DiscovererId}: {ev.Progress}/{ev.Total}: Finding servers..." + System.Environment.NewLine;
                   break;
               case DiscoveryProgressType.EndpointsDiscoveryStarted:
                   EventResult += $"[{ts}] {ev.DiscovererId}: {ev.Progress}/{ev.Total}: ... {ev.Discovered} servers found - find endpoints on {ev.RequestDetails["url"]}..." + System.Environment.NewLine;
                   break;
               case DiscoveryProgressType.EndpointsDiscoveryFinished:
                   EventResult += $"[{ts}] {ev.DiscovererId}: {ev.Progress}/{ev.Total}: ... {ev.Discovered} servers found - {ev.Result} endpoints found on {ev.RequestDetails["url"]}..." + System.Environment.NewLine;
                   break;
               case DiscoveryProgressType.ServerDiscoveryFinished:
                   EventResult += $"[{ts}] {ev.DiscovererId}: {ev.Progress}/{ev.Total}: ... {ev.Discovered} servers found." + System.Environment.NewLine;
                   break;
               case DiscoveryProgressType.Cancelled:
                   EventResult += "==========================================" + System.Environment.NewLine;
                   EventResult += $"[{ts}] {ev.DiscovererId}: Cancelled." + System.Environment.NewLine;
                   if (DiscovererData != null) {
                       DiscovererData.IsSearching = false;
                   }
                   break;
               case DiscoveryProgressType.Error:
                   EventResult += "==========================================" + System.Environment.NewLine;
                   EventResult += $"[{ts}] {ev.DiscovererId}: Failure." + System.Environment.NewLine;
                   if (DiscovererData != null) {
                       DiscovererData.IsSearching = false;
                   }
                   break;
               case DiscoveryProgressType.Finished:
                   EventResult += "==========================================" + System.Environment.NewLine;
                   EventResult += $"[{ts}] {ev.DiscovererId}: Completed." + System.Environment.NewLine;
                   if (DiscovererData != null) {
                       DiscovererData.IsSearching = false;
                   }
                   break;
           }
           StateHasChanged();
        }

        /// <summary>
        /// ClickHandler
        /// </summary>
        async Task ClickHandlerAsync(DiscovererInfo discoverer) {
           CloseDrawer();
           if (discoverer.IsAdHocDiscovery) {
               await SetAdHocScanAsync(discoverer).ConfigureAwait(false);
           }
           else {
               await OnAfterRenderAsync(true).ConfigureAwait(false);
           }
        }

        /// <summary>
        /// refresh UI on DiscovererEvent
        /// </summary>
        /// <param name="ev"></param>
        private Task DiscovererEvent(DiscovererEventApiModel ev) {
           Items.Results.Update(ev);
           StateHasChanged();
           return Task.CompletedTask;
        }

        /// <summary>
        /// Close the scan result view
        /// </summary>
        public void CloseScanResultView() {
           ScanResult = "displayNone";
        }

        public async override ValueTask DisposeAsync() {
           if (_events != null) {
               await _events.DisposeAsync();
           }

           if (Discovery != null) {
               await Discovery.DisposeAsync();
           }
        }
    }
}
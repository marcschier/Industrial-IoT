// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Models {
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Azure.IIoT.App.Data;

    public abstract class ItemsCollection<T> : ComponentBase, IAsyncDisposable where T : class {
        public PagedResult<T> Items { get; set; } = new PagedResult<T>();

        public bool IsLoading { get; set; }

        public bool IsOpen { get; set; }

        public string Status { get; set; }

#pragma warning disable CA1051 // Do not declare visible instance fields
        protected string _tableView = "visible";
        protected string _tableEmpty = "displayNone";
        protected IAsyncDisposable _events;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public virtual async ValueTask DisposeAsync() {
            if (_events != null) {
                await _events.DisposeAsync();
            }
        }

        protected override void OnInitialized() {
            IsLoading = true;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                await LoadPageContentAsync(false).ConfigureAwait(false);
                IsLoading = false;
                CheckErrorOrEmpty();
                StateHasChanged();
                await SubscribeContentEventsAsync().ConfigureAwait(false);
            }
        }

        protected virtual Task LoadPageContentAsync(bool getNextPage = false) {
            return Task.CompletedTask;
        }

        protected abstract Task SubscribeContentEventsAsync();

        /// <summary>
        /// More items should be loaded
        /// </summary>
        /// <returns></returns>
        protected async Task LoadMoreItems() {
            IsLoading = true;
            if (!string.IsNullOrEmpty(Items.ContinuationToken)) {
                await LoadPageContentAsync(true).ConfigureAwait(false);
            }
            IsLoading = false;
            StateHasChanged();
        }

        /// <summary>
        /// Close the Drawer
        /// </summary>
        protected virtual void CloseDrawer() {
            IsOpen = false;
            StateHasChanged();
        }

        /// <summary>
        /// Check if there is an error
        /// </summary>
        protected void CheckErrorOrEmpty() {
            if (Items.Error != null) {
                _tableView = "hidden";
            }
            else if (Items.Results.Count == 0) {
                _tableEmpty = "displayBlock";
            }
            else {
                _tableEmpty = "displayNone";
            }
        }
    }
}

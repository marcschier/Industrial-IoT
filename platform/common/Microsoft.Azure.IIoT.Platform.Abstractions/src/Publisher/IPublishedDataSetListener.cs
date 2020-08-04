// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher {
    using Microsoft.Azure.IIoT.Platform.Publisher.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Notified when dataset item changes
    /// </summary>
    public interface IPublishedDataSetListener {

        /// <summary>
        /// New variable definition added to dataset in writer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="dataSetVariable"></param>
        /// <returns></returns>
        Task OnPublishedDataSetVariableAddedAsync(PublisherOperationContextModel context,
            string dataSetWriterId, PublishedDataSetVariableModel dataSetVariable);

        /// <summary>
        /// Called when variable definition in dataset was updated
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="dataSetVariable"></param>
        /// <returns></returns>
        Task OnPublishedDataSetVariableUpdatedAsync(PublisherOperationContextModel context,
            string dataSetWriterId, PublishedDataSetVariableModel dataSetVariable);

        /// <summary>
        /// Called when variable state changed
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="dataSetVariable"></param>
        /// <returns></returns>
        Task OnPublishedDataSetVariableStateChangeAsync(PublisherOperationContextModel context,
            string dataSetWriterId, PublishedDataSetVariableModel dataSetVariable);

        /// <summary>
        /// Called when variable was removed from dataset
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="variableId"></param>
        /// <returns></returns>
        Task OnPublishedDataSetVariableRemovedAsync(PublisherOperationContextModel context,
            string dataSetWriterId, string variableId);

        /// <summary>
        /// New dataset event definition added to writer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="eventDataSet"></param>
        /// <returns></returns>
        Task OnPublishedDataSetEventsAddedAsync(PublisherOperationContextModel context,
            string dataSetWriterId, PublishedDataSetEventsModel eventDataSet);

        /// <summary>
        /// Called when writer event definition was updated
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="eventDataSet"></param>
        /// <returns></returns>
        Task OnPublishedDataSetEventsUpdatedAsync(PublisherOperationContextModel context,
            string dataSetWriterId, PublishedDataSetEventsModel eventDataSet);

        /// <summary>
        /// Called when writer event state changed
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="eventDataSet"></param>
        /// <returns></returns>
        Task OnPublishedDataSetEventsStateChangeAsync(PublisherOperationContextModel context,
            string dataSetWriterId, PublishedDataSetEventsModel eventDataSet);

        /// <summary>
        /// Called when writer event definition was removed
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <returns></returns>
        Task OnPublishedDataSetEventsRemovedAsync(PublisherOperationContextModel context,
            string dataSetWriterId);
    }
}

// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Publisher {
    using Microsoft.IIoT.Platform.Publisher.Models;
    using Microsoft.IIoT.Platform.Core.Models;
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
        Task OnPublishedDataSetVariableAddedAsync(OperationContextModel context,
            string dataSetWriterId, PublishedDataSetVariableModel dataSetVariable);

        /// <summary>
        /// Called when variable definition in dataset was updated
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="dataSetVariable"></param>
        /// <returns></returns>
        Task OnPublishedDataSetVariableUpdatedAsync(OperationContextModel context,
            string dataSetWriterId, PublishedDataSetVariableModel dataSetVariable);

        /// <summary>
        /// Called when variable state changed
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="dataSetVariable"></param>
        /// <returns></returns>
        Task OnPublishedDataSetVariableStateChangeAsync(OperationContextModel context,
            string dataSetWriterId, PublishedDataSetVariableModel dataSetVariable);

        /// <summary>
        /// Called when variable was removed from dataset
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="variableId"></param>
        /// <returns></returns>
        Task OnPublishedDataSetVariableRemovedAsync(OperationContextModel context,
            string dataSetWriterId, string variableId);

        /// <summary>
        /// New dataset event definition added to writer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="eventDataSet"></param>
        /// <returns></returns>
        Task OnPublishedDataSetEventsAddedAsync(OperationContextModel context,
            string dataSetWriterId, PublishedDataSetEventsModel eventDataSet);

        /// <summary>
        /// Called when writer event definition was updated
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="eventDataSet"></param>
        /// <returns></returns>
        Task OnPublishedDataSetEventsUpdatedAsync(OperationContextModel context,
            string dataSetWriterId, PublishedDataSetEventsModel eventDataSet);

        /// <summary>
        /// Called when writer event state changed
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <param name="eventDataSet"></param>
        /// <returns></returns>
        Task OnPublishedDataSetEventsStateChangeAsync(OperationContextModel context,
            string dataSetWriterId, PublishedDataSetEventsModel eventDataSet);

        /// <summary>
        /// Called when writer event definition was removed
        /// </summary>
        /// <param name="context"></param>
        /// <param name="dataSetWriterId"></param>
        /// <returns></returns>
        Task OnPublishedDataSetEventsRemovedAsync(OperationContextModel context,
            string dataSetWriterId);
    }
}

﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api {
    using Microsoft.Azure.IIoT.Platform.Publisher.Api.Models;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Publisher service events
    /// </summary>
    public interface IPublisherServiceEvents {

        /// <summary>
        /// Subscribe to writer group events
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribeWriterGroupEventsAsync(
            Func<WriterGroupEventApiModel, Task> callback);

        /// <summary>
        /// Subscribe to dataset writer events
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribeDataSetWriterEventsAsync(
            Func<DataSetWriterEventApiModel, Task> callback);

        /// <summary>
        /// Subscribe to variable value changes
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="variableId"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribeDataSetVariableMessagesAsync(
            string dataSetWriterId, string variableId,
            Func<MonitoredItemMessageApiModel, Task> callback);

        /// <summary>
        /// Subscribe to variable value changes
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribeEventDataSetMessagesAsync(
            string dataSetWriterId, 
            Func<MonitoredItemMessageApiModel, Task> callback);

        /// <summary>
        /// Subscribe to dataset state changes
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribeDataSetItemStatusAsync(
            string dataSetWriterId,
            Func<MonitoredItemMessageApiModel, Task> callback);
    }
}

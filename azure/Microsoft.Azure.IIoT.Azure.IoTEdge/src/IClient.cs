// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Azure.IoTEdge {
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Shared;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// IoT Hub device client abstraction
    /// </summary>
    public interface IClient : IDisposable {

        /// <summary>
        /// Sends an event to device hub
        /// </summary>
        /// <param name="route"></param>
        /// <param name="message"></param>
        /// <param name="ct"></param>
        /// <returns>The message containing the event</returns>
        Task SendEventAsync(string route,
            Message message, CancellationToken ct = default);

        /// <summary>
        /// Sends a batch of events to device hub
        /// </summary>
        /// <param name="route"></param>
        /// <param name="messages"></param>
        /// <param name="ct"></param>
        /// <returns>The task containing the event</returns>
        Task SendEventBatchAsync(string route,
            IEnumerable<Message> messages, CancellationToken ct = default);

        /// <summary>
        /// Registers a new delegate that is called for a method that
        /// doesn't have a delegate registered for its name.
        /// If a default delegate is already registered it will replace
        /// with the new delegate.
        /// </summary>
        /// <param name="methodHandler">The delegate to be used when
        /// a method is called by the cloud service and there is no
        /// delegate registered for that method name.</param>
        /// <param name="userContext">Generic parameter to be interpreted
        /// by the client code.</param>
        Task SetMethodDefaultHandlerAsync(MethodCallback methodHandler,
            object userContext);

        /// <summary>
        /// Registers a new delegate for the named method. If a delegate
        /// is already associated with the named method, it will be replaced
        /// with the new delegate.
        /// <param name="methodName">The name of the method to associate
        /// with the delegate.</param>
        /// <param name="methodHandler">The delegate to be used when a
        /// method with the given name is called by the cloud service.</param>
        /// <param name="userContext">generic parameter to be interpreted
        /// by the client code.</param>
        /// </summary>
        Task SetMethodHandlerAsync(string methodName, MethodCallback methodHandler,
            object userContext);

        /// <summary>
        /// Retrieve a device twin object for the current device.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns>The device twin object for the current
        /// device</returns>
        Task<Twin> GetTwinAsync(CancellationToken ct = default);

        /// <summary>
        /// Set a callback that will be called whenever the client
        /// receives a state update (desired or reported) from the service.
        /// This has the side-effect of subscribing to the PATCH topic on
        /// the service.
        /// </summary>
        /// <param name="callback">Callback to call after the state
        /// update has been received and applied</param>
        /// <param name="userContext">Context object that will be
        /// passed into callback</param>
        Task SetDesiredPropertyUpdateCallbackAsync(
            DesiredPropertyUpdateCallback callback, object userContext);

        /// <summary>
        /// Push reported property changes up to the service.
        /// </summary>
        /// <param name="reportedProperties">Reported properties
        /// to push</param>
        /// <param name="ct"></param>
        Task UpdateReportedPropertiesAsync(TwinCollection reportedProperties,
            CancellationToken ct = default);

        /// <summary>
        /// Interactively invokes a method on module
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="moduleId"></param>
        /// <param name="methodRequest"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<MethodResponse> InvokeMethodAsync(string deviceId, string moduleId,
            MethodRequest methodRequest, CancellationToken ct = default);

        /// <summary>
        /// Interactively invokes a method on a device.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="methodRequest"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<MethodResponse> InvokeMethodAsync(string deviceId,
            MethodRequest methodRequest, CancellationToken ct = default);

        /// <summary>
        /// Close the DeviceClient instance
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();
    }
}

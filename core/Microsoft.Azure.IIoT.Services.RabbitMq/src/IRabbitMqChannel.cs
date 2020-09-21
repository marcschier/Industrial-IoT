// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


namespace Microsoft.Azure.IIoT.Services.RabbitMq {
    using RabbitMQ.Client;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Channel
    /// </summary>
    public interface IRabbitMqChannel : IDisposable {

        /// <summary>
        /// Queue
        /// </summary>
        string QueueName { get; }

        /// <summary>
        /// Exchange
        /// </summary>
        string ExchangeName { get; }

        /// <summary>
        /// Publish with callback
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="token"></param>
        /// <param name="complete"></param>
        /// <param name="properties"></param>
        /// <param name="mandatory"></param>
        void Publish<T>(ReadOnlyMemory<byte> body,
            T token, Action<T, Exception> complete,
            Action<IBasicProperties> properties = null,
            bool mandatory = false);

        /// <summary>
        /// Publish batch with callback
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="batch"></param>
        /// <param name="token"></param>
        /// <param name="complete"></param>
        /// <param name="properties"></param>
        /// <param name="mandatory"></param>
        void Publish<T>(IEnumerable<ReadOnlyMemory<byte>> batch,
            T token, Action<T, Exception> complete,
            Action<IBasicProperties> properties = null,
            bool mandatory = false);
    }
}
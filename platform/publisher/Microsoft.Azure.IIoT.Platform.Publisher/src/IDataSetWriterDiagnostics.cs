// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher {
    using System;

    /// <summary>
    /// Writer group diagnostics logger
    /// </summary>
    public interface IDataSetWriterDiagnostics {

        /// <summary>
        /// Controls the interval with which diagnostics messages are emitted.
        /// </summary>
        TimeSpan? DiagnosticsInterval { get; set; }

        /// <summary>
        /// Called before diagnostics are emitted
        /// </summary>
        event EventHandler<EventArgs> BeforeDiagnosticsSending;

        /// <summary>
        /// Called after diagnostics were emitted
        /// </summary>
        event EventHandler<EventArgs> AfterDiagnosticsSending;

        /// <summary>
        /// Writer received notifications
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="count"></param>
        void ReportDataSetWriterSubscriptionNotifications(
            string dataSetWriterId, long count);

        /// <summary>
        /// Update currently batched dataset messages
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="count"></param>
        void ReportBatchedDataSetMessageCount(string dataSetWriterId, long count);

        /// <summary>
        /// Report messages currently waiting to be encoded
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="count"></param>
        void ReportDataSetMessagesReadyToEncode(string dataSetWriterId, long count);

        /// <summary>
        /// Report so far encoded messages
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="count"></param>
        void ReportEncodedNetworkMessages(string dataSetWriterId, long count);

        /// <summary>
        /// Report network messages currently ready to be sent
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="count"></param>
        void ReportNetworkMessagesReadyToSend(string dataSetWriterId, long count);

        /// <summary>
        /// Report Writer group has sent another network message
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        void ReportNetworkMessageSent(string dataSetWriterId);

        /// <summary>
        /// Reports a server connection reconnect
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        void ReportConnectionRetry(string dataSetWriterId);

        /// <summary>
        /// Report processed messages by encoder
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="messageScheme"></param>
        /// <param name="count"></param>
        void ReportEncoderNetworkMessagesProcessedCount(string dataSetWriterId,
            string messageScheme, long count);

        /// <summary>
        /// Report dropped items in dataset message by encoder
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="messageScheme"></param>
        /// <param name="count"></param>
        void ReportEncoderNotificationsDroppedCount(string dataSetWriterId,
            string messageScheme, long count);

        /// <summary>
        /// Report processed items in dataset message by encoder
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="messageScheme"></param>
        /// <param name="count"></param>
        void ReportEncoderNotificationsProcessedCount(string dataSetWriterId,
            string messageScheme, long count);

        /// <summary>
        /// Report average items in each message by encoder
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="messageScheme"></param>
        /// <param name="average"></param>
        void ReportEncoderAvgNotificationsPerMessage(string dataSetWriterId,
            string messageScheme, double average);

        /// <summary>
        /// Report average network message sizes by encoder
        /// </summary>
        /// <param name="dataSetWriterId"></param>
        /// <param name="messageScheme"></param>
        /// <param name="average"></param>
        /// <param name="max"></param>
        void ReportEncoderAvgNetworkMessageSize(string dataSetWriterId,
            string messageScheme, double average, long max);
    }
}
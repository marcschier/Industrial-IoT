﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.OpcUa.Models;
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System;

    /// <summary>
    /// Data set extensions
    /// </summary>
    public static class DataSetWriterModelEx {

        /// <summary>
        /// Create subscription info model from message trigger configuration.
        /// </summary>
        /// <param name="dataSetWriter"></param>
        /// <returns></returns>
        public static SubscriptionModel ToSubscriptionModel(
            this DataSetWriterModel dataSetWriter) {
            if (dataSetWriter == null) {
                return null;
            }
            if (dataSetWriter.DataSetWriterId == null) {
                throw new ArgumentException("Missing writer id", nameof(dataSetWriter));
            }
            if (dataSetWriter.DataSet == null) {
                throw new ArgumentException("Missing dataset", nameof(dataSetWriter));
            }
            if (dataSetWriter.DataSet.DataSetSource == null) {
                throw new ArgumentException("Missing dataset source", nameof(dataSetWriter));
            }
            if (dataSetWriter.DataSet.DataSetSource.Connection == null) {
                throw new ArgumentException("Missing dataset connection", nameof(dataSetWriter));
            }
            var monitoredItems = dataSetWriter.DataSet.DataSetSource.ToMonitoredItems();
            return new SubscriptionModel {
                Connection = dataSetWriter.DataSet.DataSetSource.Connection.Clone(),
                Id = dataSetWriter.DataSetWriterId,
                MonitoredItems = monitoredItems,
                ExtensionFields = dataSetWriter.DataSet.ExtensionFields,
                Configuration = dataSetWriter.DataSet.DataSetSource
                    .ToSubscriptionConfigurationModel()
            };
        }
    }
}
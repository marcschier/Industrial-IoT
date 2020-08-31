// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.Publisher.Runtime;

    /// <summary>
    /// Extensions for data set writer
    /// </summary>
    public static class WriterGroupModelEx {

        /// <summary>
        /// Convert to message trigger configuration
        /// </summary>
        /// <param name="model"></param>
        /// <param name="publisherId"></param>
        /// <returns></returns>
        public static IWriterGroupConfig ToWriterGroupJobConfiguration(
            this WriterGroupJobModel model, string publisherId) {
            return new WriterGroupJobConfig {
                BatchSize = model.WriterGroup?.BatchSize,
                BatchTriggerInterval = model.WriterGroup?.PublishingInterval,
                PublisherId = publisherId,
              //  DiagnosticsInterval = model.DiagnosticsInterval,
                WriterGroup = model.WriterGroup,
                MaxMessageSize = (int?)model.WriterGroup?.MaxNetworkMessageSize,
              //  MaxEgressMessageQueue = model.WriterGroup?.MaxEgressMessageQueue
            };
        }
    }
}
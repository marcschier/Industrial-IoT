// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    using Microsoft.Azure.IIoT.Platform.Core.Models;
    using System.Linq;

    /// <summary>
    /// Events extensions
    /// </summary>
    public static class PublishedDataSetEventsModelEx {

        /// <summary>
        /// Clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static PublishedDataSetEventsModel Clone(this PublishedDataSetEventsModel model) {
            if (model == null) {
                return null;
            }
            return new PublishedDataSetEventsModel {
                Id = model.Id,
                GenerationId = model.GenerationId,
                MonitoringMode = model.MonitoringMode,
                TriggerId = model.TriggerId,
                DiscardNew = model.DiscardNew,
                EventNotifier = model.EventNotifier,
                Filter = model.Filter.Clone(),
                QueueSize = model.QueueSize,
                BrowsePath = model.BrowsePath,
                Created = model.Created.Clone(),
                Updated = model.Updated.Clone(),
                SelectedFields = model.SelectedFields?
                    .Select(f => f.Clone())
                    .ToList()
            };
        }

        /// <summary>
        /// Get model from request
        /// </summary>
        /// <param name="model"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static PublishedDataSetEventsModel AsEventDataSet(
            this DataSetAddEventRequestModel model,
            PublisherOperationContextModel context) {
            if (model == null) {
                return null;
            }
            return new PublishedDataSetEventsModel {
                Id = null,
                GenerationId = null,
                MonitoringMode = model.MonitoringMode,
                TriggerId = model.TriggerId,
                DiscardNew = model.DiscardNew,
                EventNotifier = model.EventNotifier,
                Filter = model.Filter.Clone(),
                QueueSize = model.QueueSize,
                BrowsePath = model.BrowsePath,
                Updated = context,
                Created = context,
                SelectedFields = model.SelectedFields?
                    .Select(f => f.Clone())
                    .ToList()
            };
        }
    }
}
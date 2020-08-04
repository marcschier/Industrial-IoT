// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Models {
    /// <summary>
    /// Settings extensions
    /// </summary>
    public static class PublishedDataSetSettingsModelEx {

        /// <summary>
        /// Clone
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static PublishedDataSetSourceSettingsModel Clone(this PublishedDataSetSourceSettingsModel model) {
            if (model == null) {
                return null;
            }
            return new PublishedDataSetSourceSettingsModel {
                LifeTimeCount = model.LifeTimeCount,
                MaxKeepAliveCount = model.MaxKeepAliveCount,
                MaxNotificationsPerPublish = model.MaxNotificationsPerPublish,
                Priority = model.Priority,
                PublishingInterval = model.PublishingInterval,
                ResolveDisplayName = model.ResolveDisplayName
            };
        }
    }
}
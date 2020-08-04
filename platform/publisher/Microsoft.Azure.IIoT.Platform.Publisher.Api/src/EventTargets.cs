// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Api {

    /// <summary>
    /// Event target constants
    /// </summary>
    public static class EventTargets {

        /// <summary>
        /// Nodes group
        /// </summary>
        public const string Nodes = "publish";

        /// <summary>
        /// Endpoints group
        /// </summary>
        public const string Endpoints = "endpoints";

        /// <summary>
        /// Writer group group
        /// </summary>
        public const string Groups = "groups";

        /// <summary>
        /// DataSet Writers group
        /// </summary>
        public const string Writers = "writers";

        /// <summary>
        /// Publisher sample target
        /// </summary>
        public const string PublisherSampleTarget = "PublisherMessage";

        /// <summary>
        /// Dataset message target
        /// </summary>
        public const string DataSetMessageTarget = "DataSetMessage";

        /// <summary>
        /// Dataset event targets
        /// </summary>
        public const string DataSetEventTarget = "DataSetEvent";

        /// <summary>
        /// Writer event targets
        /// </summary>
        public const string WriterEventTarget = "WriterEvent";

        /// <summary>
        /// Writer Group event targets
        /// </summary>
        public const string GroupEventTarget = "GroupEvent";
    }
}

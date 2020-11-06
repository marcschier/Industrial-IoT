// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.Platform.Publisher.Cdm.Runtime {
    using Microsoft.Azure.IIoT.Platform.Publisher.Cdm;
    using Microsoft.Azure.IIoT.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// CDM storage configuration
    /// </summary>
    public class CdmFolderConfig : ConfigureOptionBase, ICdmFolderConfig {

        /// <summary>
        /// Table storage configuration
        /// </summary>
        private const string kCdmContainerName = "Cdm:ContainerName";
        private const string kCdmRootFolder = "Cdm:RootFolder";

        /// <inheritdoc/>
        public string StorageDrive => GetStringOrDefault(kCdmContainerName,
            GetStringOrDefault(PcsVariable.PCS_CDM_DRIVE_NAME,
            GetStringOrDefault("PCS_CDM_ADLSG2_BLOBNAME",
                "powerbi")));
        /// <inheritdoc/>
        public string StorageFolder => GetStringOrDefault(kCdmRootFolder,
            GetStringOrDefault(PcsVariable.PCS_CDM_ROOT_FOLDER,
            GetStringOrDefault("PCS_CDM_ROOTFOLDER",
                "IIoTDataFlow")));

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="configuration"></param>
        public CdmFolderConfig(IConfiguration configuration) :
            base(configuration) {
        }
    }
}

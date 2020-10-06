// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.IIoT.App.Models {
    using Microsoft.Azure.IIoT.Platform.Directory.Api.Models;
    using Microsoft.Azure.IIoT.Platform.Registry.Api.Models;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public class DiscovererInfo {

        /// <summary>
        /// Discoverer models.
        /// </summary>
        public DiscovererApiModel DiscovererModel { get; set; }

        /// <summary>
        /// Current
        /// </summary>
        public DiscoveryConfigApiModel Current { get; set; } = new DiscoveryConfigApiModel();

        /// <summary>
        /// Patch
        /// </summary>
        public DiscoveryConfigApiModel Patch { get; set; } = new DiscoveryConfigApiModel();

        /// <summary>
        /// scan status.
        /// </summary>
        public bool ScanStatus { get; set; }

        /// <summary>
        /// is scan searching.
        /// </summary>
        public bool IsSearching { get; set; }

        /// <summary>
        /// Discoverer has found apps.
        /// </summary>
        public bool HasApplication { get; set; }

        /// <summary>
        /// is Ad-Hoc Discovery.
        /// </summary>
        public bool IsAdHocDiscovery { get; set; }

        /// <summary>
        /// Id of discovery request
        /// </summary>
        public string DiscoveryRequestId { get; set; }

        // Bind Proxies

        /// <summary>
        /// Network probe timeout
        /// </summary>
        public string EffectiveNetworkProbeTimeout {
            get => (Current?.NetworkProbeTimeout ?? TimeSpan.MinValue)
                == TimeSpan.MinValue ?
                null : Current.NetworkProbeTimeout.ToString();
        }

        /// <summary>
        /// Max network probes that should ever run.
        /// </summary>
        public string EffectiveMaxNetworkProbes {
            get => (Current?.MaxNetworkProbes ?? -1) < 0 ?
                null : Current.MaxNetworkProbes.ToString();
        }

        /// <summary>
        /// Port probe timeout
        /// </summary>
        public string EffectivePortProbeTimeout {
            get => (Current?.PortProbeTimeout ?? TimeSpan.MinValue)
                == TimeSpan.MinValue ?
                null : Current.PortProbeTimeout.ToString();
        }

        /// <summary>
        /// Max port probes that should ever run.
        /// </summary>
        public string EffectiveMaxPortProbes {
            get => (Current?.MaxPortProbes ?? -1) < 0 ?
                null : Current.MaxPortProbes.ToString();
        }

        /// <summary>
        /// Delay time between discovery sweeps in seconds
        /// </summary>
        public string EffectiveIdleTimeBetweenScans {
            get => (Current?.IdleTimeBetweenScans ?? TimeSpan.MinValue)
                == TimeSpan.MinValue ?
                null : Current.IdleTimeBetweenScans.ToString();
        }

        /// <summary>
        /// Address ranges to scan (null == all wired nics)
        /// </summary>
        public string EffectiveAddressRangesToScan {
            get => string.IsNullOrEmpty(Current?.AddressRangesToScan) ?
                null : Current.AddressRangesToScan;
        }

        /// <summary>
        /// Port ranges to scan (null == all unassigned)
        /// </summary>
        public string EffectivePortRangesToScan {
            get => string.IsNullOrEmpty(Current?.PortRangesToScan) ?
                null : Current.PortRangesToScan;
        }

        /// <summary>
        /// List of preset discovery urls to use
        /// </summary>
        public IReadOnlyList<string> EffectiveDiscoveryUrls {
            get => Current?.DiscoveryUrls == null ?
                new List<string>() : Current.DiscoveryUrls;
        }

        /// <summary>
        /// List of locales to filter with during discovery
        /// </summary>
        public IReadOnlyList<string> EffectiveLocales {
            get => Current?.Locales == null ?
                new List<string>() : Current.Locales;
        }

        public bool TryUpdateData(DiscovererInfoRequested input) {
            if (input is null) {
                throw new ArgumentNullException(nameof(input));
            }
            try {
                Current ??= new DiscoveryConfigApiModel();

                Patch.NetworkProbeTimeout = Current.NetworkProbeTimeout =
                    string.IsNullOrWhiteSpace(input.RequestedNetworkProbeTimeout) ? TimeSpan.MinValue :
                    TimeSpan.Parse(input.RequestedNetworkProbeTimeout, CultureInfo.CurrentCulture);

                Patch.MaxNetworkProbes = Current.MaxNetworkProbes =
                    string.IsNullOrWhiteSpace(input.RequestedMaxNetworkProbes) ? -1 :
                    int.Parse(input.RequestedMaxNetworkProbes, CultureInfo.CurrentCulture);

                Patch.PortProbeTimeout = Current.PortProbeTimeout =
                    string.IsNullOrWhiteSpace(input.RequestedPortProbeTimeout) ? TimeSpan.MinValue :
                    TimeSpan.Parse(input.RequestedPortProbeTimeout, CultureInfo.CurrentCulture);

                Patch.MaxPortProbes = Current.MaxPortProbes =
                    string.IsNullOrWhiteSpace(input.RequestedMaxPortProbes) ? -1 :
                    int.Parse(input.RequestedMaxPortProbes, CultureInfo.CurrentCulture);

                Patch.IdleTimeBetweenScans = Current.IdleTimeBetweenScans =
                    string.IsNullOrWhiteSpace(input.RequestedIdleTimeBetweenScans) ? TimeSpan.MinValue :
                    TimeSpan.Parse(input.RequestedIdleTimeBetweenScans, CultureInfo.CurrentCulture);

                Patch.AddressRangesToScan = Current.AddressRangesToScan =
                    string.IsNullOrWhiteSpace(input.RequestedAddressRangesToScan) ? string.Empty :
                    input.RequestedAddressRangesToScan;

                Patch.PortRangesToScan = Current.PortRangesToScan =
                    string.IsNullOrWhiteSpace(input.RequestedPortRangesToScan) ? string.Empty :
                    input.RequestedPortRangesToScan;

                Patch.DiscoveryUrls = Current.DiscoveryUrls =
                    input.RequestedDiscoveryUrls ?? new List<string>();

                Patch.Locales = Current.Locales =
                    input.RequestedDiscoveryUrls ?? new List<string>();

                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        public DiscovererInfoRequested ToDiscovererInfoRequested() {
            return new DiscovererInfoRequested() {
                RequestedAddressRangesToScan = EffectiveAddressRangesToScan,
                RequestedPortRangesToScan = EffectivePortRangesToScan,
                RequestedMaxNetworkProbes = EffectiveMaxNetworkProbes,
                RequestedMaxPortProbes = EffectiveMaxPortProbes,
                RequestedNetworkProbeTimeout = EffectiveNetworkProbeTimeout,
                RequestedPortProbeTimeout = EffectivePortProbeTimeout,
                RequestedIdleTimeBetweenScans = EffectiveIdleTimeBetweenScans,
                RequestedDiscoveryUrls = new List<string>(EffectiveDiscoveryUrls)
            };
        }
    }
}

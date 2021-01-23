// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Protocols.Service {
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Configuration;
    using Autofac.Extensions.Hosting;
    using Prometheus;

    /// <summary>
    /// Main entry point
    /// </summary>
    public static class Program {

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args) {
            using (var source = DiagnosticSourceAdapter.StartListening()) {
                CreateHostBuilder(args).Build().Run();
            }
        }

        /// <summary>
        /// Create host builder
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) {
            return Host.CreateDefaultBuilder(args)
                .UseAutofac()
                .ConfigureHostConfiguration(builder => builder
                    .AddFromDotEnvFile())
                .ConfigureWebHostDefaults(builder => builder
                    .UseUrls("http://*:9080")
                    .UseStartup<Startup>()
                    .UseKestrel(o => o.AddServerHeader = false));
        }
    }
}

// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.IIoT.Platform.Identity.Provider {
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using IdentityServer4.Configuration;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.IIoT.Authentication;
    using Microsoft.IIoT.Authentication.Runtime;
    using Microsoft.IIoT.Azure.ActiveDirectory;
    using Microsoft.IIoT.Azure.ActiveDirectory.Runtime;
    using Microsoft.IIoT.Azure.CosmosDb;
    using Microsoft.IIoT.Http.Clients;
    using Microsoft.IIoT.Platform.Identity.Models;
    using Microsoft.IIoT.Platform.Identity.Provider.Runtime;
    using Microsoft.IIoT.Platform.Identity.Runtime;
    using Microsoft.IIoT.Platform.Identity.Services;
    using Microsoft.IIoT.Platform.Identity.Storage;
    using Microsoft.IIoT.Serializers;
    using Microsoft.IIoT.Utils;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Webservice Startup
    /// </summary>
    public class Startup {

        /// <summary>
        /// Configuration - Initialized in constructor
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Service info - Initialized in constructor
        /// </summary>
        public ServiceInfo ServiceInfo { get; } = new ServiceInfo();

        /// <summary>
        /// Current hosting environment - Initialized in constructor
        /// </summary>
        public IWebHostEnvironment Environment { get; }

        /// <summary>
        /// Create startup
        /// </summary>
        /// <param name="env"></param>
        /// <param name="configuration"></param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration) {
            Environment = env;
            Configuration = configuration;
        }

        /// <summary>
        /// Configure services
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public void ConfigureServices(IServiceCollection services) {

            services.AddControllersWithViews();

            // configures IIS out-of-proc settings
            // (see https://github.com/aspnet/AspNetCore/issues/14882)
            services.Configure<IISOptions>(iis => {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            // configures IIS in-proc settings
            services.Configure<IISServerOptions>(iis => {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            services.AddIdentity<UserModel, RoleModel>()
                .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options => {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.UserInteraction = new UserInteractionOptions {
                    LogoutUrl = "/Account/Logout",
                    LoginUrl = "/Account/Login",
                    LoginReturnUrlParameter = "returnUrl"
                };
            });
            builder
                .AddAspNetIdentity<UserModel>()
                .AddDeveloperSigningCredential() // TODO
                ;

            var authentication = services.AddAuthentication();

            // Attach aad
            var aadConfig = new AadSpClientConfig(Configuration);
            if (!string.IsNullOrEmpty(aadConfig.ClientId) &&
                !string.IsNullOrEmpty(aadConfig.ClientSecret)) {
#pragma warning disable CS0618 // Type or member is obsolete
                authentication = authentication.AddAzureAD(options => {
                    options.Instance = aadConfig.InstanceUrl;
                    options.Domain = aadConfig.GetDomain();
                    options.TenantId = aadConfig.TenantId;
                    options.ClientId = aadConfig.ClientId;
                    options.ClientSecret = aadConfig.ClientSecret;
                    options.CallbackPath = "/signin-oidc";
                });
#pragma warning restore CS0618 // Type or member is obsolete
            }

            // Attach more in the future ...
        }

        /// <summary>
        /// This method is called by the runtime, after the ConfigureServices
        /// method above and used to add middleware
        /// </summary>
        /// <param name="app"></param>
        /// <param name="appLifetime"></param>
        public void Configure(IApplicationBuilder app, IHostApplicationLifetime appLifetime) {
            var applicationContainer = app.ApplicationServices.GetAutofacRoot();
            var log = applicationContainer.Resolve<ILogger<Startup>>();

            if (Environment.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapDefaultControllerRoute();
            });

            // If you want to dispose of resources that have been resolved in the
            // application container, register for the "ApplicationStopped" event.
            appLifetime.ApplicationStopped.Register(applicationContainer.Dispose);

            // Print some useful information at bootstrap time
            log.LogInformation("{service} started with id {id}",
                ServiceInfo.Name, Uptime.ProcessId);
        }

        /// <summary>
        /// Configure Autofac container
        /// </summary>
        /// <param name="builder"></param>
        public void ConfigureContainer(ContainerBuilder builder) {
            // Register service info and configuration interfaces
            builder.RegisterInstance(ServiceInfo)
                .AsImplementedInterfaces();
            builder.AddConfiguration(Configuration);
            builder.RegisterType<HostingOptions>()
                .AsImplementedInterfaces();

            // Register http client module
            builder.RegisterModule<HttpClientModule>();
            // Add serializers
            builder.RegisterModule<NewtonSoftJsonModule>();

            // --- Logic ---

            builder.RegisterType<ClientDatabase>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<GrantDatabase>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ResourceDatabase>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<RoleDatabase>()
                .AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<UserDatabase>()
                .AsImplementedInterfaces().SingleInstance();

            builder.RegisterType<IdentityServerConfig>()
                .AsImplementedInterfaces();
            builder.RegisterType<IdentityServerStorageInit>()
                .AsImplementedInterfaces().SingleInstance();
            // ... and auto start
            builder.RegisterType<HostAutoStart>()
                .AutoActivate()
                .AsImplementedInterfaces().SingleInstance();

            // --- Dependencies ---

            // Add service to service authentication
            builder.RegisterModule<ActiveDirectorySupport>();
            builder.RegisterModule<DefaultPublicClientAuthProviders>();
            // TODO: builder.RegisterModule<DefaultConfidentialClientAuthProviders>();
            // TODO: Add openid auth providers
            // TODO: Add swagger auth providers (?)
            // Register Cosmos db for storage
            builder.RegisterModule<CosmosDbModule>();
        }
    }
}
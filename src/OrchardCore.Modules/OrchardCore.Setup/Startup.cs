using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Localization;
using OrchardCore.Modules;

namespace OrchardCore.Setup
{
    public class Startup : StartupBase
    {
        private readonly string _defaultCulture;
        private string[] _supportedCultures;

        public Startup(IShellConfiguration shellConfiguration)
        {
            var configurationSection = shellConfiguration.GetSection("OrchardCore.Setup");

            _defaultCulture = configurationSection["DefaultCulture"] ?? "";
            _supportedCultures = configurationSection.GetSection("SupportedCultures").Get<string[]>();
            var results = configurationSection.GetSection("SupportedCultures").Get<List<string>>();
            if (Array.IndexOf(_supportedCultures, null) > -1) {
                results.Add("");
            }
            _supportedCultures = results.ToArray();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization");
            services.Replace(ServiceDescriptor.Singleton<ILocalizationFileLocationProvider, ModularPoFileLocationProvider>());

            services.AddSetup();
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var localizationOptions = serviceProvider.GetService<IOptions<RequestLocalizationOptions>>().Value;

            if (!String.IsNullOrEmpty(_defaultCulture))
            {
                localizationOptions.SetDefaultCulture(_defaultCulture);
                _supportedCultures = _supportedCultures.Union(new[] { _defaultCulture }).ToArray();
            }

            if (_supportedCultures?.Length > 0)
            {
                localizationOptions
                    .AddSupportedCultures(_supportedCultures)
                    .AddSupportedUICultures(_supportedCultures);
            }

            app.UseRequestLocalization(localizationOptions);

            routes.MapAreaControllerRoute(
                name: "Setup",
                areaName: "OrchardCore.Setup",
                pattern: "",
                defaults: new { controller = "Setup", action = "Index" }
            );
        }
    }
}

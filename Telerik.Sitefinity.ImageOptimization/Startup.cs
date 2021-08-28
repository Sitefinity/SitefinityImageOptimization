using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.ImageOptimization.Configuration;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.Modules.Libraries.Configuration;
using Telerik.Sitefinity.Processors.Configuration;
using Telerik.Sitefinity.Services;

namespace Telerik.Sitefinity.ImageOptimization
{
    /// <summary>
    /// Contains the application startup event handlers registering the required components for the packaging module of Sitefinity.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Startup
    {
        /// <summary>
        /// Called before the Asp.Net application is started. Subscribes for the logging and exception handling configuration related events.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void OnPreApplicationStart()
        {
            Bootstrapper.Bootstrapped -= Bootstrapper_Bootstrapped;
            Bootstrapper.Bootstrapped += Bootstrapper_Bootstrapped;
        }

        private static void Bootstrapper_Bootstrapped(object sender, EventArgs e)
        {
            IList<IInstallableFileProcessor> imageOptimizationProcessors = new List<IInstallableFileProcessor>()
            {
                new KrakenImageOptimizationProcessor()
            };

            IList<IInstallableFileProcessor> imageOptimizationProcessorsToRegister = new List<IInstallableFileProcessor>();
            foreach (var imageOptimizationProcessor in imageOptimizationProcessors)
            {
                if (!Startup.IsImageOptimizationProcessorRegistered(imageOptimizationProcessor))
                {
                    imageOptimizationProcessorsToRegister.Add(imageOptimizationProcessor);
                }
            }

            if (imageOptimizationProcessorsToRegister.Any())
            {
                Startup.RegisterImageOptimizationProcessors(imageOptimizationProcessorsToRegister);
            }

            Startup.InitializeHelperFields();

            Res.RegisterResource<ImageOptimizationResources>();
            Config.RegisterSection<ImageOptimizationConfig>();
        }

        private static bool IsImageOptimizationProcessorRegistered(IInstallableFileProcessor imageOptimizationProcessor)
        {
            LibrariesConfig librariesConfig = Config.Get<LibrariesConfig>();

            if (!librariesConfig.FileProcessors.ContainsKey(imageOptimizationProcessor.ConfigName))
            {
                return false;
            }

            return true;
        }

        private static void RegisterImageOptimizationProcessors(IEnumerable<IInstallableFileProcessor> imageOptimizationProcessors)
        {
            SystemManager.RunWithElevatedPrivilege(d =>
            {
                var configManager = ConfigManager.GetManager();
                var librariesConfig = configManager.GetSection<LibrariesConfig>();


                foreach (var imageOptimizationProcessor in imageOptimizationProcessors)
                {
                    librariesConfig.FileProcessors.Add(imageOptimizationProcessor.ConfigName, new ProcessorConfigElement(librariesConfig.FileProcessors)
                    {
                        Enabled = true,
                        Description = imageOptimizationProcessor.ConfigDescription,
                        Name = imageOptimizationProcessor.ConfigName,
                        Type = imageOptimizationProcessor.GetType().FullName,
                        Parameters = imageOptimizationProcessor.ConfigParameters
                    });
                }

                configManager.SaveSection(librariesConfig);
            });
        }

        private static void InitializeHelperFields()
        {
            ImageOptimizationFieldBuilder.CreateRequiredFields();
        }
    }
}
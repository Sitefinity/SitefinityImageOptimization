using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
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
            Bootstrapper.Initialized -= Bootstrapper_Initialized;
            Bootstrapper.Initialized += Bootstrapper_Initialized;
        }

        private static void Bootstrapper_Initialized(object sender, ExecutedEventArgs e)
        {
            if (e.CommandName == "Bootstrapped")
            {
                SystemManager.ApplicationStart += SystemManager_ApplicationStart;
            }
        }

        static void SystemManager_ApplicationStart(object sender, System.EventArgs e)
        {
            if (!Startup.IsHubSpotModuleRegistered())
            {
                Startup.RegisterHubSpotModule();
            }
        }

        private static bool IsHubSpotModuleRegistered()
        {
            SystemConfig systemConfig = Config.Get<SystemConfig>();
            if (!systemConfig.ApplicationModules.ContainsKey(ImageOptimizationModule.ModuleName))
            {
                return false;
            }

            return true;
        }

        private static void RegisterHubSpotModule()
        {
            SystemManager.RunWithElevatedPrivilege(d =>
            {
                var configManager = ConfigManager.GetManager();
                var systemConfig = configManager.GetSection<SystemConfig>();

                systemConfig.ApplicationModules.Add(new AppModuleSettings(systemConfig.ApplicationModules)
                {
                    Name = ImageOptimizationModule.ModuleName,
                    Title = "Image Optimization",
                    Description = "Provides integration between Sitefinity and image optimization services like Kraken IO.",
                    Type = typeof(ImageOptimizationModule).AssemblyQualifiedName,
                    StartupType = StartupType.OnApplicationStart
                });

                configManager.SaveSection(systemConfig);
            });
        }
    }
}
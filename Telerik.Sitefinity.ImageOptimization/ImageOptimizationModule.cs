using System;
using System.Diagnostics.CodeAnalysis;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.ImageOptimization.Configuration;
using Telerik.Sitefinity.Services;

namespace Telerik.Sitefinity.ImageOptimization
{
    /// <summary>
    /// HubSpot connector module
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ImageOptimizationModule : ModuleBase
    {
        /// <inheritdoc />
        public override Guid LandingPageId
        {
            get
            {
                return Guid.Empty;
            }
        }

        /// <inheritdoc />
        public override void Initialize(ModuleSettings settings)
        {
            App.WorkWith()
                .Module(ImageOptimizationModule.ModuleName)
                .Initialize()
                .Configuration<ImageOptimizationConfig>();

            base.Initialize(settings);
            this.RegisterIocTypes();
        }

        /// <summary>
        /// Integrate the module into the system.
        /// </summary>
        public override void Load()
        {
            Bootstrapper.Initialized -= this.Bootstrapper_Initialized;
            Bootstrapper.Initialized += this.Bootstrapper_Initialized;
        }

        /// <summary>
        /// Handles the Initialized event of the Bootstrapper.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Sitefinity.Data.ExecutedEventArgs"/> instance containing the event data.</param>
        protected virtual void Bootstrapper_Initialized(object sender, ExecutedEventArgs e)
        {
            if (e.CommandName == "Bootstrapped" && SystemManager.GetModule(ImageOptimizationModule.ModuleName) != null)
            {
            }
        }
        
        /// <summary>
        /// This method is invoked during the unload process of an active module from Sitefinity, e.g. when a module is deactivated. For instance this method is also invoked for every active module during a restart of the application. 
        /// Typically you will use this method to unsubscribe the module from all events to which it has subscription.
        /// </summary>
        public override void Unload()
        {
            Bootstrapper.Initialized -= this.Bootstrapper_Initialized;

            base.Unload();
        }

        /// <summary>
        /// Uninstall the module from Sitefinity system. Deletes the module artifacts added with Install method.
        /// </summary>
        /// <param name="initializer">The site initializer instance.</param>
        public override void Uninstall(SiteInitializer initializer)
        {
            base.Uninstall(initializer);
        }

        /// <inheritdoc />
        public override void Install(SiteInitializer initializer)
        {
        }

        /// <inheritdoc />
        protected override ConfigSection GetModuleConfig()
        {
            return Config.Get<ImageOptimizationConfig>();
        }

        /// <inheritdoc />
        public override Type[] Managers
        {
            get
            {
                return new Type[0];
            }
        }

        private void RegisterIocTypes()
        {
        }

        /// <summary>
        /// The name of this module
        /// </summary>
        public const string ModuleName = "ImageOptimization";
        private const string HubSpotConnectorConfigName = "HubSpotConnectorConfig";
    }
}
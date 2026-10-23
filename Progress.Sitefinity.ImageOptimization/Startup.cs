using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Progress.Sitefinity.ImageOptimization.Configuration;
using Progress.Sitefinity.ImageOptimization.FileProcessors;
using Progress.Sitefinity.ImageOptimization.Scheduling;
using Progress.Sitefinity.ImageOptimization.Utils;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.Services;

namespace Progress.Sitefinity.ImageOptimization
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
            ObjectFactory.RegisteredIoCTypes -= ObjectFactory_RegisteredIoCTypes;
            Bootstrapper.Bootstrapped += Bootstrapper_Bootstrapped;
            ObjectFactory.RegisteredIoCTypes += ObjectFactory_RegisteredIoCTypes;
        }

        private static void ObjectFactory_RegisteredIoCTypes(object sender, EventArgs e)
        {
            ImageOptimizationUploadCallHandler.RegisterPolicy();
        }

        private static void Bootstrapper_Bootstrapped(object sender, EventArgs e)
        {
            Startup.InitializeHelperFields();
            ImageOptimizationTask.RemoveScheduledTasks();

            var disableImageOptimizationAppSetting = System.Configuration.ConfigurationManager.AppSettings[ImageOptimizationConstants.DisableImageOptimizationAppSettingKey];
            bool disableImageOptimization;

            if (!(bool.TryParse(disableImageOptimizationAppSetting, out disableImageOptimization) && disableImageOptimization))
            {
                IList<IInstallableFileProcessor> imageOptimizationProcessors = new List<IInstallableFileProcessor>()
                {
                    new KrakenImageOptimizationProcessor(),
                    new TinifyImageOptimizationProcessor()
                };

                IList<IInstallableFileProcessor> imageOptimizationProcessorsToRegister = new List<IInstallableFileProcessor>();
                foreach (var imageOptimizationProcessor in imageOptimizationProcessors)
                {
                    if (!ImageOptimizationProcessorsHelper.IsImageOptimizationProcessorRegistered(imageOptimizationProcessor))
                    {
                        imageOptimizationProcessorsToRegister.Add(imageOptimizationProcessor);
                    }
                }

                if (imageOptimizationProcessorsToRegister.Any())
                {
                    ImageOptimizationProcessorsHelper.RegisterImageOptimizationProcessors(imageOptimizationProcessorsToRegister);
                }

                Res.RegisterResource<ImageOptimizationResources>();
                Config.RegisterSection<ImageOptimizationConfig>();

                Startup.RegisterCrontabTasks();
            }
        }

        private static void InitializeHelperFields()
        {
            ImageOptimizationFieldBuilder.CreateRequiredFields();
        }

        private static void RegisterCrontabTasks()
        {
            ImageOptimizationTask.RemoveScheduledTasks();
            SystemManager.CrontabTasksToRun.Add(ImageOptimizationTask.GetTaskName(), ImageOptimizationTask.NewInstance);
        }
    }
}
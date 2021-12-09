using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.Events;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.ImageOptimization.Configuration;
using Telerik.Sitefinity.ImageOptimization.FileProcessors;
using Telerik.Sitefinity.ImageOptimization.Scheduling;
using Telerik.Sitefinity.ImageOptimization.Utils;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.Libraries;
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
            Bootstrapper.Initialized -= Bootstrapper_Initialized;
            Bootstrapper.Bootstrapped += Bootstrapper_Bootstrapped;
            Bootstrapper.Initialized += Bootstrapper_Initialized;
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


                Startup.hassImageOptimizationProcessorEnabled = ImageOptimizationProcessorsHelper.ValidateImageOptimizationProcessorsConfigurations();

                Res.RegisterResource<ImageOptimizationResources>();
                Config.RegisterSection<ImageOptimizationConfig>();

                Startup.RegisterCrontabTasks();
            }
        }

        private static void Bootstrapper_Initialized(object sender, ExecutedEventArgs e)
        {
            var disableImageOptimizationAppSetting = System.Configuration.ConfigurationManager.AppSettings[ImageOptimizationConstants.DisableImageOptimizationAppSettingKey];
            bool disableImageOptimization;

            if (!(bool.TryParse(disableImageOptimizationAppSetting, out disableImageOptimization) && disableImageOptimization))
            {
                if (e.CommandName == "Bootstrapped")
                {
                    EventHub.Subscribe<IDataEvent>(Content_Action);
                }
            }
        }

        private static void Content_Action(IDataEvent @event)
        {
            try
            {
                Type contentType = @event.ItemType;
                Guid itemId = @event.ItemId;
                string providerName = @event.ProviderName;
                string language = @event.GetLanguage();

                if (!ValidateEventType(@event))
                {
                    return;
                }

                if (ObjectFactory.GetArgsByName(typeof(ImageOptimizationConfig).Name, typeof(ImageOptimizationConfig)) == null)
                {
                    return;
                }

                ImageOptimizationConfig imageOptimizationConfig = Config.Get<ImageOptimizationConfig>();

                LibrariesManager manager = ManagerBase.GetMappedManager(contentType, providerName) as LibrariesManager;

                if(manager == null)
                {
                    return;
                }

                var item = manager.GetItemOrDefault(contentType, itemId);
                Image image = item as Image;

                if (image.Status == ContentLifecycleStatus.Master)
                {
                    var imageTemp = manager.Lifecycle.CheckOut(image) as Image;
                    imageTemp.SetValue(ImageOptimizationConstants.IsOptimizedFieldName, Startup.hassImageOptimizationProcessorEnabled);
                    manager.Lifecycle.CheckIn(imageTemp);
                }
                else if (image.Status == ContentLifecycleStatus.Temp)
                {
                    image.SetValue(ImageOptimizationConstants.IsOptimizedFieldName, Startup.hassImageOptimizationProcessorEnabled);
                    Image master = manager.Lifecycle.GetMaster(image) as Image;
                    master.SetValue(ImageOptimizationConstants.IsOptimizedFieldName, Startup.hassImageOptimizationProcessorEnabled);
                }

                manager.SaveChanges();

            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Error occurred while setting image optimization field value: {0}", ex.Message), ConfigurationPolicy.ErrorLog);
            }
        }

        private static bool ValidateEventType(IDataEvent @event)
        {
            string action = @event.Action;
            Type contentType = @event.ItemType;

            if (action != "Updated" || contentType != typeof(Image))
            {
                return false;
            }

            var propertyChangeDataEvent = @event as IPropertyChangeDataEvent;

            if (propertyChangeDataEvent == null || (!propertyChangeDataEvent.ChangedProperties.Any(p => p.Key == "Thumbnails") && !propertyChangeDataEvent.ChangedProperties.Any(p => p.Key == ImageOptimizationConstants.IsOptimizedFieldName)))
            {
                return false;
            }

            if(propertyChangeDataEvent.ChangedProperties.Any(p => p.Key == ImageOptimizationConstants.IsOptimizedFieldName))
            {
                var changedIsOptimized = propertyChangeDataEvent.ChangedProperties.FirstOrDefault(p => p.Key == ImageOptimizationConstants.IsOptimizedFieldName);

                if ((bool)changedIsOptimized.Value.NewValue == Startup.hassImageOptimizationProcessorEnabled)
                {
                    return false;
                }
            }

            return true;
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

        private static bool hassImageOptimizationProcessorEnabled;
    }
}
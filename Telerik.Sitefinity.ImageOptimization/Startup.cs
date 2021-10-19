using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.Events;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.ImageOptimization.Configuration;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.Libraries.Configuration;
using Telerik.Sitefinity.Processors;
using Telerik.Sitefinity.Processors.Configuration;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Utilities.TypeConverters;

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

            Startup.ValidateImageOptimizationProcessorsConfigurations();

            Res.RegisterResource<ImageOptimizationResources>();
            Config.RegisterSection<ImageOptimizationConfig>();

            Startup.RegisterCrontabTasks();
        }

        private static void Bootstrapper_Initialized(object sender, ExecutedEventArgs e)
        {
            if (e.CommandName == "Bootstrapped")
            {
                EventHub.Subscribe<IDataEvent>(Content_Action);
            }
        }

        private static void Content_Action(IDataEvent @event)
        {
            try
            {
                string action = @event.Action;
                Type contentType = @event.ItemType;
                Guid itemId = @event.ItemId;
                string providerName = @event.ProviderName;

                if (action != "New" || contentType != typeof(Image))
                {
                    return;
                }

                if (!Startup.hassImageOptimizationProcessorEnabled)
                {
                    return;
                }

                IManager manager = ManagerBase.GetMappedManager(contentType, providerName);
                var item = manager.GetItemOrDefault(contentType, itemId);
                Image imageMaster = item as Image;

                if (imageMaster.Status != ContentLifecycleStatus.Master)
                {
                    return;
                }

                imageMaster.SetValue(ImageOptimizationFieldBuilder.IsOptimizedFieldName, true);

                manager.SaveChanges();
            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Error occurred while setting image optimization field value: {0}", ex.Message), ConfigurationPolicy.ErrorLog);
            }

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

        private static void ValidateImageOptimizationProcessorsConfigurations()
        {
            try
            {
                var configFileProcessors = Config.Get<LibrariesConfig>().GetConfigProcessors();

                foreach (var configFileProcessorsElement in configFileProcessors.Values)
                {
                    if (configFileProcessorsElement.Enabled)
                    {
                        var checkType = TypeResolutionService.ResolveType(configFileProcessorsElement.Type, true);
                        IProcessor instance = (IProcessor)ObjectFactory.Resolve(checkType);

                        instance.Initialize(configFileProcessorsElement.Name, new NameValueCollection(configFileProcessorsElement.Parameters));

                        var installableFileProcessor = instance as IInstallableFileProcessor;

                        if(installableFileProcessor != null && installableFileProcessor.HasInitialized)
                        {
                            Startup.hassImageOptimizationProcessorEnabled = true;
                            return;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Write(ex.Message, TraceEventType.Error);
                Startup.hassImageOptimizationProcessorEnabled = false;
            }

            Startup.hassImageOptimizationProcessorEnabled = false;
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

        private static void RegisterCrontabTasks()
        {
            ImageOptimizationTask.RemoveScheduledTasks();
            SystemManager.CrontabTasksToRun.Add(ImageOptimizationTask.GetTaskName(), ImageOptimizationTask.NewInstance);
        }

        private static bool hassImageOptimizationProcessorEnabled;
    }
}
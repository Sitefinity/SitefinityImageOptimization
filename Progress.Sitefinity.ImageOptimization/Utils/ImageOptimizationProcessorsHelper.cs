using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Progress.Sitefinity.ImageOptimization.FileProcessors;
using Telerik.Sitefinity.Modules.Libraries.Configuration;
using Telerik.Sitefinity.Processors;
using Telerik.Sitefinity.Processors.Configuration;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace Progress.Sitefinity.ImageOptimization.Utils
{
    public static class ImageOptimizationProcessorsHelper
    {

        internal static bool IsImageOptimizationProcessorRegistered(IInstallableFileProcessor imageOptimizationProcessor)
        {
            LibrariesConfig librariesConfig = Config.Get<LibrariesConfig>();

            if (!librariesConfig.FileProcessors.ContainsKey(imageOptimizationProcessor.ConfigName))
            {
                return false;
            }

            return true;
        }

        internal static bool ValidateImageOptimizationProcessorsConfigurations()
        {
            return ImageOptimizationProcessorsHelper.GetImageOptimizationProcessors().Count > 0;
        }

        /// <summary>
        /// Creates the enabled and successfully initialized image optimization processors, in configuration order.
        /// </summary>
        internal static IList<ImageOptimizationProcessorBase> GetImageOptimizationProcessors()
        {
            var processors = new List<ImageOptimizationProcessorBase>();

            try
            {
                var configFileProcessors = Config.Get<LibrariesConfig>().GetConfigProcessors();

                foreach (var configFileProcessorsElement in configFileProcessors.Values)
                {
                    if (configFileProcessorsElement.Enabled)
                    {
                        var checkType = TypeResolutionService.ResolveType(configFileProcessorsElement.Type, true);
                        IProcessor instance = (IProcessor)ObjectFactory.Resolve(checkType);

                        instance.Initialize(configFileProcessorsElement.Name, configFileProcessorsElement.Parameters);

                        var imageOptimizationProcessor = instance as ImageOptimizationProcessorBase;

                        if (imageOptimizationProcessor != null && imageOptimizationProcessor.HasInitialized)
                        {
                            processors.Add(imageOptimizationProcessor);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message, TraceEventType.Error);
                processors.Clear();
            }

            return processors;
        }

        internal static void RegisterImageOptimizationProcessors(IEnumerable<IInstallableFileProcessor> imageOptimizationProcessors)
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
    }
}

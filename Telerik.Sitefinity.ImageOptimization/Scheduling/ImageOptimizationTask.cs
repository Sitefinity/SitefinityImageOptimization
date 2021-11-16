using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.Metadata;
using Telerik.Sitefinity.ImageOptimization.Configuration;
using Telerik.Sitefinity.ImageOptimization.Utils;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Metadata.Model;
using Telerik.Sitefinity.Scheduling;
using Telerik.Sitefinity.Scheduling.Model;
using Telerik.Sitefinity.Services;

namespace Telerik.Sitefinity.ImageOptimization.Scheduling
{
    public class ImageOptimizationTask : ScheduledTask
    {
        static ImageOptimizationTask()
        {
            CacheDependency.Subscribe(typeof(ImageOptimizationConfig), ImageOptimizationTask.ConfigUpdated);
        }

        public override void ExecuteTask()
        {
            SystemManager.RunWithElevatedPrivilege((parameters) =>
            {
                ImageOptimizationBackgroundProcessor backgroundProcessor = new ImageOptimizationBackgroundProcessor();
                backgroundProcessor.ProcessImages();
            });
        }

        public override string TaskName
        {
            get
            {
                return ImageOptimizationTask.GetTaskName();
            }
        }

        internal static ScheduledTask NewInstance()
        {
            if (ObjectFactory.GetArgsByName(typeof(ImageOptimizationConfig).Name, typeof(ImageOptimizationConfig)) == null)
            {
                return null;
            }

            ImageOptimizationConfig imageOptimizationConfig = Config.Get<ImageOptimizationConfig>();

            if (!imageOptimizationConfig.EnableImageOptimization)
            {
                return null;
            }

            if (!ImageOptimizationProcessorsHelper.ValidateImageOptimizationProcessorsConfigurations())
            {
                return null;
            }

            // Schedule a task only if required IsOptimized field is present
            Type contentType = typeof(Image);
            MetadataManager metadataManager = MetadataManager.GetManager();
            MetaType metaType = metadataManager.GetMetaType(contentType);

            if (metaType != null)
            {
                MetaField metaField = metaType.Fields.SingleOrDefault(f => string.Compare(f.FieldName, ImageOptimizationFieldBuilder.IsOptimizedFieldName, true, CultureInfo.InvariantCulture) == 0);

                if (metaField == null)
                {
                    return null;
                }
            }

            string crontabConfig = imageOptimizationConfig.ImageOptimizationCronSpec;

            if (string.IsNullOrEmpty(crontabConfig))
            {
                crontabConfig = ImageOptimizationTask.DefaultCronExpression;
            }

            ImageOptimizationTask task = new ImageOptimizationTask()
            {
                Id = Guid.NewGuid(),
                ExecuteTime = DateTime.UtcNow,
                ScheduleSpecType = CrontabScheduleCalculator.ScheduleSpecType,
                ScheduleSpec = crontabConfig
            };

            return task;
        }

        internal static void Schedule()
        {
            ImageOptimizationTask.RemoveScheduledTasks();

            ScheduledTask newTask = ImageOptimizationTask.NewInstance();
            if (newTask != null)
            {
                ImageOptimizationTask.AddScheduledTask(newTask);
            }
        }

        internal static void RemoveScheduledTasks()
        {
            SchedulingManager manager = SchedulingManager.GetManager();

            IList<ScheduledTaskData> scheduledItems = SchedulingManager.GetTasksFromAllProviders(t => t.TaskName == ImageOptimizationTask.GetTaskName());
            foreach (ScheduledTaskData task in scheduledItems)
            {
                manager.DeleteItem(task);
            }

            manager.SaveChanges();
        }

        internal static void AddScheduledTask(ScheduledTask newTask)
        {
            SchedulingManager manager = SchedulingManager.GetManager();

            manager.AddTask(newTask);
            manager.SaveChanges();
        }

        internal static string GetTaskName()
        {
            return typeof(ImageOptimizationTask).FullName;
        }

        private static void ConfigUpdated(ICacheDependencyHandler caller, Type trackedItemType, string trackedItemKey)
        {
            ImageOptimizationTask.Schedule();
        }

        private const string DefaultCronExpression = "0 */12 * * *"; // Every 12 hours
    }
}

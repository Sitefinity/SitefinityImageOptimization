using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.ImageOptimization.Configuration;
using Telerik.Sitefinity.ImageOptimization.Utils;
using Telerik.Sitefinity.Scheduling;
using Telerik.Sitefinity.Scheduling.Model;
using Telerik.Sitefinity.Services;

namespace Telerik.Sitefinity.ImageOptimization
{
    public class ImageOptimizationTask : ScheduledTask
    {
        static ImageOptimizationTask()
        {
            CacheDependency.Subscribe(typeof(ImageOptimizationConfig), ImageOptimizationTask.ConfigUpdated);
        }

        public override void ExecuteTask()
        {
            Log.Write("Image optimization task executed");
            Thread.Sleep(100000);
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
            var imageOptimizationConfig = Config.Get<ImageOptimizationConfig>();
            if (!imageOptimizationConfig.EnableImageOptimization)
            {
                return null;
            }

            string crontabConfig = imageOptimizationConfig.ImageOptimizationCronSpec;

            if (string.IsNullOrEmpty(crontabConfig))
            {
                    crontabConfig = ImageOptimizationTask.DefaultCronExpression;
            }

            var task = new ImageOptimizationTask()
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

            var newTask = ImageOptimizationTask.NewInstance();
            if (newTask != null)
            {
                ImageOptimizationTask.AddScheduledTask(newTask);
            }
        }

        internal static void RemoveScheduledTasks()
        {
            var manager = SchedulingManager.GetManager();

            IList<ScheduledTaskData> scheduledItems = SchedulingManager.GetTasksFromAllProviders(t => t.TaskName == ImageOptimizationTask.GetTaskName());
            foreach (ScheduledTaskData task in scheduledItems)
            {
                manager.DeleteItem(task);
            }

            manager.SaveChanges();
        }

        internal static void AddScheduledTask(ScheduledTask newTask)
        {
            var manager = SchedulingManager.GetManager();

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

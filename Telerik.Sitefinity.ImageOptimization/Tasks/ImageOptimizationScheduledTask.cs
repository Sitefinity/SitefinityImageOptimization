using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Telerik.OpenAccess;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Data.Linq.Dynamic;
using Telerik.Sitefinity.FileProcessors;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Scheduling;
using Telerik.Sitefinity.Workflow;

namespace Telerik.Sitefinity.Modules.Libraries.ImageOptimization.Tasks
{
    internal class ImageOptimizationScheduledTask : ScheduledTask
    {
        public ImageOptimizationScheduledTask()
            : this(LibrariesManager.GetManager())
        {
        }

        internal ImageOptimizationScheduledTask(LibrariesManager librariesManager)
        {
            this.librariesManager = librariesManager;
        }

        public override void ExecuteTask()
        {
            IList<Image> notOptimizedImages = this.GetNotOptimizedImages(10);

            foreach (Image notOptimizedImage in notOptimizedImages)
            {
                try
                {
                    using (Stream imageStream = this.librariesManager.Download(notOptimizedImage))
                    {
                        this.UpdateImage(notOptimizedImage, imageStream);
                    }
                }
                catch (Exception)
                {
                    Log.Write(
                        string.Format("Image could not be compressed: {0}", notOptimizedImage.ResolveMediaUrl()),
                        TraceEventType.Error);
                }
            }

            this.Reschedule();
        }

        private void UpdateImage(Image imageContentItem, Stream imageStream)
        {
            Image temp = this.librariesManager.Lifecycle.CheckOut(imageContentItem) as Image;

            temp.LastModified = DateTime.UtcNow;

            using (MemoryStream tempStream = new MemoryStream())
            {
                // Using a temp MemoryStream in order to prevent a closing/disposing
                // the Stream or Image (image obj which has been created from that stream and in case of disposing the image it will also dispose the stream)
                // earlier and causing a "A generic error occurred in GDI+" exception.
                imageStream.CopyTo(tempStream);
                this.librariesManager.Upload(temp, tempStream, imageContentItem.Extension);
            }

            imageContentItem = this.librariesManager.Lifecycle.CheckIn(temp) as Image;

            this.librariesManager.SaveChanges();

            var bag = new Dictionary<string, string>();
            bag.Add("ContentType", typeof(Image).FullName);
            WorkflowManager.MessageWorkflow(imageContentItem.Id, typeof(Image), null, "Publish", false, bag);
        }

        public override string BuildUniqueKey()
        {
            return this.TaskName;
        }

        private void Reschedule()
        {
            var schedulingManager = SchedulingManager.GetManager();

            var existingTasks = SchedulingManager.GetTasksFromAllProviders(i => i.TaskName == this.TaskName && !i.IsRunning);

            if (existingTasks != null)
            {
                foreach (var task in existingTasks)
                {
                    schedulingManager.DeleteTaskData(task);
                }
            }

            var imageOptimizationScheduledTask = new ImageOptimizationScheduledTask();
            imageOptimizationScheduledTask.ExecuteTime = DateTime.UtcNow.AddMinutes(1);

            schedulingManager.AddTask(imageOptimizationScheduledTask);
            schedulingManager.SaveChanges();
        }

        private IList<Image> GetNotOptimizedImages(int take)
        {
            IList<Image> notOptimizedImages = LibrariesManager.GetManager()
                .GetImages()
                .Where(i => i.Status == ContentLifecycleStatus.Master)
                .ToList()
                .Where(image => !image.FieldValue<bool>("IsOptimized"))
                .Take(take)
                .ToList();

            return notOptimizedImages;
        }

        private readonly LibrariesManager librariesManager;
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.ImageOptimization.Configuration;
using Telerik.Sitefinity.ImageOptimization.Utils;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.Libraries;

namespace Telerik.Sitefinity.ImageOptimization.Scheduling
{
    internal class ImageOptimizationBackgroundProcessor
    {

        public ImageOptimizationBackgroundProcessor()
        {
            ImageOptimizationConfig imageOptimizationConfig = Config.Get<ImageOptimizationConfig>();
            this.batchSize = imageOptimizationConfig.BatchSize;
            this.enableDetailedLogging = imageOptimizationConfig.EnableDetailLogging;
        }

        internal void ProcessImages()
        {
            foreach (var provider in this.Providers)
            {
                try
                {
                    bool itemsProcessed = this.ProcessImagesInternal(provider);

                    if (itemsProcessed)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    this.BuildTrace(string.Format("Optimization for provider {0} failed with exception {1}", provider, ex.Message), true);
                }

                this.WriteTraceLog();
            }
        }

        private bool ProcessImagesInternal(string providerName)
        {
            string transactionName = string.Format("imageoptimization_{0}", providerName);
            LibrariesManager librariesManager = LibrariesManager.GetManager(providerName, transactionName);
            bool itemsProcessed = false;
            int processedImages = 0;

            IEnumerable<Image> images = librariesManager.GetImages().Where(i => i.Status == ContentLifecycleStatus.Master && !i.GetValue<bool>(ImageOptimizationConstants.IsOptimizedFieldName)).Take(this.batchSize);

            foreach (var image in images)
                    {
                        try
                        {
                            this.BuildTrace(string.Format("{0} - Attempting to optimize image {1} ({2})", DateTime.UtcNow.ToString("yyyy-MM-dd-T-HH:mm:ss"), image.Title, image.Id));

                            Image master = image;
                            Image temp = librariesManager.Lifecycle.CheckOut(image) as Image;

                            Stream sourceImageStream = librariesManager.Download(image.Id);
                            librariesManager.Upload(temp, sourceImageStream, image.Extension, true);

                            temp.SetValue(ImageOptimizationConstants.IsOptimizedFieldName, true);

                            master = librariesManager.Lifecycle.CheckIn(temp) as Image;

                            ProcessReplacedImageTranslations(librariesManager, master);

                            if (master.ApprovalWorkflowState == "Published")
                            {
                                librariesManager.Lifecycle.Publish(master);
                            }

                            this.BuildTrace(string.Format("{0} - Image {1} ({2}) has been optimized", DateTime.UtcNow.ToString("yyyy-MM-dd-T-HH:mm:ss"), image.Title, image.Id));

                            if (processedImages % 5 == 0)
                            {
                                TransactionManager.CommitTransaction(transactionName);
                            }

                            processedImages += 1;
                        }
                        catch (Exception ex)
                        {
                            this.BuildTrace(string.Format("Optimization of image {0} ({1}) failed with exception {2}", image.Title, image.Id, ex.Message), true);
                        }

                        this.WriteTraceLog();

                        itemsProcessed = true;
                    }

                    TransactionManager.CommitTransaction(transactionName);

            return itemsProcessed;
        }

        private void ProcessReplacedImageTranslations(LibrariesManager librariesManager, Image image)
        {
            Guid defaultFileId = image.FileId;
            IEnumerable<MediaFileLink> links = image.MediaFileLinks.Where(mfl => mfl.FileId != defaultFileId);

            // process image translations that have replaced image for translation
            foreach (var linkItem in links)
            {
                IList<int> proccessedCultures = new List<int>();

                if (!proccessedCultures.Contains(linkItem.Culture))
                {
                    using (new CultureRegion(linkItem.Culture))
                    {
                        CultureInfo currentCultureInfo = new CultureInfo(linkItem.Culture);

                        try
                        {
                            this.BuildTrace(string.Format("{0} - Attempting to optimize image {1} ({2}) in culture {3}", DateTime.UtcNow.ToString("yyyy-MM-dd-T-HH:mm:ss"), image.Title, image.Id, currentCultureInfo.Name));

                            Image translatedMaster = (linkItem.MediaContent.OriginalContentId == Guid.Empty ? linkItem.MediaContent : librariesManager.GetMediaItem(linkItem.MediaContent.OriginalContentId)) as Image;

                            Image translatedTemp = librariesManager.Lifecycle.CheckOut(translatedMaster) as Image;

                            Stream translationSourceImageStream = librariesManager.Download(linkItem.MediaContentId);
                            librariesManager.Upload(translatedTemp, translationSourceImageStream, image.Extension, false);
                            translatedTemp.SetValue(ImageOptimizationConstants.IsOptimizedFieldName, true);

                            translatedMaster = librariesManager.Lifecycle.CheckIn(translatedTemp) as Image;

                            if (translatedMaster.ApprovalWorkflowState.GetString(currentCultureInfo, false) == "Published")
                            {
                                librariesManager.Lifecycle.Publish(translatedMaster, currentCultureInfo);
                            }

                            proccessedCultures.Add(linkItem.Culture);

                            this.BuildTrace(string.Format("{0} - Image {1} ({2}) in culture {3} has been optimized", DateTime.UtcNow.ToString("yyyy-MM-dd-T-HH:mm:ss"), image.Title, image.Id, currentCultureInfo.Name));
                        }
                        catch (Exception ex)
                        {
                            this.BuildTrace(string.Format("Optimization of image {0} ({1}) in culture {2} failed with exception {3}", image.Title, image.Id, currentCultureInfo.Name, ex.Message), true);
                        }
                    }
                }
            }
        }

        private IEnumerable<string> GetProviders()
        {
            LibrariesManager librariesManager = LibrariesManager.GetManager();

            return librariesManager.GetContextProviders().Select(p => p.Name);
        }

        private void BuildTrace(string input, bool wasExceptionThrown = false)
        {
            if (wasExceptionThrown || this.enableDetailedLogging)
            {
                if (this.logTraceBuilder == null)
                {
                    this.logTraceBuilder = new StringBuilder();
                }

                this.logTraceBuilder.AppendLine(input);
            }
        }

        private void WriteTraceLog()
        {
            if (this.logTraceBuilder != null)
            {
                var traceLog = this.logTraceBuilder.ToString();
                this.logTraceBuilder.Clear();
                Log.Write(traceLog, ConfigurationPolicy.Trace);
            }
        }

        internal IEnumerable<string> Providers
        {
            get
            {
                if (this.providers == null)
                {
                    this.providers = GetProviders();
                }

                return this.providers;
            }
        }

        private int batchSize;
        private bool enableDetailedLogging;
        private StringBuilder logTraceBuilder;
        private IEnumerable<string> providers;
    }
}

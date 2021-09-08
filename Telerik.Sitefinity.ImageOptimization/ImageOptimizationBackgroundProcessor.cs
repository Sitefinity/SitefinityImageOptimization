using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.Libraries;

namespace Telerik.Sitefinity.ImageOptimization
{
    internal class ImageOptimizationBackgroundProcessor
    {

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
                catch(Exception ex)
                {
                    // TODO: Handle exceptions
                }
            }
        }

        private bool ProcessImagesInternal(string providerName)
        {
            string transactionName = string.Format("imageoptimization_{0}", providerName);
            LibrariesManager librariesManager = LibrariesManager.GetManager(providerName, transactionName);
            bool itemsProcessed = false;

            IEnumerable<Image> images = librariesManager.GetImages().Where(i => i.Status == ContentLifecycleStatus.Master && !i.GetValue<bool>(ImageOptimizationFieldBuilder.IsOptimizedFieldName));

            foreach (var image in images)
            {
                Image master = image;

                Stream sourceImageStream = librariesManager.Download(image.Id);

                Image temp = librariesManager.Lifecycle.CheckOut(image) as Image;
                librariesManager.Upload(temp, sourceImageStream, image.Extension, false);
                temp.SetValue(ImageOptimizationFieldBuilder.IsOptimizedFieldName, true);

                master = librariesManager.Lifecycle.CheckIn(temp) as Image;

                ProcessReplacedImageTranslations(librariesManager, master);

                librariesManager.Lifecycle.Publish(master);

                // TODO: do it in batches
                TransactionManager.CommitTransaction(transactionName);

                itemsProcessed = true;
            }

            return itemsProcessed;
        }

        private void ProcessReplacedImageTranslations(LibrariesManager librariesManager, Image item)
        {
            Guid defaultFileId = item.FileId;
            IEnumerable<MediaFileLink> links = item.MediaFileLinks.Where(mfl => mfl.FileId != defaultFileId);

            // process image translations that have replaced image for translation
            foreach (var linkItem in links)
            {
                IList<int> proccessedCultures = new List<int>();

                if (!proccessedCultures.Contains(linkItem.Culture))
                {
                    using (new CultureRegion(linkItem.Culture))
                    {
                        Image translatedMaster = (linkItem.MediaContent.OriginalContentId == Guid.Empty ? linkItem.MediaContent : librariesManager.GetMediaItem(linkItem.MediaContent.OriginalContentId)) as Image;

                        Image translatedTemp = librariesManager.Lifecycle.CheckOut(translatedMaster) as Image;

                        Stream translationSourceImageStream = librariesManager.Download(linkItem.MediaContentId);
                        librariesManager.Upload(translatedTemp, translationSourceImageStream, item.Extension, false);
                        translatedTemp.SetValue(ImageOptimizationFieldBuilder.IsOptimizedFieldName, true);

                        translatedMaster = librariesManager.Lifecycle.CheckIn(translatedTemp) as Image;

                        librariesManager.Lifecycle.Publish(translatedMaster, new CultureInfo(linkItem.Culture));

                        proccessedCultures.Add(linkItem.Culture);
                    }
                }
            }
        }

        private IEnumerable<string> GetProviders()
        {
            LibrariesManager librariesManager = LibrariesManager.GetManager();

            return librariesManager.GetContextProviders().Select(p => p.Name);
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

        private IEnumerable<string> providers;
    }
}

using Telerik.Sitefinity.Localization;

namespace Telerik.Sitefinity.ImageOptimization.Configuration
{
    [ObjectInfo("ImageOptimizationResources", ResourceClassId = "ImageOptimizationResources", Title = "ImageOptimizationResourcesTitle", TitlePlural = "ImageOptimizationResourcesTitlePlural", Description = "ImageOptimizationResourcesDescription")]
    public class ImageOptimizationResources : Resource
    {
        /// <summary>
        /// Image optimization resources title
        /// </summary>
        /// <value>Image optimization labels</value>
        [ResourceEntry("ImageOptimizationResourcesTitle",
            Value = "Image optimization labels",
            Description = "Image optimization resources title",
            LastModified = "2021/08/28")]
        public string ImageOptimizationResourcesTitle
        {
            get
            {
                return this["ImageOptimizationResourcesTitle"];
            }
        }


        /// <summary>
        /// Image optimization resources title
        /// </summary>
        /// <value>Image optimization labels</value>
        [ResourceEntry("ImageOptimizationResourcesTitlePlural",
            Value = "Image optimization labels",
            Description = "Image optimization resources title",
            LastModified = "2021/08/28")]
        public string ImageOptimizationResourcesTitlePlural
        {
            get
            {
                return this["ImageOptimizationResourcesTitlePlural"];
            }
        }

        /// <summary>
        /// Description of the image optimization resources class
        /// </summary>
        /// <value>Contains localizable resources for image optimization</value>
        [ResourceEntry("ImageOptimizationResourcesDescription",
            Value = "Contains localizable resources for image optimization",
            Description = "Description of the image optimization resources class",
            LastModified = "2021/08/28")]
        public string ImageOptimizationResourcesDescription
        {
            get
            {
                return this["ImageOptimizationResourcesDescription"];
            }
        }

        /// <summary>
        /// Image optimization configuration caption
        /// </summary>
        /// <value>Image Optimization</value>
        [ResourceEntry("ImageOptimizationConfigCaption",
            Value = "Image Optimization",
            Description = "Image optimization configuration caption",
            LastModified = "2021/08/28")]
        public string ImageOptimizationConfigCaption
        {
            get
            {
                return this["ImageOptimizationConfigCaption"];
            }
        }

        /// <summary>
        /// Defines configuration settings for Image optimization
        /// </summary>
        /// <value>Image optimization configuration description</value>
        [ResourceEntry("ImageOptimizationConfigDescription",
            Value = "Image optimization configuration description",
            Description = "Defines configuration settings for Image optimization",
            LastModified = "2021/08/28")]
        public string ImageOptimizationConfigDescription
        {
            get
            {
                return this["ImageOptimizationConfigDescription"];
            }
        }

        /// <summary>
        /// Enable image optimization scheduled task title
        /// </summary>
        /// <value>Enable image optimization scheduled task</value>
        [ResourceEntry("EnableImageOptimizationTitle",
            Value = "Enable image optimization scheduled task",
            Description = "Enable image optimization scheduled task title",
            LastModified = "2021/08/28")]
        public string EnableImageOptimizationTitle
        {
            get
            {
                return this["EnableImageOptimizationTitle"];
            }
        }

        /// <summary>
        /// Enable image optimization scheduled task description
        /// </summary>
        /// <value>When disabled the image optimization scheduled task will not be executed.</value>
        [ResourceEntry("EnableImageOptimizationDescription",
            Value = "When disabled the image optimization scheduled task will not be executed.",
            Description = "Enable image optimization scheduled task description",
            LastModified = "2021/08/28")]
        public string EnableImageOptimizationDescription
        {
            get
            {
                return this["EnableImageOptimizationDescription"];
            }
        }

        /// <summary>
        /// Gets ImageOptimizationCronSpec
        /// </summary>
        /// <value>Image optimization cron specification</value>
        [ResourceEntry("ImageOptimizationCronSpecTitle",
            Value = "Image optimization cron specification",
            Description = "phrase: ImageOptimizationCronSpec",
            LastModified = "2019/05/30")]
        public string ImageOptimizationCronSpecTitle
        {
            get
            {
                return this["ImageOptimizationCronSpecTitle"];
            }
        }

        /// <summary>
        /// Gets cron spec
        /// </summary>
        /// <value>A configuration that specifies commands to run periodically on a given schedule. For example: 5 * * * * (run on the fifth minute of every hour)</value>
        [ResourceEntry("ImageOptimizationCronSpecDescription",
            Value = "A configuration that specifies commands to run periodically on a given schedule. For example: 5 * * * * (run on the fifth minute of every hour)",
            Description = "phrase: cron spec",
            LastModified = "2021/08/28")]
        public string ImageOptimizationCronSpecDescription
        {
            get
            {
                return this["ImageOptimizationCronSpecDescription"];
            }
        }

        /// <summary>
        /// phrase: Batch size
        /// </summary>
        /// <value>Batch size</value>
        [ResourceEntry("ImageOptimizationBatchSizeTitle",
            Value = "Batch size",
            Description = "phrase: Batch size",
            LastModified = "2015/06/19")]
        public string ImageOptimizationBatchSizeTitle
        {
            get
            {
                return this["ImageOptimizationBatchSizeTitle"];
            }
        }

        /// <summary>
        /// phrase: The number of images that will be processed on each scheduled task execution. Default: 100
        /// </summary>
        /// <value>The number of images that will be processed on each scheduled task execution. Default: 100</value>
        [ResourceEntry("ImageOptimizationBatchSizeDescription",
            Value = "The number of images that will be processed on each scheduled task execution. Default: 100",
            Description = "phrase: The number of images that will be processed on each scheduled task execution. Default: 100",
            LastModified = "2015/06/19")]
        public string ImageOptimizationBatchSizeDescription
        {
            get
            {
                return this["ImageOptimizationBatchSizeDescription"];
            }
        }

        /// <summary>
        /// Enable image optimization detail logging  title
        /// </summary>
        /// <value>Enable image optimization detail logging</value>
        [ResourceEntry("EnableDetailLoggingTitle",
            Value = "Enable image optimization detail logging",
            Description = "Enable image optimization detail logging  title",
            LastModified = "2021/09/08")]
        public string EnableDetailLoggingTitle
        {
            get
            {
                return this["EnableDetailLoggingTitle"];
            }
        }

        /// <summary>
        /// Enable image optimization detail logging description
        /// </summary>
        /// <value>When enabled the image optimization scheduled task will log detailed information on the processed images.</value>
        [ResourceEntry("EnableDetailLoggingDescription",
            Value = "When enabled the image optimization scheduled task will log detailed information on the processed images.",
            Description = "Enable image optimization detail logging description",
            LastModified = "2021/09/08")]
        public string EnableDetailLoggingDescription
        {
            get
            {
                return this["EnableDetailLoggingDescription"];
            }
        }
    }
}

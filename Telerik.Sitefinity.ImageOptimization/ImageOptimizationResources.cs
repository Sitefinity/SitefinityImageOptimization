using Telerik.Sitefinity.Localization;

namespace Telerik.Sitefinity.ImageOptimization
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
        /// <value>Contains localizable resources for image optimization.</value>
        [ResourceEntry("ImageOptimizationResourcesDescription",
            Value = "Contains localizable resources for image optimization.",
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
        /// Resource strings Image Optimization
        /// </summary>
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
        /// Resource strings for Image optimization description.
        /// </summary>
        [ResourceEntry("ImageOptimizationConfigDescription",
            Value = "Defines configuration settings for Image optimization",
            Description = "Image optimization configuration description.",
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
        /// <value>Contains localizable resources for image optimization.</value>
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
        /// <value>When disabled the image optimization scheduled task will not be executed</value>
        [ResourceEntry("EnableImageOptimizationDescription",
            Value = "When disabled the image optimization scheduled task will not be executed",
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
    }
}

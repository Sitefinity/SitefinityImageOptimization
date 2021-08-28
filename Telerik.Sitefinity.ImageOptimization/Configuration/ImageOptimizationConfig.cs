using NCrontab;
using System;
using System.Configuration;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Localization;

namespace Telerik.Sitefinity.ImageOptimization.Configuration
{
    [ObjectInfo(typeof(ImageOptimizationResources), Title = "ImageOptimizationConfigCaption", Description = "ImageOptimizationConfigDescription")]
    public class ImageOptimizationConfig : ConfigSection
    {
        /// <summary>
        ///  Gets or sets a value indicating whether to enable image optimization.
        /// </summary>
        [ConfigurationProperty(ConfigProps.EnableImageOptimization, DefaultValue = true)]
        [ObjectInfo(typeof(ImageOptimizationResources), Title = "EnableImageOptimizationTitle", Description = "EnableImageOptimizationDescription")]
        public bool EnableImageOptimization
        {
            get
            {
                return (bool)this[ConfigProps.EnableImageOptimization];
            }

            set
            {
                this[ConfigProps.EnableImageOptimization] = value;
            }
        }

        /// <summary>
        /// Gets or sets the cron specification for image optimization task.
        /// </summary>
        [ConfigurationProperty(ConfigProps.ImageOptimizationCronSpec, DefaultValue = "0 * * * *")]
        [ObjectInfo(typeof(ImageOptimizationResources), Title = "ImageOptimizationCronSpecTitle", Description = "ImageOptimizationCronSpecDescription")]
        public string ImageOptimizationCronSpec
        {
            get
            {
                return (string)this[ConfigProps.ImageOptimizationCronSpec];
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(ConfigProps.ImageOptimizationCronSpec);
                }

                value = value.Trim();
                bool valid = true;
                if (!string.IsNullOrEmpty(value))
                {
                    var parts = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 5 || CrontabSchedule.TryParse(value).IsError)
                        valid = false;
                }

                if (!valid)
                    throw new FormatException("Invalid cron date");

                this[ConfigProps.ImageOptimizationCronSpec] = value;
            }
        }

        internal struct ConfigProps
        {
            public const string EnableImageOptimization = "enableImageOptimization";
            public const string ImageOptimizationCronSpec = "imageOptimizationCronSpec";
        }
    }
}

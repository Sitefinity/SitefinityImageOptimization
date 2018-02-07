using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Telerik.Sitefinity.FileProcessors;
using Telerik.Sitefinity.Processors;

namespace Telerik.Sitefinity.ImageOptimization
{
    /// <summary>
    /// Base class for image optimization processors. Image optimization processors are used to optimize the size of the image on upload, before saving it.
    /// </summary>
    internal abstract class ImageOptimizationProcessorBase : ProcessorBase, IInstallableFileProcessor
    {
        public abstract string ConfigName { get; }
        public abstract string ConfigDescription { get; }

        public virtual NameValueCollection ConfigParameters
        {
            get
            {
                NameValueCollection configParameters = new NameValueCollection();
                configParameters.Add(ImageOptimizationProcessorBase.SupportedExtensionsConfigName, ImageOptimizationProcessorBase.SupportedExtensionsConfigDefaultValue);

                return configParameters;
            }
        }

        /// <inheritdoc />
        protected sealed override void Initialize(NameValueCollection config)
        {
            if (config == null)
            {
                return;
            }

            string extensionsToCompressString = config[ImageOptimizationProcessorBase.SupportedExtensionsConfigName];
            if (string.IsNullOrWhiteSpace(extensionsToCompressString))
            {
                return;
            }

            this.supportedExtensions = extensionsToCompressString.ToLower().Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);

            this.hasInitialized = this.InitializeOverride(config);
        }

        /// <summary>
        /// Initialize the processor instance
        /// </summary>
        /// <param name="config">Collection of additional configuration parameters</param>
        /// <returns>True if initialization was successful, otherwise false</returns>
        protected abstract bool InitializeOverride(NameValueCollection config);

        /// <inheritdoc />
        public virtual bool CanProcessFile(FileProcessorInput fileInput)
        {
            if (!this.hasInitialized || fileInput == null)
            {
                return false;
            }

            if (!this.supportedExtensions.Contains(fileInput.FileExtension))
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        Stream IFileProcessor.Process(FileProcessorInput fileInput)
        {
            if (fileInput == null)
            {
                throw new ArgumentException("fileInput cannot be null");
            }

            Stream optimizedFileStream = this.Process(fileInput);

            return optimizedFileStream;
        }

        /// <summary>
        /// Process the input file
        /// </summary>
        /// <param name="fileInput">The input file data and info.</param>
        /// <returns>Processed file output stream</returns>
        public abstract Stream Process(FileProcessorInput fileInput);

        private bool hasInitialized;

        private string[] supportedExtensions;

        private const string SupportedExtensionsConfigName = "SupportedExtensions";

        private const string SupportedExtensionsConfigDefaultValue = ".jpg;.jpeg;.png;.gif";
    }
}
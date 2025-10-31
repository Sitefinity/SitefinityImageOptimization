using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.FileProcessors;
using Telerik.Sitefinity.Modules.Libraries.Configuration;
using TinifyAPI;

namespace Progress.Sitefinity.ImageOptimization.FileProcessors
{
    /// <summary>
    /// Implementation of <see cref="ImageOptimizationProcessorBase"/> using Tinify
    /// </summary>
    internal class TinifyImageOptimizationProcessor : ImageOptimizationProcessorBase
    {
        public override string ConfigName
        {
            get
            {
                return "Tinify Image Optimization";
            }
        }

        public override string ConfigDescription
        {
            get
            {
                return "Optimizes image size using Tinify";
            }
        }

        public override NameValueCollection ConfigParameters
        {
            get
            {
                var configParameters = base.ConfigParameters;
                configParameters.Add(TinifyImageOptimizationProcessor.ApiKeyConfigName, "");
                configParameters.Add(TinifyImageOptimizationProcessor.PreserveMetadataConfigName, "");
                configParameters.Add(TinifyImageOptimizationProcessor.TimeoutConfigName, "");

                return configParameters;
            }
        }

        protected override bool InitializeOverride(NameValueCollection config)
        {
            var configFileProcessors = Config.Get<LibrariesConfig>().GetConfigProcessors();
            var processor = configFileProcessors[this.Name];
            var processorConfig = processor.Parameters;

            if (processorConfig == null)
            {
                return false;
            }

            string preserveMetadataString = processorConfig[TinifyImageOptimizationProcessor.PreserveMetadataConfigName];
            bool preserveMetadataValue;
            if (!string.IsNullOrWhiteSpace(preserveMetadataString) && bool.TryParse(preserveMetadataString, out preserveMetadataValue))
            {
                this.preserveMetadata = preserveMetadataValue;
            }

            string apiKey = processorConfig[TinifyImageOptimizationProcessor.ApiKeyConfigName];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return false;
            }

            int timeout;
            if (int.TryParse(processorConfig[TinifyImageOptimizationProcessor.TimeoutConfigName], out timeout))
            {
                timeoutDurationInSeconds = timeout;
            }
            else
            {
                timeoutDurationInSeconds = TinifyImageOptimizationProcessor.timeoutDefaultDuration;
            }

            Tinify.Key = apiKey;

            return true;
        }

        public override Stream Process(FileProcessorInput fileInput)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutDurationInSeconds)))
            {
                try
                {
                    if (fileInput == null)
                    {
                        throw new ArgumentException("fileInput cannot be null");
                    }

                    byte[] imageBytes = this.GetByteArray(fileInput.FileStream);
                    var sourceData = Tinify.FromBuffer(imageBytes);

                    if (this.preserveMetadata)
                    {
                        sourceData.Preserve(this.MetadataKeys);
                    }

                    var task = sourceData.ToBuffer();
                    if (!task.Wait(TimeSpan.FromSeconds(timeoutDurationInSeconds)))
                    {
                        Log.Write(ImageOptimizationTimeOutExceptionMessage, ConfigurationPolicy.ErrorLog);

                        return fileInput.FileStream;
                    }

                    if (task.IsCanceled)
                    {
                        Log.Write(ImageOptimizationCanceledExceptionMessage, ConfigurationPolicy.ErrorLog);

                        return fileInput.FileStream;
                    }

                    var taskResult = task.GetAwaiter().GetResult();

                    return new MemoryStream(taskResult);
                }
                catch (TimeoutException)
                {
                    Log.Write(ImageOptimizationTimeOutExceptionMessage, ConfigurationPolicy.ErrorLog);

                    return fileInput.FileStream;
                }
                catch (OperationCanceledException)
                {
                    Log.Write(ImageOptimizationCanceledExceptionMessage, ConfigurationPolicy.ErrorLog);

                    return fileInput.FileStream;
                }
                catch (Exception ex)
                {
                    Log.Write(ex, ConfigurationPolicy.ErrorLog);

                    return fileInput.FileStream;
                }
            }
        }

        protected virtual byte[] GetByteArray(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentException("stream cannot be null");
            }

            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);

            return bytes;
        }

        private bool preserveMetadata;

        private int timeoutDurationInSeconds;

        private const string ApiKeyConfigName = "ApiKey";

        private const string PreserveMetadataConfigName = "PreserveMetadata";

        private const string TimeoutConfigName = "TimeoutAfter";

        private const string ImageOptimizationTimeOutExceptionMessage = "Image optimization has timed out. Default image stream was returned.";

        private const string ImageOptimizationCanceledExceptionMessage = "Image optimization task was canceled. Default image stream was returned.";

        private readonly string[] MetadataKeys = new string[] { "copyright", "location", "creation" };

        private const int timeoutDefaultDuration = 60;
    }
}

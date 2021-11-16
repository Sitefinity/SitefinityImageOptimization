using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.FileProcessors;
using TinifyAPI;

namespace Telerik.Sitefinity.ImageOptimization.FileProcessors
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
            if (config == null)
            {
                return false;
            }

            string preserveMetadataString = config[TinifyImageOptimizationProcessor.PreserveMetadataConfigName];
            if (!string.IsNullOrWhiteSpace(preserveMetadataString))
            {
                this.preserveMetadata = bool.Parse(preserveMetadataString);
            }

            string apiKey = config[TinifyImageOptimizationProcessor.ApiKeyConfigName];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return false;
            }

            int timeout;
            if (int.TryParse(config[TinifyImageOptimizationProcessor.TimeoutConfigName], out timeout))
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

                    var resultData = sourceData.ToBuffer().ContinueWith(t => t.GetAwaiter().GetResult(), timeoutCancellationTokenSource.Token).Result;

                    Stream stream = new MemoryStream(resultData);

                    return stream;
                }
                catch (TaskCanceledException)
                {
                    Log.Write("Image optimization has timed out. Default image stream was returned.", ConfigurationPolicy.ErrorLog);
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

        private readonly string[] MetadataKeys = new string[] { "copyright", "location", "creation" };

        private const int timeoutDefaultDuration = 30;
    }
}

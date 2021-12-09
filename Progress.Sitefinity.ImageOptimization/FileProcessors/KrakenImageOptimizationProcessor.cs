using Kraken;
using Kraken.Http;
using Kraken.Model;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.FileProcessors;

namespace Progress.Sitefinity.ImageOptimization.FileProcessors
{
    /// <summary>
    /// Implementation of <see cref="ImageOptimizationProcessorBase"/> using Kraken.IO.
    /// </summary>
    internal class KrakenImageOptimizationProcessor : ImageOptimizationProcessorBase
    {
        public override string ConfigName
        {
            get
            {
                return "Kraken IO Image Optimization";
            }
        }

        public override string ConfigDescription
        {
            get
            {
                return "Optimizes image size using Kraken.IO";
            }
        }

        public override NameValueCollection ConfigParameters
        {
            get
            {
                var configParameters = base.ConfigParameters;
                configParameters.Add(KrakenImageOptimizationProcessor.ApiKeyConfigName, "");
                configParameters.Add(KrakenImageOptimizationProcessor.ApiSecretConfigName, "");
                configParameters.Add(KrakenImageOptimizationProcessor.LossyCompressionConfigName, "");
                configParameters.Add(KrakenImageOptimizationProcessor.PreserveMetadataConfigName, "");
                configParameters.Add(KrakenImageOptimizationProcessor.TimeoutConfigName, "");
                configParameters.Add(KrakenImageOptimizationProcessor.WebpCompressionConfigName, "");

                return configParameters;
            }
        }

        /// <inheritdoc />
        protected override bool InitializeOverride(NameValueCollection config)
        {
            if (config == null)
            {
                return false;
            }

            string lossyCompressionString = config[KrakenImageOptimizationProcessor.LossyCompressionConfigName];
            if (!string.IsNullOrWhiteSpace(lossyCompressionString))
            {
                this.lossyCompression = bool.Parse(lossyCompressionString);
            }

            string preserveMetadataString = config[KrakenImageOptimizationProcessor.PreserveMetadataConfigName];
            if (!string.IsNullOrWhiteSpace(preserveMetadataString))
            {
                this.preserveMetadata = bool.Parse(preserveMetadataString);
            }

            string webpCompressionString = config[KrakenImageOptimizationProcessor.WebpCompressionConfigName];
            if (!string.IsNullOrWhiteSpace(webpCompressionString))
            {
                this.webpCompression = bool.Parse(webpCompressionString);
            }

            string apiKey = config[KrakenImageOptimizationProcessor.ApiKeyConfigName];
            string apiSecret = config[KrakenImageOptimizationProcessor.ApiSecretConfigName];

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            {
                return false;
            }

            int timeout;
            if(int.TryParse(config[KrakenImageOptimizationProcessor.TimeoutConfigName], out timeout))
            {
                timeoutDurationInSeconds = timeout;
            }
            else
            {
                timeoutDurationInSeconds = KrakenImageOptimizationProcessor.timeoutDefaultDuration;
            }

            var connection = Connection.Create(apiKey, apiSecret);
            this.client = new Client(connection);

            return true;
        }

        /// <inheritdoc />
        public override Stream Process(FileProcessorInput fileInput)
        {
            if (fileInput == null)
            {
                throw new ArgumentException("fileInput cannot be null");
            }

            byte[] imageBytes = this.GetByteArray(fileInput.FileStream);
            string imageName = Guid.NewGuid().ToString();

            OptimizeUploadWaitRequest optimizeUploadWaitRequest = new OptimizeUploadWaitRequest();
            optimizeUploadWaitRequest.Lossy = this.lossyCompression;

            if (this.preserveMetadata)
            {
                optimizeUploadWaitRequest.PreserveMeta = new PreserveMeta[] { PreserveMeta.Profile, PreserveMeta.Geotag, PreserveMeta.Date, PreserveMeta.Copyright, PreserveMeta.Orientation };
            }

            optimizeUploadWaitRequest.WebP = this.webpCompression;

            using (var timeoutCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutDurationInSeconds)))
            {
                try
                {
                    IApiResponse<OptimizeWaitResult> response = this.client.OptimizeWait(imageBytes, imageName, optimizeUploadWaitRequest, timeoutCancellationTokenSource.Token).Result;

                    if (!response.Success || response.StatusCode != HttpStatusCode.OK)
                    {
                        return fileInput.FileStream;
                    }

                    Stream stream = this.GetImageStream(response.Body.KrakedUrl);

                    if (this.webpCompression)
                    {
                        fileInput.FileExtension = ".webp";
                        fileInput.MimeType = "image/webp";
                    }

                    return stream;
                }
                catch(TaskCanceledException)
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

        /// <summary>
        /// Gets an in memory stream created from downloading the image in the provided URL.
        /// </summary>
        /// <param name="url">URL of the image that needs to be downloaded.</param>
        /// <returns>The stream.</returns>
        protected virtual Stream GetImageStream(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("url cannot be null, empty or whitespace");
            }

            WebRequest webRequest = HttpWebRequest.Create(url);
            WebResponse webResponse = webRequest.GetResponse();

            return webResponse.GetResponseStream();
        }

        /// <summary>
        /// Gets a byte array from the provided stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>The byte array.</returns>
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

        private bool lossyCompression;

        private bool preserveMetadata;

        private bool webpCompression;

        private int timeoutDurationInSeconds;

        private Client client;

        private const string ApiKeyConfigName = "ApiKey";

        private const string ApiSecretConfigName = "ApiSecret";

        private const string LossyCompressionConfigName = "LossyCompression";

        private const string PreserveMetadataConfigName = "PreserveMetadata";

        private const string WebpCompressionConfigName = "WebpCompression";

        private const string TimeoutConfigName = "TimeoutAfter";

        private const int timeoutDefaultDuration = 30;
    }
}
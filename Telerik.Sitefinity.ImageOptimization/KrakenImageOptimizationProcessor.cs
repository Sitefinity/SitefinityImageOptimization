using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using Kraken;
using Kraken.Http;
using Kraken.Model;
using Telerik.Sitefinity.FileProcessors;

namespace Telerik.Sitefinity.Modules.Libraries.ImageOptimization
{
    /// <summary>
    /// Implementation of <see cref="ImageOptimizationProcessorBase"/> using Kraken.IO.
    /// </summary>
    internal class KrakenImageOptimizationProcessor : ImageOptimizationProcessorBase
    {
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

            string apiKey = config[KrakenImageOptimizationProcessor.ApiKeyConfigName];
            string apiSecret = config[KrakenImageOptimizationProcessor.ApiSecretConfigName];

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
            {
                return false;
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

            IApiResponse<OptimizeWaitResult> response = this.client.OptimizeWait(imageBytes, imageName, optimizeUploadWaitRequest).Result;

            if (!response.Success || response.StatusCode != HttpStatusCode.OK)
            {
                return fileInput.FileStream;
            }

            Stream stream = this.GetImageStream(response.Body.KrakedUrl);

            return stream;
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

        private Client client;

        private const string ApiKeyConfigName = "ApiKey";

        private const string ApiSecretConfigName = "ApiSecret";

        private const string LossyCompressionConfigName = "LossyCompression";
    }
}
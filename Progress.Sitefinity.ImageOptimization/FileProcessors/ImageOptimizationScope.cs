using System;

namespace Progress.Sitefinity.ImageOptimization.FileProcessors
{
    /// <summary>
    /// Collects the result of the image optimization processors for a single file upload.
    /// </summary>
    internal sealed class ImageOptimizationScope : IDisposable
    {
        internal static ImageOptimizationScope Current
        {
            get
            {
                return ImageOptimizationScope.current;
            }
        }

        internal bool IsOptimized { get; set; }

        internal static ImageOptimizationScope Begin()
        {
            ImageOptimizationScope.current = new ImageOptimizationScope();

            return ImageOptimizationScope.current;
        }

        public void Dispose()
        {
            ImageOptimizationScope.current = null;
        }

        // Uploads are synchronous, so the processors run on the thread that opened the scope.
        [ThreadStatic]
        private static ImageOptimizationScope current;
    }
}

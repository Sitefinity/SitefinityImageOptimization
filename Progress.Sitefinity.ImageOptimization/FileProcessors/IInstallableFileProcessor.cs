using System.Collections.Specialized;
using Telerik.Sitefinity.FileProcessors;

namespace Progress.Sitefinity.ImageOptimization.FileProcessors
{
    public interface IInstallableFileProcessor : IFileProcessor
    {
        string ConfigName { get; }

        string ConfigDescription { get; }

        bool HasInitialized { get; }

        NameValueCollection ConfigParameters { get; }
    }
}
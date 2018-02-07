using System.Collections.Specialized;
using Telerik.Sitefinity.FileProcessors;

namespace Telerik.Sitefinity.ImageOptimization
{
    public interface IInstallableFileProcessor : IFileProcessor
    {
        string ConfigName { get; }

        string ConfigDescription { get; }

        NameValueCollection ConfigParameters { get; }
    }
}
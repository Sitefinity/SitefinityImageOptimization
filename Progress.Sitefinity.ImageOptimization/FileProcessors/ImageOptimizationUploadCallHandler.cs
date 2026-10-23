using System;
using System.Reflection;
using Progress.Sitefinity.ImageOptimization.Utils;
using Telerik.Microsoft.Practices.Unity;
using Telerik.Microsoft.Practices.Unity.InterceptionExtension;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.Libraries;

namespace Progress.Sitefinity.ImageOptimization.FileProcessors
{
    /// <summary>
    /// Wraps the libraries provider upload in an <see cref="ImageOptimizationScope"/> and stores its result on the uploaded image.
    /// </summary>
    internal class ImageOptimizationUploadCallHandler : ICallHandler
    {
        public int Order { get; set; }

        /// <summary>
        /// Must be called before the libraries providers are resolved, because their interception pipeline is built on resolve.
        /// </summary>
        internal static void RegisterPolicy()
        {
            ObjectFactory.Container.Configure<Interception>()
                .AddPolicy(ImageOptimizationUploadCallHandler.PolicyName)
                .AddMatchingRule(new UploadMatchingRule())
                .AddCallHandler(new ImageOptimizationUploadCallHandler());
        }

        public IMethodReturn Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext)
        {
            Image image = input.Arguments[0] as Image;
            if (image == null)
            {
                return getNext()(input, getNext);
            }

            // The background task opens the scope itself, after it has already optimized the file.
            ImageOptimizationScope scope = ImageOptimizationScope.Current;
            bool ownsScope = scope == null;
            if (ownsScope)
            {
                scope = ImageOptimizationScope.Begin();
            }

            try
            {
                IMethodReturn result = getNext()(input, getNext);

                if (result.Exception == null)
                {
                    ImageOptimizationUploadCallHandler.SetIsOptimized(image, scope.IsOptimized);
                }

                return result;
            }
            finally
            {
                if (ownsScope)
                {
                    scope.Dispose();
                }
            }
        }

        private static void SetIsOptimized(Image image, bool isOptimized)
        {
            try
            {
                image.SetValue(ImageOptimizationConstants.IsOptimizedFieldName, isOptimized);
            }
            catch (Exception ex)
            {
                Log.Write(string.Format("Error occurred while setting image optimization field value: {0}", ex.Message), ConfigurationPolicy.ErrorLog);
            }
        }

        private const string PolicyName = "ImageOptimizationUpload";

        private class UploadMatchingRule : IMatchingRule
        {
            public bool Matches(MethodBase member)
            {
                if (member.Name != "Upload" || !typeof(LibrariesDataProvider).IsAssignableFrom(member.DeclaringType))
                {
                    return false;
                }

                // The shorter overloads delegate to this one, so the handler runs once per upload.
                ParameterInfo[] parameters = member.GetParameters();

                return parameters.Length == 5 && parameters[0].ParameterType == typeof(MediaContent);
            }
        }
    }
}

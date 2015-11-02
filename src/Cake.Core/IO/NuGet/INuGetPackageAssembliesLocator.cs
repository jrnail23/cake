using System.Runtime.Versioning;

namespace Cake.Core.IO.NuGet
{
    /// <summary>
    /// Finds assemblies included in a nuget package.
    /// </summary>
    public interface INuGetPackageAssembliesLocator
    {
        /// <summary>
        /// Finds assemblies (DLLs) included in a nuget package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="addinDirectory">The addin directory.</param>
        /// <returns>
        /// the DLLs as IFile[].
        /// </returns>
        IFile[] FindAssemblies(NuGetPackage package, DirectoryPath addinDirectory);
    }
}
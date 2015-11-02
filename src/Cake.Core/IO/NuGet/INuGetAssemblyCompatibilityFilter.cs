using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Cake.Core.IO.NuGet
{
    /// <summary>
    /// Filters assemblies for .Net target framework compatibility 
    /// </summary>
    public interface INuGetAssemblyCompatibilityFilter
    {
        /// <summary>
        /// Filters the assemblies for .Net target framework compatibility .
        /// </summary>
        /// <param name="targetFramework">The target framework.</param>
        /// <param name="assemblyFiles">The assembly files.</param>
        /// <returns>a subset of the provided assemblyFiles that match the provided targetFramework.</returns>
        IEnumerable<FilePath> FilterCompatibleAssemblies(FrameworkName targetFramework, IEnumerable<FilePath> assemblyFiles);
    }
}
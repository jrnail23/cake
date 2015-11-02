using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Cake.Core.IO;
using Cake.Core.IO.NuGet;
using NuGet;

namespace Cake.NuGet
{
    /// <summary>
    /// Filters assemblies for .Net target framework compatibility 
    /// </summary>
    public class NuGetAssemblyCompatibilityFilter : INuGetAssemblyCompatibilityFilter
    {
        private readonly INuGetVersionUtility _versionUtility;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetAssemblyCompatibilityFilter"/> class.
        /// </summary>
        /// <param name="versionUtility">The version utility.</param>
        public NuGetAssemblyCompatibilityFilter(INuGetVersionUtility versionUtility)
        {
            _versionUtility = versionUtility;
        }

        /// <summary>
        /// Filters the assemblies for .Net target framework compatibility .
        /// </summary>
        /// <param name="targetFramework">The target framework.</param>
        /// <param name="assemblyFiles">The assembly files.</param>
        /// <returns>a subset of the provided assemblyFiles that match the provided targetFramework.</returns>
        public IEnumerable<FilePath> FilterCompatibleAssemblies(FrameworkName targetFramework,
            IEnumerable<FilePath> assemblyFiles)
        {
            // create PackageReferenceSets from the given assemblies
            var referenceSets = assemblyFiles
                .Select(d => new { FilePath = d, FrameworkName = ParseFrameworkFolderName(d.FullPath) })
                .GroupBy(d => d.FrameworkName)
                .Select(v => new PackageReferenceSet(v.Key, v.Select(d => d.FilePath.FullPath)));

            IEnumerable<PackageReferenceSet> compatibleReferences;

            if (_versionUtility.TryGetCompatibleItems(targetFramework, referenceSets, out compatibleReferences))
            {
                return compatibleReferences.SelectMany(r => r.References)
                    .Select(FilePath.FromString);
            }

            return Enumerable.Empty<FilePath>();
        }

        private FrameworkName ParseFrameworkFolderName(string path)
        {
            var parsedFxName = _versionUtility.ParseFrameworkFolderName(path);

            if (parsedFxName != null && parsedFxName.Identifier == "Unsupported")
            {
                var filePath = FilePath.FromString(path);
                if (filePath.Segments.Length > 1)
                {
                    // keep chopping the filePath until we find a folder name that matches a framework version.
                    var choppedPath = string.Join("/", filePath.Segments.Skip(1));
                    return ParseFrameworkFolderName(choppedPath);
                }
            }

            return parsedFxName;
        }
    }
}
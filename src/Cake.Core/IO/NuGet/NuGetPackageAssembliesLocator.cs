using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Core.Diagnostics;

namespace Cake.Core.IO.NuGet
{
    /// <summary>
    /// Finds assemblies (DLLs) included in a nuget package.
    /// </summary>
    public class NuGetPackageAssembliesLocator : INuGetPackageAssembliesLocator
    {
        private readonly INuGetAssemblyCompatibilityFilter _assemblyCompatibilityFilter;
        private readonly ICakeEnvironment _environment;
        private readonly IFileSystem _fileSystem;
        private readonly ICakeLog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetPackageAssembliesLocator"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="log">The log.</param>
        /// <param name="assemblyCompatibilityFilter">The assembly compatibility filter.</param>
        /// <param name="environment">The environment.</param>
        public NuGetPackageAssembliesLocator(IFileSystem fileSystem, ICakeLog log,
            INuGetAssemblyCompatibilityFilter assemblyCompatibilityFilter, ICakeEnvironment environment)
        {
            _fileSystem = fileSystem;
            _log = log;
            _assemblyCompatibilityFilter = assemblyCompatibilityFilter;
            _environment = environment;
        }

        /// <summary>
        /// Finds assemblies (DLLs) included in a nuget package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="addinDirectory">The addin directory.</param>
        /// <returns>
        /// the DLLs as IFile[].
        /// </returns>
        public IFile[] FindAssemblies(NuGetPackage package,
            DirectoryPath addinDirectory)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }
            if (addinDirectory == null)
            {
                throw new ArgumentNullException("addinDirectory");
            }

            var addInDirectory = _fileSystem.GetDirectory(addinDirectory);
            if (!addInDirectory.Exists)
            {
                return new IFile[0];
            }

            var assemblies = addInDirectory.GetFiles("*.dll", SearchScope.Recursive)
                .Where(file => !file.Path.FullPath.EndsWith("Cake.Core.dll", StringComparison.OrdinalIgnoreCase))
                .Select(a => a.Path)
                .ToArray();

            if (!assemblies.Any())
            {
                _log.Warning("Unable to locate any assemblies under {0}", addInDirectory.Path.FullPath);
            }

            var compatibleAssemblies = FilterCompatibleAssemblies(assemblies, addinDirectory);

            return compatibleAssemblies.Select(ca => _fileSystem.GetFile(ca)).ToArray();
        }

        private IEnumerable<FilePath> FilterCompatibleAssemblies(
            IEnumerable<FilePath> assemblies, DirectoryPath addInDirectoryPath)
        {
            var assemblyPathsRelativeToAddinDir =
                assemblies.Select(a => a.IsRelative ? a : a.FullPath.Substring(addInDirectoryPath.FullPath.Length + 1));
            var targetFramework = _environment.GetTargetFramework();
            var compatibleAssemblyPathsRelativeToAddinDir =
                _assemblyCompatibilityFilter.FilterCompatibleAssemblies(targetFramework, assemblyPathsRelativeToAddinDir);

            return compatibleAssemblyPathsRelativeToAddinDir.Select(cp => cp.MakeAbsolute(addInDirectoryPath));
        }
    }
}
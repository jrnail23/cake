using System.Collections.Generic;
using System.Runtime.Versioning;
using NuGet;

namespace Cake.NuGet
{
    /// <summary>
    /// Thin wrapper around NuGet's VersionUtility, purely intended as a testing/DI seam.
    /// </summary>
    public interface INuGetVersionUtility
    {
        /// <summary>
        /// Tries the get compatible items.
        /// </summary>
        /// <typeparam name="T">the type of items being retrieved</typeparam>
        /// <param name="projectFramework">The project framework.</param>
        /// <param name="items">The items.</param>
        /// <param name="compatibleItems">The compatible items.</param>
        /// <returns>boolean indicating whether the items were retrieved successfully</returns>
        bool TryGetCompatibleItems<T>(FrameworkName projectFramework, IEnumerable<T> items,
            out IEnumerable<T> compatibleItems)
            where T : IFrameworkTargetable;

        /// <summary>
        /// Parses the name of the framework folder.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>the FrameworkName derived from the given path</returns>
        FrameworkName ParseFrameworkFolderName(string path);
    }
}
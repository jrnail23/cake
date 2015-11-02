using System.Collections.Generic;
using System.Runtime.Versioning;
using NuGet;

namespace Cake.NuGet
{
    /// <summary>
    ///     Thin wrapper around NuGet's VersionUtility, purely intended as a testing/DI seam.
    /// </summary>
    public class NuGetVersionUtilityWrapper : INuGetVersionUtility
    {
        /// <summary>
        ///     Tries the get compatible items.
        /// </summary>
        /// <typeparam name="T">the type of items being retrieved</typeparam>
        /// <param name="projectFramework">The project framework.</param>
        /// <param name="items">The items.</param>
        /// <param name="compatibleItems">The compatible items.</param>
        /// <returns>boolean indicating whether the items were retrieved successfully</returns>
        public bool TryGetCompatibleItems<T>(FrameworkName projectFramework, IEnumerable<T> items,
            out IEnumerable<T> compatibleItems) where T : IFrameworkTargetable
        {
            return VersionUtility.TryGetCompatibleItems(projectFramework, items, out compatibleItems);
        }

        /// <summary>
        ///     Parses the name of the framework folder.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>the FrameworkName derived from the given path</returns>
        public FrameworkName ParseFrameworkFolderName(string path)
        {
            return VersionUtility.ParseFrameworkFolderName(path);
        }
    }
}
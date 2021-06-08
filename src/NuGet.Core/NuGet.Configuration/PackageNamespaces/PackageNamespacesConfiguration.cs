// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace NuGet.Configuration
{
    public class PackageNamespacesConfiguration
    {
        /// <summary>
        /// Max allowed length for package Id.
        /// In case update this value please update in C:\NuGetProj\NuGet.Client\src\NuGet.Core\NuGet.Packaging\PackageCreation\Utility\PackageIdValidator.cs too.
        /// </summary>
        public static int PackageIdMaxLength { get; } = 100;

        /// <summary>
        /// Source name to package namespace list.
        /// </summary>
        internal Dictionary<string, IReadOnlyList<string>> Namespaces { get; }

        private Lazy<SearchTree> SearchTree { get; }

        /// <summary>
        /// Lookup package sources matches with a term from package namespaces.
        /// </summary>
        /// <param name="term">Search term. Never null. </param>
        /// <returns>Package sources with a term from package namespaces.</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="term"/> is null or empty.</exception>
        public HashSet<string> Match(string term)
        {
            return SearchTree.Value?.PrefixMatch(term);
        }

        internal PackageNamespacesConfiguration(Dictionary<string, IReadOnlyList<string>> namespaces)
        {
            Namespaces = namespaces ?? throw new ArgumentNullException(nameof(namespaces));
            SearchTree = new Lazy<SearchTree>(() => GetSearchTree());
        }

        /// <summary>
        /// Generates a <see cref="PackageNamespacesConfiguration"/> based on the settings object.
        /// </summary>
        /// <param name="settings">Settings. Never null. </param>
        /// <returns>A <see cref="PackageNamespacesConfiguration"/> based on the settings.</returns>
        /// <exception cref="ArgumentNullException"> if <paramref name="settings"/> is null.</exception>
        public static PackageNamespacesConfiguration GetPackageNamespacesConfiguration(ISettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var packageNamespacesProvider = new PackageNamespacesProvider(settings);

            var namespaces = new Dictionary<string, IReadOnlyList<string>>();

            foreach (PackageNamespacesSourceItem packageSourceNamespaceItem in packageNamespacesProvider.GetPackageSourceNamespaces())
            {
                namespaces.Add(packageSourceNamespaceItem.Key, new List<string>(packageSourceNamespaceItem.Namespaces.Select(e => e.Id)));
            }

            return new PackageNamespacesConfiguration(namespaces);
        }

        private SearchTree GetSearchTree()
        {
            SearchTree nameSpaceLookup = null;

            if (Namespaces.Keys.Any())
            {
                nameSpaceLookup = new SearchTree(this);
            }

            return nameSpaceLookup;
        }
    }
}

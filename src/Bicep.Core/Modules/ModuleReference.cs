// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Bicep.Core.Modules
{
    /// <summary>
    /// Strongly typed representation of a module reference string.
    /// </summary>
    public abstract class ModuleReference
    {
        /// <summary>
        /// Gets the fully qualified module reference, which includes the scheme.
        /// </summary>
        public abstract string FullyQualifiedReference { get; }

        /// <summary>
        /// Gets the unqualified module reference, which does not include the scheme.
        /// </summary>
        public abstract string UnqualifiedReference { get; }

        /// <summary>
        /// Formats the fully qualified reference by adding the scheme as a prefix.
        /// </summary>
        /// <param name="scheme">The scheme</param>
        protected string FormatFullyQualifiedReference(string scheme) => $"{scheme}:{this.UnqualifiedReference}";
    }
}

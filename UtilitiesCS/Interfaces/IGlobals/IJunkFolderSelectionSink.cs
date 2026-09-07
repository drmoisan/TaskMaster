namespace UtilitiesCS
{
    /// <summary>
    /// Typed seam through which the store settings dialog persists the user's junk-folder
    /// selections (issue #797, AC5). It replaces a reflection lookup for a method named
    /// <c>ApplyJunkFolderSelections</c>, which bound the call site to the implementation by name
    /// only and therefore failed silently after a rename. Declared in UtilitiesCS and implemented in
    /// TaskMaster, so the one-way TaskMaster to UtilitiesCS project reference direction is
    /// preserved. The member is deliberately not added to <see cref="IOlObjects"/>, which would
    /// force every existing implementer and test stub to change.
    /// </summary>
    public interface IJunkFolderSelectionSink
    {
        /// <summary>
        /// Persists the two junk-folder selections. The parameter order is fixed and is part of the
        /// contract: the junk-certain path is supplied first and the junk-potential path second.
        /// </summary>
        /// <param name="junkCertainRelativePath">
        /// The store-relative path of the junk-certain folder. Supplied first.
        /// </param>
        /// <param name="junkPotentialRelativePath">
        /// The store-relative path of the junk-potential folder. Supplied second.
        /// </param>
        void ApplyJunkFolderSelections(
            string junkCertainRelativePath,
            string junkPotentialRelativePath
        );
    }
}

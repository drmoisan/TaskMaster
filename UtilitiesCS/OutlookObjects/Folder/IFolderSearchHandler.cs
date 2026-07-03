using System.Collections.Generic;

namespace UtilitiesCS
{
    /// <summary>
    /// Cycle-3 (P10-T1) narrow seam over the <see cref="FolderPredictor"/> members
    /// <see cref="QfcItemController"/> actually consumes (<c>FolderArray</c>, <c>Suggestions</c>,
    /// <c>FindFolder</c>). Construction still goes through an injectable
    /// <c>Func&lt;IApplicationGlobals, object, FolderPredictor.InitOptions, FolderPredictor&gt;</c>
    /// factory delegate (concrete return type) because <c>LoadFolderHandlerAsync</c> also calls
    /// <c>FolderPredictor.InitAsync</c>, which is not part of this narrow consuming surface.
    /// </summary>
    public interface IFolderSearchHandler
    {
        /// <summary>Matches <see cref="FolderPredictor.FolderArray"/>.</summary>
        string[] FolderArray { get; }

        /// <summary>Matches <see cref="FolderPredictor.Suggestions"/>.</summary>
        FolderScorer Suggestions { get; }

        /// <summary>Matches <see cref="FolderPredictor.FindFolder"/> exactly.</summary>
        string[] FindFolder(
            string searchString,
            object objItem,
            bool reloadCTFStagingFiles = true,
            List<string> emailSearchRoots = null,
            bool recalcSuggestions = false,
            IEnumerable<(string root, string excludedFolder, bool excludeChildren)> exclusions =
                null
        );
    }
}

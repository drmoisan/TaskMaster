namespace UtilitiesCS
{
    /// <summary>
    /// Classifies a <see cref="FolderRow"/> so downstream renderers can distinguish section
    /// separators, search results, scored suggestions, and recent selections by
    /// <see cref="FolderRow.Kind"/> rather than by string-matching separator text
    /// (for example <c>.StartsWith("====")</c>).
    /// </summary>
    public enum FolderRowKind
    {
        /// <summary>A non-selectable section header row (for example "===== SUGGESTIONS =====").</summary>
        Separator,

        /// <summary>A folder produced by the search-results block of <see cref="FolderPredictor.FindFolder"/>.</summary>
        SearchResult,

        /// <summary>A scored folder suggestion; the only kind that carries a non-null <see cref="FolderRow.Score"/>.</summary>
        Suggestion,

        /// <summary>A recently used folder selection.</summary>
        Recent,
    }

    /// <summary>
    /// Immutable, additive row model mirroring a single entry of the legacy string arrays produced
    /// by <see cref="FolderPredictor"/> (<c>FolderArray</c> / <c>FindFolder</c>). <see cref="Text"/>
    /// equals the exact legacy string, so a renderer can consume either the legacy <c>string[]</c>
    /// or the new <c>FolderRow[]</c>. net48-safe <c>readonly struct</c> (no <c>record</c>/<c>init</c>).
    /// </summary>
    public readonly struct FolderRow
    {
        /// <summary>
        /// Creates a <see cref="FolderRow"/>.
        /// </summary>
        /// <param name="text">The exact string the legacy array places at this position.</param>
        /// <param name="kind">The row classification.</param>
        /// <param name="score">
        /// The scored projection for a <see cref="FolderRowKind.Suggestion"/> row; <c>null</c> for
        /// every other kind.
        /// </param>
        public FolderRow(string text, FolderRowKind kind, FolderScore? score)
        {
            Text = text;
            Kind = kind;
            Score = score;
        }

        /// <summary>The exact string currently placed in the legacy array at this position.</summary>
        public string Text { get; }

        /// <summary>The row classification.</summary>
        public FolderRowKind Kind { get; }

        /// <summary>
        /// The scored projection for this row; non-null only for
        /// <see cref="FolderRowKind.Suggestion"/> rows (separators, search results, and recents
        /// carry <c>null</c>).
        /// </summary>
        public FolderScore? Score { get; }
    }
}

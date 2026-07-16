namespace UtilitiesCS
{
    /// <summary>
    /// Immutable, additive projection of a single folder suggestion produced by
    /// <see cref="FolderScorer"/>. Carries the folder identity together with its raw ranking
    /// <see cref="Score"/> and a normalized <see cref="Probability"/> display value. This type
    /// is a net48-safe <c>readonly struct</c> (no <c>record</c>/<c>init</c>; precedent
    /// <c>ResourceTimingRow</c>) so it compiles under <c>TreatWarningsAsErrors</c> without an
    /// <c>IsExternalInit</c> polyfill.
    /// </summary>
    public readonly struct FolderScore
    {
        /// <summary>
        /// Creates a <see cref="FolderScore"/> for a single folder suggestion.
        /// </summary>
        /// <param name="folderPath">The folder identity (path) used as the scoring key.</param>
        /// <param name="score">The raw accumulated ranking score, verbatim.</param>
        /// <param name="probability">
        /// The max-normalized relative display value in <c>[0,1]</c> (see <see cref="Probability"/>).
        /// </param>
        public FolderScore(string folderPath, long score, double probability)
        {
            FolderPath = folderPath;
            Score = score;
            Probability = probability;
        }

        /// <summary>
        /// The folder identity (path); the unchanged scoring key from
        /// <see cref="FolderScorer"/>.
        /// </summary>
        public string FolderPath { get; }

        /// <summary>
        /// The raw accumulated ranking score, verbatim — the exact value used for internal
        /// ranking. Because the three score sources (Bayesian, conversation, word-sequence) are
        /// mixed-scale and accumulate per folder, this value is not bounded to any calibrated range.
        /// </summary>
        public long Score { get; }

        /// <summary>
        /// A max-normalized value in <c>[0,1]</c> (<c>Score / TopScore</c>) intended purely as a
        /// <b>relative display value</b> — the relative confidence of this suggestion versus the
        /// best suggestion in the same projected set. It is <b>not</b> a calibrated Bayesian
        /// posterior: the underlying scores are mixed-scale and accumulate across sources, so this
        /// value only communicates rank-relative strength, not an absolute probability. When the
        /// top score is 0 (empty scorer or all-zero seeds) this is 0 for every row, so no
        /// division by zero occurs. Downstream may still compute <c>Score / 1000</c> from the raw
        /// <see cref="Score"/> if a future Bayesian-only surface needs a calibrated value.
        /// </summary>
        public double Probability { get; }
    }
}

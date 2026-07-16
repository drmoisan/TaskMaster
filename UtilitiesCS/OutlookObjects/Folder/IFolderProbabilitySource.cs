#nullable enable
namespace UtilitiesCS
{
    /// <summary>
    /// Narrow consumption seam for the upstream <c>folder-probability-plumbing</c> contract (epic
    /// placeholder issue 9001). Maps a full folder-path string to its prediction probability in
    /// <c>[0,1]</c>. This interface is the only coupling point between this feature and 9001: if the
    /// finalized upstream surface differs from the assumed shape, only this seam and
    /// <see cref="FolderProbabilityAdapter"/> change. Probabilities are consumed, never recomputed.
    /// </summary>
    public interface IFolderProbabilitySource
    {
        /// <summary>
        /// Attempts to retrieve the prediction probability for a presented folder path.
        /// </summary>
        /// <param name="fullFolderPath">The full folder-path string used as the join key.</param>
        /// <param name="probability">
        /// When this method returns <c>true</c>, the probability in <c>[0,1]</c> for the path;
        /// otherwise <c>0</c>.
        /// </param>
        /// <returns><c>true</c> when a probability is available for the path; otherwise <c>false</c>.</returns>
        bool TryGetProbability(string fullFolderPath, out double probability);
    }
}

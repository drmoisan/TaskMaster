using System.Collections.Generic;
using System.Linq;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    /// <summary>
    /// Seam interface shared by folder predictors. Declares exactly the members the
    /// <c>Manager["Folder"]</c> callers (<c>EmailFiler</c>, <c>SortEmail</c>,
    /// <c>FolderScorer</c>) use, with signatures identical to
    /// <see cref="BayesianClassifierGroup"/> so the flat predictor satisfies the
    /// interface without behavior change.
    /// </summary>
    public interface IFolderPredictor
    {
        /// <summary>
        /// Trains the predictor on one or more emails filed to <paramref name="tag"/>.
        /// </summary>
        /// <param name="tag">The leaf folder identifier (relative path) the email was filed to.</param>
        /// <param name="matchTokens">The tokenized email content.</param>
        /// <param name="emailCount">The number of emails represented by the tokens.</param>
        void Train(string tag, IEnumerable<string> matchTokens, int emailCount);

        /// <summary>
        /// Reverses prior training for <paramref name="tag"/>, decrementing token and email counts.
        /// </summary>
        /// <param name="tag">The leaf folder identifier (relative path) to untrain.</param>
        /// <param name="matchTokens">The tokenized email content previously trained.</param>
        /// <param name="emailCount">The number of emails represented by the tokens.</param>
        void UnTrain(string tag, IEnumerable<string> matchTokens, int emailCount);

        /// <summary>
        /// Classifies tokenized email content and returns folder predictions ordered by
        /// descending probability.
        /// </summary>
        /// <param name="tokens">The tokenized email content.</param>
        /// <returns>Predictions ordered by descending <see cref="Prediction{T}.Probability"/>.</returns>
        OrderedParallelQuery<Prediction<string>> Classify(string[] tokens);

        /// <summary>
        /// Persists the predictor to its configured disk location. Satisfied by
        /// <c>SmartSerializable&lt;T&gt;.Serialize()</c> on both implementations.
        /// </summary>
        void Serialize();
    }
}

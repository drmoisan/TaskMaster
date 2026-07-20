#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    /// <summary>
    /// Scores the direct children of a single parent folder node using hierarchical-shrinkage
    /// Naive Bayes. Wraps one <see cref="BayesianClassifierGroup"/> whose
    /// <see cref="BayesianClassifierGroup.Classifiers"/> are keyed by direct child segment and
    /// whose <see cref="BayesianClassifierGroup.SharedTokenBase"/> is the parent-scoped token
    /// <see cref="Corpus"/>. The existing <see cref="BayesianClassifierShared"/> and
    /// <see cref="Corpus"/> count machinery is reused without modification.
    /// </summary>
    /// <remarks>
    /// Per-child scoring blends the child (leaf) per-token estimate with the parent-scope
    /// estimate: <c>P_smoothed(t | c) = λ·P_leaf(t | c) + (1-λ)·P_parent(t | p)</c>, where λ is
    /// <see cref="ShrinkageLambda"/>. When the total number of examples under the parent is below
    /// <see cref="MinColdStartExamples"/>, scoring falls back to unsmoothed Naive Bayes (the
    /// child-only estimate, equivalent to λ = 1). This type is pure logic with no Outlook COM or
    /// filesystem dependency.
    /// </remarks>
    public sealed class PerParentClassifier
    {
        // Laplace add-one smoothing constant applied to every per-token estimate so that an
        // unseen token never zeroes the product. Documented as a fixed internal constant rather
        // than a tunable knob to keep the blend deterministic and reproducible in tests.
        private const double LaplaceAlpha = 1.0;

        private BayesianClassifierGroup _group;

        // Newtonsoft constructor: properties are populated after construction, so validation runs
        // on the deserialized values via OnDeserialized.
        [JsonConstructor]
        private PerParentClassifier()
        {
            _group = new BayesianClassifierGroup();
        }

        /// <summary>
        /// Initializes a per-parent classifier.
        /// </summary>
        /// <param name="shrinkageLambda">
        /// Weight on the leaf estimate in the shrinkage blend; must be in the inclusive range
        /// <c>[0, 1]</c>.
        /// </param>
        /// <param name="minColdStartExamples">
        /// Minimum total examples under the parent before the shrinkage blend is applied; must be
        /// non-negative. Below this threshold, scoring uses unsmoothed Naive Bayes.
        /// </param>
        /// <param name="group">
        /// Optional pre-populated child-keyed group. When null, a new empty group is created.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="shrinkageLambda"/> is outside <c>[0, 1]</c> or
        /// <paramref name="minColdStartExamples"/> is negative.
        /// </exception>
        public PerParentClassifier(
            double shrinkageLambda,
            int minColdStartExamples,
            BayesianClassifierGroup? group = null
        )
        {
            ValidateInvariants(shrinkageLambda, minColdStartExamples);
            ShrinkageLambda = shrinkageLambda;
            MinColdStartExamples = minColdStartExamples;
            _group = group ?? new BayesianClassifierGroup();
        }

        private static void ValidateInvariants(double shrinkageLambda, int minColdStartExamples)
        {
            if (shrinkageLambda < 0.0 || shrinkageLambda > 1.0 || double.IsNaN(shrinkageLambda))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(shrinkageLambda),
                    shrinkageLambda,
                    "ShrinkageLambda must be in the inclusive range [0, 1]."
                );
            }

            if (minColdStartExamples < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minColdStartExamples),
                    minColdStartExamples,
                    "MinColdStartExamples must be non-negative."
                );
            }
        }

        /// <summary>The shrinkage weight on the leaf estimate; in the range <c>[0, 1]</c>.</summary>
        [JsonProperty]
        public double ShrinkageLambda { get; private set; }

        /// <summary>The minimum examples under the parent before the blend is applied.</summary>
        [JsonProperty]
        public int MinColdStartExamples { get; private set; }

        /// <summary>The wrapped child-segment-keyed classifier group.</summary>
        [JsonProperty]
        public BayesianClassifierGroup Group
        {
            get => _group;
            private set => _group = value ?? new BayesianClassifierGroup();
        }

        // Re-applies the construction invariants after JSON deserialization so a tampered or
        // malformed document fails fast rather than producing an out-of-range classifier.
        [System.Runtime.Serialization.OnDeserialized]
        internal void OnDeserialized(System.Runtime.Serialization.StreamingContext context)
        {
            ValidateInvariants(ShrinkageLambda, MinColdStartExamples);
        }

        /// <summary>The total number of training examples across all children of this parent.</summary>
        public int TotalExamples => _group.TotalEmailCount;

        /// <summary>The direct child segments registered under this parent.</summary>
        public IReadOnlyCollection<string> ChildSegments => _group.Classifiers.Keys.ToArray();

        /// <summary>
        /// Indicates whether the cold-start fallback is active (total examples below the
        /// configured threshold).
        /// </summary>
        public bool IsColdStart => TotalExamples < MinColdStartExamples;

        /// <summary>
        /// Trains the child <paramref name="childSegment"/> on the supplied tokens. Registers the
        /// child if it does not already exist; sibling children are unaffected.
        /// </summary>
        /// <param name="childSegment">The direct child segment.</param>
        /// <param name="tokens">The tokenized email content.</param>
        /// <param name="emailCount">The number of emails represented by the tokens.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="childSegment"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokens"/> is null.</exception>
        public void Train(string childSegment, IEnumerable<string> tokens, int emailCount)
        {
            RequireChildSegment(childSegment);
            if (tokens is null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            _group.Train(childSegment, tokens, emailCount);
        }

        /// <summary>
        /// Reverses prior training for <paramref name="childSegment"/>, decrementing its counts and
        /// the shared parent corpus. Sibling children are unaffected.
        /// </summary>
        /// <param name="childSegment">The direct child segment.</param>
        /// <param name="tokens">The tokenized email content previously trained.</param>
        /// <param name="emailCount">The number of emails represented by the tokens.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="childSegment"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokens"/> is null.</exception>
        public void UnTrain(string childSegment, IEnumerable<string> tokens, int emailCount)
        {
            RequireChildSegment(childSegment);
            if (tokens is null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            _group.UnTrain(childSegment, tokens, emailCount);
        }

        /// <summary>
        /// Computes <c>P(child | parent, tokens)</c> for every registered child, normalized to sum
        /// to one across the children. Uses the shrinkage blend when at or above
        /// <see cref="MinColdStartExamples"/>, and unsmoothed Naive Bayes below it.
        /// </summary>
        /// <param name="tokens">The query tokens.</param>
        /// <returns>
        /// A dictionary from child segment to normalized conditional probability. Empty when no
        /// children are registered.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tokens"/> is null.</exception>
        public IReadOnlyDictionary<string, double> ScoreChildren(IEnumerable<string> tokens)
        {
            if (tokens is null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            var tokenIncidence = GroupAndCount(tokens);
            var children = _group.Classifiers;
            if (children.IsEmpty)
            {
                return new Dictionary<string, double>();
            }

            var useBlend = !IsColdStart;
            var parentTokenTotal = _group.SharedTokenBase.TokenFrequency.Values.Sum();
            var vocabulary = _group.SharedTokenBase.TokenFrequency.Count;

            // Compute an unnormalized log-score per child, then convert to normalized linear
            // probabilities via a numerically stable softmax over the children.
            var logScores = new Dictionary<string, double>(children.Count);
            foreach (var entry in children)
            {
                logScores[entry.Key] = ChildLogScore(
                    entry.Value,
                    tokenIncidence,
                    useBlend,
                    parentTokenTotal,
                    vocabulary
                );
            }

            return Normalize(logScores);
        }

        // Log P(c) + sum over query tokens of incidence * log P_smoothed(t | c).
        private double ChildLogScore(
            BayesianClassifierShared child,
            IReadOnlyDictionary<string, int> tokenIncidence,
            bool useBlend,
            int parentTokenTotal,
            int vocabulary
        )
        {
            var childTokenTotal = child.Match.TokenFrequency.Values.Sum();
            var childEmailCount = Math.Max(child.MatchEmailCount, 0);
            var prior =
                (childEmailCount + LaplaceAlpha)
                / (TotalExamples + LaplaceAlpha * Math.Max(_group.Classifiers.Count, 1));
            var logScore = Math.Log(prior);

            foreach (var kvp in tokenIncidence)
            {
                var leaf = LaplaceProbability(
                    child.Match.TokenFrequency.TryGetValue(kvp.Key, out var childCount)
                        ? childCount
                        : 0,
                    childTokenTotal,
                    vocabulary
                );

                double blended;
                if (useBlend)
                {
                    var parent = LaplaceProbability(
                        _group.SharedTokenBase.TokenFrequency.TryGetValue(
                            kvp.Key,
                            out var parentCount
                        )
                            ? parentCount
                            : 0,
                        parentTokenTotal,
                        vocabulary
                    );
                    blended = (ShrinkageLambda * leaf) + ((1.0 - ShrinkageLambda) * parent);
                }
                else
                {
                    // Cold-start: unsmoothed Naive Bayes uses the leaf estimate only.
                    blended = leaf;
                }

                logScore += kvp.Value * Math.Log(blended);
            }

            return logScore;
        }

        private static double LaplaceProbability(int count, int total, int vocabulary)
        {
            // Add-one smoothing keeps every per-token probability strictly positive.
            return (count + LaplaceAlpha) / (total + (LaplaceAlpha * Math.Max(vocabulary, 1)));
        }

        private static IReadOnlyDictionary<string, double> Normalize(
            Dictionary<string, double> logScores
        )
        {
            var max = logScores.Values.Max();
            var exponentials = logScores.ToDictionary(
                kvp => kvp.Key,
                kvp => Math.Exp(kvp.Value - max)
            );
            var sum = exponentials.Values.Sum();
            if (sum <= 0.0)
            {
                var uniform = 1.0 / exponentials.Count;
                return exponentials.ToDictionary(kvp => kvp.Key, _ => uniform);
            }

            return exponentials.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / sum);
        }

        private static Dictionary<string, int> GroupAndCount(IEnumerable<string> tokens)
        {
            var incidence = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var token in tokens)
            {
                if (token is null)
                {
                    continue;
                }

                incidence[token] = incidence.TryGetValue(token, out var count) ? count + 1 : 1;
            }

            return incidence;
        }

        private static void RequireChildSegment(string childSegment)
        {
            if (string.IsNullOrEmpty(childSegment))
            {
                throw new ArgumentException(
                    "Child segment must be a non-empty string.",
                    nameof(childSegment)
                );
            }
        }
    }
}

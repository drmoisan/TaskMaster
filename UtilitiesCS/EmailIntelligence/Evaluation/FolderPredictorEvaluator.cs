#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UtilitiesCS.EmailIntelligence.Bayesian;

namespace UtilitiesCS.EmailIntelligence.Evaluation
{
    /// <summary>
    /// Configuration for <see cref="FolderPredictorEvaluator"/>. Controls the deterministic
    /// train/test split. Validates its invariant on construction.
    /// </summary>
    public sealed class EvaluationConfig
    {
        /// <summary>
        /// Creates an evaluation configuration.
        /// </summary>
        /// <param name="trainFraction">
        /// Fraction of the corpus (by stable index order) used for training; the remainder is the
        /// test slice. Must be strictly between 0 and 1.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="trainFraction"/> is not strictly between 0 and 1.
        /// </exception>
        public EvaluationConfig(double trainFraction = 0.7)
        {
            if (trainFraction <= 0.0 || trainFraction >= 1.0 || double.IsNaN(trainFraction))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(trainFraction),
                    trainFraction,
                    "TrainFraction must be strictly between 0 and 1."
                );
            }

            TrainFraction = trainFraction;
        }

        /// <summary>Fraction of the corpus used for training (by stable index order).</summary>
        public double TrainFraction { get; }
    }

    /// <summary>
    /// Deterministic, time-sliced evaluation harness for an <see cref="IFolderPredictor"/>. The
    /// time order is proxied by the corpus array index because <see cref="MinedMailInfo"/> exposes
    /// no timestamp field: the first <c>TrainFraction</c> of the array (by index) is the train
    /// slice and the remainder is the test slice. A predictor built from the train slice classifies
    /// each test example; per-leaf precision/recall/F1, macro F1, and the abstention rate are then
    /// computed. All logic is pure and testable without Outlook COM or any I/O.
    /// </summary>
    public sealed class FolderPredictorEvaluator
    {
        private readonly IReadOnlyList<MinedMailInfo> _corpus;
        private readonly EvaluationConfig _config;
        private readonly Func<IReadOnlyList<MinedMailInfo>, IFolderPredictor> _predictorFactory;

        /// <summary>
        /// Creates an evaluator over a fixed corpus.
        /// </summary>
        /// <param name="predictorFactory">
        /// Builds the <see cref="IFolderPredictor"/> under test from the train slice. The factory
        /// receives the train slice in stable index order and must return a trained predictor.
        /// </param>
        /// <param name="corpus">The mined corpus to split and evaluate; must not be null.</param>
        /// <param name="config">The evaluation configuration; must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public FolderPredictorEvaluator(
            Func<IReadOnlyList<MinedMailInfo>, IFolderPredictor> predictorFactory,
            MinedMailInfo[] corpus,
            EvaluationConfig config
        )
        {
            _predictorFactory =
                predictorFactory ?? throw new ArgumentNullException(nameof(predictorFactory));
            _corpus = corpus ?? throw new ArgumentNullException(nameof(corpus));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Computes the deterministic train/test boundary index using the corpus index as the time
        /// proxy. Examples before the boundary are training; the rest are test.
        /// </summary>
        /// <returns>The exclusive upper index of the train slice.</returns>
        public int ComputeTrainBoundary()
        {
            // Floor keeps the split deterministic and reproducible for a given corpus length.
            var boundary = (int)Math.Floor(_corpus.Count * _config.TrainFraction);

            // Guarantee a non-empty train and test slice whenever the corpus has at least two items.
            if (_corpus.Count >= 2)
            {
                boundary = Math.Max(1, Math.Min(boundary, _corpus.Count - 1));
            }

            return boundary;
        }

        /// <summary>
        /// Runs the evaluation: splits the corpus by index, builds the predictor from the train
        /// slice, classifies each test example, and returns the aggregated metrics.
        /// </summary>
        /// <returns>The per-leaf and macro metrics plus the abstention rate.</returns>
        public EvaluationResult Evaluate()
        {
            var boundary = ComputeTrainBoundary();
            var train = _corpus.Take(boundary).ToList();
            var test = _corpus.Skip(boundary).ToList();

            var predictor = _predictorFactory(train);

            // Per-leaf confusion counts. Every observed true class and predicted class is a key.
            var truePositives = new Dictionary<string, int>(StringComparer.Ordinal);
            var falsePositives = new Dictionary<string, int>(StringComparer.Ordinal);
            var falseNegatives = new Dictionary<string, int>(StringComparer.Ordinal);
            var leaves = new HashSet<string>(StringComparer.Ordinal);
            var abstentions = 0;

            foreach (var example in test)
            {
                var trueLeaf = example?.FolderInfo?.RelativePath;
                if (string.IsNullOrEmpty(trueLeaf))
                {
                    continue;
                }

                leaves.Add(trueLeaf!);
                var predicted = PredictTop(predictor, example!.Tokens!);

                if (predicted is null)
                {
                    // Abstention: counts as a false negative for the true class and a true negative
                    // for every other class. It never increments any class's false positives.
                    Increment(falseNegatives, trueLeaf!);
                    abstentions++;
                    continue;
                }

                leaves.Add(predicted);
                if (string.Equals(predicted, trueLeaf, StringComparison.Ordinal))
                {
                    Increment(truePositives, trueLeaf!);
                }
                else
                {
                    // A wrong, non-abstaining prediction is a false positive for the predicted class
                    // and a false negative for the true class.
                    Increment(falsePositives, predicted);
                    Increment(falseNegatives, trueLeaf!);
                }
            }

            var perLeaf = new Dictionary<string, LeafMetrics>(StringComparer.Ordinal);
            var f1Sum = 0.0;
            foreach (var leaf in leaves)
            {
                var tp = Get(truePositives, leaf);
                var fp = Get(falsePositives, leaf);
                var fn = Get(falseNegatives, leaf);

                var precision = tp + fp == 0 ? 0.0 : (double)tp / (tp + fp);
                var recall = tp + fn == 0 ? 0.0 : (double)tp / (tp + fn);
                var f1 =
                    precision + recall == 0.0
                        ? 0.0
                        : 2.0 * precision * recall / (precision + recall);

                perLeaf[leaf] = new LeafMetrics(leaf, precision, recall, f1);
                f1Sum += f1;
            }

            var macroF1 = perLeaf.Count == 0 ? 0.0 : f1Sum / perLeaf.Count;
            var abstentionRate = test.Count == 0 ? 0.0 : (double)abstentions / test.Count;

            return new EvaluationResult(perLeaf, macroF1, abstentionRate, test.Count);
        }

        // Returns the highest-probability predicted leaf, or null when the predictor abstains
        // (an empty classification result).
        private static string? PredictTop(IFolderPredictor predictor, string[] tokens)
        {
            var top = predictor.Classify(tokens ?? Array.Empty<string>()).Take(1).ToArray();
            return top.Length == 0 ? null : top[0].Class;
        }

        private static void Increment(Dictionary<string, int> counts, string key)
        {
            counts.TryGetValue(key, out var current);
            counts[key] = current + 1;
        }

        private static int Get(Dictionary<string, int> counts, string key)
        {
            counts.TryGetValue(key, out var value);
            return value;
        }
    }
}

#nullable enable
using System.Collections.Generic;

namespace UtilitiesCS.EmailIntelligence.Evaluation
{
    /// <summary>
    /// Per-leaf precision, recall, and F1 for a single folder class produced by the evaluation
    /// harness. Immutable value object.
    /// </summary>
    public sealed class LeafMetrics
    {
        /// <summary>Creates a per-leaf metric triple.</summary>
        /// <param name="leaf">The folder leaf (relative path) the metrics describe.</param>
        /// <param name="precision">Precision for the leaf in <c>[0, 1]</c>.</param>
        /// <param name="recall">Recall for the leaf in <c>[0, 1]</c>.</param>
        /// <param name="f1">F1 score for the leaf in <c>[0, 1]</c>.</param>
        public LeafMetrics(string leaf, double precision, double recall, double f1)
        {
            Leaf = leaf;
            Precision = precision;
            Recall = recall;
            F1 = f1;
        }

        /// <summary>The folder leaf (relative path) these metrics describe.</summary>
        public string Leaf { get; }

        /// <summary>Precision: true positives / (true positives + false positives).</summary>
        public double Precision { get; }

        /// <summary>Recall: true positives / (true positives + false negatives).</summary>
        public double Recall { get; }

        /// <summary>F1: harmonic mean of precision and recall.</summary>
        public double F1 { get; }
    }

    /// <summary>
    /// Result of a time-sliced folder-prediction evaluation. Carries per-leaf precision/recall/F1,
    /// the macro-averaged F1 across observed leaves, and the abstention rate over the test slice.
    /// Immutable value object; pure data with no Outlook COM or I/O dependency.
    /// </summary>
    public sealed class EvaluationResult
    {
        /// <summary>Creates an evaluation result.</summary>
        /// <param name="perLeaf">Per-leaf metrics keyed by leaf relative path.</param>
        /// <param name="macroF1">Macro-averaged F1 across the observed true leaves.</param>
        /// <param name="abstentionRate">Fraction of test examples for which the predictor abstained.</param>
        /// <param name="testCount">Number of test examples evaluated.</param>
        public EvaluationResult(
            IReadOnlyDictionary<string, LeafMetrics> perLeaf,
            double macroF1,
            double abstentionRate,
            int testCount
        )
        {
            PerLeaf = perLeaf;
            MacroF1 = macroF1;
            AbstentionRate = abstentionRate;
            TestCount = testCount;
        }

        /// <summary>Per-leaf precision/recall/F1 keyed by leaf relative path.</summary>
        public IReadOnlyDictionary<string, LeafMetrics> PerLeaf { get; }

        /// <summary>Macro-averaged F1 across the observed true leaves in <c>[0, 1]</c>.</summary>
        public double MacroF1 { get; }

        /// <summary>Fraction of test examples for which the predictor abstained, in <c>[0, 1]</c>.</summary>
        public double AbstentionRate { get; }

        /// <summary>Number of test examples evaluated.</summary>
        public int TestCount { get; }
    }
}

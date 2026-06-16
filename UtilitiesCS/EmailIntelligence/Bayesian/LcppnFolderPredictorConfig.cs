using System;
using Newtonsoft.Json;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    /// <summary>
    /// Configuration for the hierarchy-aware LCPPN folder predictor. Holds the feature flag and
    /// the four scoring/descent parameters with their documented defaults. The type is
    /// serializable by Newtonsoft.Json and validates its invariants when
    /// <see cref="Validate"/> is called (also invoked automatically by the construction helper
    /// <see cref="Create"/>).
    /// </summary>
    public sealed class LcppnFolderPredictorConfig
    {
        /// <summary>
        /// Selects the LCPPN predictor when true. This class-level default stays false so configs
        /// constructed directly (including AC13 flag-off tests) keep flat behavior. The production
        /// default is NOT sourced here: it is resolved at <c>OlFolderClassifierGroup</c>
        /// construction from the persisted <see cref="IAppAutoFileObjects.UseLcppnPredictor"/>
        /// setting (default ON), so flipping this class default would mask OFF in tests that
        /// construct the config directly.
        /// </summary>
        [JsonProperty]
        public bool UseLcppnPredictor { get; set; }

        /// <summary>Beam width for path descent. Default 3. Must be at least 1.</summary>
        [JsonProperty]
        public int BeamWidth { get; set; } = 3;

        /// <summary>
        /// Abstention threshold on the path-product probability. Default 0.5. Must be strictly
        /// between 0 and 1.
        /// </summary>
        [JsonProperty]
        public double MinimumPathProbability { get; set; } = 0.5;

        /// <summary>
        /// Weight on the leaf estimate in the shrinkage blend. Default 0.7. Must be in the
        /// inclusive range <c>[0, 1]</c>.
        /// </summary>
        [JsonProperty]
        public double ShrinkageLambda { get; set; } = 0.7;

        /// <summary>
        /// Minimum total examples under a parent before the shrinkage blend is applied. Default 5.
        /// Must be non-negative.
        /// </summary>
        [JsonProperty]
        public int MinColdStartExamples { get; set; } = 5;

        /// <summary>
        /// Creates a validated configuration. Equivalent to constructing the object and calling
        /// <see cref="Validate"/>.
        /// </summary>
        /// <returns>The validated configuration.</returns>
        public static LcppnFolderPredictorConfig Create(
            bool useLcppnPredictor = false,
            int beamWidth = 3,
            double minimumPathProbability = 0.5,
            double shrinkageLambda = 0.7,
            int minColdStartExamples = 5
        )
        {
            var config = new LcppnFolderPredictorConfig
            {
                UseLcppnPredictor = useLcppnPredictor,
                BeamWidth = beamWidth,
                MinimumPathProbability = minimumPathProbability,
                ShrinkageLambda = shrinkageLambda,
                MinColdStartExamples = minColdStartExamples,
            };
            config.Validate();
            return config;
        }

        /// <summary>
        /// Validates the configuration invariants, failing fast with an explicit exception on
        /// violation: <see cref="BeamWidth"/> &gt;= 1, 0 &lt; <see cref="MinimumPathProbability"/> &lt; 1,
        /// 0 &lt;= <see cref="ShrinkageLambda"/> &lt;= 1, and <see cref="MinColdStartExamples"/> &gt;= 0.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any invariant is violated.</exception>
        public void Validate()
        {
            if (BeamWidth < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(BeamWidth),
                    BeamWidth,
                    "BeamWidth must be at least 1."
                );
            }

            if (
                MinimumPathProbability <= 0.0
                || MinimumPathProbability >= 1.0
                || double.IsNaN(MinimumPathProbability)
            )
            {
                throw new ArgumentOutOfRangeException(
                    nameof(MinimumPathProbability),
                    MinimumPathProbability,
                    "MinimumPathProbability must be strictly between 0 and 1."
                );
            }

            if (ShrinkageLambda < 0.0 || ShrinkageLambda > 1.0 || double.IsNaN(ShrinkageLambda))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(ShrinkageLambda),
                    ShrinkageLambda,
                    "ShrinkageLambda must be in the inclusive range [0, 1]."
                );
            }

            if (MinColdStartExamples < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(MinColdStartExamples),
                    MinColdStartExamples,
                    "MinColdStartExamples must be non-negative."
                );
            }
        }
    }
}

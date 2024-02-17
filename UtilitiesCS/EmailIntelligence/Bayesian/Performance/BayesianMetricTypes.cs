using System.Windows.Forms.DataVisualization.Charting;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace UtilitiesCS.EmailIntelligence.Bayesian.Performance
{
    public record TestOutcome()
    {
        public string Actual { get; set; }
        public string Predicted { get; set; }
        public int SourceIndex { get; set; }
    }

    public record VerboseTestOutcome()
    {
        public string Actual { get; set; }
        public string Predicted { get; set; }
        public MinedMailInfo Source { get; set; }
        public int SourceIndex { get; set; }
        public (string Token, double TokenProbability)[] Drivers { get; set; }
        public double Probability { get; set; }
    }

    public record ClassCounts()
    {
        public string Class { get; set; }
        public int TP { get; set; }
        public int FP { get; set; }
        public int FN { get; set; }
        public int TN { get; set; }
    }

    public class VerboseClassCounts() 
    {
        public string Class { get; set; }
        public int TP { get; set; }
        public int FP { get; set; }
        public int FN { get; set; }
        public int TN { get; set; }
        public int Errors { get; set; }

        [JsonIgnore]
        public Dictionary<VerboseTestOutcome, string> VerboseOutcomes { get; set; }

        [JsonProperty]
        private IEnumerable<KeyValuePair<VerboseTestOutcome, string>> VerboseOutcomesJson
        {
            get => VerboseOutcomes?.ToArray() ?? [];
            set => VerboseOutcomes = value?.ToDictionary();
        }
    
    }

    //public record VerboseClassCounts()
    //{
    //    public string Class { get; set; }
    //    public int TPCount { get; set; }
    //    public int FPCount { get; set; }
    //    public int FNCount { get; set; }
    //    public int TNCount { get; set; }
    //    public VerboseTestOutcome[] TPDetails { get; set; }
    //    public VerboseTestOutcome[] FPDetails { get; set; }
    //    public VerboseTestOutcome[] FNDetails { get; set; }
    //    public VerboseTestOutcome[] TNDetails { get; set; }
    //}

    public record TestScores()
    {
        public string Class { get; set; }
        public int TP { get; set; }
        public int FP { get; set; }
        public int FN { get; set; }
        public int TN { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1 { get; set; }
    }

    //public record VerboseTestScores()
    //{
    //    public string Class { get; set; }
    //    public int TP { get; set; }
    //    public int FP { get; set; }
    //    public int FN { get; set; }
    //    public int TN { get; set; }
    //    public double Precision { get; set; }
    //    public double Recall { get; set; }
    //    public double F1 { get; set; }
    //    public VerboseTestOutcome[] TPDetails { get; set; }
    //    public VerboseTestOutcome[] FPDetails { get; set; }
    //    public VerboseTestOutcome[] FNDetails { get; set; }
    //    public VerboseTestOutcome[] TNDetails { get; set; }
    //}

    public class VerboseTestScores
    {
        public string Class { get; set; }
        public int TP { get; set; }
        public int FP { get; set; }
        public int FN { get; set; }
        public int TN { get; set; }
        public int Errors { get; set; }
        public double Precision { get; set; }
        public double Recall { get; set; }
        public double F1 { get; set; }

        [JsonIgnore]
        public Dictionary<VerboseTestOutcome, string> VerboseOutcomes { get; set; }

        [JsonProperty]
        private IEnumerable<KeyValuePair<VerboseTestOutcome, string>> VerboseOutcomesJson
        {
            get => VerboseOutcomes?.ToArray() ?? [];
            set => VerboseOutcomes = value?.ToDictionary();
        }

    }

    public record GroupedTestOutcome()
    {
        public string Actual { get; set; }
        public string Predicted { get; set; }
        public int Count { get; set; }
    }

    public record VerboseGroupedTestOutcome()
    {
        public string Actual { get; set; }
        public string Predicted { get; set; }
        public int Count { get; set; }
        public VerboseTestOutcome[] Details { get; set; }
    }

    public class ClassificationErrors
    {
        public ClassificationErrors() { }

        [JsonConstructor]
        public ClassificationErrors(
            string @class, 
            IEnumerable<KeyValuePair<VerboseTestOutcome, string>> verboseOutcomes, 
            int falsePositives, 
            int falseNegatives)
        {
            Class = @class;
            VerboseOutcomesJson = verboseOutcomes;
            FP = falsePositives;
            FN = falseNegatives;
        }

        public string Class { get; set; }
        
        [JsonIgnore]
        public Dictionary<VerboseTestOutcome, string> VerboseOutcomes { get; set; }

        [JsonProperty]
        private IEnumerable<KeyValuePair<VerboseTestOutcome, string>> VerboseOutcomesJson
        {
            get => VerboseOutcomes?.ToArray() ?? [];
            set => VerboseOutcomes = value?.ToDictionary();
        }

        public int FP { get; set; }
        public int FN { get; set; }
        public int TP { get; set; }
        public int TN { get; set; }
        public int Errors { get; set; }
    }

    public class VerboseOutcomeClass 
    { 

    }

    public record ThresholdMetric()
    {
        public double Threshold { get; set; }
        public double Precision { get; set; }
        public int PrecisionCount { get; set; }
        public double Recall { get; set; }
        public int RecallCount { get; set; }
        public double F1 { get; set; }
        public int F1Count { get; set; }
    }

    public record ThresholdMetrics()
    {
        public Series Precision { get; set; }
        public Series Recall { get; set; }
        public Series F1 { get; set; }
    }
    
}

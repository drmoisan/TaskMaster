using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.Test.EmailIntelligence.Bayesian
{
    internal static class SampleTestSets
    {
        internal const string alphabet = "abcdefghijklmnopqrstuvwxyz";

        internal static SubBayesianClassifier CreateBayesianClassifier()
        {
            return new SubBayesianClassifier();
        }

        internal static SubBayesianClassifier SetupClassifierScenario1()
        {
            var classifier = CreateBayesianClassifier();
            classifier.Tag = "folderA";
            classifier.Parent = new SubClassifierGroup
            {
                SharedTokenBase = new SubCorpus(new Dictionary<string, int>
                {
                    ["shared1"] = 6,
                    ["shared2"] = 4,
                    ["shared3"] = 2,
                    ["shared4"] = 6,
                    ["shared5"] = 4,
                    ["shared6"] = 1,
                    ["shared7"] = 50,
                    ["shared8"] = 40,
                    ["dedicated1"] = 6,
                    ["dedicated4"] = 6,
                    ["dedicated7"] = 8,
                    ["dedicated3"] = 1,
                    ["dedicated6"] = 1,
                    ["dedicated2"] = 4,
                    ["dedicated5"] = 4,
                    ["dedicated8"] = 20,
                }),
                TotalEmailCount = 163,
            };
            classifier.Prob = new ConcurrentDictionary<string, double>(
                Enumerable.Range(0, 26)
                .Select(i => new KeyValuePair<string, double>(
                alphabet[i].ToString(), i / (double)100 + 0.6)));

            classifier.Prob.LogProbabilities("Source token probability");

            classifier.Parent.SharedTokenBase.TokenFrequency.LogTokenFrequency("Shared tokens");

            return classifier;
        }

        internal static SubBayesianClassifier SetupClassifierScenario1A()
        {
            var classifier = CreateBayesianClassifier();
            classifier.Tag = "folderC";
            classifier.Parent = new SubClassifierGroup
            {
                SharedTokenBase = new SubCorpus(new Dictionary<string, int>
                {
                    ["shared1"] = 6,
                    ["shared2"] = 4,
                    ["shared3"] = 2,
                    ["shared4"] = 6,
                    ["shared5"] = 4,
                    ["shared6"] = 1,
                    ["shared7"] = 50,
                    ["shared8"] = 40,
                    ["dedicated1"] = 6,
                    ["dedicated4"] = 6,
                    ["dedicated7"] = 8,
                    ["dedicated3"] = 1,
                    ["dedicated6"] = 1,
                    ["dedicated2"] = 4,
                    ["dedicated5"] = 4,
                    ["dedicated8"] = 20,
                }),
                TotalEmailCount = 163,

            };
            classifier.Prob = new ConcurrentDictionary<string, double>
            {
                ["shared1"] = 0.714285714285714,
                ["shared2"] = 0.142857142857143,
                ["shared7"] = 0.333333333333333,
                ["shared8"] = 0.333333333333333,
                ["dedicated7"] = 0.99980,
                ["dedicated8"] = 0.99990
            };

            classifier.Prob.LogProbabilities("Source token probability");
            classifier.Parent.SharedTokenBase.TokenFrequency.LogTokenFrequency("Shared tokens");
            return classifier;
        }

        internal static SubBayesianClassifier GetClassifier3a()
        {
            var classifier = CreateBayesianClassifier();
            classifier.Parent = new SubClassifierGroup
            {
                SharedTokenBase = new SubCorpus(new Dictionary<string, int>
                {
                    ["token03"] = 4,
                    ["token04"] = 4,
                    ["token05"] = 12,
                    ["token06"] = 12,
                    ["token07"] = 4
                }),
                TotalEmailCount = 9
            };
            classifier.Tag = "folderA";
            return classifier;
        }

        internal static SubBayesianClassifier GetClassifier3b()
        {
            var classifier = CreateBayesianClassifier();
            classifier.Match = new SubCorpus(new Dictionary<string, int>
            {
                ["token00"] = 4,
                ["token01"] = 4,
                ["token02"] = 12,
                ["token03"] = 12,
                ["token04"] = 4
            });
            classifier.MatchEmailCount = 7;
            classifier.Parent = new SubClassifierGroup
            {
                SharedTokenBase = new SubCorpus(new Dictionary<string, int>
                {
                    ["token00"] = 4,
                    ["token01"] = 4,
                    ["token02"] = 12,
                    ["token03"] = 16,
                    ["token04"] = 8,
                    ["token05"] = 12,
                    ["token06"] = 12,
                    ["token07"] = 4,
                }),
                TotalEmailCount = 16
            };
            classifier.Prob = new ConcurrentDictionary<string, double>(new Dictionary<string, double>
            {
                ["token00"] = 0.94944,
                ["token01"] = 0.94944,
                ["token02"] = 0.98193,
                ["token03"] = 0.78607,
                ["token04"] = 0.55917
                //["token00"] = 0.99990,
                //["token02"] = 0.99990,
                //["token03"] = 0.52941,
                //["token04"] = 0.39130,
            });
            classifier.Tag = "folderA";
            return classifier;
        }

        internal static SubBayesianClassifier GetClassifier3c()
        {
            var classifier = CreateBayesianClassifier();
            classifier.Match = new SubCorpus(new Dictionary<string, int>
            {
                ["token00"] = 5,
                ["token01"] = 4,
                ["token02"] = 12,
                ["token03"] = 12,
                ["token04"] = 4,
                ["token08"] = 4,
                ["token09"] = 5,
                ["token10"] = 11
            });
            classifier.MatchEmailCount = 8;
            classifier.Parent = new SubClassifierGroup
            {
                SharedTokenBase = new SubCorpus(new Dictionary<string, int>
                {
                    ["token00"] = 5,
                    ["token01"] = 4,
                    ["token02"] = 12,
                    ["token03"] = 16,
                    ["token04"] = 8,
                    ["token05"] = 12,
                    ["token06"] = 12,
                    ["token07"] = 4,
                    ["token08"] = 4,
                    ["token09"] = 5,
                    ["token10"] = 11
                }),
                TotalEmailCount = 17
            };
            classifier.Prob = new ConcurrentDictionary<string, double>(new Dictionary<string, double>
            {
                ["token00"] = 0.95872,
                ["token01"] = 0.94944,
                ["token02"] = 0.98193,
                ["token03"] = 0.76400,
                ["token04"] = 0.52785,
                ["token08"] = 0.94944,
                ["token09"] = 0.95872,
                ["token10"] = 0.98035
                //["token00"] = 0.99980,
                //["token02"] = 0.99990,
                //["token03"] = 0.52941,
                //["token04"] = 0.36000,
                //["token09"] = 0.99980,
                //["token10"] = 0.99990
            });
            classifier.Tag = "folderA";
            return classifier;
        }

        internal static SubBayesianClassifier GetClassifierTemplate()
        {
            var classifier = CreateBayesianClassifier();
            classifier.Match = new SubCorpus(new Dictionary<string, int>
            {

            });
            classifier.MatchEmailCount = 0;
            classifier.Parent = new SubClassifierGroup
            {
                SharedTokenBase = new SubCorpus(new Dictionary<string, int>
                {

                }),
                TotalEmailCount = 0
            };
            classifier.Prob = new ConcurrentDictionary<string, double>(new Dictionary<string, double>
            {

            });
            classifier.Tag = "";
            return classifier;
        }

    }
}

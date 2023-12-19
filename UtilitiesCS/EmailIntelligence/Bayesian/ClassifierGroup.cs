using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    public class ClassifierGroup
    {
        public ClassifierGroup()
        {
            _classifiers = [];
        }

        public ConcurrentDictionary<string, BayesianClassifier> Classifiers { get => _classifiers; protected set => _classifiers = value; }
        private ConcurrentDictionary<string, BayesianClassifier> _classifiers;

        public void ForceClassifierUpdate(string tag, IEnumerable<string> positiveTokens, IEnumerable<string> negativeTokens)
        {
            _classifiers[tag] = new BayesianClassifier(tag, positiveTokens, negativeTokens);
        }

        public void AddOrUpdateClassifier(string tag, IEnumerable<string> positiveTokens, IEnumerable<string> negativeTokens)
        {
            _classifiers.GetOrAdd(tag, new BayesianClassifier(tag)).AddTokens(positiveTokens, negativeTokens);
        }

        public IOrderedEnumerable<KeyValuePair<string, double>> Classify(object source)
        {
            return this.Classify(_tokenizer(source));
        }

        public IOrderedEnumerable<KeyValuePair<string, double>> Classify(IEnumerable<string> tokens)
        {
            var results = Classifiers.Select(
                classifier => new KeyValuePair<string, double>(
                    classifier.Key, classifier.Value.CalculateProbability(tokens)))
                .OrderByDescending(x => x.Value);
            return results;
        }

        [JsonIgnore]
        public Func<object, IEnumerable<string>> Tokenizer { get => _tokenizer; set => _tokenizer = value; }
        private Func<object, IEnumerable<string>> _tokenizer;

    }
}

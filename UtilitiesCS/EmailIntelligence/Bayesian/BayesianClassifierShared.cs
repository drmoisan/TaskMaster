using Fizzler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    /// <summary>
    /// Naive Baysiam Spam Filter.  Basically, an implementation of this:
    /// http://www.paulgraham.com/spam.html
    /// </summary>
    public class BayesianClassifierShared
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public BayesianClassifierShared() { }

        public BayesianClassifierShared(string tag)
        {
            _tag = tag;
            _prob = new ConcurrentDictionary<string, double>();
            _match = new Corpus();
        }

        private BayesianClassifierShared(string tag, Corpus match, BayesianClassifierGroup parent)
        {
            _tag = tag;
            _match = match;
            Parent = parent;
        }

        public static BayesianClassifierShared FromTokenBase(
            BayesianClassifierGroup parent,
            string tag,
            IEnumerable<string> matchTokens)
        {

            var match = new Corpus(matchTokens);
            var notMatch = parent.SharedTokenBase - match;
            var classifier = new BayesianClassifierShared(tag, match, parent);

            return classifier;
        }

        public static async Task<BayesianClassifierShared> FromTokenBaseAsync(
            BayesianClassifierGroup parent,
            string tag,
            IEnumerable<string> matchTokens,
            CancellationToken token)
        {
            var classifier = new BayesianClassifierShared();
            //await Task.Factory.StartNew(
            await Task.Run(
                () =>
                {
                    var match = new Corpus(matchTokens);
                    PrintTokenFrequency(match.TokenFrequency, "Match Token Frequency");

                    var (notMatchFiltered, matchFiltered) = Corpus.SubtractFilter(
                        parent.SharedTokenBase,
                        match,
                        classifier.Knobs.NotMatchTokenWeight,
                        classifier.Knobs.MinCountForInclusion);

                    PrintTokenFrequency(matchFiltered.TokenFrequency, "Filtered Match Token Frequency");
                    PrintTokenFrequency(notMatchFiltered.TokenFrequency, "Filtered Not Match Token Frequency");

                    var sharedNotMatchCount = notMatchFiltered.TokenFrequency.Values.Sum();
                    var matchCount = matchFiltered.TokenFrequency.Values.Sum();

                    classifier.Tag = tag;
                    var dedicatedNotMatchCount = parent.DedicatedTokens
                                                       .Where(x =>
                                                       (!match.TokenFrequency.ContainsKey(x.Value.Token)) &&
                                                       (x.Value.Count * classifier.Knobs.NotMatchTokenWeight >
                                                       classifier.Knobs.MinCountForInclusion))
                                                       .Sum(x => x.Value.Count);

                    classifier._matchEmailCount = matchCount;
                    

                    Console.WriteLine($"Filtered Shared Match Count: {classifier._matchEmailCount}");
                    
                    classifier.Match = match;
                    classifier.Parent = parent;
                    classifier._prob = new ConcurrentDictionary<string, double>();
                    classifier.Match.TokenFrequency.Keys.ForEach(classifier.CacheProbability);
                    //parent.SharedTokenBase.TokenFrequency.Keys.ForEach(classifier.UpdateProbabilityShared);
                },
                token);

            return classifier;
        }

        #endregion Constructors

        #region private fields

        private static void PrintTokenFrequency(IDictionary<string, int> dict, string title)
        {
            string text = dict.ToFormattedText(
                (key) => key,
                (value) => value.ToString("N0"),
                headers: ["Token", "Count"],
                justifications: [Enums.Justification.Left, Enums.Justification.Right],
                title: title);
            Console.WriteLine(text);
        }

        private static void PrintProbability(IDictionary<string, double> dict, string title)
        {
            string text = dict.ToFormattedText(
                (key) => key,
                (value) => value.ToString("N5"),
                headers: ["Token", "Prob"],
                justifications: [Enums.Justification.Left, Enums.Justification.Right],
                title: title);
            Console.WriteLine(text);
        }

        #endregion private fields

        #region public properties

        [JsonIgnore]
        public bool Loaded { get => _loaded; private set => _loaded = value; }
        private bool _loaded = false;

        /// <summary>
        /// A list of words that show tend to show up in Spam text
        /// </summary>
        [JsonProperty]
        public Corpus Match { get => _match; protected set => _match = value; }
        protected Corpus _match;

        [JsonProperty]
        public int MatchEmailCount { get => _matchEmailCount; set => _matchEmailCount = value; }
        private int _matchEmailCount;
                
        /// <summary>
        /// A list of probabilities that the given word might appear in a Spam text
        /// </summary>
        [JsonProperty]
        public ConcurrentDictionary<string, double> Prob { get => _prob; private set => _prob = value; }
        protected ConcurrentDictionary<string, double> _prob;

        [JsonProperty]
        public BayesianClassifierGroup Parent { get => _parent; internal set => _parent = value; }
        protected BayesianClassifierGroup _parent;

        public string Tag { get => _tag; set => _tag = value; }
        private string _tag;

        #endregion public properties

        #region population

        public void Train(IDictionary<string, int> tokenFrequency, int emailCount)
        {
            var otherMatches = Match.TokenFrequency.Keys.Except(tokenFrequency.Keys);
            
            Interlocked.Add(ref _matchEmailCount, emailCount);
            tokenFrequency.ForEach(kvp => 
            { 
                var matchCount = Match.AddOrSumTokenValue(kvp.Key, kvp.Value);
                var tokenCount = Parent.SharedTokenBase.TokenFrequency.AddOrUpdate(kvp.Key, kvp.Value,
                    (sharedKey, existingValue) => existingValue + kvp.Value);
            });
        }

        public async Task TrainAsync(IDictionary<string, int> tokenFrequency, int emailCount, CancellationToken cancel)
        {
            await Task.Run(() => Train(tokenFrequency, emailCount), cancel);
        }

        internal protected virtual void CacheProbability(string token, int matchCount, int notMatchCount)
        {
            /*
			 * This is a direct implementation of Paul Graham's algorithm from
			 * http://www.paulgraham.com/spam.html
			 * 
			 *	(let ((g (* 2 (or (gethash word good) 0)))
			 *		  (b (or (gethash word bad) 0)))
			 *	   (unless (< (+ g b) 5)
			 *		 (max .01
			 *			  (min .99 (float (/ (min 1 (/ b nbad))
			 *								 (+ (min 1 (/ g ngood))   
			 *									(min 1 (/ b nbad)))))))))
			 */
            int m = matchCount;
            int nm = notMatchCount;

            double prob;

            if (nm * Knobs.NotMatchTokenWeight + m >= Knobs.MinCountForInclusion)
            {
                // special case for tokens that only appear in the match
                if (nm == 0)
                {
                    prob = (m > Knobs.CertainMatchCount) ? Knobs.CertainMatchScore : Knobs.LikelyMatchScore;
                }
                else
                {
                    double matchFactor = Math.Min(1, (double)m / (double)MatchEmailCount);

                    double notMatchfactor = Math.Min(1, (double)nm * Knobs.NotMatchTokenWeight / (double)(Parent.TotalEmailCount - MatchEmailCount));

                    prob = Math.Max(Knobs.MinScore,
                        Math.Min(Knobs.MaxScore,
                        matchFactor / (notMatchfactor + matchFactor)));
                }
                Prob[token] = prob;
            }
            else
            {
                Prob.TryRemove(token, out _);
            }
        }

        internal protected virtual void CacheProbability(string token)
        {
            /*
			 * This is a direct implementation of Paul Graham's algorithm from
			 * http://www.paulgraham.com/spam.html
			 * 
			 *	(let ((g (* 2 (or (gethash word good) 0)))
			 *		  (b (or (gethash word bad) 0)))
			 *	   (unless (< (+ g b) 5)
			 *		 (max .01
			 *			  (min .99 (float (/ (min 1 (/ b nbad))
			 *								 (+ (min 1 (/ g ngood))   
			 *									(min 1 (/ b nbad)))))))))
			 */
            int m = _match.TokenFrequency.TryGetValue(token, out int bCount) ? bCount : 0;
            int nm = Parent.SharedTokenBase.TokenFrequency
                .TryGetValue(token, out int gCount) ? Math.Max(gCount - m, 0) : 0;

            double prob;

            if (nm * Knobs.NotMatchTokenWeight + m >= Knobs.MinCountForInclusion)
            {
                // special case for tokens that only appear in the match
                if (nm == 0)
                {
                    prob = (m > Knobs.CertainMatchCount) ? Knobs.CertainMatchScore : Knobs.LikelyMatchScore;
                }
                else
                {
                    double matchFactor = Math.Min(1, (double)m / (double)MatchEmailCount);

                    double notMatchfactor = Math.Min(1, (double)nm * Knobs.NotMatchTokenWeight / (double)(Parent.TotalEmailCount - MatchEmailCount));
                
                    prob = Math.Max(Knobs.MinScore, 
                        Math.Min(Knobs.MaxScore, 
                        matchFactor / (notMatchfactor + matchFactor)));
                }
                Prob[token] = prob;
            }
            else
            {
                Prob.TryRemove(token, out _);
            }
        }

        #endregion population

        #region Serialization Helpers

        #endregion Serialization Helpers

        #region classifier testing
        public double GetMatchProbability(IDictionary<string, int> tokenIncidence)
        {
            var interestingList = GetInterestingList(tokenIncidence);

            var combined = CombineProbabilities(interestingList);

            return combined;
        }

        private Dictionary<string, int> GetNotMatchIncidence(IDictionary<string, int> tokenIncidence, string[] matchKeys)
        {
            return tokenIncidence
                            .Where(kvp => !matchKeys.Contains(kvp.Key))
                            .Where(kvp => Parent.SharedTokenBase.TokenFrequency.TryGetValue(kvp.Key, out var value)
                                && value * Knobs.NotMatchTokenWeight >= Knobs.MinCountForInclusion)
                            .ToDictionary();
        }

        private (string Token, double Prob, int Incidence)[] 
            MergeProbabilityIncidence(IDictionary<string, int> tokenIncidence)
        {
            return Prob.Select(x => (Token: x.Key, Prob: x.Value,
                            Incidence: tokenIncidence.TryGetValue(x.Key, out int count) ? count : 0))
                            .Where(x => x.Incidence != 0)
                            .ToArray();
        }

        /// <summary>
        /// Returns the probability that the supplied tokens are a _positive match with the Classifier
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public double GetMatchProbability(IEnumerable<string> tokens)
        {
            var probabilities = GetInterestingList(tokens);

            var text = probabilities.ToFormattedText(
                (key) => key,
                (value) => value.ToString("N4"),
                headers: ["Class", "Probability"],
                justifications: [Enums.Justification.Left, Enums.Justification.Right],
                title: "Probability List");
            Console.WriteLine(text);

            var combined = CombineProbabilities(probabilities);

            return combined;
        }

        public SortedList<string, double> GetInterestingList(IDictionary<string, int> tokenIncidence)
        {
            SortedList<string, double> interestingList = [];

            if (tokenIncidence is null)
            {
                logger.Debug($"Parameter {nameof(tokenIncidence)} is null. Returning empty list");
                return interestingList;
            }

            // Inner Merge of match probabilities and token incidence
            var matchIncidence = MergeProbabilityIncidence(tokenIncidence);
            
            var matchTokens = matchIncidence
                .Select(x =>
                {
                    // Convert token and probability to a key that can be sorted by "interestingness"
                    var interestingKey = (0.5 - Math.Abs(0.5 - x.Prob)).ToString(".00000") + x.Token;
                
                    // Add the key to the list the number of times it appears in the body
                    Enumerable.Range(0, x.Incidence)
                        .ForEach(i => interestingList.Add(interestingKey + i, x.Prob));

                    // Return the token to use in the notMatchIncidence
                    return x.Token;
                })
                .ToArray();
                        
            var notMatchIncidence = GetNotMatchIncidence(tokenIncidence, matchTokens);
                        
            notMatchIncidence.ForEach(x =>
            {
                var interestingKey = (0.5 - Math.Abs(0.5 - Knobs.MinScore)).ToString(".00000") + x.Key;
                Enumerable.Range(0, x.Value)
                    .ForEach(i => interestingList.Add(interestingKey + i, Knobs.MinScore));
            });
            
            return interestingList;
        }

        public SortedList<string, double> GetInterestingList(IEnumerable<string> tokens)
        {
            SortedList<string, double> probabilities = [];

            if (tokens is null)
            {
                logger.Debug($"Parameter {nameof(tokens)} is null. Returning empty list");
                return probabilities;
            }

            // Spin through every word in the body and look up its individual spam probability.
            // Keep the list in decending order of "Interestingness"
            int index = 0;
            foreach (var token in tokens)
            {
                if (_prob.TryGetValue(token, out double prob))
                {
                    // "interestingness" == how far our score is from 50%.  
                    // The crazy math below is building a string that lets us sort alphabetically by interestingness.
                    string key = (0.5 - Math.Abs(0.5 - prob)).ToString(".00000") + token + index++;
                    probabilities.Add(key, prob);
                }

                else if (Parent?.DedicatedTokens?.TryGetValue(token, out var dedicated) ?? false)
                {
                    if (dedicated.Count * Knobs.NotMatchTokenWeight >= Knobs.MinCountForInclusion)
                    {
                        prob = Knobs.MinScore;
                        string key = (0.5 - Math.Abs(0.5 - prob))
                            .ToString(".00000") + token + index++;
                        probabilities.Add(key, prob);
                    }
                }

                else if (Parent?.SharedTokenBase?.TokenFrequency.TryGetValue(token, out var count) ?? false)
                {
                    if (count * Knobs.NotMatchTokenWeight >= Knobs.MinCountForInclusion)
                    {
                        prob = Knobs.MinScore;
                        string key = (0.5 - Math.Abs(0.5 - prob))
                            .ToString(".00000") + token + index++;
                        probabilities.Add(key, prob);
                    }
                }
            }

            return probabilities;
        }

        public double CombineProbabilities(SortedList<string, double> probabilities)
        {
            /* Combine the 20 most interesting probabilities together into one.  
                         * The algorithm to do this is shown below and described here:
                         * http://www.paulgraham.com/naivebayes.html
                         * 
                         *				abc           
                         *	---------------------------
                         *	abc + (1 - a)(1 - b)(1 - c)
                         *
                         */

            double mult = 1;
            double comb = 1;
            //int index = 0;

            if (probabilities is null) { throw new ArgumentNullException(nameof(probabilities)); }

            if (probabilities.Count == 0)
            {
                return 0;
            }

            probabilities
                .Take(Knobs.InterestingWordCount)
                .ForEach(kvp =>
                {
                    mult *= kvp.Value;
                    comb *= (1 - kvp.Value);
                });

            var combined = mult / (mult + comb);
            return combined;

        }

        #endregion classifier testing

        #region knobs for dialing in performance

        /// <summary>
        /// These are constants used in the Bayesian algorithm, presented in a form that lets you monkey with them.
        /// </summary>
        public class KnobList
        {
            // Values in PG's original article:
            public int NotMatchTokenWeight = 2;             // 2
            public int MinTokenCount = 0;                   // 0
            public int MinCountForInclusion = 5;            // 5
            public double MinScore = 0.011;                 // 0.01
            public double MaxScore = 0.99;                  // 0.99
            public double LikelyMatchScore = 0.9998;        // 0.9998
            public double CertainMatchScore = 0.9999;       // 0.9999
            public int CertainMatchCount = 10;              // 10
            public int InterestingWordCount = 20;           // 15 (later changed to 20)
        }

        /// <summary>
        /// These are the knobs you can turn to dial in performance on the algorithm.
        /// Hopefully the names make a little bit of sense and you can find where
        /// they fit into the original algorithm.
        /// </summary>
        public KnobList Knobs
        {
            get { return _knobs; }
            set { _knobs = value; }
        }
        private KnobList _knobs = new();

        #endregion knobs for dialing in performance
    }
}

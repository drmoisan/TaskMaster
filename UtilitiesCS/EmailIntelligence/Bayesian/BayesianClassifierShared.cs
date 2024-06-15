using C;
using Fizzler;
using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using UtilitiesCS.HelperClasses;
using UtilitiesCS.Threading;
using static Microsoft.FSharp.Core.ByRefKinds;

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

        public BayesianClassifierShared(string tag, BayesianClassifierGroup parent)
        {
            _tag = tag;
            _prob = new ConcurrentDictionary<string, double>();
            _match = new Corpus();
            Parent = parent;
        }

        protected BayesianClassifierShared(string tag, Corpus match, BayesianClassifierGroup parent)
        {
            _tag = tag;
            _match = match;
            Parent = parent;
        }

        protected BayesianClassifierShared(BayesianClassifierShared classifierShared)
        {
            _tag = classifierShared.Tag;
            _prob = classifierShared.Prob;
            _match = classifierShared.Match;
            _matchEmailCount = classifierShared.MatchEmailCount;
            _parent = classifierShared.Parent;
        }

        public static async Task<BayesianClassifierShared> FromTokenBaseAsync(
            BayesianClassifierGroup parent,
            string tag,
            IDictionary<string, int> matches,
            int matchEmailCount,
            bool addToParent,
            CancellationToken token)
        {
            var nullPositions = NullCheckParams(parent, tag, matches);
            if (nullPositions > 0) 
            { 
                List<string>paramNames = [];
                if ((nullPositions & 1) == 1) { paramNames.Add(nameof(parent)); }
                if ((nullPositions & 2) == 2) { paramNames.Add(nameof(tag)); }
                if ((nullPositions & 4) == 4) { paramNames.Add(nameof(matches)); }
                throw new ArgumentNullException($"Null parameters received: {paramNames.StringJoin(", ")}");
            }
            if (matchEmailCount < 1) { throw new ArgumentOutOfRangeException(nameof(matchEmailCount), 
                $"Parameter {nameof(matchEmailCount)} was {matchEmailCount} must be greater than 0");}
            
            BayesianClassifierShared classifier = null;
            
            await Task.Run(
                () =>
                {
                    // Call constructor
                    classifier = new BayesianClassifierShared(tag, new Corpus(matches), parent)
                    {
                        MatchEmailCount = matchEmailCount,
                    };
                    
                    // Update and cache probabilities
                    matches.ForEach(kvp =>
                    {
                        var tokenCount = addToParent ?
                            parent.SharedTokenBase.TokenFrequency.AddOrUpdate(kvp.Key, kvp.Value,
                                (sharedKey, existingValue) => existingValue + kvp.Value) :
                            parent.SharedTokenBase.TokenFrequency[kvp.Key];
                        classifier.UpdateProbabilitySb(kvp.Key, kvp.Value, tokenCount - kvp.Value);
                    });
                },
                token);

            return classifier;
        }

        private static int NullCheckParams(params object[] parameters)
        {
            int nullPositions = 0;
            Enumerable.Range(0, parameters.Count()).ForEach(i => nullPositions += parameters[i] is null ? (int)Math.Pow(2, i) : 0);
            return nullPositions;
            //return parameters.Any(p => p is null);
        }

        #endregion Constructors

        #region private fields and methods

        private ThreadSafeSingleShotGuard probRefresh = new ThreadSafeSingleShotGuard();

        #endregion private fields and methods

        #region public properties

        /// <summary>
        /// A list of words that show tend to show up in Spam text
        /// </summary>
        [JsonProperty]
        public Corpus Match { get => _match; protected set => _match = value; }
        protected Corpus _match = new();

        [JsonProperty]
        public int MatchEmailCount { get => _matchEmailCount; set => _matchEmailCount = value; }
        private int _matchEmailCount;
                
        /// <summary>
        /// A list of probabilities that the given word might appear in a Spam text
        /// </summary>
        [JsonProperty]
        public ConcurrentDictionary<string, double> Prob { get => _prob; private set => _prob = value; }
        protected ConcurrentDictionary<string, double> _prob = new();
        
        [JsonProperty]
        public BayesianClassifierGroup Parent { get => _parent; internal set => _parent = value; }
        protected BayesianClassifierGroup _parent;

        public string Tag { get => _tag; set => _tag = value; }
        private string _tag;

        #endregion public properties

        #region Public Model Training Methods

        public void Train(IDictionary<string, int> tokenFrequency, int emailCount)
        {
            var otherMatches = Match.TokenFrequency.Keys.Except(tokenFrequency.Keys);
            
            Interlocked.Add(ref _matchEmailCount, emailCount);
            Parent.AddToEmailCount(emailCount);
            tokenFrequency.ForEach(kvp => 
            { 
                var matchCount = Match.AddOrSumTokenValue(kvp.Key, kvp.Value);
                var tokenCount = Parent.SharedTokenBase.TokenFrequency.AddOrUpdate(kvp.Key, kvp.Value,
                    (sharedKey, existingValue) => existingValue + kvp.Value);
                UpdateProbabilitySb(kvp.Key, matchCount, tokenCount - matchCount);
            });

            otherMatches.ForEach(token => UpdateProbabilitySb(token));
        }

        public async Task TrainAsync(IDictionary<string, int> tokenFrequency, int emailCount, CancellationToken cancel)
        {
            await Task.Run(() => Train(tokenFrequency, emailCount), cancel);
        }

        internal protected virtual void UpdateProbability(string token, int matchCount, int notMatchCount)
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

        internal protected virtual void UpdateProbabilitySb(string token, int matchCount, int notMatchCount)
        {
            
            int m = matchCount;
            int nm = notMatchCount;

            double prob;

            double nham = Math.Max(1.0, Parent.TotalEmailCount - MatchEmailCount);
            double nspam = Math.Max(1, MatchEmailCount);

            var hamratio = nm / nham;
            var spamratio = m / nspam;

            prob = spamratio / (spamratio + hamratio);

            var S = Knobs.UnknownWordStrength;
            var StimesX = S * Knobs.UnknownWordProb;

            // Now do Robinson's Bayesian adjustment.
            //
            // s*x + n*p(w)
            // f(w) = --------------
            // s + n
            //
            // I find this easier to reason about like so (equivalent when
            // s != 0):
            //
            // x - p
            // p +  -------
            //       1 + n/s
            //
            // IOW, it moves p a fraction of the distance from p to x, and
            // less so the larger n is, or the smaller s is.

            var n = m + nm;
            prob = (StimesX + n * prob) / (S + n);

            Prob[token] = prob;
        }

        internal protected virtual double UpdateProbabilitySb(WordInfo record) 
        {
            int m = record.MatchCount;
            int nm = record.NotMatchCount;

            double prob;

            double nham = Math.Max(1.0, Parent.TotalEmailCount - MatchEmailCount);
            double nspam = Math.Max(1, MatchEmailCount);

            var hamratio = nm / nham;
            var spamratio = m / nspam;

            prob = spamratio / (spamratio + hamratio);

            var S = Knobs.UnknownWordStrength;
            var StimesX = S * Knobs.UnknownWordProb;

            var n = m + nm;
            prob = (StimesX + n * prob) / (S + n);

            return prob;
        }

        /// <summary>
        /// Compute, store, and return prob(msg is spam | msg contains word).
        /// This is the Graham calculation, but stripped of biases, and
        /// stripped of clamping into 0.01 thru 0.99.  The Bayesian
        /// adjustment following keeps them in a sane range, and one
        /// that naturally grows the more evidence there is to back up
        /// a probability.
        /// </summary>
        /// <param name="token"></param>
        internal protected virtual void UpdateProbabilitySb(string token)
        {
            int m = _match.TokenFrequency.TryGetValue(token, out int bCount) ? bCount : 0;
            int nm = Parent.SharedTokenBase.TokenFrequency
                .TryGetValue(token, out int gCount) ? Math.Max(gCount - m, 0) : 0;

            double prob;

            double nham = Math.Max(1.0, Parent.TotalEmailCount - MatchEmailCount);
            double nspam = Math.Max(1, MatchEmailCount);

            var hamratio = nm / nham;
            var spamratio = m / nspam;

            prob = spamratio / (spamratio + hamratio);

            var S = Knobs.UnknownWordStrength;
            var StimesX = S * Knobs.UnknownWordProb;


            // Now do Robinson's Bayesian adjustment.
            //
            // s*x + n*p(w)
            // f(w) = --------------
            // s + n
            //
            // I find this easier to reason about like so (equivalent when
            // s != 0):
            //
            // x - p
            // p +  -------
            //       1 + n/s
            //
            // IOW, it moves p a fraction of the distance from p to x, and
            // less so the larger n is, or the smaller s is.

            var n = m + nm;
            prob = (StimesX + n * prob) / (S + n);

            Prob[token] = prob;

            
        }

        internal protected virtual void UpdateProbability(string token)
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

        #endregion Public Model Training Methods

        #region Public Classification Prediction Methods

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
        
        public SortedList<string, double> GetInterestingList(IDictionary<string, int> tokenIncidence)
        {
            SortedList<string, double> interestingList = [];

            if (tokenIncidence is null)
            {
                logger.Debug($"Parameter {nameof(tokenIncidence)} is null. Returning empty list");
                return interestingList;
            }

            // Inner Merge of match probabilities and token incidence
            var matchIncidence = MergeProb(tokenIncidence);
            
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

            var newWordProbs = tokenIncidence.Where(
                kvp => !matchTokens.Contains(kvp.Key) && 
                !Parent.SharedTokenBase.TokenFrequency.ContainsKey(kvp.Key))
                //!notMatchIncidence.ContainsKey(kvp.Key))
                .ToDictionary();

            newWordProbs.ForEach(x =>
            {

                var interestingKey = (0.5 - Math.Abs(0.5 - Knobs.UnknownWordProb)).ToString(".00000") + x.Key;
                Enumerable.Range(0, x.Value)
                    .ForEach(i => interestingList.Add(interestingKey + i, Knobs.UnknownWordProb));
            });
            

            return interestingList;
        }

        public double GetMatchProbability(IDictionary<string, int> tokenIncidence)
        {
            var interestingList = GetInterestingList(tokenIncidence);

            var combined = CombineProbabilities(interestingList);

            return combined;
        }

        public (double Probability, (string Token, double TokenProbability)[]) GetProbabilityDrivers(
            IDictionary<string, int> tokenIncidence)
        {
            var probabilities = GetInterestingList(tokenIncidence);

            var combined = CombineProbabilities(probabilities);
            
            var drivers = probabilities.Select(x => 
                { 
                    var key = x.Key.Substring(6);
                    key = key.Substring(0, key.Length - 1);
                    var value = x.Value;
                    return (key, value);
                })
                .OrderByDescending(x => x.value)
                .Take(Knobs.InterestingWordCount)
                .ToArray();
            
            return (combined, drivers);
        }

        public Task<double> GetMatchProbabilityAsync(
            IDictionary<string, int> tokenIncidence, CancellationToken token)
        {
            return Task.Run(() => GetMatchProbability(tokenIncidence), token);
        }

        /// <summary>
        /// Returns the probability that the supplied tokens are a _positive match with the Classifier
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public double GetMatchProbability(IEnumerable<string> tokens)
        {
            var probabilities = GetInterestingList(tokens.GroupAndCount());

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
        
        private Dictionary<string, int> GetNotMatchIncidence(IDictionary<string, int> tokenIncidence, string[] matchKeys)
        {
            return tokenIncidence
                            .Where(kvp => !matchKeys.Contains(kvp.Key))
                            .Where(kvp => Parent.SharedTokenBase.TokenFrequency.TryGetValue(kvp.Key, out var value)
                                && value * Knobs.NotMatchTokenWeight >= Knobs.MinCountForInclusion)
                            .ToDictionary();
        }

        private (string Token, double Prob, int Incidence)[] MergeProb(IDictionary<string, int> tokenIncidence)
        {
            return Prob.Select(x => (Token: x.Key, Prob: x.Value,
                Incidence: tokenIncidence.TryGetValue(x.Key, out int count) ? count : 0))
                .Where(x => x.Incidence != 0)
                .ToArray();
        }

        #endregion Public Classification Prediction Methods

        #region SpamBayes Ported Functions

        public class WordInfo(int matchCount, int notMatchCount)
        {
            public int MatchCount = matchCount;
            public int NotMatchCount = notMatchCount;
        }
        
        public struct WordStream(string name, string[] words)
        {
            public string Name = name;
            public string[] Words = words;
        }

        
        public (double, List<(string word, double prob)>) Chi2SpamProb(WordStream wordStream, bool evidence) => Chi2SpamProb(wordStream.Words, evidence);
        public double Chi2SpamProb(WordStream wordStream) => Chi2SpamProb(wordStream, false).Item1;
        public (double, List<(string word, double prob)>) Chi2SpamProb(IDictionary<string, int> tokenFrequency, bool evidence) => Chi2SpamProb([..tokenFrequency.Keys], evidence);
        public double Chi2SpamProb(IDictionary<string, int> tokenFrequency) => Chi2SpamProb(tokenFrequency, false).Item1;

        /// <summary>
        /// Return best-guess probability that wordstream is spam. 
        /// wordstream is an iterable object producing words. 
        /// The return value is a float in [0.0, 1.0]. 
        /// If optional arg evidence is True, the return value is a pair 
        /// probability, evidence 
        /// where evidence is a list of(word, probability) pairs.
        /// </summary>
        /// <param name="wordStream"></param>
        /// <param name="evidence"></param>
        /// <returns></returns>
        public (double, List<(string word, double prob)>) Chi2SpamProb(string[] tokens, bool evidence)
        {
            /*
             # We compute two chi-squared statistics, one for ham and one for
             # spam.  The sum-of-the-logs business is more sensitive to probs
             # near 0 than to probs near 1, so the spam measure uses 1-p (so
             # that high-spamprob words have greatest effect), and the ham
             # measure uses p directly (so that lo-spamprob words have greatest
             # effect).
             #
             # For optimization, sum-of-logs == log-of-product, and f.p.
             # multiplication is a lot cheaper than calling ln().  It's easy
             # to underflow to 0.0, though, so we simulate unbounded dynamic
             # range via frexp.  The real product H = this H * 2**Hexp, and
             # likewise the real product S = this S * 2**Sexp.
             */

            double H = 1, S = 1;
            int Hexp = 0, Sexp = 0;

            var clues = GetClues(tokens.ToHashSet());
            foreach (var (probability, word, record) in clues)
            {
                S *= (1.0 - probability);
                H *= probability;
                if (S < 1e-200)
                {
                    int e = 0;
                    S = math.frexp(S, ref e);
                    Sexp += e;
                }
                if (H < 1e-200)
                {
                    int e = 0;
                    H = math.frexp(H, ref e);
                    Hexp += e;
                }
            }

            // # Compute the natural log of the product = sum of the logs:
            // # ln(x * 2**i) = ln(x) + i * ln(2).

            S = Math.Log(S) + Sexp * Math.Log(2);
            H = Math.Log(H) + Hexp * Math.Log(2);

            var n = clues.Count;
            double prob = 0;
            if (n > 0)
            {
                S = 1.0 - chi2Q(-2.0 * S, 2 * n);
                H = 1.0 - chi2Q(-2.0 * H, 2 * n);
                prob = (S - H + 1.0) / 2.0;
            }
            else
            {
                prob = 0.5;
            }

            if (evidence)
            {
                var clues2 = clues.Select(clue => (clue.word, clue.prob))
                    .OrderByDescending(clue => clue.word)
                    .ToList();
                clues2.Insert(0, ("*S*", S));
                clues2.Insert(0, ("*H*", H));
                return (prob, clues2);
            }
            else
            {
                return (prob, null);
            }


        }
        public double chi2_spamprob(string[] tokens) => Chi2SpamProb(tokens, false).Item1;

        public async Task<double> Chi2SpamProbAsync(string[] tokens) => await Task.Run(() => chi2_spamprob(tokens));

        public double chi2Q(double x2, int v)
        {
            
            var m = x2 / 2.0;
            var sum = Math.Exp(-m);
            var term = sum;
            for (int i = 1; i < v / 2; i++)
            {
                term *= m / i;
                sum += term;
            }

            return Math.Min(sum, 1.0);
        
        }

        
        
        

        //public List<(double prob, string word, WordInfo record)> GetClues(WordStream wordStream) => GetClues([.. wordStream.Words]);
        //public List<(double prob, string word, WordInfo record)> GetClues(IDictionary<string, int> tokenFrequency) => GetClues([.. tokenFrequency.Keys]);

        public List<(double prob, string word, WordInfo record)> GetClues(HashSet<string> tokenSet)
        {
            var mindist = Knobs.MinDist;
            var clues = new List<(double distance, double prob, string word, WordInfo record)>();
            var push = clues.Add;
            foreach (string token in tokenSet)
            {
                var tup = GetWordDistance(token);
                if (tup.distance >= mindist) { push(tup); }
            }
            var selectedClues = clues.OrderByDescending(x => x.distance)
                                     .Take(Knobs.MaxDiscriminators)
                                     .Select(clue => (clue.prob, clue.word, clue.record))
                                     .ToList();

            return selectedClues;
        }

        
        public (double distance, double prob, string word, WordInfo record) GetWordDistance(string word)
        {
            WordInfo record = GetWordInfo(word);
            double prob;
            if (record is null)
            {
                prob = Knobs.UnknownWordProb;
            }
            else
            {
                prob = UpdateProbabilitySb(record);
            }
            double distance = Math.Abs(prob - 0.5);

            return (distance, prob, word, record);
        }

        public WordInfo GetWordInfo(string word)
        {
            int m = _match.TokenFrequency.TryGetValue(word, out int bCount) ? bCount : 0;
            int nm = Parent.SharedTokenBase.TokenFrequency
                .TryGetValue(word, out int gCount) ? Math.Max(gCount - m, 0) : 0;
            if (m + nm == 0)
            {
                return null;
            }
            else
            {
                var wordInfo = new WordInfo(m, nm);
                return wordInfo;
            }

        }

        #endregion SpamBayes Ported Functions

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
            public double UnknownWordProb = 0.5;            // 0.5
            public double UnknownWordStrength = 0.45;       // 0.45
            public double MinDist = 0;
            public int MaxDiscriminators = 150;             // 150
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

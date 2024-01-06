using Expat.Bayesian;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Threading;
using UtilitiesCS.HelperClasses;

namespace UtilitiesCS.EmailIntelligence.Bayesian
{
    /// <summary>
    /// Naive Baysiam Spam Filter.  Basically, an implementation of this:
    /// http://www.paulgraham.com/spam.html
    /// </summary>
    public class BayesianClassifier
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors

        public BayesianClassifier() { }
        
        public BayesianClassifier(string tag)
        {
            _tag = tag;
            _prob = new ConcurrentDictionary<string, double>();
            _positive = new Corpus();
            _negative = new Corpus();
        }

        public BayesianClassifier(string tag, IEnumerable<string> positiveTokens, IEnumerable<string> negativeTokens)
        {
            _tag = tag;
            _prob = new ConcurrentDictionary<string, double>();
            Load(positiveTokens, negativeTokens);
        }

        private BayesianClassifier(string tag, Corpus positive, Corpus negative, Corpus tokenBase)
        {
            _tag = tag;
            _positive = positive;
            _nPositive = positive.TokenCounts.Values.Sum();
            _negative = negative;
            _nNegative = negative.TokenCounts.Values.Sum();
            TokenBase = tokenBase;
            _prob = new ConcurrentDictionary<string, double>();
            TokenBase.TokenCounts.Keys.ForEach(UpdateTokenProbability);
        }

        public static BayesianClassifier FromTokenBase(
            Corpus tokenBase,
            string tag, 
            IEnumerable<string> positiveTokens)
        {
            
            var positive = new Corpus(positiveTokens);
            var negative = tokenBase - positive;
            var classifier = new BayesianClassifier(tag, positive, negative, tokenBase);

            return classifier;
        }

        public static async Task<BayesianClassifier> FromTokenBaseAsync(
            Corpus tokenBase,
            string tag,
            IEnumerable<string> positiveTokens,
            CancellationToken token)
        {
            var classifier = new BayesianClassifier();
            await Task.Factory.StartNew(
                () => 
                {
                    var positive = new Corpus(positiveTokens);
                    var negative = tokenBase - positive;
                    classifier.Tag = tag;
                    classifier.Positive = positive;
                    classifier._nPositive = positive.TokenCounts.Values.Sum();
                    classifier.Negative = negative;
                    classifier._nNegative = negative.TokenCounts.Values.Sum();
                    classifier.TokenBase = tokenBase;
                    classifier._prob = new ConcurrentDictionary<string, double>();
                    tokenBase.TokenCounts.Keys.ForEach(classifier.UpdateTokenProbability);
                }, 
                token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            
            return classifier;
        }

        #endregion Constructors

        #region private fields

        private int _nPositive;
        private int _nNegative;

        #endregion private fields

        #region public properties

        [JsonIgnore]
        public bool Loaded { get => _loaded; private set => _loaded = value; }
        private bool _loaded = false;

        /// <summary>
        /// A list of words that show tend to show up in Spam text
        /// </summary>
        [JsonIgnore]
        public Corpus Negative { get => _negative; private set => _negative = value; }
        private Corpus _negative;

        /// <summary>
        /// A list of words that tend to show up in non-spam text
        /// </summary>
        public Corpus Positive { get => _positive; set => _positive = value; }
        private Corpus _positive;

        /// <summary>
        /// A list of probabilities that the given word might appear in a Spam text
        /// </summary>
        [JsonProperty]
        public ConcurrentDictionary<string, double> Prob { get => _prob; private set => _prob = value; }
        private ConcurrentDictionary<string, double> _prob;

        public Corpus TokenBase = null;

        public string Tag { get => _tag; set => _tag = value; }
        private string _tag;

        #endregion public properties

        #region population

        public void AddPositive(IEnumerable<string> tokens) 
        {
            _positive.AddOrIncrementTokens(tokens);
            tokens.Distinct().ForEach(UpdateTokenProbability);
        }

        public void AddNegative(IEnumerable<string> tokens)
        {
            _negative.AddOrIncrementTokens(tokens);
            tokens.Distinct().ForEach(UpdateTokenProbability);
        }

        public void AddTokens(IEnumerable<string> positiveTokens, IEnumerable<string> negativeTokens)
        {
            _positive.AddOrIncrementTokens(positiveTokens);
            _nPositive = _positive.TokenCounts.Values.Sum();
            _negative.AddOrIncrementTokens(negativeTokens);
            _nNegative = _negative.TokenCounts.Values.Sum();

            positiveTokens.Concat(negativeTokens).Distinct().ForEach(UpdateTokenProbability);
        }

        public void RemovePositive(IEnumerable<string> tokens)
        {
            foreach (var token in tokens)
            {
                _positive.DecrementOrRemoveToken(token);
                UpdateTokenProbability(token);
            }
        }

        public void RemoveNegative(IEnumerable<string> tokens)
        {
            foreach (var token in tokens)
            {
                _negative.DecrementOrRemoveToken(token);
                UpdateTokenProbability(token);
            }
        }

        public void Load(IEnumerable<string> positiveTokens, IEnumerable<string> negativeTokens)
        {
            _positive = new Corpus();
            _negative = new Corpus();
            AddTokens(positiveTokens, negativeTokens);
        }

        /// <summary>
        /// For a given token, calculate the probability that will appear in a spam text
        /// by comparing the number of good and bad texts it appears in already.
        /// </summary>
        /// <param name="token"></param>
        private void UpdateTokenProbability(string token)
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
            int g = _positive.TokenCounts.TryGetValue(token, out int gCount) ? gCount * Knobs.GoodTokenWeight : 0 ;
            int b = _negative.TokenCounts.TryGetValue(token, out int bCount) ? bCount : 0;

            if (g + b >= Knobs.MinCountForInclusion)
            {
                double goodfactor = Math.Min(1, (double)g / (double)_nPositive);
                double badfactor = Math.Min(1, (double)b / (double)_nNegative);

                double prob = Math.Max(Knobs.MinScore,
                              Math.Min(Knobs.MaxScore, badfactor / (goodfactor + badfactor)));

                
                // special case for Spam-only tokens.
                // .9998 for tokens only found in spam, or .9999 if found more than 10 times
                if (g == 0)
                {
                    prob = (b > Knobs.CertainSpamCount) ? Knobs.CertainSpamScore : Knobs.LikelySpamScore;
                }

                _prob[token] = prob;
            }
        }

        #endregion population

        #region Serialization Helpers

        public async Task InferNegativeTokensAsync(CancellationToken token, SegmentStopWatch sw = null)
        {
            token.ThrowIfCancellationRequested();

            Negative ??= await Corpus.SubtractAsync(TokenBase, Positive, token, sw);
            sw?.LogDuration("Infer Negative Tokens");

            token.ThrowIfCancellationRequested();

            _nPositive = Positive.TokenCounts.Values.Sum();
            sw?.LogDuration("Calculate _nPositive");

            _nNegative = Negative.TokenCounts.Values.Sum();
            sw?.LogDuration("Calculate _nNegative");
        }

        public async Task RecalcProbsAsync(CancellationToken token, SegmentStopWatch sw = null)
        { 
            Prob = new ConcurrentDictionary<string, double>();
            sw?.LogDuration("Create new Prob Dict");

            var processors = Math.Max(Environment.ProcessorCount - 2, 1);
            var chunkSize = (int)Math.Round((double)TokenBase.TokenCounts.Keys.Count() / (double)processors, 0);
            sw?.LogDuration("Calculate Chunk Size");

            var chunks = TokenBase.TokenCounts.Keys.Chunk(chunkSize);
            sw?.LogDuration("Divide Keys in to Chunks");

            var tasks = chunks.Select(chunk => Task.Run(() => chunk.ForEach(UpdateTokenProbability)));
            sw?.LogDuration("Start tasks to Calculate Token Probabilities");

            await Task.WhenAll(tasks);
            sw?.LogDuration("Complete Tasks To Calculate Token Probabilities");
        }
        
        public async Task AfterDeserialize(CancellationToken token, SegmentStopWatch sw = null)
        {                       
            await Task.Run(async() => await InferNegativeTokensAsync(token, sw), token).ConfigureAwait(false);
            
            if (Prob is null)
            {
                await RecalcProbsAsync(token, sw).ConfigureAwait(false);
            }
            Loaded = true;
            sw.LogDuration("Set Loaded to True");
        }

        #endregion Serialization Helpers

        #region classifier testing
        /// <summary>
        /// Returns the probability that the supplied tokens are a _positive match with the Classifier
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public double CalculateProbability(IEnumerable<string> tokens)
        {
            SortedList probs = [];

            // Spin through every word in the body and look up its individual spam probability.
            // Keep the list in decending order of "Interestingness"
            int index = 0;
            foreach (var token in tokens)
            {
                if (_prob.ContainsKey(token))
                {
                    // "interestingness" == how far our score is from 50%.  
                    // The crazy math below is building a string that lets us sort alphabetically by interestingness.
                    double prob = _prob[token];
                    string key = (0.5 - Math.Abs(0.5 - prob)).ToString(".00000") + token + index++;
                    probs.Add(key, prob);

                }
            }

            /* Combine the 15 most interesting probabilities together into one.  
			 * The algorithm to do this is shown below and described here:
			 * http://www.paulgraham.com/naivebayes.html
			 * 
			 *				abc           
			 *	---------------------------
			 *	abc + (1 - a)(1 - b)(1 - c)
			 *
			 */

            double mult = 1;  // for holding abc..n
            double comb = 1;  // for holding (1 - a)(1 - b)(1 - c)..(1-n)
            index = 0;
            foreach (string key in probs.Keys)
            {
                double prob = (double)probs[key];
                mult *= prob;
                comb *= (1 - prob);

                Debug.WriteLine(index + " " + probs[key] + " " + key);

                if (++index > Knobs.InterestingWordCount)
                    break;
            }

            return mult / (mult + comb);

        }
        #endregion classifier testing

        #region knobs for dialing in performance

        /// <summary>
        /// These are constants used in the Bayesian algorithm, presented in a form that lets you monkey with them.
        /// </summary>
        public class KnobList
        {
            // Values in PG's original article:
            public int GoodTokenWeight = 2;             // 2
            public int MinTokenCount = 0;               // 0
            public int MinCountForInclusion = 5;        // 5
            public double MinScore = 0.011;             // 0.01
            public double MaxScore = 0.99;              // 0.99
            public double LikelySpamScore = 0.9998;     // 0.9998
            public double CertainSpamScore = 0.9999;    // 0.9999
            public int CertainSpamCount = 10;           // 10
            public int InterestingWordCount = 15;       // 15 (later changed to 20)
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

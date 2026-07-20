#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net.Repository.Hierarchy;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence.ClassifierGroups.OlFolder;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{
    public class FolderScorer
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        #region constructors and private variables

        public FolderScorer() { }

        private ScoDictionaryNew<string, long> _folderNameScores = new();
        private static readonly char[] _wordChars = { '&' };
        private Regex _tokenizerRegex = Tokenizer.GetRegex(_wordChars.AsTokenPattern());

        #endregion

        #region public properties

        private VerboseLogger<FolderScorer> _verboseLogger = new();
        public VerboseLogger<FolderScorer> Vlog => _verboseLogger;

        public int Count
        {
            get => _folderNameScores.Count;
        }

        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= _folderNameScores.Count)
                    throw new IndexOutOfRangeException();
                return _folderNameScores.ElementAt(index).Key;
            }
        }

        #endregion

        #region public methods

        public bool LoadFromField(MailItem olMail, IApplicationGlobals appGlobals)
        {
            _folderNameScores.Clear();
            AddConversationBasedSuggestions(olMail, appGlobals);
            if (AddOlFolderKeys(olMail, appGlobals))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool LoadFromField(MailItemHelper mailInfo, IApplicationGlobals appGlobals)
        {
            _folderNameScores.Clear();
            AddConversationBasedSuggestions(mailInfo.Item, appGlobals);
            if (AddOlFolderKeys(mailInfo.Item, appGlobals))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AddOlFolderKeys(MailItem olMail, IApplicationGlobals appGlobals, int topN = -1)
        {
            var objProperty = olMail.UserProperties.Find("FolderKey");
            if (objProperty is null)
            {
                return false;
            }

            var foldersObject = objProperty.Value;
            if (foldersObject is null)
            {
                return false;
            }
            else if (foldersObject is Array)
            {
                return AddArray(foldersObject, topN);
            }
            else
            {
                return AddSuggestion(foldersObject, 0);
            }
        }

        public void RefreshSuggestions(
            MailItem olMail,
            IApplicationGlobals appGlobals,
            int topNfolderKeys = -1,
            bool parallel = false
        )
        {
            var _globals = appGlobals;
            _folderNameScores.Clear();

            AddConversationBasedSuggestions(olMail, _globals);
            if (topNfolderKeys > 0)
            {
                AddOlFolderKeys(olMail, _globals, topNfolderKeys);
            }

            AddWordSequenceSuggestions(olMail, appGlobals, parallel);

            Vlog.LogObject(_folderNameScores, nameof(_folderNameScores));
        }

        public async Task RefreshSuggestions(
            MailItemHelper mailInfo,
            IApplicationGlobals appGlobals,
            int topNfolderKeys = -1,
            bool parallel = false
        )
        {
            var _globals = appGlobals;
            _folderNameScores.Clear();

            await AddBayesianSuggestionsAsync(mailInfo, appGlobals, topNfolderKeys);

            AddConversationBasedSuggestions(mailInfo.Item, _globals);
            //if (topNfolderKeys > 0)
            //{
            //    AddOlFolderKeys(mailInfo.Item, _globals, topNfolderKeys);
            //}

            //AddWordSequenceSuggestions(mailInfo.Item, appGlobals, parallel);

            Vlog.LogObject(_folderNameScores, nameof(_folderNameScores));
        }

        private async Task AddBayesianSuggestionsAsync(
            MailItemHelper mailInfo,
            IApplicationGlobals globals,
            int topNfolderKeys
        )
        {
            EmailIntelligence.Bayesian.Prediction<string>[]? predictions = null;

            if (topNfolderKeys > 0)
            {
                predictions = (await new OlFolderClassifierGroup(globals).GetFolderPredictorAsync())
                    .Classify(mailInfo.Tokens)
                    .Take(topNfolderKeys)
                    .ToArray();
            }
            else
            {
                predictions = (await new OlFolderClassifierGroup(globals).GetFolderPredictorAsync())
                    .Classify(mailInfo.Tokens)
                    .ToArray();
            }

            foreach (var prediction in predictions)
            {
                long score = (long)Math.Round(prediction.Probability * 1000, 0);
                AddSuggestion(prediction.Class!, score);
            }
        }

        public bool AddSuggestion(object folderObject, long score)
        {
            var folder = folderObject as string;
            if ((folder is null) || (folder == "Error"))
            {
                return false;
            }
            else
            {
                AddSuggestion(folder, score);
                return true;
            }
        }

        public void AddSuggestion(string folderPath, long score)
        {
            if (!_folderNameScores.TryAdd(folderPath, score))
                _folderNameScores[folderPath] += score;
        }

        public bool AddArray(object foldersObject, int topN)
        {
            string[]? folders = foldersObject as string[];
            return AddArray(folders, topN);
        }

        public bool AddArray(string[]? folders, int topN)
        {
            if ((folders is null) || (folders[0] == "Error"))
            {
                return false;
            }
            else
            {
                if (topN > 0)
                {
                    folders = folders.Take(topN).ToArray();
                }
                folders.ForEach(folder => AddSuggestion(folder, 0));
                return true;
            }
        }

        public void FromArray(string[] folderPaths)
        {
            _folderNameScores.Clear();
            AddArray(folderPaths, -1);
        }

        /// <summary>
        /// Returns the highest folder suggestion score currently held, in 0-1000 score units,
        /// or 0 when no suggestions are present. This is a pure in-memory read with no I/O and is
        /// safe to call on any thread once the scores have been populated.
        /// </summary>
        public long TopScore() =>
            _folderNameScores.Count == 0 ? 0 : _folderNameScores.Max(x => x.Value);

        // The secondary ThenBy on the key gives a deterministic, culture-independent tie-break
        // for folders that share the same score. ScoDictionaryNew (ConcurrentDictionary-backed)
        // does not enumerate in insertion order, so an explicit ordinal key ordering preserves
        // stable ranking output. This single ordered enumeration is the structural parity
        // mechanism: both the name-only ToArray* projections and the scored ToScoredArray*
        // projections consume it, so their ordering is guaranteed identical by construction.
        private IEnumerable<KeyValuePair<string, long>> OrderedScores() =>
            _folderNameScores
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key, StringComparer.Ordinal);

        public string[] ToArray() => OrderedScores().Select(x => x.Key).ToArray();

        public string[] ToArray(int topN) =>
            OrderedScores().Take(topN).Select(x => x.Key).ToArray();

        /// <summary>
        /// Additive scored projection mirroring <see cref="ToArray()"/> ordering exactly, but
        /// carrying each folder's raw <see cref="FolderScore.Score"/> and a max-normalized
        /// <see cref="FolderScore.Probability"/>. Returns an empty array when no suggestions are
        /// held.
        /// </summary>
        /// <returns>The scored folder suggestions in ranking order.</returns>
        public FolderScore[] ToScoredArray() => BuildScoredArray(OrderedScores());

        /// <summary>
        /// Top-<paramref name="topN"/> scored projection mirroring <see cref="ToArray(int)"/>
        /// ordering exactly. <see cref="FolderScore.Probability"/> is normalized against the
        /// maximum score of the full ordered set (stable per folder regardless of
        /// <paramref name="topN"/>). A <paramref name="topN"/> larger than the number of held
        /// suggestions returns all rows.
        /// </summary>
        /// <param name="topN">The maximum number of scored rows to return.</param>
        /// <returns>The top-<paramref name="topN"/> scored folder suggestions in ranking order.</returns>
        public FolderScore[] ToScoredArray(int topN) => BuildScoredArray(OrderedScores(), topN);

        // Projects the shared ordered enumeration into FolderScore rows. Probability is
        // max-normalized (Score / TopScore) with a zero-guard: when the top score is 0 (empty
        // scorer or all-zero seeds) every Probability is 0 and no divide-by-zero occurs. maxScore
        // is computed once over the full ordered set so per-folder Probability is stable
        // regardless of any topN applied to the returned rows.
        private static FolderScore[] BuildScoredArray(
            IEnumerable<KeyValuePair<string, long>> orderedScores,
            int topN = -1
        )
        {
            var ordered = orderedScores.ToArray();
            if (ordered.Length == 0)
            {
                return Array.Empty<FolderScore>();
            }

            long maxScore = ordered[0].Value;
            IEnumerable<KeyValuePair<string, long>> selected =
                topN < 0 ? ordered : ordered.Take(topN);

            return selected
                .Select(x => new FolderScore(
                    x.Key,
                    x.Value,
                    maxScore == 0 ? 0 : (double)x.Value / maxScore
                ))
                .ToArray();
        }

        internal void AddConversationBasedSuggestions(
            MailItem OlMail,
            IApplicationGlobals _globals,
            int topN = 5
        )
        {
            var map = _globals.AF.CtfMap;
            // Is the conversationID already mapped to an email Folder. If so, grab the index of it
            if (map.ContainsId(OlMail.ConversationID))
            {
                var matches = map.TopEntriesById(OlMail.ConversationID, topN);
                foreach (var match in matches)
                {
                    long score = match.EmailCount;
                    score = (long)
                        Math.Round(
                            Math.Pow(score, _globals.AF.LngConvCtPwr)
                                * _globals.AF.Conversation_Weight
                        );
                    AddSuggestion(match.EmailFolder!, score);
                }
            }
        }

        internal void AddWordSequenceSuggestions(
            MailItem olMail,
            IApplicationGlobals appGlobals,
            bool parallel = true
        )
        {
            if ((olMail.Subject is not null) && (olMail.Subject.Length > 0))
            {
                try
                {
                    var target = new SubjectMapEntry(
                        emailSubject: olMail.Subject,
                        emailSubjectCount: 1,
                        commonWords: appGlobals.AF.CommonWords,
                        tokenizerRegex: _tokenizerRegex,
                        encoder: appGlobals.AF.Encoder
                    );

                    if (Vlog.IsVerbose())
                    {
                        target.LogObjectState();
                    }

                    if (!target.SubjectEncoded.SequenceEqual(new int[] { }))
                    {
                        AddWordSequenceSuggestions(target, appGlobals, parallel);
                    }
                }
                catch (System.Exception e)
                {
                    logger.Error(e.Message);
                }
            }
        }

        internal void AddWordSequenceSuggestions(
            SubjectMapEntry target,
            IApplicationGlobals appGlobals,
            bool parallel = true
        )
        {
            int matchScore = appGlobals.AF.SmithWatterman_MatchScore;
            int mismatchScore = appGlobals.AF.SmithWatterman_MismatchScore;
            int gapPenalty = appGlobals.AF.SmithWatterman_GapPenalty;
            int convCtPwr = appGlobals.AF.LngConvCtPwr;

            dynamic map;
            //if (parallel) { map = appGlobals.AF.SubjectMap.ToList().AsParallel(); }
            //else { map = appGlobals.AF.SubjectMap.ToList(); }
            if (parallel)
            {
                map = appGlobals.AF.SubjectMap.AsParallel();
            }
            else
            {
                map = appGlobals.AF.SubjectMap;
            }

            var querySubject = QuerySubject(
                map,
                target,
                matchScore,
                mismatchScore,
                gapPenalty,
                convCtPwr
            );
            var queryFolder = QueryFolder(map, target, matchScore, mismatchScore, gapPenalty);
            var queryCombined = QueryCombined(querySubject, queryFolder);

            foreach (var entry in queryCombined)
            {
                if (entry.Score > 5)
                {
                    AddSuggestion(entry.FolderPath, entry.Score);
                }
            }
        }

        #endregion

        #region Word Sequence Query Construction

        internal ParallelQuery<FolderScoring> QuerySubject(
            ParallelQuery<ISubjectMapEntry> map,
            SubjectMapEntry target,
            int matchScore,
            int mismatchScore,
            int gapPenalty,
            int convCtPwr
        )
        {
            return map.AsParallel()
                .Where(entry =>
                {
                    if (!entry.Validate())
                        return false;
                    return entry.SubjectEncoded is not null;
                })
                .Select(entry =>
                {
                    int subjScore = SmithWaterman.CalculateScore(
                        entry.SubjectEncoded!,
                        entry.SubjectWordLengths!,
                        target.SubjectEncoded!,
                        target.SubjectWordLengths!,
                        matchScore,
                        mismatchScore,
                        gapPenalty
                    );

                    int subjScoreWt = (int)
                        Math.Round(Math.Pow(subjScore, convCtPwr) * entry.EmailSubjectCount);

                    entry.Score = subjScoreWt;
                    return entry;
                })
                .GroupBy(
                    entry => entry.Folderpath,
                    entry => entry,
                    (folderpath, grouping) =>
                        new FolderScoring
                        {
                            FolderPath = folderpath!,
                            FolderName = grouping.Select(x => x.Foldername).First()!,
                            FolderEncoding = grouping.Select(x => x.FolderEncoded).First()!,
                            FolderWordLengths = grouping.Select(x => x.FolderWordLengths).First()!,
                            Score = grouping.Select(x => x.Score).Sum(),
                        }
                );
        }

        internal ParallelQuery<FolderScoring> QueryFolder(
            ParallelQuery<SubjectMapEntry> map,
            SubjectMapEntry target,
            int matchScore,
            int mismatchScore,
            int gapPenalty
        )
        {
            return map.AsParallel()
                .Where(entry =>
                {
                    if (!entry.Validate())
                        return false;
                    return entry.FolderEncoded is not null;
                })
                .GroupBy(
                    entry => entry.Folderpath,
                    entry => entry,
                    (folderpath, grouping) =>
                        new FolderScoring
                        {
                            FolderPath = folderpath!,
                            FolderName = grouping.Select(x => x.Foldername).First()!,
                            FolderEncoding = grouping.Select(x => x.FolderEncoded).First()!,
                            FolderWordLengths = grouping.Select(x => x.FolderWordLengths).First()!,
                            Score = 0,
                        }
                )
                .Select(entry =>
                {
                    int fldrScore = SmithWaterman.CalculateScore(
                        entry.FolderEncoding,
                        entry.FolderWordLengths,
                        target.SubjectEncoded!,
                        target.SubjectWordLengths!,
                        matchScore,
                        mismatchScore,
                        gapPenalty
                    );
                    entry.Score = (int)(fldrScore * fldrScore);
                    return entry;
                });
        }

        internal ParallelQuery<FolderScoring> QueryCombined(
            ParallelQuery<FolderScoring> querySubject,
            ParallelQuery<FolderScoring> queryFolder
        )
        {
            return querySubject
                .Concat(queryFolder)
                .GroupBy(
                    entry => entry.FolderPath,
                    entry => entry,
                    (folderpath, grouping) =>
                        new FolderScoring
                        {
                            FolderPath = folderpath,
                            FolderName = grouping.Select(x => x.FolderName).First(),
                            FolderEncoding = grouping.Select(x => x.FolderEncoding).First(),
                            FolderWordLengths = grouping.Select(x => x.FolderWordLengths).First(),
                            Score = grouping.Select(x => x.Score).Sum(),
                        }
                )
                .OrderByDescending(entry => entry.Score)
                .Take(5);
        }

        internal IEnumerable<FolderScoring> QuerySubject(
            SubjectMapSco map,
            SubjectMapEntry target,
            int matchScore,
            int mismatchScore,
            int gapPenalty,
            int convCtPwr
        )
        {
            //int threshhold = 1000;
            return map.Where(entry =>
                {
                    if (!entry.Validate())
                        return false;
                    return entry.SubjectEncoded is not null;
                })
                .Select(entry =>
                {
                    //var thresh = entry.Folderpath == "Reference\\HR - Personal - Offers LOIs Expats" ? (int)Math.Round(Math.Pow(threshhold / entry.EmailSubjectCount, 1/convCtPwr),0): -1;
                    var thresh = -1;
                    int subjScore = SmithWaterman.CalculateScore(
                        entry.SubjectEncoded!,
                        entry.SubjectWordLengths!,
                        target.SubjectEncoded!,
                        target.SubjectWordLengths!,
                        matchScore,
                        mismatchScore,
                        gapPenalty,
                        entry.EmailSubject!,
                        target.EmailSubject!,
                        thresh
                    );
                    int subjScoreWt = (int)
                        Math.Round(Math.Pow(subjScore, convCtPwr) * entry.EmailSubjectCount);

                    entry.Score = subjScoreWt;
                    return entry;
                })
                .GroupBy(
                    entry => entry.Folderpath,
                    entry => entry,
                    (folderpath, grouping) =>
                        new FolderScoring
                        {
                            FolderPath = folderpath!,
                            FolderName = grouping.Select(x => x.Foldername).First()!,
                            FolderEncoding = grouping.Select(x => x.FolderEncoded).First()!,
                            FolderWordLengths = grouping.Select(x => x.FolderWordLengths).First()!,
                            Score = grouping.Select(x => x.Score).Sum(),
                        }
                );
        }

        internal IEnumerable<FolderScoring> QueryFolder(
            SubjectMapSco map,
            SubjectMapEntry target,
            int matchScore,
            int mismatchScore,
            int gapPenalty
        )
        {
            //int threshhold = 1000;
            return map.Where(entry =>
                {
                    if (!entry.Validate())
                        return false;
                    return entry.FolderEncoded is not null;
                })
                .GroupBy(
                    entry => entry.Folderpath,
                    entry => entry,
                    (folderpath, grouping) =>
                        new FolderScoring
                        {
                            FolderPath = folderpath!,
                            FolderName = grouping.Select(x => x.Foldername).First()!,
                            FolderEncoding = grouping.Select(x => x.FolderEncoded).First()!,
                            FolderWordLengths = grouping.Select(x => x.FolderWordLengths).First()!,
                            Score = 0,
                        }
                )
                .Select(entry =>
                {
                    //var thresh = entry.FolderPath == "Reference\\HR - Personal - Offers LOIs Expats" ? (int)Math.Round(Math.Pow(threshhold, 0.5), 0):-1;
                    var thresh = -1;
                    int fldrScore = SmithWaterman.CalculateScore(
                        entry.FolderEncoding,
                        entry.FolderWordLengths,
                        target.SubjectEncoded!,
                        target.SubjectWordLengths!,
                        matchScore,
                        mismatchScore,
                        gapPenalty,
                        entry.FolderName,
                        target.EmailSubject!,
                        thresh
                    );
                    entry.Score = (int)(fldrScore * fldrScore);
                    return entry;
                });
        }

        internal IEnumerable<FolderScoring> QueryCombined(
            IEnumerable<FolderScoring> querySubject,
            IEnumerable<FolderScoring> queryFolder
        )
        {
            return querySubject
                .Concat(queryFolder)
                .GroupBy(
                    entry => entry.FolderPath,
                    entry => entry,
                    (folderpath, grouping) =>
                        new FolderScoring
                        {
                            FolderPath = folderpath,
                            FolderName = grouping.Select(x => x.FolderName).First(),
                            FolderEncoding = grouping.Select(x => x.FolderEncoding).First(),
                            FolderWordLengths = grouping.Select(x => x.FolderWordLengths).First(),
                            Score = grouping.Select(x => x.Score).Sum(),
                        }
                )
                .OrderByDescending(entry => entry.Score)
                .Take(5);
        }

        #endregion

        internal struct FolderScoring
        {
            public string FolderPath;
            public string FolderName;
            public int[] FolderEncoding;
            public int[] FolderWordLengths;
            public int Score;
        }
    }
}

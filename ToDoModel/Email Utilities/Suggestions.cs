using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UtilitiesCS;
using static ToDoModel.SuggestionsLegacy;

namespace ToDoModel
{
    public class Suggestions
    {
        #region constructors and private variables

        public Suggestions() { }

        private const int maxSuggestions = 5;

        private ConcurrentDictionary<string, long> _folderNameScores = new();
        private static char[] _wordChars = { '&' };
        private Regex _tokenizerRegex = Tokenizer.GetRegex(_wordChars.AsTokenPattern());

        #endregion

        #region public properties

        public int Count { get => _folderNameScores.Count; }

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

        public void RefreshSuggestions(MailItem OlMail,
                                       IApplicationGlobals AppGlobals,
                                       bool ReloadCTFStagingFiles = true,
                                       bool InBackground = false,
                                       bool parallel = false)
        {
            var _globals = AppGlobals;
            _folderNameScores.Clear();

            AddConversationBasedSuggestions(OlMail, _globals);
            AddAnythingInAutoFileField(OlMail, _globals);
            if ((OlMail.Subject is not null) && (OlMail.Subject.Length > 0))
            {
                var target = new SubjectMapEntry(emailSubject: OlMail.Subject,
                                                 emailSubjectCount: 1,
                                                 commonWords: AppGlobals.AF.CommonWords,
                                                 tokenizerRegex: _tokenizerRegex,
                                                 encoder: AppGlobals.AF.Encoder);
                if (!target.SubjectEncoded.SequenceEqual(new int[] { }))
                {
                    AddWordSequenceSuggestions(target, AppGlobals, parallel);
                }
            }
        }

        public void AddSuggestion(string folderPath, long score)
        {
            if (!_folderNameScores.TryAdd(folderPath, score))
                _folderNameScores[folderPath] += score;
        }

        public void FromArray(string[] folderPaths)
        {
            _folderNameScores.Clear();
            foreach (var folderPath in folderPaths)
            {
                AddSuggestion(folderPath, 0);
            }
        }

        public string[] ToArray() => _folderNameScores.Keys.ToArray();

        internal void AddConversationBasedSuggestions(MailItem OlMail, IApplicationGlobals _globals)
        {
            // Is the conversationID already mapped to an email Folder. If so, grab the index of it
            int Inc_Num = _globals.AF.CTFList.CTF_Incidence_FIND(OlMail.ConversationID);
            
            // If an incidence is found score it and add it to the list of suggestions
            if (Inc_Num > 0) { ScoreAndAddConv(_globals.AF.CTFList.CTF_Inc[Inc_Num], 
                                               _globals.AF.LngConvCtPwr, 
                                               _globals.AF.Conversation_Weight); }
        }

        internal void ScoreAndAddConv(CTF_Incidence ctfIncidence, int convCtPwr, int convWeight)
        {
            // For each Folder that already contains at least one email with the conversationID ...
            for (int i = 1; i <= ctfIncidence.Folder_Count; i++)
            {
                // Calculate the weight of the suggestion based on how much of the conversation is already in the folder
                long score = ctfIncidence.Email_Conversation_Count[i];
                score = (long)Math.Round(Math.Pow(score, convCtPwr) * convWeight);

                // Add or increment the score for the folder
                AddSuggestion(ctfIncidence.Email_Folder[i], score);
            }
        }

        internal void AddAnythingInAutoFileField(MailItem OlMail, IApplicationGlobals _globals)
        {
            // TODO: Determine if this property still exists
            dynamic objProperty = OlMail.UserProperties.Find("AutoFile");
            if (objProperty is not null)
            {
                AddSuggestion(objProperty.Value, (long)Math.Round(Math.Pow(4d, _globals.AF.LngConvCtPwr) * _globals.AF.Conversation_Weight));
                throw new NotImplementedException("Please investigate what this is and why it fired");
            }
        }
                
        internal void AddWordSequenceSuggestions(SubjectMapEntry target, IApplicationGlobals appGlobals, bool parallel = true)
        {
            int matchScore = appGlobals.AF.SmithWatterman_MatchScore;
            int mismatchScore = appGlobals.AF.SmithWatterman_MismatchScore;
            int gapPenalty = appGlobals.AF.SmithWatterman_GapPenalty;
            int convCtPwr = appGlobals.AF.LngConvCtPwr;

            dynamic map;
            if (parallel) { map = appGlobals.AF.SubjectMap.ToList().AsParallel(); }
            else { map = appGlobals.AF.SubjectMap.ToList(); }
            
            var querySubject = QuerySubject(map, target, matchScore, mismatchScore, gapPenalty, convCtPwr);
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

        internal ParallelQuery<FolderScoring> QuerySubject(ParallelQuery<ISubjectMapEntry> map,
                                                           SubjectMapEntry target,
                                                           int matchScore, 
                                                           int mismatchScore, 
                                                           int gapPenalty,
                                                           int convCtPwr)
        {
            return map.AsParallel()
                      .Where(entry => entry.SubjectEncoded is not null)
                      .Select(entry =>
                      {
                          int subjScore = SmithWaterman.SW_CalcInt(entry.SubjectEncoded,
                                                                   entry.SubjectWordLengths,
                                                                   target.SubjectEncoded,
                                                                   target.SubjectWordLengths,
                                                                   matchScore,
                                                                   mismatchScore,
                                                                   gapPenalty);
                          int subjScoreWt = (int)Math.Round(
                                     Math.Pow(subjScore, convCtPwr) * entry.EmailSubjectCount);

                          entry.Score = subjScoreWt;
                          return entry;
                      })
                      .GroupBy(entry => entry.Folderpath,
                               entry => entry,
                               (folderpath, grouping) => new FolderScoring
                               {
                                   FolderPath = folderpath,
                                   FolderName = grouping.Select(x => x.Foldername).First(),
                                   FolderEncoding = grouping.Select(x => x.FolderEncoded).First(),
                                   FolderWordLengths = grouping.Select(x => x.FolderWordLengths).First(),
                                   Score = grouping.Select(x => x.Score).Sum()
                               });
        }

        internal ParallelQuery<FolderScoring> QueryFolder(ParallelQuery<ISubjectMapEntry> map,
                                                          SubjectMapEntry target,
                                                          int matchScore,
                                                          int mismatchScore,
                                                          int gapPenalty)

        {
            return map.AsParallel()
                      .Where(entry => entry.FolderEncoded is not null)
                      .GroupBy(entry => entry.Folderpath,
                               entry => entry,
                               (folderpath, grouping) => new FolderScoring
                               {
                                   FolderPath = folderpath,
                                   FolderName = grouping.Select(x => x.Foldername).First(),
                                   FolderEncoding = grouping.Select(x => x.FolderEncoded).First(),
                                   FolderWordLengths = grouping.Select(x => x.FolderWordLengths).First(),
                                   Score = 0
                               })
                      .Select(entry =>
                      {
                          int fldrScore = SmithWaterman.SW_CalcInt(entry.FolderEncoding,
                                                                   entry.FolderWordLengths,
                                                                   target.SubjectEncoded,
                                                                   target.SubjectWordLengths,
                                                                   matchScore,
                                                                   mismatchScore,
                                                                   gapPenalty);
                          entry.Score = (int)(fldrScore * fldrScore);
                          return entry;
                      });
        }

        internal ParallelQuery<FolderScoring> QueryCombined(ParallelQuery<FolderScoring> querySubject,
                                                            ParallelQuery<FolderScoring> queryFolder)
        {
            return querySubject.Concat(queryFolder)
                               .GroupBy(entry => entry.FolderPath,
                                        entry => entry,
                                        (folderpath, grouping) => new FolderScoring
                                        {
                                            FolderPath = folderpath,
                                            FolderName = grouping.Select(x => x.FolderName).First(),
                                            FolderEncoding = grouping.Select(x => x.FolderEncoding).First(),
                                            FolderWordLengths = grouping.Select(x => x.FolderWordLengths).First(),
                                            Score = grouping.Select(x => x.Score).Sum()
                                        })
                               .OrderByDescending(entry => entry.Score)
                               .Take(5);
        }


        internal IEnumerable<FolderScoring> QuerySubject(List<ISubjectMapEntry> map,
                                                         SubjectMapEntry target,
                                                         int matchScore,
                                                         int mismatchScore,
                                                         int gapPenalty,
                                                         int convCtPwr)
        {
            return map.Where(entry => entry.SubjectEncoded is not null)
                      .Select(entry =>
                      {
                            int subjScore = SmithWaterman.SW_CalcInt(entry.SubjectEncoded,
                                                                     entry.SubjectWordLengths,
                                                                     target.SubjectEncoded,
                                                                     target.SubjectWordLengths,
                                                                     matchScore,
                                                                     mismatchScore,
                                                                     gapPenalty);
                            int subjScoreWt = (int)Math.Round(
                                       Math.Pow(subjScore, convCtPwr) * entry.EmailSubjectCount);

                            entry.Score = subjScoreWt;
                            return entry;
                      })
                      .GroupBy(entry => entry.Folderpath,
                               entry => entry,
                               (folderpath, grouping) => new FolderScoring
                               {
                                    FolderPath = folderpath,
                                    FolderName = grouping.Select(x => x.Foldername).First(),
                                    FolderEncoding = grouping.Select(x => x.FolderEncoded).First(),
                                    FolderWordLengths = grouping.Select(x => x.FolderWordLengths).First(),
                                    Score = grouping.Select(x => x.Score).Sum()
                               });
        }


        internal IEnumerable<FolderScoring> QueryFolder(List<ISubjectMapEntry> map,
                                                        SubjectMapEntry target,
                                                        int matchScore,
                                                        int mismatchScore,
                                                        int gapPenalty)                                                                  
        {
            return map.Where(entry => entry.FolderEncoded is not null)
                      .GroupBy(entry => entry.Folderpath,
                               entry => entry,
                               (folderpath, grouping) => new FolderScoring
                               {
                                    FolderPath = folderpath,
                                    FolderName = grouping.Select(x => x.Foldername).First(),
                                    FolderEncoding = grouping.Select(x => x.FolderEncoded).First(),
                                    FolderWordLengths = grouping.Select(x => x.FolderWordLengths).First(),
                                    Score = 0
                               })
                      .Select(entry =>
                      {
                            int fldrScore = SmithWaterman.SW_CalcInt(entry.FolderEncoding,
                                                                     entry.FolderWordLengths,
                                                                     target.SubjectEncoded,
                                                                     target.SubjectWordLengths,
                                                                     matchScore,
                                                                     mismatchScore,
                                                                     gapPenalty);
                            entry.Score = (int)(fldrScore * fldrScore);
                            return entry;
                      });
        }

        internal IEnumerable<FolderScoring> QueryCombined(IEnumerable<FolderScoring> querySubject,
                                                          IEnumerable<FolderScoring> queryFolder)
        {
            return querySubject.Concat(queryFolder)
                               .GroupBy(entry => entry.FolderPath,
                                        entry => entry,
                                        (folderpath, grouping) => new FolderScoring
                                        {
                                            FolderPath = folderpath,
                                            FolderName = grouping.Select(x => x.FolderName).First(),
                                            FolderEncoding = grouping.Select(x => x.FolderEncoding).First(),
                                            FolderWordLengths = grouping.Select(x => x.FolderWordLengths).First(),
                                            Score = grouping.Select(x => x.Score).Sum()
                                        })
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

        //internal void AddWordSequenceSuggestionsP(SubjectMapEntry target, IApplicationGlobals appGlobals)
        //{
        //    var map = appGlobals.AF.SubjectMap.ToList();
        //    var map2 = map.AsParallel();
        //    var querySubject = map.AsParallel()
        //                       .Where(entry => entry.SubjectEncoded is not null)
        //                       .Select(entry =>
        //                       {
        //                           int subjScore = SmithWaterman.SW_CalcInt(entry.SubjectEncoded,
        //                                                                    entry.SubjectWordLengths,
        //                                                                    target.SubjectEncoded,
        //                                                                    target.SubjectWordLengths,
        //                                                                    appGlobals.AF);
        //                           int subjScoreWt = (int)Math.Round(
        //                               Math.Pow(subjScore, appGlobals.AF.LngConvCtPwr) * entry.EmailSubjectCount);

        //                           entry.Score = subjScoreWt;
        //                           return entry;
        //                       })
        //                       .GroupBy(entry => entry.Folderpath,
        //                                entry => entry,
        //                                (folderpath, grouping) => new FolderScoring
        //                                {
        //                                    FolderPath = folderpath,
        //                                    FolderName = grouping.Select(x => x.Foldername).First(),
        //                                    FolderEncoding = grouping.Select(x => x.FolderEncoded).First(),
        //                                    FolderWordLengths = grouping.Select(x => x.FolderWordLengths).First(),
        //                                    Score = grouping.Select(x => x.Score).Sum()
        //                                });

        //    var queryFolder = map.AsParallel()
        //                      .GroupBy(entry => entry.Folderpath,
        //                               entry => entry,
        //                               (folderpath, grouping) => new FolderScoring
        //                               {
        //                                   FolderPath = folderpath,
        //                                   FolderName = grouping.Select(x => x.Foldername).First(),
        //                                   FolderEncoding = grouping.Select(x => x.FolderEncoded).First(),
        //                                   FolderWordLengths = grouping.Select(x => x.FolderWordLengths).First(),
        //                                   Score = 0
        //                               })
        //                      .Select(entry =>
        //                      {
        //                          int fldrScore = SmithWaterman.SW_CalcInt(entry.FolderEncoding,
        //                                                                   entry.FolderWordLengths,
        //                                                                   target.SubjectEncoded,
        //                                                                   target.SubjectWordLengths,
        //                                                                   appGlobals.AF);
        //                          entry.Score = (int)(fldrScore * fldrScore);
        //                          return entry;
        //                      });

        //    ParallelQuery<FolderScoring> queryCombined = QueryCombined(querySubject, queryFolder);

        //    foreach (var entry in queryCombined)
        //    {
        //        if (entry.Score > 5)
        //        {
        //            AddSuggestion(entry.FolderPath, entry.Score);
        //        }
        //    }
        //}

    }
}

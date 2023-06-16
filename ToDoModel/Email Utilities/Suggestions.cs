using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Office.Interop.Outlook;
using System.Linq;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace ToDoModel
{

    public class Suggestions
    {

        private int _count;
        private string[] _strFolderArray;
        private long[] lngValor;
        private const int MaxSuggestions = 5;
        private static char[] _wordChars = { '&' };
        private Regex _tokenizerRegex = Tokenizer.GetRegex(_wordChars.AsTokenPattern());

        public Suggestions()
        {

        }

        public int Count
        {
            get
            {
                return _count;
            }
        }

        public long[] Valor
        {
            get
            {
                long[] ValorRet = default;
                ValorRet = lngValor;
                return ValorRet;
            }
            set
            {
                lngValor = value;
            }
        }

        public string[] FolderSuggestionsArray
        {
            get
            {
                string[] FolderSuggestionsArrayRet = default;
                FolderSuggestionsArrayRet = _strFolderArray;
                return FolderSuggestionsArrayRet;
            }
            set
            {
                _strFolderArray = value;
            }
        }

        public string get_FolderList_ItemByIndex(int idx)
        {
            return _strFolderArray[idx];

        }

        private int find(string strFolderName)
        {
            int findRet = default;

            int i;
            findRet = 0;
            var loopTo = _strFolderArray.Length -1;
            for (i = 1; i <= loopTo; i++)
            {
                if ((_strFolderArray[i] ?? "") == (strFolderName ?? ""))
                    findRet = i;
            }

            return findRet;

        }

        public void ADD_END(string fldr)
        {
            int i;

            _count = _count + 1;
            Array.Resize(ref _strFolderArray, _count + 1);
            Array.Resize(ref lngValor, _count + 1);

            _strFolderArray[_count] = fldr;
            lngValor[_count] = 0L;

            var loopTo = _count;
            for (i = 1; i <= loopTo; i++)
                lngValor[i] = lngValor[i] + 1L;
        }
        public void Add(string fldr, long Val, int mxsug = MaxSuggestions)
        {
            int i, j;
            bool added;
            int found;
            string tempStr;
            long tempVal;

            added = false;

            if (_count == 0)                                                       // 
            {
                _strFolderArray = new string[2];
                lngValor = new long[2];
                _count = 1;
                _strFolderArray[1] = fldr;
                lngValor[1] = Val;
            }

            else
            {

                found = find(fldr);
                if (found == 0)                                                           // Check to see if folder has already been captured in results
                {

                    if (_count < mxsug)                                  // If there are less results than the max, add a result
                    {
                        _count = _count + 1;
                        Array.Resize(ref _strFolderArray, _count + 1);
                        Array.Resize(ref lngValor, _count + 1);
                    }

                    var loopTo = _count - 1;
                    for (i = 1; i <= loopTo; i++)                                          // Put the result into the right sequence based on
                    {
                        if (Val > lngValor[i])                                       // highest _score to lowest _score
                        {
                            added = true;
                            var loopTo1 = i;
                            for (j = _count - 1; j >= loopTo1; j -= 1)                          // Loop shifts every entry down one for middle insertion
                            {
                                _strFolderArray[j + 1] = _strFolderArray[j];
                                lngValor[j + 1] = lngValor[j];
                            }
                            _strFolderArray[i] = fldr;
                            lngValor[i] = Val;
                            break;
                        }
                    }

                    if (added == false)                                                   // If it was not at the beginning or in the middle,
                    {
                        if (Val > lngValor[_count])                            // Check to see if it goes at the end
                        {
                            _strFolderArray[_count] = fldr;                          // and replace the last entry if it is better
                            lngValor[_count] = Val;
                        }
                    }
                }
                // _____________________________________________________________________

                else
                {
                    // _____________________________________________________________________
                    // ------ Case where we add the value to an existing entry and resort---
                    lngValor[found] = lngValor[found] + Val;
                    if (found > 1)
                    {
                        for (i = found; i >= 2; i -= 1)
                        {


                            if (lngValor[i] > lngValor[i - 1])                           // If the entry above has a lower value, switch them
                            {
                                tempVal = lngValor[i - 1];
                                lngValor[i - 1] = lngValor[i];
                                lngValor[i] = tempVal;
                                tempStr = _strFolderArray[i - 1];
                                _strFolderArray[i - 1] = _strFolderArray[i];
                                _strFolderArray[i] = tempStr;
                            }
                            else
                            {
                                break;
                            }                                                // Stop reordering when it is in order


                        }                                                              // End loop to raise up higher values to top
                    }

                }
            }

        }

        public void PrintDebug()
        {
            int i;
            var loopTo = _count;
            for (i = 1; i <= loopTo; i++)
                Debug.WriteLine("Folder: " + _strFolderArray[i] + "   Value: " + lngValor[i]);
        }


        public void RefreshSuggestions(MailItem OlMail, IApplicationGlobals AppGlobals, bool ReloadCTFStagingFiles = true, bool InBackground = false)
        {

            var _globals = AppGlobals;

            // QUESION: Will reloading staging files for CTF ever be necessary. I think not.
            if (ReloadCTFStagingFiles)
                ReloadStagingFiles(_globals);

            ClearSuggestions();
            AddConversationBasedSuggestions(OlMail, _globals);
            AddAnythingInAutoFileField(OlMail, _globals);
            if ((OlMail.Subject is not null)&&(OlMail.Subject.Length > 0))
            {
                var target = new SubjectMapEntry(emailSubject: OlMail.Subject,
                                                 emailSubjectCount: 1,
                                                 commonWords: AppGlobals.AF.CommonWords,
                                                 tokenizerRegex: _tokenizerRegex,
                                                 encoder: AppGlobals.AF.Encoder);
                if (!target.SubjectEncoded.SequenceEqual(new int[] { }))
                {
                    AddWordSequenceSuggestionsP(target, AppGlobals);
                }
            }
            
        }

        private void ClearSuggestions()
        {
            if (_strFolderArray is not null)
            {
                Array.Clear(_strFolderArray, 0, _strFolderArray.Length);
            }
        }

        private void AddWordSequenceSuggestions(MailItem OlMail, IApplicationGlobals AppGlobals)
        {
            int i;
            object[,] Matrix = null;
            string SubjectStripped;
            int SWVal, Val, Val1;
            string strTmpFldr;
            string[] varFldrSubs;

            SubjectStripped = OlMail.Subject.StripCommonWords(AppGlobals.AF.CommonWords); // Eliminate common words from the subject
            var loopTo = (int)SubjectMapModule.SubjectMapCt;
            for (i = 1; i <= loopTo; i++)   // Loop through every subject / folder combination
            {
                {
                    // Run Smith Waterman on Email Subject and the SubjectMap
                    SWVal = SmithWaterman.SW_Calc(SubjectStripped, SubjectMapModule.SubjectMap[i].EmailSubject, ref Matrix, AppGlobals.AF, SmithWaterman.SW_Options.ByWords);

                    // Calculate a weighted score
                    Val = (int)Math.Round(Math.Pow(SWVal, AppGlobals.AF.LngConvCtPwr) * SubjectMapModule.SubjectMap[i].EmailSubjectCount);

                    // Execute on only distinct Email Folder Names 
                    if ((SubjectMapModule.SubjectMap[i].Folderpath ?? "") != (SubjectMapModule.SubjectMap[i - 1].Folderpath ?? ""))
                    {
                        // Get the top level folder name in the folder tree
                        varFldrSubs = SubjectMapModule.SubjectMap[i].Folderpath.Split("\\");

                        // Run Smith Waterman on Email Subject and the distinct Email Folder Names
                        strTmpFldr = varFldrSubs[varFldrSubs.Length-1];

                        // Run Smith Waterman on Email Subject and the distinct Email Folder Names
                        Val1 = SmithWaterman.SW_Calc(SubjectStripped, strTmpFldr, ref Matrix, AppGlobals.AF, SmithWaterman.SW_Options.ByWords);
                        
                        // Combine the two scores using relative weights
                        Val = Val1 * Val1 + Val;
                    }

                    if (Val > 5)
                    {
                        Add(SubjectMapModule.SubjectMap[i].Folderpath, Val);
                    }
                }
            }
        }

        internal struct FolderScoring
        {
            public string FolderPath;
            public string FolderName;
            public int[] FolderEncoding;
            public int[] FolderWordLengths;
            public int Score;
        }

        internal void AddWordSequenceSuggestionsP(SubjectMapEntry target, IApplicationGlobals appGlobals)
        {
            var map = appGlobals.AF.SubjectMap.ToList();
            
            var querySubject = map.AsParallel()
                               .Where(entry => entry.SubjectEncoded is not null)
                               .Select(entry =>
                               {
                                   int subjScore = SmithWaterman.SW_CalcInt(entry.SubjectEncoded,
                                                                            entry.SubjectWordLengths,
                                                                            target.SubjectEncoded,
                                                                            target.SubjectWordLengths,
                                                                            appGlobals.AF);
                                   int subjScoreWt = (int)Math.Round(
                                       Math.Pow(subjScore, appGlobals.AF.LngConvCtPwr) * entry.EmailSubjectCount);

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

            var queryFolder = map.AsParallel()
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
                                                                           appGlobals.AF);
                                  entry.Score = (int)(fldrScore * fldrScore);
                                  return entry;
                              });
            
            var queryCombined = querySubject
                                .Concat(queryFolder) //.AsParallel()
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

            foreach(var entry in queryCombined)
            {
                if (entry.Score > 5)
                {
                    Add(entry.FolderPath, entry.Score);
                }
            }                     
        }

        private void AddAnythingInAutoFileField(MailItem OlMail, IApplicationGlobals _globals)
        {
            // TODO: Determine if this property still exists
            dynamic objProperty = OlMail.UserProperties.Find("AutoFile");
            if (objProperty is not null)
            {
                Add(objProperty.Value, (long)Math.Round(Math.Pow(4d, _globals.AF.LngConvCtPwr) * _globals.AF.Conversation_Weight));
                throw new NotImplementedException("Please investigate what this is and why it fired");
            }
        }

        private void AddConversationBasedSuggestions(MailItem OlMail, IApplicationGlobals _globals)
        {
            // Is the conversationID already mapped to an email Folder. If so, grab the index of it
            int Inc_Num = _globals.AF.CTFList.CTF_Incidence_FIND(OlMail.ConversationID);
            if (Inc_Num > 0)
            {
                {
                    ref var withBlock = ref _globals.AF.CTFList.CTF_Inc[Inc_Num];
                    // For each Folder that already contains at least one email with the conversationID ...
                    for (int i = 1, loopTo = withBlock.Folder_Count; i <= loopTo; i++)
                    {
                        // Calculate the weight of the suggestion based on how much of the conversation is already in the folder
                        long Val = withBlock.Email_Conversation_Count[i];
                        Val = (long)Math.Round(Math.Pow(Val, _globals.AF.LngConvCtPwr) * _globals.AF.Conversation_Weight);
                        Add(withBlock.Email_Folder[i], Val);
                    }
                }
            }
        }

        private static void ReloadStagingFiles(IApplicationGlobals _globals)
        {
            // Throw New NotImplementedException("CTF_Incidence_Text_File_READ, SubjectMapReadTextFile, " _
            // & "and Common_Words_Text_File_READ are not implemented. Cannot reload")
            // CTF_Incidence_Text_File_READ(_globals.FS)
            SubjectMapModule.Subject_MAP_Text_File_READ(_globals.FS, (IList<string>)_globals.AF.CommonWords);
            SubjectMapModule.Common_Words_Text_File_READ(_globals.FS);

            string[] strFList = NavigateOlFolders.OlFolderlist_GetAll(_globals.Ol);
        }
    }
}
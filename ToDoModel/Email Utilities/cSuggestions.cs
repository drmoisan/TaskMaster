using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Office.Interop.Outlook;

using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesVB;

namespace ToDoModel
{

    public class cSuggestions
    {

        private int _count;
        private string[] _strFolderArray;
        private long[] lngValor;
        private const int MaxSuggestions = 5;

        public cSuggestions()
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
            AddWordSequenceSuggestions(OlMail, AppGlobals);
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
            long SWVal, Val, Val1;
            string strTmpFldr;
            string[] varFldrSubs;

            SubjectStripped = OlMail.Subject.StripCommonWords((IList<string>)AppGlobals.AF.CommonWords); // Eliminate common words from the subject
            var loopTo = (int)SubjectMapModule.SubjectMapCt;
            for (i = 1; i <= loopTo; i++)   // Loop through every subject of every email ever received
            {
                {
                    ref var withBlock = ref SubjectMapModule.SubjectMap[i];
                    SWVal = Smith_Watterman.SW_Calc(SubjectStripped, SubjectMapModule.SubjectMap[i].EmailSubject, ref Matrix, AppGlobals.AF, Smith_Watterman.SW_Options.ByWords);
                    Val = (long)Math.Round(Math.Pow(SWVal, AppGlobals.AF.LngConvCtPwr) * SubjectMapModule.SubjectMap[i].EmailSubjectCount);
                    if ((SubjectMapModule.SubjectMap[i].EmailFolder ?? "") != (SubjectMapModule.SubjectMap[i - 1].EmailFolder ?? ""))
                    {
                        varFldrSubs = SubjectMapModule.SubjectMap[i].EmailFolder.Split("\\");
                        strTmpFldr = varFldrSubs[varFldrSubs.Length-1];
                        Val1 = Smith_Watterman.SW_Calc(SubjectStripped, strTmpFldr, ref Matrix, AppGlobals.AF, Smith_Watterman.SW_Options.ByWords);
                        Val = Val1 * Val1 + Val;
                    }

                    if (Val > 5L)
                    {
                        Add(withBlock.EmailFolder, Val);
                    }
                }
            }
        }

        private void AddAnythingInAutoFileField(MailItem OlMail, IApplicationGlobals _globals)
        {
            // TODO: Determine if this property still exists
            var objProperty = OlMail.UserProperties.Find("AutoFile");
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
using System;
using System.Collections.Generic;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesVB;

namespace ToDoModel
{

    public class SubjectIncidence
    {
        public SubjectIncidence() { }
        public SubjectIncidence(int score, string folderName)
        {
            Score = score;
            FolderName = folderName;
        }

        private int _score;
        private string _folderName;

        public int Score { get => _score; set => _score = value; }
        public string FolderName { get => _folderName; set => _folderName = value; }
    }

    //public class SubjectMapClass 
    //{
    //    public SubjectMapClass() { }        
    //    public List<SubjectMapEntry> SubjectMapEntries;
    //    public static string[] WordList;
    //    public static int WordCount;

    //    public List<SubjectIncidence> SubjectIncidences;


    //    public void SubjectMapAdd(string subject, string folderName)
    //    {
    //        int idx = SubjectMapEntries.FindIndex(entry => ((entry.EmailSubject == subject) && (entry.Folderpath == folderName)));

    //        // If it doesn't exist, add an entry. If it does exist, increase the count
    //        if (idx == -1)
    //        {
    //            SubjectMapEntries.Add(
    //                new SubjectMapEntry(emailFolder: folderName, emailSubject: subject, emailSubjectCount: 1));
    //        }
    //        else
    //        {
    //            SubjectMapEntries[idx].EmailSubjectCount +=  1;
    //        }

    //        // Check to see if any mapping exists. If not, add the first entry
    //        //if (SubjectMapEntries.Count == 0)
    //        //{
    //        //    SubjectMapEntries.Add(
    //        //        new SubjectMapEntry(emailFolder: folderName, emailSubject: subject, emailSubjectCount: 1));
    //        //}

    //        //// Else, try to find the item 
    //        //else
    //        //{
    //        //}
    //    }

    //    public void SubjectIncAdd(string folderName, int score)
    //    {
    //        int idx;

    //        // Check to see if any mapping exists. If not, add the first entry
    //        if (SubjectIncidences.Count == 0)
    //        {
    //            SubjectIncidences.Add(new SubjectIncidence(score: score, folderName: folderName));
    //        }

    //        // Else, find the item and insert it
    //        else
    //        {
    //            idx = SubjectIncidences.FindIndex(incidence => incidence.FolderName == folderName);                            // Find a matching pair

    //            // If it doesn't exist, add an entry. If it does exist, increase the count
    //            if (idx == -1)
    //            {
    //                SubjectIncidences.Add(new SubjectIncidence(score: score, folderName: folderName));
    //            }

    //            else
    //            {
    //                SubjectIncidences[idx].Score += score;
    //            }

    //        }
    //    }
        
    //}


    public static class SubjectMapModule
    {
        public static int SubjectMapCt = 0;
        public static SubjectMapEntry[] SubjectMap;
        public static string[] WordList;
        public static int WordCount;
        
        public static SubjectIncidence[] Subject_Inc;
        public static int Subject_Inc_Ct;

        public static void Subject_MAP_Text_File_READ(IFileSystemFolderPaths fs, IList<string> commonWords)
        {

            SubjectMapCt = 0;
            SubjectMap = new SubjectMapEntry[1];

            string[] fileContents = FileIO2.CSV_Read(filename: fs.Filenames.SubjectMap, fileaddress: fs.FldrPythonStaging, SkipHeaders: true);
            var rowQueue = new Queue<string>(fileContents);

            while (rowQueue.Count > 0)
            {
                SubjectMapCt += 1;
                Array.Resize(ref SubjectMap, (int)(SubjectMapCt + 1));
                SubjectMap[(int)SubjectMapCt].Folderpath = rowQueue.Dequeue();
                SubjectMap[(int)SubjectMapCt].EmailSubject = CommonWordsModule.StripCommonWords(rowQueue.Dequeue(), commonWords);
                SubjectMap[(int)SubjectMapCt].EmailSubjectCount = int.Parse(rowQueue.Dequeue());
            }

        }

        public static void Common_Words_Text_File_READ(IFileSystemFolderPaths fs)
        {
            string[] fileContents = FileIO2.CSV_Read(filename: fs.Filenames.CommonWords, fileaddress: fs.FldrPythonStaging, SkipHeaders: false);
            int i = 0;
            WordList = new string[fileContents.Length + 1];
            WordList[0] = "";
            WordCount = fileContents.Length;
            foreach (string row in fileContents)
            {
                i += 1;
                WordList[i] = row;
            }
        }

        public static void Subject_Map_Add(string Subj, string FolderName)
        {
            int Subject_Map_Idx;


            // Check to see if any mapping exists. If not, add the first entry
            if (SubjectMapCt == 0L)
            {
                SubjectMapCt = 1;
                Array.Resize(ref SubjectMap, 2);
                Subject_Map_Set(Subj, 1, FolderName, 1);
            }

            // Else, find the item and insert it
            else
            {
                Subject_Map_Idx = Subject_Map_Find(Subj, FolderName);              // Find a matching pair

                // If it doesn't exist, add an entry. If it does exist, increase the count
                if (Subject_Map_Idx == 0)
                {
                    SubjectMapCt = SubjectMapCt + 1;                             // Increase the max count
                    Array.Resize(ref SubjectMap, (int)(SubjectMapCt + 1));                               // Add another slot to the array
                    Subject_Map_Set(Subj, 1, FolderName, (int)SubjectMapCt);     // Set the value to the last spot in the array
                }
                else
                {
                    SubjectMap[Subject_Map_Idx].EmailSubjectCount = SubjectMap[Subject_Map_Idx].EmailSubjectCount + 1;
                }

            }
        }

        public static void Subject_Map_Set(string Subj, int SubjCt, string FolderName, int Subject_Map_Idx)
        {
            SubjectMap[Subject_Map_Idx].Folderpath = FolderName;
            SubjectMap[Subject_Map_Idx].EmailSubject = Subj;
            SubjectMap[Subject_Map_Idx].EmailSubjectCount = SubjCt;
        }

        public static int Subject_Map_Find(string Subj, string FolderName)
        {
            int Subject_Map_FindRet = default;
            int i;
            int Subject_Map_Idx;

            Subject_Map_Idx = 0;                                     // Initialize

            // Loop to try and find an entry that matches the subject and Folder Name
            var loopTo = (int)SubjectMapCt;
            for (i = 1; i <= loopTo; i++)
            {
                if ((SubjectMap[i].EmailSubject ?? "") == (Subj ?? "") & (SubjectMap[i].Folderpath ?? "") == (FolderName ?? ""))
                {
                    Subject_Map_Idx = i;
                    break;
                }
            }

            Subject_Map_FindRet = Subject_Map_Idx;
            return Subject_Map_FindRet;

        }

        public static void Subject_Inc_Add(string FolderName, int Val)
        {
            int Subject_Inc_Idx;


            // Check to see if any mapping exists. If not, add the first entry
            if (Subject_Inc_Ct == 0)
            {
                Subject_Inc_Ct = 1;
                Array.Resize(ref Subject_Inc, 2);
                Subject_Inc[Subject_Inc_Ct].FolderName = FolderName;
                Subject_Inc[Subject_Inc_Ct].Score = Val;
            }

            // Else, find the item and insert it
            else
            {
                Subject_Inc_Idx = Subject_Inc_Find(FolderName);                            // Find a matching pair

                // If it doesn't exist, add an entry. If it does exist, increase the count
                if (Subject_Inc_Idx == 0)
                {
                    Subject_Inc_Ct = Subject_Inc_Ct + 1;                                         // Increase the max count
                    Array.Resize(ref Subject_Inc, Subject_Inc_Ct + 1);                                  // Add another slot to the array
                    Subject_Inc[Subject_Inc_Ct].FolderName = FolderName;
                    Subject_Inc[Subject_Inc_Ct].Score = Val;
                }

                else
                {
                    Subject_Inc[Subject_Inc_Idx].Score = Subject_Inc[Subject_Inc_Idx].Score + Val;
                }

            }
        }

        public static int Subject_Inc_Find(string FolderName)
        {
            int Subject_Inc_FindRet = default;
            int i;
            int Subject_Inc_Idx;

            Subject_Inc_Idx = 0;                                     // Initialize

            // Loop to try and find an entry that matches the subject and Folder Name
            var loopTo = Subject_Inc_Ct;
            for (i = 1; i <= loopTo; i++)
            {
                if ((Subject_Inc[i].FolderName ?? "") == (FolderName ?? ""))
                {
                    Subject_Inc_Idx = i;
                    break;
                }
            }

            Subject_Inc_FindRet = Subject_Inc_Idx;
            return Subject_Inc_FindRet;

        }



    }
}
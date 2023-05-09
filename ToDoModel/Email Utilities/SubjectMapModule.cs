using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.CompilerServices;
using UtilitiesVB;

namespace ToDoModel
{

    internal static class SubjectMapModule
    {
        public static long SubjectMapCt = 0L;
        public static Subject_Mapping[] SubjectMap;
        public static string[] WordList;
        public static int WordCount;
        public struct Subject_Incidence
        {
            public int Val;
            public string fldr;
        }

        public static Subject_Incidence[] Subject_Inc;
        public static int Subject_Inc_Ct;

        public static void Subject_MAP_Text_File_READ(IFileSystemFolderPaths fs, IList<string> commonWords)
        {

            SubjectMapCt = 0L;
            SubjectMap = new Subject_Mapping[1];

            string[] fileContents = FileIO2.CSV_Read(filename: fs.Filenames.SubjectMap, fileaddress: fs.FldrPythonStaging, SkipHeaders: true);
            var rowQueue = new Queue<string>(fileContents);

            while (rowQueue.Count > 0)
            {
                SubjectMapCt += 1L;
                Array.Resize(ref SubjectMap, (int)(SubjectMapCt + 1));
                SubjectMap[(int)SubjectMapCt].Email_Folder = rowQueue.Dequeue();
                SubjectMap[(int)SubjectMapCt].Email_Subject = CommonWordsModule.StripCommonWords(rowQueue.Dequeue(), commonWords);
                SubjectMap[(int)SubjectMapCt].Email_Subject_Count = Conversions.ToInteger(rowQueue.Dequeue());
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
                SubjectMapCt = 1L;
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
                    SubjectMapCt = SubjectMapCt + 1L;                             // Increase the max count
                    Array.Resize(ref SubjectMap, (int)(SubjectMapCt + 1));                               // Add another slot to the array
                    Subject_Map_Set(Subj, 1, FolderName, (int)SubjectMapCt);     // Set the value to the last spot in the array
                }
                else
                {
                    SubjectMap[Subject_Map_Idx].Email_Subject_Count = SubjectMap[Subject_Map_Idx].Email_Subject_Count + 1;
                }

            }
        }


        public static void Subject_Map_Set(string Subj, int SubjCt, string FolderName, int Subject_Map_Idx)
        {
            SubjectMap[Subject_Map_Idx].Email_Folder = FolderName;
            SubjectMap[Subject_Map_Idx].Email_Subject = Subj;
            SubjectMap[Subject_Map_Idx].Email_Subject_Count = SubjCt;
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
                if ((SubjectMap[i].Email_Subject ?? "") == (Subj ?? "") & (SubjectMap[i].Email_Folder ?? "") == (FolderName ?? ""))
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
                Subject_Inc[Subject_Inc_Ct].fldr = FolderName;
                Subject_Inc[Subject_Inc_Ct].Val = Val;
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
                    Subject_Inc[Subject_Inc_Ct].fldr = FolderName;
                    Subject_Inc[Subject_Inc_Ct].Val = Val;
                }

                else
                {
                    Subject_Inc[Subject_Inc_Idx].Val = Subject_Inc[Subject_Inc_Idx].Val + Val;
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
                if ((Subject_Inc[i].fldr ?? "") == (FolderName ?? ""))
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
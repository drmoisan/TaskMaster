using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic;
using UtilitiesCS;
using System.Windows.Forms;

namespace UtilitiesCS
{

    public class CtfIncidenceList : SerializableList<CTF_Incidence>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private int _maxFoldersPerConv = 3;

        public CtfIncidenceList() : base() { }
        public CtfIncidenceList(string filename, string folderpath, string backupFilepath) : base(
            filename: filename, folderpath: folderpath,
            backupLoader: new CSVLoader<CTF_Incidence>(ReadTextFile),
            backupFilepath: backupFilepath,
            askUserOnError: true)
        {
            CTF_Inc = base.ToList().ToArray(Base1Simulation: true);
            CTF_Inc_Ct = CTF_Inc.Length - 1;
        }

        public CTF_Incidence[] CTF_Inc;
        public int CTF_Inc_Ct = 0;

        public void CTF_Inc_Position_ADD(int Inc_Num, Conversation_To_Folder CTF_Map)
        {
            int i, j;                                                                                                     // Variables to hold loop counters
            bool added;

            added = false;

            if (_maxFoldersPerConv == 1)                                                                                           // If the MaxFolders is 1 then do the second check
            {
                if (CTF_Map.Email_Conversation_Count > CTF_Inc[Inc_Num].Email_Conversation_Count[1])                             // If the conversation count is more than the folder stored,
                {
                    CTF_Incidence_SET(Inc_Num, 1, 1, CTF_Map);                                                                  // then call the subroutine to replace the value
                }
            }

            else                                                                                                                    // SECTION FOR WHEN MaxFolders IS MORE THAN 1
            {

                if (CTF_Inc[Inc_Num].Folder_Count < _maxFoldersPerConv)
                {
                    CTF_Inc[Inc_Num].Folder_Count = CTF_Inc[Inc_Num].Folder_Count + 1;                                               // If folder count is less than max, increase count
                }

                var loopTo = _maxFoldersPerConv - 1;
                for (i = 1; i <= loopTo; i++)                                                                                  // Sorting routine to insert the new value in sequential order
                {
                    if (CTF_Map.Email_Conversation_Count > CTF_Inc[Inc_Num].Email_Conversation_Count[i])                         // from largest folder count to least folder count. Items that
                    {
                        var loopTo1 = i;
                        for (j = _maxFoldersPerConv - 1; j >= loopTo1; j -= 1)                                                                  // have a lower count than all items up to the max will not be added
                        {
                            CTF_Inc[Inc_Num].Email_Conversation_Count[j + 1] = CTF_Inc[Inc_Num].Email_Conversation_Count[j];
                            CTF_Inc[Inc_Num].Email_Folder[j + 1] = CTF_Inc[Inc_Num].Email_Folder[j];
                        }
                        CTF_Inc[Inc_Num].Email_Conversation_Count[i] = CTF_Map.Email_Conversation_Count;
                        CTF_Inc[Inc_Num].Email_Folder[i] = CTF_Map.Email_Folder;
                        added = true;
                        break;
                    }
                }

                if (added == false)
                {

                    if (CTF_Map.Email_Conversation_Count > CTF_Inc[Inc_Num].Email_Conversation_Count[_maxFoldersPerConv])
                    {
                        CTF_Inc[Inc_Num].Email_Conversation_Count[_maxFoldersPerConv] = CTF_Map.Email_Conversation_Count;
                        CTF_Inc[Inc_Num].Email_Folder[_maxFoldersPerConv] = CTF_Map.Email_Folder;
                    }

                }

            }
        }

        public int CTF_Incidence_FIND(string ConvID)
        {
            int CTF_Incidence_FINDRet = default;
            int i;

            CTF_Incidence_FINDRet = 0;

            var loopTo = CTF_Inc_Ct;
            for (i = 0; i <= loopTo; i++)
            {
                if ((CTF_Inc[i].Email_Conversation_ID ?? "") == (ConvID ?? ""))
                {
                    CTF_Incidence_FINDRet = i;
                    break;
                }
            }

            return CTF_Incidence_FINDRet;

        }

        public void CTF_Incidence_INIT(int Inc_Num)
        {
            int i;

            var loopTo = _maxFoldersPerConv;
            for (i = 1; i <= loopTo; i++)                                                      // Loop through the number of Folders we are saving
            {
                CTF_Inc[Inc_Num].Folder_Count = 0;
                CTF_Inc[Inc_Num].Email_Conversation_Count[i] = 0;                                // Set count to 0 so any value wins
                CTF_Inc[Inc_Num].Email_Folder[i] = "===============================";            // Set Folder name to lines so that they will not be accepted if they show up in selection list
            }
        }

        public void CTF_Incidence_SET(int Inc_Num, int Inc_Position, int Folder_Count, Conversation_To_Folder Map)
        {
            CTF_Inc[Inc_Num].Folder_Count = Folder_Count;
            CTF_Inc[Inc_Num].Email_Conversation_ID = Map.Email_Conversation_ID;
            CTF_Inc[Inc_Num].Email_Conversation_Count[Inc_Position] = Map.Email_Conversation_Count;
            CTF_Inc[Inc_Num].Email_Folder[Inc_Position] = Map.Email_Folder;
        }

        // ****************************************************************************************************************************************************
        // ****This Subroutine Writes to the File System the conversation id's with the Folders that have the most emails from the conversation in them********
        // ****************************************************************************************************************************************************

        public void CTF_Incidence_Text_File_WRITE(string folderpath, string filename)
        {

            var listOutput = new List<string>();
            listOutput.Add("This file contains a mapping of folders to email conversations based on incidence");

            int i, j;
            var loopTo = CTF_Inc_Ct;
            for (i = 1; i <= loopTo; i++)
            {
                listOutput.Add(CTF_Inc[i].Email_Conversation_ID);
                listOutput.Add(CTF_Inc[i].Folder_Count.ToString());
                var loopTo1 = CTF_Inc[i].Folder_Count;
                for (j = 1; j <= loopTo1; j++)
                {
                    listOutput.Add(CTF_Inc[i].Email_Folder[j]);
                    listOutput.Add(CTF_Inc[i].Email_Conversation_Count[j].ToString());
                }
            }

            string filepath = Path.Combine(folderpath, filename);
            using (var sw = new StreamWriter(filepath, false, System.Text.Encoding.ASCII))
            {
                foreach (var line in listOutput)
                    sw.WriteLine(line);
            }

        }

        public static IList<CTF_Incidence> ReadTextFile(string filepath)
        {
            string[] fileContents = ReadFileToArray(filepath);
            Queue<string> lines = ArrayToQueue(fileContents);
            IList<CTF_Incidence> listCTF = ProcessQueue(lines);

            return listCTF;
        }

        public void CTF_Incidence_Text_File_READ(string folderpath, string filename)
        {
            InitializeInternalVariables();
            string filepath = Path.Combine(folderpath, filename);
            if (File.Exists(filepath))
            {
                string[] fileContents = ReadFileToArray(filepath);
                Queue<string> lines = ArrayToQueue(fileContents);
                IList<CTF_Incidence> listCTF = ProcessQueue(lines);
                CTF_Inc = listCTF.ToArray(Base1Simulation: true);
                CTF_Inc_Ct = listCTF.Count - 1;
            }
            else
            {
                MessageBox.Show("Error", "Index file not found. Please run indexer.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeInternalVariables()
        {
            int i;
            CTF_Inc_Ct = 0;
            CTF_Inc = new CTF_Incidence[1];
        }

        private static IList<CTF_Incidence> ProcessQueue(Queue<string> lines)
        {
            IList<CTF_Incidence> listCTF = new List<CTF_Incidence>();

            int i;
            while (lines.Count > 0)
            {
                var tmpCTF_Inc = new CTF_Incidence();
                tmpCTF_Inc.Email_Conversation_ID = lines.Dequeue();
                tmpCTF_Inc.Folder_Count = int.Parse(lines.Dequeue());
                var loopTo = tmpCTF_Inc.Folder_Count;
                for (i = 1; i <= loopTo; i++)
                {
                    tmpCTF_Inc.Email_Folder[i] = lines.Dequeue();
                    tmpCTF_Inc.Email_Conversation_Count[i] = int.Parse(lines.Dequeue());
                }
                listCTF.Add(tmpCTF_Inc);
            }

            return listCTF;
        }

        private static Queue<string> ArrayToQueue(string[] array)
        {
            var queue = new Queue<string>(array.Skip(1));
            return queue;
        }

        private static string[] ReadFileToArray(string filepath)
        {
            string[] filecontents = null;
            try
            {
                filecontents = File.ReadAllLines(filepath, System.Text.Encoding.ASCII);
            }
            catch (FileNotFoundException e)
            {
                log.Error(e);
                throw;
            }
            catch (System.Exception e)
            {
                log.Error(e);
                throw;
            }

            return filecontents;

        }
    }
}
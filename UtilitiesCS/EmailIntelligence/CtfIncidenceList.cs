using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic;
using UtilitiesCS;
using System.Windows.Forms;
using System.Diagnostics;


namespace UtilitiesCS
{
    //TODO: Deprecate this CtfIncidence class
    public class CtfIncidenceList : SerializableList<CtfIncidence>
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region "Constructors and private variables"

        public CtfIncidenceList() : base() { }
        
        public CtfIncidenceList(string filename, string folderpath, string backupFilepath) : base(
            filename: filename, folderpath: folderpath,
            backupLoader: new CSVLoader<CtfIncidence>(ReadTextFile),
            backupFilepath: backupFilepath,
            askUserOnError: true) { }

        private int _maxFoldersPerConv = 3;

        #endregion

        //public CTF_Incidence[] CTF_Inc;
        
        public int CTF_Inc_Ct = 0;

        public void CtfIncidencePositionAdd(int idx, CtfMapEntry CtfMap)
        {
            // Variables to hold loop counters
            int i, j;                                                                                                     
            
            bool added;

            added = false;

            // If the MaxFolders is 1 then do the second check
            if (_maxFoldersPerConv == 1)                                                                                           
            {
                // If the conversation count is more than the folder stored,
                if (CtfMap.EmailCount > this[idx].EmailCounts[1])                             
                    
                {
                    // then call the subroutine to replace the value
                    CTF_Incidence_SET(idx, 1, 1, CtfMap);
                }
            }

            // SECTION FOR WHEN MaxFolders IS MORE THAN 1
            else
            {

                if (this[idx].FolderCount < _maxFoldersPerConv)
                {
                    // If folder count is less than max, increase count
                    this[idx].FolderCount++;                                               
                }

                var loopTo = _maxFoldersPerConv - 1;
                // Sorting routine to insert the new value in sequential order
                for (i = 1; i <= loopTo; i++)                                                                                  
                {
                    // from largest folder count to least folder count. Items that
                    if (CtfMap.EmailCount > this[idx].EmailCounts[i])
                    {
                        var loopTo1 = i;
                        // have a lower count than all items up to the max will not be added
                        for (j = _maxFoldersPerConv - 1; j >= loopTo1; j -= 1)                                                                  
                        {
                            this[idx].EmailCounts[j + 1] = this[idx].EmailCounts[j];
                            this[idx].EmailFolders[j + 1] = this[idx].EmailFolders[j];
                        }
                        this[idx].EmailCounts[i] = CtfMap.EmailCount;
                        this[idx].EmailFolders[i] = CtfMap.EmailFolder;
                        added = true;
                        break;
                    }
                }

                if (added == false)
                {
                    if (CtfMap.EmailCount > this[idx].EmailCounts[_maxFoldersPerConv])
                    {
                        this[idx].EmailCounts[_maxFoldersPerConv] = CtfMap.EmailCount;
                        this[idx].EmailFolders[_maxFoldersPerConv] = CtfMap.EmailFolder;
                    }
                }

            }
        }

        //public IEnumerable<CtfIncidence> GetByID(string conversationID, int topN)
        //{
        //    return this.OrderByDescending(entry => entry.E;
        //}

        public int FindID(string ConvID)
        {
            return this.FindIndex(x => x.EmailConversationID == ConvID);
            //int CTF_Incidence_FINDRet = default;
            //int i;

            //CTF_Incidence_FINDRet = 0;

            //var loopTo = CTF_Inc_Ct;
            //for (i = 0; i <= loopTo; i++)
            //{
            //    if ((CTF_Inc[i].Email_Conversation_ID ?? "") == (ConvID ?? ""))
            //    {
            //        CTF_Incidence_FINDRet = i;
            //        break;
            //    }
            //}

            //return CTF_Incidence_FINDRet;

        }

        public void CTF_Incidence_INIT(int Inc_Num)
        {
            int i;

            var loopTo = _maxFoldersPerConv;
            for (i = 1; i <= loopTo; i++)                                                      // Loop through the number of Folders we are saving
            {
                this[Inc_Num].FolderCount = 0;
                this[Inc_Num].EmailCounts[i] = 0;                                // Set count to 0 so any value wins
                this[Inc_Num].EmailFolders[i] = "===============================";            // Set Folder name to lines so that they will not be accepted if they show up in selection list
            }
        }

        public void CTF_Incidence_SET(int Inc_Num, int Inc_Position, int Folder_Count, CtfMapEntry Map)
        {
            this[Inc_Num].FolderCount = Folder_Count;
            this[Inc_Num].EmailConversationID = Map.ConversationID;
            this[Inc_Num].EmailCounts[Inc_Position] = Map.EmailCount;
            this[Inc_Num].EmailFolders[Inc_Position] = Map.EmailFolder;
        }

        #region "Backup Loader and Writer"
            

        
        #endregion

        #region "Deprecated Backup Loader and Writer"

        /// <summary>
        /// Method Writes a csv to the File System the conversation id's with 
        /// the Folders that have the most emails from the conversation in them
        /// </summary>
        /// <param name="folderpath"></param>
        /// <param name="filename"></param>
        public void CTF_Incidence_Text_File_WRITE(string folderpath, string filename)
        {

            var listOutput = new List<string>();
            listOutput.Add("This file contains a mapping of folders to email conversations based on incidence");

            int i, j;
            var loopTo = CTF_Inc_Ct;
            for (i = 1; i <= loopTo; i++)
            {
                listOutput.Add(this[i].EmailConversationID);
                listOutput.Add(this[i].FolderCount.ToString());
                var loopTo1 = this[i].FolderCount;
                for (j = 1; j <= loopTo1; j++)
                {
                    listOutput.Add(this[i].EmailFolders[j]);
                    listOutput.Add(this[i].EmailCounts[j].ToString());
                }
            }

            string filepath = Path.Combine(folderpath, filename);
            using (var sw = new StreamWriter(filepath, false, System.Text.Encoding.ASCII))
            {
                foreach (var line in listOutput)
                    sw.WriteLine(line);
            }

        }

        public static IList<CtfIncidence> ReadTextFile(string filepath)
        {
            string[] fileContents = ReadFileToArray(filepath);
            Queue<string> lines = ArrayToQueue(fileContents);
            IList<CtfIncidence> listCTF = ProcessQueue(lines);

            return listCTF;
        }

        public static IList<CtfIncidence> ProcessQueue(Queue<string> lines)
        {
            IList<CtfIncidence> listCTF = new List<CtfIncidence>();

            while (lines.Count > 0)
            {
                var incidence = TryDequeueIncidence(ref lines);
                if (incidence is not null) { listCTF.Add(incidence); }
            }

            return listCTF;
        }

        public static CtfIncidence TryDequeueIncidence(ref Queue<string> lines)
        {
            var incidence = new CtfIncidence();
            try 
            {
                incidence.EmailConversationID = lines.Dequeue();
                incidence.FolderCount = int.Parse(lines.Dequeue());
                var loopTo = incidence.FolderCount;
                for (int i = 1; i <= loopTo; i++)
                {
                    incidence.EmailFolders.Add(lines.Dequeue());
                    incidence.EmailCounts.Add(int.Parse(lines.Dequeue()));
                }
                return incidence;
            }
            catch (System.FormatException e)
            {
                string message = $"Error converting to int at line {e.GetLineNumber()} in {nameof(CtfIncidence)}.{nameof(TryDequeueIncidence)} of the backup loader";
                log.Error(message, e);
                Debug.WriteLine(message);
                DequeueToNextRecord(ref lines);
                return null;
            }
            catch (System.OverflowException e)
            {
                string message = $"Error converting to int at line {e.GetLineNumber()} in {nameof(CtfIncidence)}.{nameof(TryDequeueIncidence)} of the backup loader";
                log.Error(message, e);
                Debug.WriteLine(message);
                DequeueToNextRecord(ref lines);
                return null;
            }
            catch (System.InvalidOperationException e)
            {
                string message = $"Error dequeuing at line {e.GetLineNumber()} in {nameof(CtfIncidence)}.{nameof(TryDequeueIncidence)} of the backup loader";
                log.Error(message, e);
                Debug.WriteLine(message);
                DequeueToNextRecord(ref lines);
                return null;
            }   
        }

        internal static void DequeueToNextRecord(ref Queue<string> lines)
        {
            while ((lines.Count > 0)&&((lines.Peek().Length!=32)||lines.Peek().Contains(" ") || lines.Peek().Contains("\\")))
            {
                lines.Dequeue();
            }
        }

        internal static Queue<string> ArrayToQueue(string[] array)
        {
            // TODO: Move ArrayToQueue method to common location and make an extension if it is
            // not duplicative of other methods.
            // TODO: Parameterize the skip count if method is necessary
            var queue = new Queue<string>(array.Skip(1));
            return queue;
        }

        internal static string[] ReadFileToArray(string filepath)
        {
            //QUESTION: Is ReadFileToArray method duplicative of read csv? Should it be moved to a common location?
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

        #endregion
    }
}
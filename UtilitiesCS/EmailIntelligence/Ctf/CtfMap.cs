using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace UtilitiesCS
{
    public class CtfMap : ScoCollection<CtfMapEntry>
    {
        public CtfMap() : base() { }

        public CtfMap(IEnumerable<CtfMapEntry> enumerable) : base(enumerable) { }

        public CtfMap(string filename,
                      string folderpath,
                      string backupFilepath,
                      bool askUserOnError) : base(fileName: filename, 
                                                  folderPath: folderpath,
                                                  backupLoader: ReadTextFile,
                                                  backupFilepath: backupFilepath,
                                                  askUserOnError: askUserOnError) { }
                
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public CtfMapEntry[] TopEntriesById(string id, int topN)
        {
            var entries = this.Where(x => x.ConversationID == id)
                              .OrderByDescending(x => x.EmailCount)
                              .Take(topN)
                              .ToArray();
            return entries;
        }

        public void Add(string emailFolder, string conversationID, int emailCount)
        {
            var idx = this.FindIndex(x => (x.ConversationID == conversationID)&(x.EmailFolder == emailFolder));

            if (idx != -1)
            {
                this[idx].EmailCount += emailCount;
            }
            else
            {
                var entry = new CtfMapEntry(emailFolder, conversationID, emailCount);
                this.Add(entry);
            }
            
        }

        public bool ContainsId(string id)
        {
            var idx = this.FindIndex(x => x.ConversationID == id);
            return idx != -1;
        }

        public int FindId(string id)
        {
            int idx = this.FindIndex(x => x.ConversationID == id);
            return idx;
        }

        // TODO: Implement RebuildMap method
        public void RebuildMap(IApplicationGlobals appGlobals) 
        { 
            NotImplementedDialog.StopAtNotImplemented("RebuildMap");
        }

        #region Backup Loader

        public static IList<CtfMapEntry> ReadTextFile(string filePath)
        {
            string[] fileContents = ReadFileToArray(filePath);
            Queue<string> lines = ArrayToQueue(fileContents);
            IList<CtfMapEntry> listCTF = ProcessQueue(lines);

            return listCTF;
        }

        public static IList<CtfMapEntry> ProcessQueue(Queue<string> lines)
        {
            IList<CtfMapEntry> listCTF = new List<CtfMapEntry>();

            while (lines.Count > 0)
            {
                var incidence = TryDequeueEntry(ref lines);
                if (incidence is not null) { listCTF.Add(incidence); }
            }

            return listCTF;
        }

        public static CtfMapEntry TryDequeueEntry(ref Queue<string> lines)
        {
            var entry = new CtfMapEntry();
            try
            {
                entry.EmailFolder = lines.Dequeue();
                entry.ConversationID = lines.Dequeue();
                var emailCount = lines.Dequeue();
                entry.EmailCount = int.Parse(emailCount.Trim());
                return entry;
            }
            catch (System.FormatException e)
            {
                string message = $"Error converting to int at line {e.GetLineNumber()} in {nameof(CtfMapEntry)}.{nameof(TryDequeueEntry)} of the backup loader";
                log.Error(message, e);
                Debug.WriteLine(message);
                DequeueToNextRecord(ref lines);
                return null;
            }
            catch (System.OverflowException e)
            {
                string message = $"Error converting to int at line {e.GetLineNumber()} in {nameof(CtfMapEntry)}.{nameof(TryDequeueEntry)} of the backup loader";
                log.Error(message, e);
                Debug.WriteLine(message);
                DequeueToNextRecord(ref lines);
                return null;
            }
            catch (System.InvalidOperationException e)
            {
                string message = $"Error dequeuing at line {e.GetLineNumber()} in {nameof(CtfMapEntry)}.{nameof(TryDequeueEntry)} of the backup loader";
                log.Error(message, e);
                Debug.WriteLine(message);
                DequeueToNextRecord(ref lines);
                return null;
            }   
        }

        public static void DequeueToNextRecord(ref Queue<string> lines)
        {
            bool continueLoop = true;
            while ((lines.Count > 1) && continueLoop)
            {
                var line2Ahead = lines.ElementAt(1);
                if (IsEntryID(line2Ahead))
                {
                    continueLoop = false;
                }
                else { lines.Dequeue(); }
            }
            if (lines.Count == 1) { lines.Dequeue(); }
        }

        public static bool IsEntryID(string line)
        {
            if ((line.Length == 32) & (!line.Contains(" ")) & (!line.Contains("\\")))
            {
                return true;
            }
            else { return false; }
        }

        private static Queue<string> ArrayToQueue(string[] array)
        {
            // TODO: Move ArrayToQueue method to common location and make an extension if it is
            // not duplicative of other methods.
            // TODO: Parameterize the skip count if method is necessary
            var queue = new Queue<string>(array.Skip(1));
            return queue;
        }

        private static string[] ReadFileToArray(string filepath)
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

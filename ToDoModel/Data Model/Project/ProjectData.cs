using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using UtilitiesCS;
using UtilitiesCS.OutlookExtensions;
using Deedle;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace ToDoModel
{
    [Serializable()]
    public class ProjectData : SerializableList<IProjectEntry>, IProjectData
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ProjectData() : base() { }
        public ProjectData(IList<IProjectEntry> projInfoEntries) : base(projInfoEntries) { }
        public ProjectData(IEnumerable<IProjectEntry> projInfoEntries) : base(projInfoEntries) { }
        public ProjectData(string filename, string folderpath) : base(filename, folderpath) { }
        public ProjectData(string filename,
                           string folderpath,
                           CSVLoader<IProjectEntry> backupLoader,
                           string backupFilepath,
                           bool askUserOnError) : base(filename,
                                                       folderpath,
                                                       backupLoader,
                                                       backupFilepath,
                                                       askUserOnError)
        { }

        public void Save(string filepath) 
        {
            base.Sort();
            base.Serialize(filepath); 
        }
        public void Save() 
        {
            base.Sort();
            base.Serialize(); 
        }

        private Action<string, string> _idUpdateAction;
        public void SetIdUpdateAction(Action<string, string> action)
        {
            _idUpdateAction = action;
            foreach(var entry in this)
            {
                entry.SetIdUpdateAction(action);
            }
        }

        public (bool Any, int[] Indices) IsCorrupt()
        {
            if (this.Any()) 
            {
                var indices = Enumerable.Range(0, this.Count).Where(i => this[i].IsAnyNull()).ToArray();
                if (indices.Any()) { return (true, indices); }
                else { return (false, indices); }
            }
            else { return (false, new int[] { -1 }); } 
        }

        internal Frame<string, string> GetDfToDo(Outlook.Store store)
        {
            var table = store.GetToDoTable();
            
            if (table is null) { return null; }

            (var data, var columnInfo) = table.ETL();
            
            var df = DfDeedle.FromArray2D(data: data, columnInfo);
                        
            df = df.FillMissing("");

            df = Frame.FromRows(df.Rows.Where(x => (string)x.Value["ToDoID"] != ""));


            Frame<string, string> dfToDo = null;
            try
            {
                dfToDo = df.IndexRows<string>("ToDoID");
            }
            catch (Exception e)
            {                
                var duplicateIDs = df.GetDuplicateEntriesByColumn<int,string,string>("ToDoID");

                if (duplicateIDs.Count() > 0)
                {
                    var duplicateRowsGroups = duplicateIDs.Select(x => df.FilterRowsBy("ToDoID", x));
                    LogDuplicates(duplicateRowsGroups);
                    var dfTemp = DropDuplicates(df, duplicateRowsGroups);
                    try
                    {
                        dfToDo = dfTemp.IndexRows<string>("ToDoID");
                    }
                    catch (Exception e2)
                    {
                        logger.Error(e2.Message, e2);
                        throw;
                    }
                }

                else
                {
                    logger.Error(e.Message, e);
                    throw;
                }
            }

            return dfToDo;
        }


        private static Frame<int,string> DropDuplicates(Frame<int, string> df, IEnumerable<Frame<int, string>> duplicateRows)
        {
            var dfTemp = df.Clone();
            foreach (var frame in duplicateRows)
            {
                dfTemp = dfTemp.Exclude(frame.DropFirstN(1));
            }
            return dfTemp;
        }

        

        private static void LogDuplicates(IEnumerable<Frame<int, string>> duplicateRows)
        {
            var dfDuplicates = duplicateRows.FirstOrDefault();
            foreach (var frame in duplicateRows.Skip(1))
            {
                dfDuplicates = dfDuplicates.Merge(frame);
            }
            dfDuplicates.PrintToLog(logger);
        }

        /// <summary>
        /// Function filters a Deedle dataframe to entries that relate to projects 
        /// by utilizing the fact that project IDs are only 4 digits
        /// </summary>
        /// <param name="df">Deedle dataframe</param>
        /// <returns>Filtered Deedle dataframe</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal Frame<string, string> FilterToProjectIDs(Frame<string, string> df) 
        { 
            if (df is null) { return df; }
            
            df.AddColumn("IdLength", df.RowIndex.Keys.Select(id => id.Length));
            df = df.FilterRowsBy("IdLength", 4);
            df.DropColumn("IdLength");
            //df.Print();

            return df; 
        }

        /// <summary>
        /// Converts from a Deedle dataframe containing Project IDs and a list of 
        /// Outlook Categories to a list of project info entries. 
        /// Function parses Categories using <seealso cref=" FlagParser"/> class 
        /// and extracts suggested program name using a dash mark as a delimeter 
        /// between program name and project details in the project name. If no
        /// delimiter is present, project name is the same as program name
        /// </summary>
        /// <param name="df">Deedle dataframe with ToDoID and Categories</param>
        /// <returns>A new <seealso cref="List{T}"/> where T is <seealso cref="IProjectEntry"/></returns>
        internal List<IProjectEntry> DfToListEntries(Frame<string, string> df)
        {
            return df.Rows
                     .Select(row => new
                     {
                        ID = row.Key,
                         //ID = row.Value.GetAs<string>("ToDoID"),
                         Categories = row.Value.GetAs<string>("Categories")
                     })
                     .Values
                     .Select(x =>
                     {
                        var categories = x.Categories;
                        FlagParser parser = new(ref categories);
                        var projectName = parser.GetProjects();
                        var programName = projectName.Split('-')[0];
                        IProjectEntry entry = new ProjectEntry(projectName, x.ID, programName);
                        return entry;
                     })
                     .ToList();
        }

        /// <summary>
        /// Rebuilds the Project Info list from data existing in the underlying outlook
        /// ToDo items
        /// </summary>
        /// <param name="olApp">Handle to current <seealso cref="Outlook.Application"/></param>
        public void Rebuild(Outlook.Application olApp)
        {
            Frame<string,string> df = null;
            foreach (Outlook.Store store in olApp.Session.Stores)
            {
                var dfTemp = GetDfToDo(store);
                if (df is null) { df = dfTemp; }
                else if (dfTemp is not null) 
                {
                    //df.Print();
                    //dfTemp.Print();
                    try
                    {
                        df = df.Merge(dfTemp);
                    }
                    catch (Exception e)
                    {
                        
                        logger.Debug($"\n{TraceUtility.GetMyTraceString(new StackTrace())}\n");
                        var overlapIDs = df.RowIndex.Keys.Intersect(dfTemp.RowIndex.Keys).ToArray();
                        if (overlapIDs.Count() > 0)
                        { 
                            logger.Error($"{e.Message}\n\nOverlap found in following ToDoID's: {overlapIDs.SentenceJoin()}", e); 
                            var dfOverlap = df.Where(x => overlapIDs.Contains(x.Key));
                            dfOverlap.PrintToLog(logger);
                            var dfTempOverlap = dfTemp.Where(x => overlapIDs.Contains(x.Key));
                            dfTempOverlap.PrintToLog(logger);
                            dfTemp = dfTemp.Exclude(dfOverlap);
                            df = df.Merge(dfTemp);
                        }
                        else
                        {
                            logger.Error(e.Message, e);
                            throw;
                        }   

                    }
                    
                    df.PrintToLog(logger);
                }
            }

            df = FilterToProjectIDs(df);
            
            var result = DfToListEntries(df).OrderBy(x => x.ProjectID).ToList();
            
            this.FromList(result);
            this.Serialize();
        }

        public bool Contains_ProjectName(string projectName)
        {
            return base.FindIndex(x => x.ProjectName.ToLower() == projectName.ToLower()) != -1;
        }

        public string Programs_ByProjectNames(string projectNames)
        {
            try
            {
                var query = from project in projectNames.Split(',').Select(x => x.Trim())
                            join projectInfo in this on project equals projectInfo.ProjectName
                            select projectInfo.ProgramName;

                //string strTemp = query.First().ToString();
                string strTemp = string.Join(",",query.Distinct());

                return strTemp;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                return "";
            }

        }

        public List<IProjectEntry> Find_ByProjectName(string projectName)
        {
            return this.Where(x => x.ProjectName.ToLower() == projectName.ToLower()).ToList();
        }

        public bool Contains_ProjectID(string projectID)
        {
            return base.FindIndex(x => x.ProjectID == projectID) != -1;
        }

        public List<IProjectEntry> Find_ByProjectID(string projectID)
        {
            return this.Where(x => x.ProjectID == projectID).ToList();
        }

        public bool Contains_ProgramName(string programName)
        {
            return base.FindIndex(x => x.ProgramName.ToLower() == programName.ToLower()) != -1;
        }

        public List<IProjectEntry> Find_ByProgramName(string programName)
        {
            return this.Where(x => x.ProgramName.ToLower() == programName.ToLower()).ToList();
        }

        
    }

    
}
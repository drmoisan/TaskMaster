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

        public void Save(string filepath) => base.Serialize(filepath);
        public void Save() => base.Serialize();

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

        internal Frame<int, string> GetDfToDo(Microsoft.Office.Interop.Outlook.Store store)
        {
            var table = store.GetToDoTable();
            
            if (table is null) { return null; }

            (var data, var columnInfo) = table.ETL();
            
            Frame<int, string> df = DfDeedle.FromArray2D(data: data, columnInfo);
                        
            df = df.FillMissing("");

            df = Frame.FromRows(df.Rows.Where(x => (string)x.Value["ToDoID"] != ""));           
            return df;
        }

        /// <summary>
        /// Function filters a Deedle dataframe to entries that relate to projects 
        /// by utilizing the fact that project IDs are only 4 digits
        /// </summary>
        /// <param name="df">Deedle dataframe</param>
        /// <returns>Filtered Deedle dataframe</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal Frame<int, string> FilterToProjectIDs(Frame<int, string> df) 
        { 
            if (df is null) { return df; }
            if (!df.ColumnKeys.Contains("ToDoID")) 
            { throw new ArgumentOutOfRangeException(nameof(df), $"{nameof(df)} is missing column ToDoID"); }

            df.AddColumn("IdLength", df.GetColumn<string>("ToDoID").Select(id => id.Value.Length));
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
        internal List<IProjectEntry> DfToListEntries(Frame<int, string> df)
        {
            return df.Rows
                     .Select(row => new
                     {
                        ID = row.Value.GetAs<string>("ToDoID"),
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
            Frame<int,string> df = null;
            foreach (Outlook.Store store in olApp.Session.Stores)
            {
                var dfTemp = GetDfToDo(store);
                if (df is null) { df = dfTemp; }
                else if (dfTemp is not null) 
                {
                    //df.Print();
                    //dfTemp.Print();
                    df = df.Merge(dfTemp);
                    df.Print();
                }
            }

            df = FilterToProjectIDs(df);
            
            var result = DfToListEntries(df);
            
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
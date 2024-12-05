using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using UtilitiesCS;
using ToDoModel.Legacy;

namespace ToDoModel.Legacy
{

    [Serializable()]
    public class ProjectInfoLegacy : List<IProjectEntry>, IProjectInfoLegacy
    {
        public string FileName = "";

        public void Save(string FileName_IDList)
        {
            if (!Directory.Exists(Path.GetDirectoryName(FileName_IDList)))
            {
                var unused = Directory.CreateDirectory(Path.GetDirectoryName(FileName_IDList));
            }
            Stream TestFileStream = File.Create(FileName_IDList);
            var serializer = new BinaryFormatter();
            serializer.Serialize(TestFileStream, this);
            TestFileStream.Close();
            FileName = FileName_IDList;
        }

        public void Save()
        {
            if (FileName.Length > 0)
            {
                Stream TestFileStream = File.Create(FileName);
                var serializer = new BinaryFormatter();
                serializer.Serialize(TestFileStream, this);
                TestFileStream.Close();
            }
            else
            {
                MessageBox.Show("Can't save. IDList FileName not set yet");
            }
        }

        public bool Contains_ProjectName(string projectName)
        {
            return base.FindIndex(x => x.ProjectName.ToLower() == projectName.ToLower()) !=-1;
        }

        public string Programs_ByProjectNames(string projectNames)
        {
            try
            {
                var query = from project in projectNames.Split(',').Select(x => x.Trim())
                            join projectInfo in this on project equals projectInfo.ProjectName
                            select projectInfo.ProgramName;

                string strTemp = query.First().ToString();
                
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
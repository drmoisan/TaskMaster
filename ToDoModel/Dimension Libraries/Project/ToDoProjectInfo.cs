using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using UtilitiesVB;

namespace ToDoModel
{

    [Serializable()]
    public class ProjectInfo : List<IToDoProjectInfoEntry>, IProjectInfo
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
                var unused = Interaction.MsgBox("Can't save. IDList FileName not set yet");
            }
        }

        public bool Contains_ProjectName(string StrProjectName)
        {
            var common = StrProjectName.Split(Conversions.ToChar(", ")).ToList().Intersect(Enumerable.Select(b => b.ProjectName));
            return StrProjectName.Split(Conversions.ToChar(", ")).ToList().Intersect(Enumerable.Select(b => b.ProjectName)).ToList().Count > 0;
            // Return Me.Any(Function(p) String.Equals(p.ProjectName, StrProjectName, StringComparison.CurrentCulture))
        }

        public string Programs_ByProjectNames(string StrProjectNames)
        {
            try
            {
                //var names = this.Select(x => x.ProjectName);
                //string strTemp = string.Join(", ",
                //                 StrProjectNames.Split(',')
                //                 .Where(x =>names.Contains(x.Trim())));
                //var projects = StrProjectNames.Split(',').Select(x => x.Trim());
                var query = from project in StrProjectNames.Split(',').Select(x => x.Trim())
                            join projectInfo in this on project equals projectInfo.ProjectName
                            select projectInfo.ProgramName;

                string strTemp = query.First().ToString();
                //string strTemp = string.Join(", ", 
                //                             Enumerable
                //                             .Where(p => StrProjectNames.Split(", " , StringSplitOptions.None)
                //                             .ToList()
                //                             .Contains(p.ProjectName))
                //                             .Select(q => q.ProgramName)
                //                             .Distinct());
                return strTemp;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                return "";
            }

        }

        public List<IToDoProjectInfoEntry> Find_ByProjectName(string StrProjectName)
        {
            return Enumerable.Where(p => string.Equals(p.ProjectName, StrProjectName, StringComparison.CurrentCulture)).ToList();
        }

        public bool Contains_ProjectID(string StrProjectID)
        {
            // Dim common = StrProjectID.Split(", ").ToList().Intersect([Select](Function(b) b.ProjectID))
            // Return Me.Any(StrProjectID.Split(", ").ToList().Intersect([Select](Function(b) b.ProjectID)))
            return Enumerable.Any(p => string.Equals(p.ProjectID, StrProjectID, StringComparison.Ordinal));
        }

        public List<IToDoProjectInfoEntry> Find_ByProjectID(string StrProjectID)
        {
            return Enumerable.Where(p => string.Equals(p.ProjectID, StrProjectID, StringComparison.CurrentCulture)).ToList();
        }

        public bool Contains_ProgramName(string StrProgramName)
        {
            return Enumerable.Any(p => string.Equals(p.ProgramName, StrProgramName, StringComparison.CurrentCulture));
        }

        public List<IToDoProjectInfoEntry> Find_ByProgramName(string StrProgramName)
        {
            return Enumerable.Where(p => string.Equals(p.ProgramName, StrProgramName, StringComparison.CurrentCulture)).ToList();
        }
    }
}
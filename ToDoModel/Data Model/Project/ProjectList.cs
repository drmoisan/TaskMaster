using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ToDoModel
{
    [Serializable()]
    public class ProjectList
    {

        public Dictionary<string, string> ProjectDictionary;

        public ProjectList(Dictionary<string, string> dictProjectList)
        {
            ProjectDictionary = dictProjectList;
        }

        public void ToCSV(string FileName)
        {
            string csv = string.Join(Environment.NewLine, ProjectDictionary.Select(d => $"{d.Key};{d.Value};"));
            System.IO.File.WriteAllText(FileName, csv);
        }

    }
}
using System;
using System.Collections.Generic;


namespace UtilitiesCS
{
    public interface IProjectData: ISerializableList<IProjectEntry>
    {
        bool Contains_ProgramName(string programName);
        bool Contains_ProjectID(string projectID);
        bool Contains_ProjectName(string projectName);
        List<IProjectEntry> Find_ByProgramName(string programName);
        List<IProjectEntry> Find_ByProjectID(string projectID);
        List<IProjectEntry> Find_ByProjectName(string projectName);
        string Programs_ByProjectNames(string projectNames);
        void Save();
        void Save(string filepath);
        (bool Any, int[] Indices) IsCorrupt();
        void Rebuild(Microsoft.Office.Interop.Outlook.Application olApp);
        void SetIdUpdateAction(Action<string, string> action);
    }
}
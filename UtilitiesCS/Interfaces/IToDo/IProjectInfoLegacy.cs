using System.Collections;
using System.Collections.Generic;
using UtilitiesCS;

namespace UtilitiesCS
{
    public interface IProjectInfoLegacy : IList
    {
        void Save();
        void Save(string FileName_IDList);
        bool Contains_ProgramName(string StrProgramName);
        bool Contains_ProjectID(string StrProjectID);
        bool Contains_ProjectName(string StrProjectName);
        List<IProjectEntry> Find_ByProgramName(string StrProgramName);
        List<IProjectEntry> Find_ByProjectID(string StrProjectID);
        List<IProjectEntry> Find_ByProjectName(string StrProjectName);
        string Programs_ByProjectNames(string StrProjectNames);
    }
}
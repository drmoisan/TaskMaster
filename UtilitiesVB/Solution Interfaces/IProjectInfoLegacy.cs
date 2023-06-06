using System.Collections;
using System.Collections.Generic;
using UtilitiesCS;

namespace UtilitiesVB
{
    public interface IProjectInfoLegacy : IList
    {
        void Save();
        void Save(string FileName_IDList);
        bool Contains_ProgramName(string StrProgramName);
        bool Contains_ProjectID(string StrProjectID);
        bool Contains_ProjectName(string StrProjectName);
        List<IToDoProjectInfoEntry> Find_ByProgramName(string StrProgramName);
        List<IToDoProjectInfoEntry> Find_ByProjectID(string StrProjectID);
        List<IToDoProjectInfoEntry> Find_ByProjectName(string StrProjectName);
        string Programs_ByProjectNames(string StrProjectNames);
    }
}
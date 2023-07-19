
using System;

namespace UtilitiesCS
{
    public interface IToDoProjectInfoEntry
    {
        string ProgramName { get; set; }
        string ProjectID { get; set; }
        string ProjectName { get; set; }
        int CompareTo(IToDoProjectInfoEntry other);
        bool Equals(object obj);
        bool Equals(IToDoProjectInfoEntry other);
        string ToCSV();
        bool IsAnyNull();
        void SetIdUpdateAction(Action<string, string> action);
    }
}
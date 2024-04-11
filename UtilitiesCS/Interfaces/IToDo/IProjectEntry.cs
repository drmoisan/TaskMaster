
using System;

namespace UtilitiesCS
{
    public interface IProjectEntry
    {
        string ProgramName { get; set; }
        string ProjectID { get; set; }
        string ProjectName { get; set; }
        int CompareTo(IProjectEntry other);
        bool Equals(object obj);
        bool Equals(IProjectEntry other);
        string ToCSV();
        bool IsAnyNull();
        void SetIdUpdateAction(Action<string, string> action);
    }
}
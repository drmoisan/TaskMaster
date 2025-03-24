
using System;

namespace UtilitiesCS
{
    public interface IProjectEntry: IComparable<IProjectEntry>, IEquatable<IProjectEntry>, IComparable
    {
        string ProgramName { get; set; }
        string ProjectID { get; }
        string ProjectName { get; set; }
        string ProgramID { get; set; }
        bool Equals(object obj);        
        string ToCSV();
        bool IsAnyNull();
        void SetIdUpdateAction(Action<string, string> action);
    }
}
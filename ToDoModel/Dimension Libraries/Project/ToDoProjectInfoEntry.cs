using System;
using UtilitiesVB;
using UtilitiesCS;
using System.Collections.Generic;

namespace ToDoModel
{

    [Serializable()]
    public class ToDoProjectInfoEntry : IEquatable<IToDoProjectInfoEntry>, IComparable, IComparable<IToDoProjectInfoEntry>, IToDoProjectInfoEntry, IEquatable<ToDoProjectInfoEntry>
    {

        private string _projectName;
        private string _projectID;
        private string _programName;

        public string ProjectName { get => _projectName; set => _projectName = value; }
        public string ProjectID { get => _projectID; set => _projectID = value; }
        public string ProgramName { get => _programName; set => _programName = value; }

        public ToDoProjectInfoEntry(string ProjName, string ProjID, string ProgName)
        {
            ProjectName = ProjName;
            ProjectID = ProjID;
            ProgramName = ProgName;
        }

        public int CompareTo(IToDoProjectInfoEntry other)
        {
            if (other is null) { return 1; }
            else if (ProjectID is null) { return -1; }
            else
            {
                int x = string.CompareOrdinal(ProjectID, other.ProjectID);
                if (x == 0)
                {
                    if (ProjectID.Length < other.ProjectID.Length)
                    {
                        x = -1;
                    }
                    else if (ProjectID.Length > other.ProjectID.Length)
                    {
                        x = 1;
                    }
                }
                return x;
                // Return Me.ProjectID.CompareTo(Other.ProjectID)
            }
        }

        public int CompareTo(object obj)
        {
            if (obj is null)
                return 1;
            IToDoProjectInfoEntry other = obj as IToDoProjectInfoEntry;

            if (other is not null)
            {
                return CompareTo(other);
            }
            else
            {
                throw new ArgumentException("Object cannot be cast to IToDoProjectInfoEntry");
            }
        }

        public string ToCSV()
        {
            return ProjectID + "," + ProjectName + "," + ProgramName;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IToDoProjectInfoEntry);
        }

        public bool Equals(ToDoProjectInfoEntry other)
        {
            return other is not null &&
                   ProjectName == other.ProjectName &&
                   ProjectID == other.ProjectID &&
                   ProgramName == other.ProgramName;
        }

        public bool Equals(IToDoProjectInfoEntry other)
        {
            return other is not null &&
                   ProjectName == other.ProjectName &&
                   ProjectID == other.ProjectID &&
                   ProgramName == other.ProgramName;
        }

        public override int GetHashCode()
        {
            int hashCode = 682028280;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProjectName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProjectID);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProgramName);
            return hashCode;
        }
    
        public bool IsAnyNull() 
        { return (_projectName is null)||(_projectID is null)||(_programName is null); }
    }
}
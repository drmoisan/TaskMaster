using System;
using UtilitiesVB;

namespace ToDoModel
{

    [Serializable()]
    public class ToDoProjectInfoEntry : IEquatable<IToDoProjectInfoEntry>, IComparable, IComparable<IToDoProjectInfoEntry>, IToDoProjectInfoEntry
    {

        public string ProjectName { get; set; }
        public string ProjectID { get; set; }
        public string ProgramName { get; set; }

        public ToDoProjectInfoEntry(string ProjName, string ProjID, string ProgName)
        {
            ProjectName = ProjName;
            ProjectID = ProjID;
            ProgramName = ProgName;
        }

        public bool Equals(IToDoProjectInfoEntry other)
        {

            return other is not null && ProjectName.Equals(other.ProjectName);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            ToDoProjectInfoEntry other = obj as ToDoProjectInfoEntry;
            return other is not null && Equals(other);
        }

        public int CompareTo(IToDoProjectInfoEntry other)
        {
            if (other is null)
            {
                return 1;
            }
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
                // Return Me.ProjectID.CompareTo(other.ProjectID)
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

    }
}
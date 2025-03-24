using System;
using UtilitiesCS;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Office.Core;

namespace ToDoModel
{
    [Serializable()]
    public class ProjectEntry : IProjectEntry
    {
        private string _projectName;
        private string _projectID;
        private string _programName;
        private Action<string, string> _idUpdate;

        public string ProjectName { get => _projectName; set => _projectName = value; }
        public string ProgramName { get => _programName; set => _programName = value; }
        public string ProjectID 
        { 
            get => _projectID;
            //private set => _projectID = value;
            //[MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if ((value is not null) && (value.Length != 4))
                {
                    MessageBox.Show($"{nameof(ProjectID)} cannot be set with malformed value {value}." +
                        "Value should be 4 digits or characters");
                }
                else if (_projectID is null)
                {
                    _projectID = value;
                }
                else if (_projectID != value)
                {
                    var response = MessageBox.Show($"Are you sure you want to change {nameof(ProjectID)} from" +
                        $"{_projectID} to {value}", "Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (response == DialogResult.Yes)
                    {
                        if (_idUpdate is not null)
                        {
                            var response2 = MessageBox.Show("Would you like to change underlying outlook objects, " +
                            "child objects, and update ID List?", "Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (response2 == DialogResult.Yes)
                            {
                                _idUpdate.Invoke(_projectID, value);
                            }
                        }
                        _projectID = value;
                    }
                }

            }
        }
        private string _programID;
        public string ProgramID { get => _programID; set => _programID = value; }

        public ProjectEntry(string ProjName, string ProjID, string ProgName)
        {
            ProjectName = ProjName;                        
            ProjectID = ProjID;
            ProgramName = ProgName;
        }

        [JsonConstructor]
        public ProjectEntry(string ProjName, string ProjID, string ProgName, string programID)
        {
            ProjectName = ProjName;            
            ProjectID = ProjID;
            ProgramName = ProgName;
            ProgramID = programID;
        }

        public bool SetProjectId(string newID) 
        {
            if (ProjectID.IsNullOrEmpty() && !newID.IsNullOrEmpty())
            {
                ProjectID = newID;
                return true;
            }

            switch (newID)
            {
                case null:
                    ProjectID = newID;
                    return true;

                case string s when s.Length != 4:
                    MyBox.ShowDialog($"{nameof(ProjectID)} cannot be set with malformed value {newID}." +
                        "Value should be 4 digits or characters", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;

                case string s when s == ProjectID:
                    break;
                    
                case string s when s != ProjectID:
                    return ChangeId(newID);

                default:
                    throw new ArgumentException($"Unsupported value for {nameof(newID)} of {newID}");                   
            }
            
            return false;
        }

        internal bool ChangeId(string newID) 
        {
            var response = MyBox.ShowDialog($"Are you sure you want to change {nameof(ProjectID)} from" +
                    $"{ProjectID} to {newID}", "Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (response == DialogResult.Yes)
            {
                if (_idUpdate is not null)
                {
                    var response2 = MyBox.ShowDialog("Would you like to change underlying outlook objects, " +
                    "child objects, and update ID List?", "Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (response2 == DialogResult.Yes)
                    {
                        _idUpdate.Invoke(ProjectID, newID);
                    }
                }
                ProjectID = newID;
                return true;
            }
            return false;
        }
        
        public async Task<bool> SetProjectIdAsync(string newID, CancellationToken cancel)
        {
            return await Task.Run(() => SetProjectId(newID), cancel);
        }

        public void SetIdUpdateAction(Action<string, string> action) 
        { 
            _idUpdate = action;
        }

        public int CompareTo(IProjectEntry other)
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
            IProjectEntry other = obj as IProjectEntry;

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
            return $"{ProgramID},{ProgramName},{ProjectID},{ProjectName}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IProjectEntry);
        }

        public bool Equals(ProjectEntry other)
        {
            return other is not null &&
                   ProjectName == other.ProjectName &&
                   ProjectID == other.ProjectID &&
                   ProgramName == other.ProgramName;
        }

        public bool Equals(IProjectEntry other)
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
        { return (ProjectName is null)||(ProjectID is null)||(ProgramName is null); }
    }
}
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    public interface IToDoItem
    {
        bool ActiveBranch { get; set; }
        bool Bullpin { get; set; }
        bool Complete { get; set; }
        FlagTranslator Context { get; }
        DateTime DueDate { get; set; }
        bool EC_Change { get; set; }
        bool EC2 { get; set; }
        bool EC3 { get; set; }
        string ExpandChildren { get; set; }
        string ExpandChildrenState { get; set; }
        bool FlagAsTask { get; set; }
        FlagParser Flags { get; }
        bool IdAutoCoding { get; set; }
        string Identifier { get; set; }
        IIDList IdList { get; set; }
        string InFolder { get; }
        FlagTranslator KB { get; }
        string KBSimple { get; set; }
        string MetaTaskLvl { get; set; }
        string MetaTaskSubject { get; set; }
        OutlookItem OlItem { get; }
        FlagTranslator People { get; }
        OlImportance Priority { get; set; }
        FlagTranslator Program { get; }
        IProjectData ProjectData { get; set; }
        FlagTranslator Projects { get; }
        Func<string, string> ProjectsToPrograms { get; set; }
        bool ReadOnly { get; set; }
        DateTime ReminderTime { get; set; }
        DateTime StartDate { get; set; }
        DateTime TaskCreateDate { get; set; }
        string TaskSubject { get; set; }
        bool Today { get; set; }
        string ToDoID { get; set; }
        FlagTranslator Topics { get; }
        int TotalWork { get; set; }
        int VisibleTreeState { get; set; }

        Task ForceSave();
        object GetItem();
        string get_KB(bool IncludePrefix = false);
        bool get_PA_FieldExists(string PA_Schema);
        bool get_VisibleTreeStateLVL(int Lvl);
        void SetKB(bool IncludePrefix = false, string value = null);
        void set_VisibleTreeStateLVL(int Lvl, bool value);
        void SplitID();
        Task WriteFlagsBatch();
        void WriteFlagsBatch(Enums.FlagsToSet flagsToSet);
    }
}
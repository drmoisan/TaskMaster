using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;

namespace UtilitiesCS
{
    public interface IMovedMailInfo
    {
        string EntryId { get; set; }
        
        [JsonIgnore]
        Folder FolderOld { get; set; }
        
        string FolderPathNew { get; set; }
        string folderPathOld { get; set; }
        
        [JsonIgnore]
        MailItem MailItem { get; set; }
        
        [JsonIgnore]
        Application olApp { get; set; }

        [JsonIgnore]
        string olRootPath { get; set; }
        string StoreId { get; set; }

        MailItem UndoMove();
        string UndoMoveMessage(Outlook.Application olApp);
    }
}
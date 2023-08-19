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
        string FolderPathOld { get; set; }
        
        [JsonIgnore]
        MailItem MailItem { get; set; }
        
        [JsonIgnore]
        Application OlApp { get; set; }

        [JsonIgnore]
        string OlRootPath { get; set; }
        string StoreId { get; set; }

        bool UndoMove();
        string UndoMoveMessage(Outlook.Application olApp);
    }
}
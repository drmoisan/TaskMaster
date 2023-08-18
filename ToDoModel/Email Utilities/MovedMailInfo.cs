using Microsoft.Office.Interop.Outlook;
using Outlook = Microsoft.Office.Interop.Outlook;
using Newtonsoft.Json;
using UtilitiesCS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace ToDoModel
{
    public class MovedMailInfo : IMovedMailInfo
    {
        public MovedMailInfo() { }

        public MovedMailInfo(MailItem beforeMove, MailItem afterMove, string olRootPath)
        {
            _olRootPath = olRootPath;
            _mailItem = afterMove;
            var folderNew = (Folder)afterMove.Parent;
            _folderPathNew = folderNew.FolderPath.Replace(_olRootPath, "");
            _storeId = folderNew.StoreID;
            _entryId = afterMove.EntryID;
            _folderOld = (Folder)beforeMove.Parent;
            _folderPathOld = _folderOld.FolderPath.Replace(_olRootPath, "");
        }

        private string _folderPathOld;
        public string FolderPathOld { get => _folderPathOld; set => _folderPathOld = value; }

        private string _folderPathNew;
        public string FolderPathNew { get => _folderPathNew; set => _folderPathNew = value; }

        private string _entryId;
        public string EntryId { get => _entryId; set => _entryId = value; }

        private string _storeId;
        public string StoreId { get => _storeId; set => _storeId = value; }

        private Outlook.Application _olApp;
        [JsonIgnore]
        public Outlook.Application OlApp
        {
            get => _olApp;
            set
            {
                _olApp = value;
                _olRootPath = _olApp.Session.DefaultStore.GetRootFolder().FolderPath;
            }
        }

        private string _olRootPath;
        public string OlRootPath { get => _olRootPath; set => _olRootPath = value; }

        private MailItem _mailItem;
        [JsonIgnore]
        public MailItem MailItem
        {
            get
            {
                if (_mailItem is null && this.OlApp is not null)
                {
                    _mailItem = (MailItem)this.OlApp.Session.GetItemFromID(this.EntryId, this.StoreId);
                }
                return _mailItem;
            }
            set => _mailItem = value;
        }

        private Folder _folderOld;
        [JsonIgnore]
        public Folder FolderOld
        {
            get
            {
                if (_folderOld is null && NotNull(FolderPathOld, OlRootPath, OlApp))
                {
                    _folderOld = FolderHandler.GetFolder($"{OlRootPath}\\{FolderPathOld}", OlApp);
                }
                return _folderOld;
            }
            set => _folderOld = value;
        }

        internal bool NotNull(params object[] parameters) => parameters.Any(x => x is null);

        public bool IsReadyToUndoMove { get => NotNull(MailItem, FolderOld);}

        public bool UndoMove()
        {
            if (NotNull(MailItem, FolderOld))
            {
                MailItem.Move(FolderOld);
                return true;
            }
            return false;
        }

    }
}

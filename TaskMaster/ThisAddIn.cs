using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Outlook;
using Microsoft.VisualBasic.FileIO;
using ToDoModel;

namespace TaskMaster
{

    public partial class ThisAddIn
    {

        private ApplicationGlobals _globals;

        public List<string> CatFilterList;
        private Items _OlToDoItems;

        private Items OlToDoItems
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _OlToDoItems;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_OlToDoItems != null)
                {
                    _OlToDoItems.ItemAdd -= OlToDoItems_ItemAdd;
                    _OlToDoItems.ItemChange -= OlToDoItems_ItemChange;
                }

                _OlToDoItems = value;
                if (_OlToDoItems != null)
                {
                    _OlToDoItems.ItemAdd += OlToDoItems_ItemAdd;
                    _OlToDoItems.ItemChange += OlToDoItems_ItemChange;
                }
            }
        }
        private List<Items> ListOfPSTtodo;
        private List<Items> ListToDoItems;
        private Items _OlInboxItems;

        private Items OlInboxItems
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _OlInboxItems;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _OlInboxItems = value;
            }
        }
        private Reminders _OlReminders;

        private Reminders OlReminders
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _OlReminders;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _OlReminders = value;
            }
        }
        private NameSpace OlNS;

        private RibbonController _ribbonController;

        private readonly string _filenameProjectList;
        private readonly string _filenameProjInfo2;
        private readonly string _filenameProjInfo = "ProjInfo.bin";
        public readonly string FilenameDictPpl = "pplkey.xml";
        public readonly string StagingPath = SpecialDirectories.MyDocuments;
        public string EmailRoot;
        private const string _appDataFolder = "TaskMaster";

        public ProjectInfo ProjInfo;
        public Dictionary<string, string> DictPPL;
        private ListOfIDs _IDList;

        public ListOfIDs IDList
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _IDList;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _IDList = value;
            }
        }
        public TreeOfToDoItems DM_CurView;
        public FlagParser Cats;

        private void ThisAddIn_Startup()
        {
            _globals = new ApplicationGlobals(Application);

            {
                ref var withBlock = ref _globals;
                OlNS = withBlock.Ol.NamespaceMAPI;
                OlToDoItems = withBlock.Ol.ToDoFolder.Items;
                OlInboxItems = withBlock.Ol.Inbox.Items;
                OlReminders = withBlock.Ol.OlReminders;
                ProjInfo = (ProjectInfo)withBlock.TD.ProjInfo;
                DictPPL = withBlock.TD.DictPPL;
                IDList = (ListOfIDs)withBlock.TD.IDList;
                EmailRoot = withBlock.Ol.EmailRootPath;
            }

            _ribbonController.SetGlobals(_globals);

        }

        protected override IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            _ribbonController = new RibbonController();
            return new RibbonViewer(_ribbonController);
        }

        #region Explorer Event Hooks
        internal void Events_Hook()
        {
            {
                ref var withBlock = ref _globals;
                OlToDoItems = withBlock.Ol.ToDoFolder.Items;
                OlInboxItems = withBlock.Ol.Inbox.Items;
                OlReminders = withBlock.Ol.OlReminders;
            }
        }

        internal void Events_Unhook()
        {
            OlToDoItems = null;
            OlInboxItems = null;
            OlReminders = null;
        }

        private void OlToDoItems_ItemAdd(object Item)
        {
            ToDoEvents.OlToDoItems_ItemAdd(Item, _globals);
        }

        private void OlToDoItems_ItemChange(object Item)
        {
            ToDoEvents.OlToDoItems_ItemChange(Item, OlToDoItems, _globals);
        }
        #endregion

    }
}
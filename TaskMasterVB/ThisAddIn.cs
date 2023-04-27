using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Outlook;
using System.IO;
using ToDoModel;
using System;
using UtilitiesVB;

namespace TaskMaster
{

    public partial class ThisAddIn
    {

        private IApplicationGlobals _globals;

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
        
        public readonly string FilenameDictPpl = "pplkey.xml";
        public readonly string StagingPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
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
            //_globals = new ApplicationGlobals(Application);

            {
                OlNS = _globals.Ol.NamespaceMAPI;
                OlToDoItems = _globals.Ol.ToDoFolder.Items;
                OlInboxItems = _globals.Ol.Inbox.Items;
                OlReminders = _globals.Ol.OlReminders;
                ProjInfo = (ProjectInfo)_globals.TD.ProjInfo;
                DictPPL = _globals.TD.DictPPL;
                IDList = (ListOfIDs)_globals.TD.IDList;
                EmailRoot = _globals.Ol.EmailRootPath;
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
                OlToDoItems = _globals.Ol.ToDoFolder.Items;
                OlInboxItems = _globals.Ol.Inbox.Items;
                OlReminders = _globals.Ol.OlReminders;
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
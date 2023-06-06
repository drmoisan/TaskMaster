using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Outlook = Microsoft.Office.Interop.Outlook;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Interop.Outlook;
using System.Runtime.CompilerServices;
using ToDoModel;
using Microsoft.Office.Core;
using QuickFiler;

namespace TaskMaster
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            _globals = new ApplicationGlobals(Application);

            {
                DebugTextWriter tw = new DebugTextWriter();
                Console.SetOut(tw);
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


        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            // Note: Outlook no longer raises this event. If you have code that 
            //    must run when Outlook shuts down, see https://go.microsoft.com/fwlink/?LinkId=506785
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}

using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesCS;

namespace TaskMaster
{
    public class AppEvents : IAppEvents
    {
        public AppEvents(IApplicationGlobals appGlobals)
        {
            _globals = appGlobals;
        }

        private IApplicationGlobals _globals;

        private Items _olToDoItems;
        private Items OlToDoItems
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _olToDoItems;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_olToDoItems != null)
                {
                    _olToDoItems.ItemAdd -= OlToDoItems_ItemAdd;
                    _olToDoItems.ItemChange -= OlToDoItems_ItemChange;
                }

                _olToDoItems = value;
                if (_olToDoItems != null)
                {
                    _olToDoItems.ItemAdd += OlToDoItems_ItemAdd;
                    _olToDoItems.ItemChange += OlToDoItems_ItemChange;
                }
            }
        }
        
        private Items _olInboxItems;
        private Items OlInboxItems
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _olInboxItems;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _olInboxItems = value;
            }
        }

        private Reminders _olReminders;
        private Reminders OlReminders
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _olReminders;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                _olReminders = value;
            }
        }


        #region Events

        public void Hook()
        {
            {
                OlToDoItems = _globals.Ol.ToDoFolder.Items;
                OlInboxItems = _globals.Ol.Inbox.Items;
                OlReminders = _globals.Ol.OlReminders;
            }
        }

        public void Unhook()
        {
            OlToDoItems = null;
            OlInboxItems = null;
            OlReminders = null;
        }

        private void OlToDoItems_ItemAdd(object item)
        {
            ToDoEvents.OlToDoItems_ItemAdd(item, _globals);
        }

        private void OlToDoItems_ItemChange(object item)
        {
            ToDoEvents.OlToDoItems_ItemChange(item, OlToDoItems, _globals);
        }

        #endregion


    }
}
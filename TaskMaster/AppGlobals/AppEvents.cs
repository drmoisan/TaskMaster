using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesCS;
using UtilitiesCS.EmailIntelligence;
using UtilitiesCS.OutlookExtensions;
using System.Windows.Forms;
using System.Threading;
using UtilitiesCS.Extensions;
using System.Collections.Concurrent;


namespace TaskMaster
{
    public class AppEvents : IAppEvents
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AppEvents(IApplicationGlobals globals)
        {
            Globals = globals;
        }
                
        //public ConcurrentBag<IConditionalEngine<MailItemHelper>> InboxEngines {get; protected set; } = [];

        internal async Task<AppEvents> LoadAsync()
        {
            //await Task.WhenAll(SetupSpamBayesAsync(), SetupTriageAsync());
            await ProcessNewInboxItemsAsync();

            return this;
        }

        internal IApplicationGlobals Globals {get; set; }

        private Items _olToDoItems;
        public Items OlToDoItems
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _olToDoItems;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            private set
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
                if (_olInboxItems != null)
                {
                    _olInboxItems.ItemAdd -= OlInboxItems_ItemAdd;
                }
                _olInboxItems = value;
                if (_olInboxItems != null)
                {
                    _olInboxItems.ItemAdd += OlInboxItems_ItemAdd;
                }
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
                OlToDoItems = Globals.Ol.ToDoFolder.Items;
                OlInboxItems = Globals.Ol.Inbox.Items;
                OlReminders = Globals.Ol.OlReminders;
            }
        }

        public void Unhook()
        {
            OlToDoItems = null;
            OlInboxItems = null;
            OlReminders = null;
        }
                
        internal async Task LogAsync(string message)
        {
            await Task.Run(() => logger.Debug(message));
        }
                
        private void OlToDoItems_ItemAdd(object item)
        {
            ToDoEvents.OlToDoItems_ItemAdd(item, Globals);
        }

        private async void OlToDoItems_ItemChange(object item)
        {
            await ToDoEvents.OlToDoItems_ItemChange(item, OlToDoItems, Globals);
        }

        private async void OlInboxItems_ItemAdd(object item)
        {
            await ProcessMailItemAsync(item);               
        }

        public async Task ProcessMailItemAsync(object item)
        {
            if (item is MailItem mailItem)
            {
                var engines = await Globals.Engines.InboxEngines
                    .ToAsyncEnumerable()
                    .Where(kvp => kvp.Value is not null)
                    .WhereAwait(async kvp => await kvp.Value.AsyncCondition(mailItem))
                    .Where(kvp => kvp.Value.Engine is not null)
                    .ToArrayAsync();

                if (engines.Length > 0)
                {
                    var helper = await MailItemHelper.FromMailItemAsync(mailItem, Globals, default, false);
                    await Task.Run(() => _ = helper.Tokens);
                    await engines.ToAsyncEnumerable().ForEachAwaitAsync(async e => await e.Value.AsyncAction(helper));
                    helper.Item.SetUdf("AutoProcessed", true, OlUserPropertyType.olYesNo);
                }
            }
        }

        internal async Task ProcessNewInboxItemsAsync()
        {
            if (OlInboxItems is not null)
            {
                // Restrict to unprocessed items
                string filter = $"@SQL=\"{OlTableExtensions.SchemaCustomPrefix}AutoProcessed\" is null";
                
                var olMailItems = OlInboxItems.Restrict("[MessageClass] = 'IPM.Note'");
                var unprocessedItems = olMailItems.Restrict(filter);

                await unprocessedItems
                    .Cast<object>()
                    .ToAsyncEnumerable()
                    .ForEachAwaitAsync(ProcessMailItemAsync);
                
                logger.Debug("Finished processing new inbox items");
            }
        }

        #endregion

    }
}
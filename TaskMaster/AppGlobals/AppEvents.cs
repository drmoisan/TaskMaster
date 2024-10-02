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

        internal async Task SetupSpamBayesAsync()
        {
            var ce = new ConditionalItemEngine<MailItemHelper>();
            
            ce.AsyncCondition = (item) => Task.Run(() => 
                item is MailItem mailItem && mailItem.MessageClass == "IPM.Note" && 
                mailItem.UserProperties.Find("Spam") is null);

            ce.EngineInitializer = async (globals) => ce.Engine = await SpamBayes.CreateAsync(globals);
            await ce.EngineInitializer(Globals);
            ce.AsyncAction = (item) => ce.Engine is not null ? ((SpamBayes)ce.Engine).TestAsync(item) : null;
            ce.EngineName = "SpamBayes";
            ce.Message = $"{ce.EngineName} is null. Skipping actions";
            //InboxEngines.Add(ce);
            Globals.Engines.InboxEngines.TryAdd(ce.EngineName, ce);

        }

        internal async Task LogAsync(string message)
        {
            await Task.Run(() => logger.Debug(message));
        }

        internal async Task SetupTriageAsync()
        {
            var ce = new ConditionalItemEngine<MailItemHelper>();

            ce.AsyncCondition = (item) => Task.Run(() =>
                item is MailItem mailItem && mailItem.MessageClass == "IPM.Note" &&
                mailItem.UserProperties.Find("Triage") is null);

            ce.EngineInitializer = async (globals) => ce.Engine = await Triage.CreateAsync(globals);
            await ce.EngineInitializer(Globals);
            ce.AsyncAction = (item) => ce.Engine is not null ? ((Triage)ce.Engine).TestAsync(item) : null;
            ce.EngineName = "Triage";
            ce.Message = $"{ce.EngineName} is null. Skipping actions";
            //InboxEngines.Add(ce);
            Globals.Engines.InboxEngines.TryAdd(ce.EngineName, ce);
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
                    .WhereAwait(async e => await e.Value.AsyncCondition(mailItem))
                    .Where(e => e.Value.Engine is not null)
                    .ToArrayAsync();

                if (engines.Length > 0)
                {
                    var helper = await MailItemHelper.FromMailItemAsync(mailItem, Globals, default, false);
                    await Task.Run(() => _ = helper.Tokens);
                    await engines.ToAsyncEnumerable().ForEachAwaitAsync(async e => await e.Value.AsyncAction(helper));
                }
            }
        }

        internal async Task ProcessNewInboxItemsAsync()
        {
            if (OlInboxItems is not null)
            {
                // Restrict to unread MailItems
                var unreadItems = OlInboxItems.Restrict("[UnRead] = true");

                foreach (object item in unreadItems)
                {
                    await ProcessMailItemAsync(item);
                }
            }
        }

        #endregion

    }
}
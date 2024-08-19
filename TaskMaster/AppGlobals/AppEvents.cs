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
using UtilitiesCS.EmailIntelligence.ClassifierGroups.Triage;
using System.Collections.Concurrent;


namespace TaskMaster
{
    public class AppEvents : IAppEvents
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AppEvents(IApplicationGlobals globals)
        {
            _globals = globals;
        }

        private ConcurrentBag<ConditionalItemEngine<MailItemHelper>> _mailAddEngines = [];
        internal ConcurrentBag<ConditionalItemEngine<MailItemHelper>> MailAddEngines => _mailAddEngines;
        
        internal async Task<AppEvents> LoadAsync()
        {
            var spamBayesTask = Task.Run(SetupSpamBayesAsync);
            var triageTask = Task.Run(SetupTriageAsync);
            await Task.WhenAll(spamBayesTask, triageTask);
            await ProcessNewInboxItemsAsync();

            return this;
        }

        private IApplicationGlobals _globals;
        
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

        
        internal async Task SetupSpamBayesAsync()
        {
            var ce = new ConditionalItemEngine<MailItemHelper>();
            
            ce.AsyncCondition = (item) => Task.Run(() => 
                item is MailItem mailItem && mailItem.MessageClass == "IPM.Note" && 
                mailItem.UserProperties.Find("Spam") is null);

            ce.EngineInitializer = async () => ce.Engine = await SpamBayes.CreateAsync(_globals);
            await ce.EngineInitializer();
            ce.AsyncAction = (item) => ce.Engine is not null ? ((SpamBayes)ce.Engine).TestAsync(item) : null;
            ce.EngineName = "SpamBayes";
            ce.Message = $"{ce.EngineName} is null. Skipping actions";
            MailAddEngines.Add(ce);

            //    (item) => LogAsync("SpamBayes is null. Skipping spam check");
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

            ce.EngineInitializer = async () => ce.Engine = await Triage.CreateAsync(_globals);
            await ce.EngineInitializer();
            ce.AsyncAction = (item) => ce.Engine is not null ? ((Triage)ce.Engine).TestAsync(item) : null;
            ce.EngineName = "Triage";
            ce.Message = $"{ce.EngineName} is null. Skipping actions";
            MailAddEngines.Add(ce);

        }

        private void OlToDoItems_ItemAdd(object item)
        {
            ToDoEvents.OlToDoItems_ItemAdd(item, _globals);
        }

        private void OlToDoItems_ItemChange(object item)
        {
            ToDoEvents.OlToDoItems_ItemChange(item, OlToDoItems, _globals);
        }

        private async void OlInboxItems_ItemAdd(object item)
        {
            var engines = await MailAddEngines.ToAsyncEnumerable().WhereAwait(async e => await e.AsyncCondition(item)).Where(e => e.Engine is not null).ToArrayAsync();
            if (engines.Count() > 0) 
            {
                var helper = await MailItemHelper.FromMailItemAsync(item as MailItem, _globals, default, false);
                await Task.Run(() => _ = helper.Tokens);
                await engines.ToAsyncEnumerable().ForEachAwaitAsync(async e => await e.AsyncAction(helper));
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
                    if (item is MailItem mailItem)
                    {
                        var engines = await MailAddEngines.ToAsyncEnumerable()
                            .WhereAwait(async e => await e.AsyncCondition(mailItem))
                            .Where(e => e.Engine is not null)
                            .ToArrayAsync();

                        if (engines.Length > 0)
                        {
                            var helper = await MailItemHelper.FromMailItemAsync(mailItem, _globals, default, false);
                            await Task.Run(() => _ = helper.Tokens);
                            await engines.ToAsyncEnumerable().ForEachAwaitAsync(async e => await e.AsyncAction(helper));
                        }
                    }
                }
            }
        }

        //private async void OlInboxItems_ItemAdd_TriageFilter(object item)
        //{
        //    if (item is MailItem mailItem && mailItem.MessageClass == "IPM.Note" && mailItem.UserProperties.Find("Triage") is null)
        //    {
        //        if (_triage is null)
        //        {
        //            logger.Debug("Triage is null. Skipping triage.");
        //        }
        //        else
        //        {
        //            await _triage.TestAsync(mailItem);
        //        }
        //    }
        //}


        #endregion

        internal class ConditionalItemEngine<T> 
        {
            public ConditionalItemEngine() { }

            public ConditionalItemEngine(
                object engine,
                string engineName,
                Func<object, Task<bool>> asyncCondition,
                Func<T, Task> asyncAction,
                string message)
            {
                Engine = engine;
                EngineName = engineName;
                AsyncCondition = asyncCondition.ThrowIfNull();
                AsyncAction = asyncAction.ThrowIfNull();
                Message = message.ThrowIfNull();   
            }

            public Func<object, Task<bool>> AsyncCondition { get; set; }
            public Func<T, Task> AsyncAction { get; set; }            
            public string Message { get; set; }
            public object Engine { get; set; }
            public Func<Task> EngineInitializer { get; set; }
            public string EngineName { get; set; }
            public T TypedItem { get; set; }


            
        }

    }
}
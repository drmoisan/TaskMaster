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
using TaskMaster.Properties;
using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Dictionary;
using UtilitiesCS.ReusableTypeClasses;
using UtilitiesCS.OutlookObjects.Fields;


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
            if (Settings.Default.EventsHooked) { Hook(); }
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

        //private Items _olInboxItems;
        //private Items OlInboxItems
        //{
        //    [MethodImpl(MethodImplOptions.Synchronized)]
        //    get
        //    {
        //        return _olInboxItems;
        //    }

        //    [MethodImpl(MethodImplOptions.Synchronized)]
        //    set
        //    {
        //        if (_olInboxItems != null)
        //        {
        //            _olInboxItems.ItemAdd -= OlInboxItems_ItemAdd;
        //        }
        //        _olInboxItems = value;
        //        if (_olInboxItems != null)
        //        {
        //            _olInboxItems.ItemAdd += OlInboxItems_ItemAdd;
        //        }
        //    }
        //}

        internal LockingLinkedList<Items> OlInboxes { get; set; } = new();

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
                OlReminders = Globals.Ol.OlReminders;
                Globals.Ol.Inboxes.ForEach(x => OlInboxes.AddLast(x.Items, items => items.ItemAdd += OlInboxItems_ItemAdd));
            }
        }
                
        public void Unhook()
        {
            OlToDoItems = null;            
            OlReminders = null;
            OlInboxes.Clear(items => items.ItemAdd -= OlInboxItems_ItemAdd);                  
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
            try
            {
                await ToDoEvents.OlToDoItems_ItemChange(item, OlToDoItems, Globals);
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        internal async void OlInboxItems_ItemAdd(object item)
        {
            try
            {
                await ProcessMailItemAsync(item);               
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<bool> ProcessMailItemAsync(object item)
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
                    return true;
                }
            }
            return false;
        }

        public async Task ProcessNewInboxItemsAsync()
        {
            if (!OlInboxes.IsNullOrEmpty())
            {
                // Restrict to unprocessed items
                string filter = $"@SQL=\"{MAPIFields.Schemas.CustomPrefix}AutoProcessed\" is null";                
                var unprocessedQueue = new ConcurrentQueue<object>();

                foreach (var inbox in OlInboxes)
                {
                    var olMailItems = inbox.Restrict("[MessageClass] = 'IPM.Note'");
                    var unprocessedItems = olMailItems?.Restrict(filter)?
                        .Cast<object>()
                        .Where(x => x is MailItem)
                        .Cast<MailItem>()
                        .Where(x => x.UserProperties.Find("AutoProcessed") is null)
                        .ToArray();
                    //var unprocessedItems = olMailItems?.Restrict("[AutoProcessed] Is Null")?.Cast<object>();
                    if (unprocessedItems is null) { continue; }
                    unprocessedItems.ForEach(x => unprocessedQueue.Enqueue(x));
                }
                
                int errors = 0;
                int success = 0;
                var unprocessedCount = unprocessedQueue.Count();
                logger.Debug($"Unprocessed queue has {unprocessedCount} items");

                var syncContext = SynchronizationContext.Current;

                while (unprocessedQueue.Count > 0)
                {
                    var remaining = unprocessedQueue.Count();
                    if (unprocessedQueue.TryDequeue(out var item) && await ProcessMailItemAsync(item))
                    {
                        success++;                        
                        logger.Debug($"Successfully processed item {success + errors} of {unprocessedCount} in the unprocessed Queue");
                    }
                    else if (++errors == 3) 
                    {
                        var response = MyBox.ShowDialog($"Tried to process remaining {remaining} unprocessed " +
                            $"items 3 times without success. Continue trying?","Error",
                            MessageBoxButtons.YesNo,MessageBoxIcon.Hand);

                        if (response == DialogResult.No) 
                        { 
                            logger.Warn($"Tried to process remaining {remaining} unprocessed items 3 times without success. Exiting loop.");
                            break; 
                        }                        
                    }
                    else
                    {
                        logger.Debug($"Error processing item {success + errors} of {unprocessedCount} in the unprocessed Queue");
                        //if (item != default) { unprocessedQueue.Enqueue(item); }
                        await Task.Delay(100);
                    }

                    // Pump messages to keep the UI responsive
                    syncContext?.Post(_ => System.Windows.Forms.Application.DoEvents(), null);
                }
                logger.Debug($"Successfully processed {success} of {unprocessedCount} items in the " +
                    $"unprocessed Queue with {errors} errors");

                logger.Debug("Finished processing new inbox items");
            }
        }

        #endregion

    }
}
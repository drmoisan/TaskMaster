using log4net.Repository.Hierarchy;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuickFiler.Helper_Classes
{
    // TODO: Determine what EmailMoveMonitor was supposed to be used for. It is now malfunctioning. Temprorarily disabling.
    internal class EmailMoveMonitor
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public EmailMoveMonitor()
        {
            SetupBeforeItemMove();
        }

        private List<EmailMoveAction> _hookedItems = [];

        public void HookItem(
            MailItem mail,
            Action<MailItem> moveAction)
        {
            lock (_hookedItems)
            {
                Folder folder = (Folder)mail.Parent;
                if (!_hookedItems.Any(x => x.Folder.EntryID == folder.EntryID))
                    folder.BeforeItemMove += BeforeItemMove;
                _hookedItems.Add(new EmailMoveAction(mail, folder, moveAction));
            }
        }

        public void UnhookItem(MailItem mail)
        {
            if (mail is null) { return; }
            lock (_hookedItems)
            {
                var count = _hookedItems.Count(x => x.Folder.EntryID == (mail.Parent as Folder)?.EntryID);
                var hookedItem = _hookedItems.FirstOrDefault(x => x.Mail.EntryID == mail.EntryID);
                if (hookedItem != null)
                {
                    if (count == 1)
                        hookedItem.Folder.BeforeItemMove -= BeforeItemMove;
                    _hookedItems.Remove(hookedItem);
                }
            }
        }

        public async Task UnhookItemAsync(MailItem mail, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();

            if (mail is null) 
            { 
                //logger.Debug("Mail item is null. Returning.");
                return; 
            }
            var parent = await GetParentFolderAsync(mail);
            if (parent is null) 
            { 
                //logger.Debug("Parent folder is null. Returning.");
                return; 
            }
            lock (_hookedItems)
            {
                var count = _hookedItems.Count(x => x.Folder.EntryID == parent.EntryID);
                var hookedItem = _hookedItems.FirstOrDefault(x => x.Mail.EntryID == mail.EntryID);
                if (hookedItem != null)
                {
                    if (count == 1)
                        hookedItem.Folder.BeforeItemMove -= BeforeItemMove;
                    _hookedItems.Remove(hookedItem);
                }
            }
        }

        private async Task<Folder> GetParentFolderAsync(MailItem mail, int remaining = 2)
        {
            if (mail is null) { return null; }
                        
            var parentFolder = await Task.Run(async () => 
            {
                try
                {
                    return mail.Parent as Folder;
                }
                catch (System.Exception e)
                {
                    string entryId = "";
                    try
                    {
                        entryId = mail.EntryID;
                    }
                    catch (System.Exception ex)
                    {
                        entryId = "[Error getting EntryID]";
                    }

                    if (remaining > 0)
                    {
                        logger.Error($"Error getting parent folder for mail item {entryId}. {remaining} remaining attempts.");
                        return await GetParentFolderAsync(mail, remaining - 1);
                    }
                    else
                    {
                        logger.Error($"Error getting parent folder for mail item {entryId}. No remaining attempts. Returning null", e);
                        return null;
                    }
                }
                
            });

            return parentFolder;

        }

        public void UnhookAll()
        {
            lock (_hookedItems)
            {
                foreach (var item in _hookedItems)
                {
                    item.Folder.BeforeItemMove -= BeforeItemMove;
                }
                _hookedItems.Clear();
            }
        }

        private MAPIFolderEvents_12_BeforeItemMoveEventHandler BeforeItemMove;
        private void SetupBeforeItemMove()
        {
            BeforeItemMove = delegate (object item, MAPIFolder moveTo, ref bool cancel)
            {
                if (item is MailItem mail)
                {
                    lock (_hookedItems)
                    {
                        var hookedItem = _hookedItems.FirstOrDefault(x => x.Mail.EntryID == mail.EntryID);
                        if (hookedItem != null)
                        {
                            hookedItem.MoveAction(mail);
                            _hookedItems.Remove(hookedItem);
                        }
                    }
                }
            };
        }




    }

    internal class EmailMoveAction
    {
        public EmailMoveAction(
            MailItem mail,
            Folder folder,
            Action<MailItem> moveAction)
        {
            _mail = mail;
            _folder = folder;
            _moveAction = moveAction;
        }

        private MailItem _mail;
        public MailItem Mail => _mail;

        private Folder _folder;
        public Folder Folder => _folder;

        private Action<MailItem> _moveAction;
        public Action<MailItem> MoveAction => _moveAction;
    }


}

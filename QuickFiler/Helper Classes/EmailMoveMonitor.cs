using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net.Repository.Hierarchy;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using UtilitiesCS;

namespace QuickFiler.Helper_Classes
{
    // Watches Outlook's BeforeItemMove event on behalf of a single owning controller, so that an
    // owner can register a per-MailItem action to run when that item is moved out of the folder it
    // was staged from. Hooks are held in an instance-scoped list: BeforeItemMove dispatches at most
    // one action per MailItem via FirstOrDefault, and UnhookAll clears that whole list, which is why
    // each owner constructs its own monitor rather than sharing one (issue #731 finding 1).
    internal class EmailMoveMonitor : IEmailMoveMonitor
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        /// <summary>
        /// Marshals an action onto the captured Outlook STA thread. All Outlook COM member
        /// access in this class flows through this delegate so that calls originating on
        /// ThreadPool/background threads do not raise cross-thread COMExceptions.
        /// </summary>
        private readonly Action<System.Action> _marshalToSta;

        /// <param name="marshalToSta">
        /// Optional marshal-to-STA delegate. When null (production default), Outlook COM access
        /// is dispatched synchronously onto the captured UI/STA thread via
        /// <see cref="UiThread.Dispatcher"/>. Tests supply a deterministic pass-through such as
        /// <c>a =&gt; a()</c>. Mirrors the default-to-real-implementation seam style used for
        /// <c>TimeProvider</c> in <c>QfcDatamodel</c>.
        /// </param>
        public EmailMoveMonitor(Action<System.Action> marshalToSta = null)
        {
            _marshalToSta = marshalToSta ?? (action => UiThread.Dispatcher.Invoke(action));
            SetupBeforeItemMove();
        }

        private List<EmailMoveAction> _hookedItems = [];

        public void HookItem(MailItem mail, Action<MailItem> moveAction)
        {
            // Marshal all Outlook COM access (mail.Parent, folder.EntryID, BeforeItemMove +=)
            // and the EmailMoveAction construction (which reads EntryIDs) onto the STA thread.
            _marshalToSta(() =>
            {
                lock (_hookedItems)
                {
                    Folder folder = (Folder)mail.Parent;
                    string folderEntryId = folder.EntryID;
                    if (!_hookedItems.Any(x => x.FolderEntryId == folderEntryId))
                        folder.BeforeItemMove += BeforeItemMove;
                    _hookedItems.Add(new EmailMoveAction(mail, folder, moveAction));
                }
            });
        }

        public void UnhookItem(MailItem mail)
        {
            if (mail is null)
            {
                return;
            }
            // Marshal the COM-dependent reads (mail.EntryID, mail.Parent, folder.EntryID) and the
            // BeforeItemMove -= unsubscribe onto the STA thread. Live reads are compared against
            // the cached EntryID strings captured at hook time.
            _marshalToSta(() =>
            {
                string mailEntryId = mail.EntryID;
                string parentFolderEntryId = (mail.Parent as Folder)?.EntryID;
                lock (_hookedItems)
                {
                    var count = _hookedItems.Count(x => x.FolderEntryId == parentFolderEntryId);
                    var hookedItem = _hookedItems.FirstOrDefault(x => x.MailEntryId == mailEntryId);
                    if (hookedItem != null)
                    {
                        if (count == 1)
                            hookedItem.Folder.BeforeItemMove -= BeforeItemMove;
                        _hookedItems.Remove(hookedItem);
                    }
                }
            });
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
            // Dormant member (no active caller). Marshal the COM-dependent reads and the
            // BeforeItemMove -= unsubscribe onto the STA thread, comparing live reads against
            // the cached EntryID strings, consistent with the active UnhookItem path.
            _marshalToSta(() =>
            {
                string parentEntryId = parent.EntryID;
                string mailEntryId = mail.EntryID;
                lock (_hookedItems)
                {
                    var count = _hookedItems.Count(x => x.FolderEntryId == parentEntryId);
                    var hookedItem = _hookedItems.FirstOrDefault(x => x.MailEntryId == mailEntryId);
                    if (hookedItem != null)
                    {
                        if (count == 1)
                            hookedItem.Folder.BeforeItemMove -= BeforeItemMove;
                        _hookedItems.Remove(hookedItem);
                    }
                }
            });
        }

        private async Task<Folder> GetParentFolderAsync(MailItem mail, int remaining = 2)
        {
            if (mail is null)
            {
                return null;
            }

            // Dormant member (no active caller). Marshal the COM read (mail.Parent / mail.EntryID)
            // onto the STA thread instead of the prior Task.Run hop, which was the cross-thread
            // defect pattern. The retry/recursion and log4net logging are preserved.
            Folder parentFolder = null;
            System.Exception comFailure = null;
            _marshalToSta(() =>
            {
                try
                {
                    parentFolder = mail.Parent as Folder;
                }
                catch (System.Exception e)
                {
                    comFailure = e;
                }
            });

            if (comFailure is null)
            {
                return parentFolder;
            }

            string entryId = "";
            _marshalToSta(() =>
            {
                try
                {
                    entryId = mail.EntryID;
                }
                catch (System.Exception)
                {
                    entryId = "[Error getting EntryID]";
                }
            });

            if (remaining > 0)
            {
                logger.Error(
                    $"Error getting parent folder for mail item {entryId}. {remaining} remaining attempts."
                );
                return await GetParentFolderAsync(mail, remaining - 1);
            }
            else
            {
                logger.Error(
                    $"Error getting parent folder for mail item {entryId}. No remaining attempts. Returning null",
                    comFailure
                );
                return null;
            }
        }

        public void UnhookAll()
        {
            // Marshal the per-item BeforeItemMove -= unsubscribe (Outlook COM) onto the STA thread,
            // preserving the lock scope and clearing the bookkeeping list exactly once.
            _marshalToSta(() =>
            {
                lock (_hookedItems)
                {
                    foreach (var item in _hookedItems)
                    {
                        item.Folder.BeforeItemMove -= BeforeItemMove;
                    }
                    _hookedItems.Clear();
                }
            });
        }

        private MAPIFolderEvents_12_BeforeItemMoveEventHandler BeforeItemMove;

        private void SetupBeforeItemMove()
        {
            BeforeItemMove = delegate(object item, MAPIFolder moveTo, ref bool cancel)
            {
                if (item is MailItem mail)
                {
                    lock (_hookedItems)
                    {
                        var hookedItem = _hookedItems.FirstOrDefault(x =>
                            x.Mail.EntryID == mail.EntryID
                        );
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
        /// <summary>
        /// Captures the mail/folder pair and its move action. The <paramref name="mail"/> and
        /// <paramref name="folder"/> EntryID strings are read once here so callers can compare
        /// against stable cached identifiers instead of re-reading live COM properties. This
        /// constructor must run on the STA thread (its EntryID reads touch Outlook COM).
        /// </summary>
        public EmailMoveAction(MailItem mail, Folder folder, Action<MailItem> moveAction)
        {
            _mail = mail;
            _folder = folder;
            _moveAction = moveAction;
            _mailEntryId = mail.EntryID;
            _folderEntryId = folder.EntryID;
        }

        private MailItem _mail;
        public MailItem Mail => _mail;

        private Folder _folder;
        public Folder Folder => _folder;

        private Action<MailItem> _moveAction;
        public Action<MailItem> MoveAction => _moveAction;

        private readonly string _mailEntryId;

        /// <summary>Stable mail EntryID captured at hook time on the STA thread.</summary>
        public string MailEntryId => _mailEntryId;

        private readonly string _folderEntryId;

        /// <summary>Stable parent-folder EntryID captured at hook time on the STA thread.</summary>
        public string FolderEntryId => _folderEntryId;
    }
}

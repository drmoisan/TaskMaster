using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickFiler.Helper_Classes
{
    // TODO: Determine what EmailMoveMonitor was supposed to be used for. It is now malfunctioning. Temprorarily disabling.
    internal class EmailMoveMonitor
    {
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
            lock (_hookedItems)
            {
                var count = _hookedItems.Count(x => x.Folder.EntryID == ((Folder)mail.Parent).EntryID);
                var hookedItem = _hookedItems.FirstOrDefault(x => x.Mail.EntryID == mail.EntryID);
                if (hookedItem != null)
                {
                    if (count == 1)
                        hookedItem.Folder.BeforeItemMove -= BeforeItemMove;
                    _hookedItems.Remove(hookedItem);
                }
            }
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

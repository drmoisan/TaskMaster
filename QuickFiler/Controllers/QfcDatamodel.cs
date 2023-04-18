using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesVB;
using UtilitiesCS;

namespace QuickFiler
{
    internal class QfcDatamodel : IQfcDatamodel
    {
        public QfcDatamodel(Explorer ActiveExplorer) 
        { 
            _activeExplorer = ActiveExplorer;
            var listEmailsInFolder = FolderSuggestionsModule.LoadEmailDataBase(_activeExplorer);
            _masterQueue = new Queue<MailItem>();
            foreach (MailItem email in listEmailsInFolder)
            {
                _masterQueue.Enqueue(email);
            }
        }

        private Explorer _activeExplorer;
        private Queue<MailItem> _masterQueue;
        private StackObjectCS<MailItem> _movedMails;

        public StackObjectCS<MailItem> MovedMails { get => _movedMails; set => _movedMails = value; }

        public void CountMailsInConv(int ct = 0)
        {
            throw new NotImplementedException();
        }

        public IList<MailItem> DequeueNextEmailGroup(int quantity)
        {
            int i;
            IList<MailItem> listEmails = new List<MailItem>();
            int adjustedQuantity = quantity < _masterQueue.Count ? quantity : _masterQueue.Count;
            for (i = 1; i <= adjustedQuantity; i++)
                listEmails.Add(_masterQueue.Dequeue());
            return listEmails;
        }
        
        public bool MoveEmails(ref StackObjectCS<MailItem> MovedMails)
        {
            throw new NotImplementedException();
        }

        public void UndoMove()
        {
            throw new NotImplementedException();
        }
    }
}

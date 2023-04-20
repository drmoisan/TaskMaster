using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoModel;
using UtilitiesVB;
using UtilitiesCS;

namespace QuickFiler.Controllers
{
    internal class QfcDatamodel : IQfcDatamodel
    {
        public QfcDatamodel(Explorer ActiveExplorer) 
        { 
            _activeExplorer = ActiveExplorer;
            var listEmailsInFolder = FolderSuggestionsModule.LoadEmailDataBase(_activeExplorer);
            _masterQueue = new Queue<object>();
            foreach (MailItem email in listEmailsInFolder)
            {
                _masterQueue.Enqueue(email);
            }
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Explorer _activeExplorer;
        private Queue<object> _masterQueue;
        private StackObjectCS<object> _movedObjects;

        public StackObjectCS<object> StackMovedItems { get => _movedObjects; set => _movedObjects = value; }

        public IList<object> DequeueNextItemGroup(int quantity)
        {
            int i;
            IList<object> listObjects = new List<object>();
            int adjustedQuantity = quantity < _masterQueue.Count ? quantity : _masterQueue.Count;
            for (i = 1; i <= adjustedQuantity; i++)
                listObjects.Add(_masterQueue.Dequeue());
            return listObjects;
        }

        public void UndoMove()
        {
            throw new NotImplementedException();
        }

        public bool MoveItems(ref StackObjectCS<object> StackMovedItems)
        {
            throw new NotImplementedException();
        }

        public void CountMailsInConv(int ct = 0)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using UtilitiesCS;
using static QuickFiler.Controllers.QfcCollectionController;

namespace QuickFiler.Controllers
{
    public class QfcItemGroup
    {
        public QfcItemGroup() { }

        public QfcItemGroup(MailItem mailItem)
        {
            _mailItem = mailItem;
        }

        private MailItem _mailItem;
        internal MailItem MailItem
        {
            get => _mailItem;
            set => _mailItem = value;
        }

        internal ItemViewer ItemViewer
        {
            get => _itemViewer;
            set => _itemViewer = value;
        }
        private ItemViewer _itemViewer;

        internal IQfcItemController ItemController
        {
            get => _itemController;
            set => _itemController = value;
        }
        private IQfcItemController _itemController;
    }
}

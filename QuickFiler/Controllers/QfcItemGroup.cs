using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickFiler.Interfaces;
using static QuickFiler.Controllers.QfcCollectionController;
using System.Windows.Forms;
using UtilitiesCS;
using System.Collections.Specialized;
using System.ComponentModel;

namespace QuickFiler.Controllers
{
    public class QfcItemGroup
    {
        public QfcItemGroup() { }
        public QfcItemGroup(MailItem mailItem) { _mailItem = mailItem; }

        private IQfcItemController _itemController;
        private MailItem _mailItem;

        private ItemViewer _itemViewer;
        internal ItemViewer ItemViewer { get => _itemViewer; set => _itemViewer = value; }

        internal IQfcItemController ItemController { get => _itemController; set => _itemController = value; }
        internal MailItem MailItem { get => _mailItem; set => _mailItem = value; }
    }

}

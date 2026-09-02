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

        /// <summary>
        /// The predetermined high-confidence folder path (Issue #171) carried from the pre-filter
        /// through the carrier-list load path. Null on the standard (non-high-confidence) load path.
        /// </summary>
        internal string PredeterminedFolder { get; set; }

        /// <summary>
        /// Issue #678. The already-initialised folder search handler carried alongside
        /// <see cref="PredeterminedFolder"/> from the dequeue-time confidence gate, so the item
        /// controller adopts it instead of running a second
        /// <c>FolderPredictor.InitAsync(FromField)</c> pass. Null on the standard load path and
        /// whenever the producer published no handler.
        /// </summary>
        internal IFolderSearchHandler CarriedFolderHandler { get; set; }
    }
}

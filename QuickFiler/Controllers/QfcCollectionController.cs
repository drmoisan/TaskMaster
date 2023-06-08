using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilitiesCS;
using UtilitiesCS;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;


namespace QuickFiler.Controllers
{
    internal class QfcCollectionController : IQfcCollectionController
    {
        public QfcCollectionController(IApplicationGlobals AppGlobals,
                                       QfcFormViewer viewerInstance,
                                       bool darkMode,
                                       Enums.InitTypeEnum InitType,
                                       IQfcFormController ParentObject)
        {

            _formViewer = viewerInstance;
            _itemTLP = _formViewer.L1v0L2L3v_TableLayout;
            _initType = InitType;
            _globals = AppGlobals;
            _parent = ParentObject;
            SetupLightDark(darkMode);
        }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private QfcFormViewer _formViewer;
        private Enums.InitTypeEnum _initType;
        private IApplicationGlobals _globals;
        private IQfcFormController _parent;
        private int _itemHeight;
        private TableLayoutPanel _itemTLP;
        private List<ItemGroup> _itemGroups = new List<ItemGroup>();
        private bool _darkMode;
        private RowStyle _template;
        private int _intActiveSelection;

        public int EmailsLoaded
        {
            get 
            {
                return _itemGroups.Count;
            } 
        }

        public bool ReadyForMove
        {
            get
            {
                bool blReadyForMove = true;
                string strNotifications = "Can't complete actions! Not all emails assigned to folder" + System.Environment.NewLine;

                foreach (var grp in _itemGroups)
                {
                    string[] headers = {"======= SEARCH RESULTS =======",
                                        "======= RECENT SELECTIONS ========",
                                        "========= SUGGESTIONS =========" };
                    if ((grp.ItemController.SelectedFolder is null) || 
                        headers.Contains(grp.ItemController.SelectedFolder))
                    {
                        blReadyForMove = false;
                        strNotifications = strNotifications + 
                                           grp.ItemController.Position + 
                                           "  " + 
                                           grp.ItemController.Mail.SentOn.ToString("MM/dd/yyyy") +
                                           "  " + 
                                           grp.ItemController.Mail.Subject + 
                                           Environment.NewLine;
                    }
                }
                if (!blReadyForMove)
                    MessageBox.Show("Error Notification", strNotifications, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return blReadyForMove;
            }
        }

        public void LoadControlsAndHandlers(IList<MailItem> listMailItems, RowStyle template)
        {
            _itemTLP.SuspendLayout();
            _template = template;
            TableLayoutHelper.InsertSpecificRow(_itemTLP, 0, template, listMailItems.Count);
            int i = 0;
            foreach (MailItem mailItem in listMailItems)
            {
                ItemGroup grp = new(mailItem);
                grp.ItemViewer = LoadItemViewer(i++, template, true);
                _itemGroups.Add(grp);
            }

            _formViewer.WindowState = FormWindowState.Maximized;

            //foreach (var grp in _itemGroups)
            Parallel.ForEach(_itemGroups, grp =>
            {
            grp.ItemController = new QfcItemController(_globals, grp.ItemViewer, i, grp.MailItem, this);
            Parallel.Invoke(
                () => grp.ItemController.PopulateConversation(),
                () => grp.ItemController.PopulateFolderCombobox(),
                () => 
                {
                    if (_darkMode) { grp.ItemController.SetThemeDark(); }
                    else { grp.ItemController.SetThemeLight(); } 
                });
            });

            
            _itemTLP.ResumeLayout();
        }

        public QfcItemViewer LoadItemViewer(int itemNumber,
                                            RowStyle template,
                                            bool blGroupConversation = true, 
                                            int columnNumber = 0)
        {
            QfcItemViewer itemViewer = new();
            _itemTLP.MinimumSize = new System.Drawing.Size(
                _itemTLP.MinimumSize.Width, 
                _itemTLP.MinimumSize.Height + 
                (int)Math.Round(template.Height, 0));
            // moved to caller for efficencies of multiple insertions
            //TableLayoutHelper.InsertSpecificRow(_itemTLP, itemNumber - 1, template.Clone());
            itemViewer.Parent = _itemTLP;
            if (columnNumber == 0)
            {
                _itemTLP.SetCellPosition(itemViewer, new TableLayoutPanelCellPosition(columnNumber, itemNumber - 1));
                _itemTLP.SetColumnSpan(itemViewer, 2);
            }
            else
            {
                _itemTLP.SetCellPosition(itemViewer, new TableLayoutPanelCellPosition(1, itemNumber - 1));
                _itemTLP.SetColumnSpan(itemViewer, 1);
            }
            
            itemViewer.AutoSize = true;
            itemViewer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            itemViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            itemViewer.Dock = DockStyle.Fill;
            return itemViewer;
        }

        public void AddEmailControlGroup(MailItem mailItem,
                                         int posInsert = 0,
                                         bool blGroupConversation = true,
                                         int ConvCt = 0,
                                         object varList = null,
                                         bool blChild = false)
        {
            
            if (posInsert == 0)
                posInsert = _itemGroups.Count + 1;
                
            //LoadGroupOfCtrls(ref listCtrls, _intUniqueItemCounter, posInsert, blGroupConversation);
            //itemController = new QfcController(mailItem, listCtrls, posInsert, _boolRemoteMouseApp, this, _globals);
            //if (blChild)
            //    itemController.BlHasChild = true;
            //if (folderList is Array == true)
            //{
            //    if (((Array)folderList).GetUpperBound(0) == 0)
            //    {
            //        itemController.PopulateFolderCombobox();
            //    }
            //    else
            //    {
            //        itemController.PopulateFolderCombobox(folderList);
            //    }
            //}
            //else
            //{
            //    itemController.PopulateFolderCombobox(folderList);
            //}
            //itemController.CountMailsInConv(insertionCount);

            //if (posInsert > _listQFClass.Count)
            //{
            //    _listQFClass.Add(itemController);
            //}
            //else
            //{
            //    // _listQFClass.Add(qf, qf.mailItem.Subject & qf.mailItem.SentOn & qf.mailItem.Sender, posInsert)
            //    _listQFClass.Insert(posInsert, itemController);
            //}

            //// For i = 1 To _listQFClass.Count
            //// qf = _listQFClass(i)
            //// Debug.WriteLine("_listQFClass(" & i & ")   MyPosition " & qf.intMyPosition & "   " & qf.mailItem.Subject)
            //// Next i

            
        }

        public void RemoveControls()
        {
            //TODO: Optimize removal so all are removed at once using new helper
            if (_itemGroups is not null)
            {
                _itemTLP.SuspendLayout();
                while (_itemGroups.Count > 0)
                {
                    int i = _itemGroups.Count - 1;

                    // Remove event managers and dispose unmanaged
                    _itemGroups[i].ItemController.Cleanup();

                    // Remove Item Viewer and Row from the form
                    TableLayoutHelper.RemoveSpecificRow(_itemTLP, i);

                    // Remove Handle on item viewer and controller
                    _itemGroups.RemoveAt(i);  
                }
                _itemTLP.ResumeLayout();
            }
        }

        public void RemoveSpaceToCollapseConversation()
        {
            // Perhaps can eliminate
            throw new NotImplementedException();
        }

        public void RemoveSpecificControlGroup(int intPosition)
        {
            throw new NotImplementedException();
        }

        public int ActivateByIndex(int intNewSelection, bool blExpanded)
        {
            if (intNewSelection > 0 & intNewSelection <= _itemGroups.Count)
            {
                IQfcItemController itemController = _itemGroups[intNewSelection - 1].ItemController;
                QfcItemViewer itemViewer = _itemGroups[intNewSelection - 1].ItemViewer;
                
                itemController.Accel_FocusToggle();
                if (blExpanded)
                {
                    // BUGFIX: Replace Function MoveDownPix
                    //MoveDownPix(intNewSelection + 1, itemController.ItemPanel.Height);
                    itemController.ExpandCtrls1();
                }
                _intActiveSelection = intNewSelection;
                _formViewer.L1v0L2L3v_TableLayout.ScrollControlIntoView(itemViewer);
            }
            return _intActiveSelection;
        }

        public bool ToggleOffActiveItem(bool parentBlExpanded)
        {
            bool blExpanded = parentBlExpanded;
            if (_intActiveSelection != 0)
            {
                //adjusted to _intActiveSelection -1 to accommodate zero based
                IQfcItemController itemController = _itemGroups[_intActiveSelection - 1].ItemController;
                if (itemController.BlExpanded)
                {
                    //TODO: Replace MoveDownPix Function
                    //MoveDownPix(_intActiveSelection + 1, (int)Math.Round(itemController.ItemPanel.Height * -0.5d));
                    itemController.ExpandCtrls1();
                    blExpanded = true;
                }
                itemController.Accel_FocusToggle();

                //QUESTION: This assignment worries me and will be out of sync 
                _intActiveSelection = 0;
            }
            return blExpanded;
        }

        public void SelectNextItem()
        {
            if (_intActiveSelection < _itemGroups.Count)
            {
                //BUGFIX: Write logic to select the next item
                //_viewer.KeyboardDialog.Text = (_intActiveSelection + 1).ToString();
            }
            // Deactivated code to reset dialog accelerator since not using
            //_viewer.KeyboardDialog.SelectionStart = _viewer.KeyboardDialog.TextLength;
        }

        public void SelectPreviousItem()
        {
            if (_intActiveSelection > 0)
            {
                //BUGFIX: Write logic to select the next item
                // _viewer.KeyboardDialog.Text = (_intActiveSelection - 1).ToString();
            }
            // Deactivated code to reset dialog accelerator since not using
            // _viewer.KeyboardDialog.SelectionStart = _viewer.KeyboardDialog.TextLength;
        }

        public void MoveDownControlGroups(int intPosition, int intMoves)
        {
            // Perhaps this can be eliminated
            // throw new NotImplementedException();
        }

        public void MoveDownPix(int intPosition, int intPix)
        {
            // Perhaps this can be eliminated
            // throw new NotImplementedException();
        }

        public void ResizeChildren(int intDiffx)
        {
            // Perhaps this can be eliminated
            // throw new NotImplementedException();
        }

        public void ConvToggle_Group(IList<MailItem> selItems, int indexOriginal)
        {
            _itemTLP.SuspendLayout();

            int removalIndex = indexOriginal + 1;
            int membersToRemove = selItems.Count - 1;

            var qfOriginal = _itemGroups[indexOriginal].ItemController;
            TableLayoutHelper.RemoveSpecificRow(_itemTLP, removalIndex, membersToRemove);
            
            for (int i = 0; i < membersToRemove; i++)
            {
                _itemGroups[removalIndex].ItemController.Cleanup();
                _itemGroups.RemoveAt(removalIndex);
            }
            
        }

        /// <summary>
        /// Expands each member of a conversation into its own ItemViewer/ItemController while replicating
        /// the sorting suggestions of the base member
        /// </summary>
        /// <param name="mailItems">Qualifying Conversation Members</param>
        /// <param name="baseEmailIndex">Index of base member in collection</param>
        /// <param name="conversationCount">Number of qualifying conversation members</param>
        /// <param name="folderList">Sorting suggestions from base member</param>
        public void ConvToggle_UnGroup(IList<MailItem> mailItems,
                                       int baseEmailIndex,
                                       int conversationCount,
                                       object folderList)
        {
            _itemTLP.SuspendLayout();
            int insertionIndex = baseEmailIndex + 1;
            int membersToInsert = conversationCount - 1;
            
            TableLayoutHelper.InsertSpecificRow(panel: _itemTLP, 
                                                rowIndex: insertionIndex, 
                                                templateStyle: _template, 
                                                insertCount: membersToInsert);

            // Insert the datamodel placeholders
            InsertItemGroups(baseEmailIndex, conversationCount);
            EnumerateConversationMembers(mailItems, baseEmailIndex, conversationCount, folderList);

            _itemTLP.ResumeLayout();
        }

        /// <summary>
        /// Parallel function to expand each member of a conversation into individual ItemViewers/Controllers.
        /// Expanded members are inserted into the base collection and conversation count and folder suggestions
        /// are replicated from the base member. This enables distinct actions to be taken with each member
        /// </summary>
        /// <param name="mailItems">List of MailItems in a conversation</param>
        /// <param name="insertionIndex">Location of the Item Group collection where the base member is stored</param>
        /// <param name="conversationCount">Number of qualifying conversation members</param>
        /// <param name="folderList">Folder suggestions for the first email</param>
        internal void EnumerateConversationMembers(IList<MailItem> mailItems, int insertionIndex, int conversationCount, object folderList)
        {
            Enumerable.Range(0, conversationCount - 1).AsParallel().Select(i =>
            {
                var grp = _itemGroups[i + insertionIndex];
                grp.ItemViewer = LoadItemViewer(i + insertionIndex, _template, false, 1);
                grp.MailItem = mailItems[i];
                grp.ItemController = new QfcItemController(_globals, grp.ItemViewer, i + insertionIndex, grp.MailItem, this);
                Parallel.Invoke(
                    () => grp.ItemController.PopulateConversation(conversationCount),
                    () => grp.ItemController.PopulateFolderCombobox(folderList),
                    () =>
                    {
                        if (_darkMode) { grp.ItemController.SetThemeDark(); }
                        else { grp.ItemController.SetThemeLight(); }
                    });
                return i;
            });
        }

        /// <summary>
        /// Creates empty item groups and inserts them into the 
        /// collection at the targeted location
        /// </summary>
        /// <param name="insertionIndex">Targeted location for the insertion</param>
        /// <param name="insertionCount">Number of elements to insert</param>
        internal void InsertItemGroups(int insertionIndex, int insertionCount)
        {
            for (int i = 0; i < insertionCount - 1; i++)
            {
                var grp = new ItemGroup();
                _itemGroups.Insert(insertionIndex, grp);
            }
        }

        public void MakeSpaceToEnumerateConversation()
        {
            throw new NotImplementedException();
        }

        public bool IsSelectionBelowMax(int intNewSelection)
        {
            throw new NotImplementedException();
        }

        private void SetupLightDark(bool initDarkMode)
        {
            _darkMode = initDarkMode;
            _formViewer.DarkMode.CheckedChanged += new System.EventHandler(DarkMode_CheckedChanged);
            
        }

        private void DarkMode_CheckedChanged(object sender, EventArgs e)
        {
            if (_formViewer.DarkMode.Checked==true)
            {
                SetDarkMode();
            }
            else
            {
                SetLightMode();
            }
        }

        public void SetDarkMode()
        {
            foreach (ItemGroup itemGroup in _itemGroups)
            {
                itemGroup.ItemController.SetThemeDark();
            }
        }

        public void SetLightMode()
        {
            foreach (ItemGroup itemGroup in _itemGroups)
            {
                itemGroup.ItemController.SetThemeLight();
            }
        }

        public void Cleanup()
        {
            RemoveControls();
            _formViewer = null;
            _globals = null;
            _parent = null;
            _itemTLP = null;
            _itemGroups = null;
        }

        public void MoveEmails(StackObjectCS<MailItem> stackMovedItems)
        {
            foreach (var grp in _itemGroups)
            {
                //TODO: function needed to shut off KeyboardDialog at this step if active
                grp.ItemController.MoveMail();
                stackMovedItems.Push(grp.MailItem);
            }
        }

        public string[] GetMoveDiagnostics(string durationText, string durationMinutesText, double Duration, string dataLineBeg, DateTime OlEndTime, ref AppointmentItem OlAppointment)
        {
            int k;
            string[] strOutput = new string[EmailsLoaded + 1];
            var loopTo = EmailsLoaded;
            for (k = 1; k <= loopTo; k++)
            {
                var QF = _itemGroups[k].ItemController;
                var infoMail = new cInfoMail();
                if (infoMail.Init_wMail(QF.Mail, OlEndTime: OlEndTime, lngDurationSec: (int)Math.Round(Duration)))
                {
                    if (string.IsNullOrEmpty(OlAppointment.Body))
                    {
                        OlAppointment.Body = infoMail.ToString;
                        OlAppointment.Save();
                    }
                    else
                    {
                        OlAppointment.Body = OlAppointment.Body + System.Environment.NewLine + infoMail.ToString;
                        OlAppointment.Save();
                    }
                }
                string dataLine = dataLineBeg + xComma(QF.Subject);
                dataLine = dataLine + "," + "QuickFiled";
                dataLine = dataLine + "," + durationText;
                dataLine = dataLine + "," + durationMinutesText;
                dataLine = dataLine + "," + xComma(QF.To);
                dataLine = dataLine + "," + xComma(QF.Sender);
                dataLine = dataLine + "," + "Email";
                dataLine = dataLine + "," + xComma(QF.SelectedFolder);           // Target Folder
                dataLine = dataLine + "," + QF.SentDate;
                dataLine = dataLine + "," + QF.SentTime;
                strOutput[k] = dataLine;
            }

            return default;
        }

        private string xComma(string str)
        {
            string xCommaRet = default;
            string strTmp;

            strTmp = str.Replace(", ", "_");
            strTmp = strTmp.Replace(",", "_");
            xCommaRet = StringManipulation.GetStrippedText(strTmp);
            return xCommaRet;
            // xComma = StripAccents(strTmp)
        }

        public class ItemGroup
        {
            public ItemGroup() { }
            public ItemGroup(MailItem mailItem) { _mailItem = mailItem; }

            private QfcItemViewer _itemViewer;
            private IQfcItemController _itemController;
            private MailItem _mailItem;

            internal QfcItemViewer ItemViewer { get => _itemViewer; set => _itemViewer = value; }
            internal IQfcItemController ItemController { get => _itemController; set => _itemController = value; }
            internal MailItem MailItem { get => _mailItem; set => _mailItem = value; }

        }

        
    }
}

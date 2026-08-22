using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using QuickFiler.Interfaces;
using UtilitiesCS;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace QuickFiler.Controllers
{
    internal class QfcExplorerController : IQfcExplorerController
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        public QfcExplorerController(
            QfEnums.InitTypeEnum initType,
            IApplicationGlobals appGlobals,
            IFilerHomeController parent
        )
        {
            _initType = initType;
            _globals = appGlobals;
            _activeExplorer = _globals.Ol.App.ActiveExplorer();
            _parent = parent;
        }

        private QfEnums.InitTypeEnum _initType;
        private IApplicationGlobals _globals;
        private IFilerHomeController _parent;
        private Explorer _activeExplorer;
        private Outlook.View _objView;
        private string _objViewMem;
        public Outlook.View ObjViewTemp;

        //PRIORITY: Implement BlShowInConversations
        private bool _blShowInConversations;
        public bool BlShowInConversations
        {
            get => _blShowInConversations;
            set => _blShowInConversations = value;
        }

        internal bool CurrentConversationState
        {
            get => _activeExplorer.CommandBars.GetPressedMso("ShowInConversations");
        }

        // Injectable seam for the not-in-view prompt. The branch it guards calls a modal WinForms
        // dialog, which cannot be exercised in a headless unit test: the dialog blocks on user input
        // and requires a message pump. Tests replace this delegate with a stub that records the
        // arguments and returns the DialogResult under test. The delegate type is written fully
        // qualified as System.Func<...> so the seam does not resurrect the `using System;` directive
        // that was removed as orphaned, matching the file's existing fully-qualified style for
        // log4net.ILog and System.Reflection.MethodBase above.
        internal System.Func<
            string,
            string,
            MessageBoxButtons,
            MessageBoxIcon,
            DialogResult
        > NotInViewDialogInvoker { get; set; } =
            (text, caption, buttons, icon) => MessageBox.Show(text, caption, buttons, icon);

        public void ExplConvView_ReturnState()
        {
            if (BlShowInConversations)
                ExplConvView_ToggleOn();
        }

        public void ExplConvView_ToggleOff()
        {
            if (_activeExplorer.CommandBars.GetPressedMso("ShowInConversations"))
            {
                BlShowInConversations = true;
                _objView = (Outlook.View)_activeExplorer.CurrentView;

                if (_objView.Name == "tmpNoConversation")
                {
                    if (_activeExplorer.CommandBars.GetPressedMso("ShowInConversations"))
                    {
                        _objView.XML = _objView.XML.Replace("<upgradetoconv>1</upgradetoconv>", "");
                        _objView.Save();
                        _objView.Apply();
                    }
                }
                _objViewMem = _objView.Name;
                if (_objViewMem == "tmpNoConversation")
                    _objViewMem = _globals.Ol.ViewWide;

                //ObjViewTemp = ObjView.Parent("tmpNoConversation");
                ObjViewTemp = GetSiblingView(_objView, "tmpNoConversation");

                if (ObjViewTemp is null)
                {
                    ObjViewTemp = _objView.Copy(
                        "tmpNoConversation",
                        OlViewSaveOption.olViewSaveOptionThisFolderOnlyMe
                    );
                    ObjViewTemp.XML = _objView.XML.Replace("<upgradetoconv>1</upgradetoconv>", "");
                    ObjViewTemp.Save();
                }
                ObjViewTemp.Apply();
            }
        }

        public Outlook.View GetSiblingView(Outlook.View currentView, string viewName)
        {
            Outlook.View view = null;
            var views = (Views)currentView.Parent;
            foreach (Outlook.View v in views)
            {
                if (v.Name == viewName)
                {
                    view = v;
                    break;
                }
            }
            return view;
        }

        public void ExplConvView_ToggleOn()
        {
            if (BlShowInConversations)
            {
                _objView = _activeExplorer.CurrentFolder.Views[_objViewMem];
                _objView.Apply();
                BlShowInConversations = false;
            }
        }

        private void NavigateToOutlookFolder(MailItem mailItem)
        {
            if (
                _activeExplorer.CurrentFolder.FolderPath != ((MAPIFolder)mailItem.Parent).FolderPath
            )
            {
                ExplConvView_ReturnState();
                _activeExplorer.CurrentFolder = (MAPIFolder)mailItem.Parent;
                BlShowInConversations = AutoFile.AreConversationsGrouped(_activeExplorer);
            }
        }

        //PRIORITY: Implement OpenQFItem
        public async Task OpenQFItem(MailItem mailItem)
        {
            _parent.FormController.MinimizeFormViewer();
            NavigateToOutlookFolder(mailItem);
            if (
                _initType.HasFlag(QfEnums.InitTypeEnum.Sort)
                & AutoFile.AreConversationsGrouped(_activeExplorer)
            )
                await Task.Run(() => ExplConvView_ToggleOff());

            if (_activeExplorer.IsItemSelectableInView(mailItem))
            {
                await Task.Run(() => _activeExplorer.ClearSelection());
                await Task.Run(() => _activeExplorer.AddToSelection(mailItem));

                //MAPIFolder tmp = _activeExplorer.CurrentFolder;
                //MAPIFolder drafts = _globals.Ol.NamespaceMAPI.GetDefaultFolder(OlDefaultFolders.olFolderDrafts);
                //_activeExplorer.CurrentFolder = drafts;
                //_activeExplorer.CurrentFolder.Display();
            }
            else
            {
                DialogResult result = NotInViewDialogInvoker(
                    "Selected message is not in view. Would you like to open it?",
                    "Error",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error
                );
                if (result == DialogResult.Yes)
                {
                    mailItem.Display();
                }
            }
            if (_initType.HasFlag(QfEnums.InitTypeEnum.Sort) & BlShowInConversations)
                await Task.Run(() => ExplConvView_ToggleOn());
        }
    }
}
